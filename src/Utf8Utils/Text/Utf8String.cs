using Utf8Utils.Collections;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Utf8Utils.Text
{
    /// <summary>
    /// UTF8 を直接読み書きする構造体。
    /// </summary>
    /// <remarks>
    /// corefxlab で同系統の型が作られてるんだけど、それが依存している System.Memory パッケージが .NET 3.5 では動かないので。
    /// <see cref="ArraySegment{T}"/>ベースで自作。
    /// <see cref="ArraySegment{T}"/>なのと、最適化甘いのとであんまり高速じゃないはずだけど、
    /// それでも<see cref="System.Text.Encoding.GetString(byte[])"/>とかでデコードするよりはだいぶ速いはず。
    /// </remarks>
    public struct Utf8String : IEnumerable<byte>, IEquatable<Utf8String>, IEquatable<string>
    {
        private readonly ArraySegment<byte> _buffer;

        /// <summary>
        /// string から初期化。
        /// </summary>
        public Utf8String(string s) : this(System.Text.Encoding.UTF8.GetBytes(s)) { }

        /// <summary>
        /// UTF8 文字列が入った byte 配列から初期化。
        /// </summary>
        public Utf8String(byte[] encodedBytes) => _buffer = encodedBytes == null ? default(ArraySegment<byte>) : new ArraySegment<byte>(encodedBytes);

        /// <summary>
        /// UTF8 文字列が入った byte 配列から初期化。
        /// </summary>
        public Utf8String(byte[] data, int offset, int count) => _buffer = new ArraySegment<byte>(data, offset, count);

        /// <summary>
        /// UTF8 文字列が入った <see cref="ArraySegment{T}"/> から初期化。
        /// </summary>
        public Utf8String(ArraySegment<byte> encodedBytes) => _buffer = encodedBytes;

        internal ArraySegment<byte> Buffer => _buffer;

        /// <summary>
        /// byte 列の列挙用。
        /// </summary>
        /// <returns></returns>
        public Enumerator GetEnumerator() => new Enumerator(_buffer);

        IEnumerator<byte> IEnumerable<byte>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// コードポイントの列挙用。
        /// </summary>
        public CodePointEnumerable CodePoints => new CodePointEnumerable(_buffer);

        /// <summary>
        /// string 化。
        /// </summary>
        /// <returns></returns>
        public override string ToString() => _buffer.Array == null ? null : System.Text.Encoding.UTF8.GetString(_buffer.Array, _buffer.Offset, _buffer.Count);

        /// <summary>
        /// i byte 目を取得。
        /// </summary>
        public byte this[int i] => _buffer.At(i);

        /// <summary>
        /// UTF8 エンコード済みの byte 数。
        /// </summary>
        public int Length => _buffer.Count;

        /// <summary>
        /// コードポイントで数えた文字数。
        /// </summary>
        public int CodePointLength => Utf8Decoder.GetLength(_buffer);

        #region equality
#pragma warning disable 1591

        public static bool operator ==(Utf8String x, Utf8String y) => x.Equals(y);
        public static bool operator !=(Utf8String x, Utf8String y) => x.Equals(y);

        public bool Equals(Utf8String other) => _buffer.SequenceEqual(other._buffer);
        public bool Equals(string other)
        {
            // default(Utf8String) は Empty 扱い
            if (other == null) return false;

            var len = Length;
            if (len == 0 && other.Length == 0) return true;

            // UTF16 → UTF8: 全部が3バイト文字でも最大で3倍差
            if (3 * other.Length < len) return false;
            // 全部が1バイト文字のときに長さ一致するのが最小
            if (other.Length > len) return false;

            // 短い時だけ stackalloc を使う
            if (other.Length < 300)
            {
                return ShortEquals(other);
            }

            return LongEquals(other);
        }

        /// <summary>
        /// 文字列が短い時は、stackalloc した領域に GetBytes してバイナリ比較。
        /// </summary>
        private bool ShortEquals(string other) => ArraySegmentExtensions.SequenceEqual(Buffer.Array, Buffer.Offset, Buffer.Count, other);

        /// <summary>
        /// 文字列が長い時は1文字ずつコードポイント比較。
        /// </summary>
        /// <remarks>
        /// 正直、<see cref="ShortEquals(string)"/>をループで回した方が速いかも。
        /// </remarks>
        private bool LongEquals(string other)
        {
            var e1 = new CodePointEnumerator(_buffer);
            var e2 = new Utf16Enumerator(other);

            while (true)
            {
                if (e1.MoveNext())
                {
                    if (!e2.MoveNext()) return false;
                    if (e1.Current != e2.Current) return false;
                }
                else
                {
                    if (e2.MoveNext()) return false;
                    else return true;
                }
            }
        }

        public override bool Equals(object obj) => obj is Utf8String other && Equals(other) || obj is string s && Equals(s);

        public override int GetHashCode() => FarmHash.GetHashCode(_buffer);

#pragma warning restore
        #endregion

        /// <summary>
        /// 部分文字列を取得。
        /// </summary>
        /// <param name="index">取得したい部分文字列の開始位置。</param>
        public Utf8String Substring(int index)
        {
            return Substring(index, Length - index);
        }

        /// <summary>
        /// 部分文字列を取得。
        /// </summary>
        /// <param name="index">取得したい部分文字列の開始位置。</param>
        /// <param name="length">取得したい部分文字列の文字数。</param>
        public Utf8String Substring(int index, int length)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if (length == 0)
            {
                return default(Utf8String);
            }

            if (length == Length)
            {
                return this;
            }

            if (index + length > Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            return new Utf8String(_buffer.Slice(index, length));
        }

        /// <summary>
        /// 先頭の空白文字を除去。
        /// </summary>
        public Utf8String TrimStart()
        {
            var it = new CodePointEnumerator(_buffer);
            while (it.MoveNext() && IsWhitespace(it.Current)) ;
            return Substring(it.PositionInCodeUnits);
        }

        /// <summary>
        /// 空白文字かどうかを判定。
        /// </summary>
        /// <remarks>
        /// BOM も空白文字扱いしちゃってるので注意。
        /// </remarks>
        public static bool IsWhitespace(uint codePoint)
        {
            return Array.BinarySearch(SortedWhitespaceCodePoints, codePoint) >= 0;
        }

        // BOM も空白扱いしてしまう
        private static readonly uint[] SortedWhitespaceCodePoints = new uint[26]
        {
            0x0009, 0x000A, 0x000B, 0x000C, 0x000D,
            0x0020,
            0x0085,
            0x00A0,
            0x1680,
            0x2000, 0x2001, 0x2002, 0x2003, 0x2004, 0x2005, 0x2006,
            0x2007,
            0x2008, 0x2009, 0x200A,
            0x2028, 0x2029,
            0x202F,
            0x205F,
            0x3000,
            0xFEFF
        };

        /// <summary>
        /// 文字列の中から特定のパターンを探す。
        /// </summary>
        /// <param name="pattern">パターン。</param>
        /// <returns>見つかった場合その開始位置。見つからない場合 -1。</returns>
        public int IndexOf(Utf8String pattern) => BoyerMoore.IndexOf(_buffer, pattern._buffer);

        /// <summary>
        /// 文字列の中から特定のパターンを探す。
        /// </summary>
        /// <param name="startIndex">検索開始位置。</param>
        /// <param name="pattern">パターン。</param>
        /// <returns>見つかった場合その開始位置。見つからない場合 -1。</returns>
        /// <summary>
        public int IndexOf(Utf8String pattern, int startIndex) => BoyerMoore.IndexOf(_buffer.Slice(startIndex), pattern._buffer);

        /// <summary>
        /// <see cref="Utf8String"/>のバイト列を列挙するための enumerator。
        /// 実際のところ、<see cref="ArraySegment{T}"/>列挙子。
        /// </summary>
        public struct Enumerator : IEnumerator<byte>
        {
            private readonly ArraySegment<byte> _buffer;
            private int _index;

            internal Enumerator(ArraySegment<byte> buffer)
            {
                _buffer = buffer;
                _index = -1;
            }

            /// <summary><see cref="IEnumerator{T}.Current"/></summary>
            public byte Current => _buffer.At(_index);

            /// <summary><see cref="IEnumerator.MoveNext"/></summary>
            public bool MoveNext() => ++_index < _buffer.Count;

            /// <summary><see cref="IEnumerator.Reset"/></summary>
            public void Reset() => _index = -1;

            object IEnumerator.Current => Current;
            void IDisposable.Dispose() { }
        }

        /// <summary>
        /// コードポイント列挙用の enumerable
        /// </summary>
        public struct CodePointEnumerable : IEnumerable<uint>
        {
            private readonly ArraySegment<byte> _buffer;

            /// <summary></summary>
            /// <param name="buffer">UTF8 が入った byte 列。</param>
            public CodePointEnumerable(ArraySegment<byte> buffer) => _buffer = buffer;

            /// <summary><see cref="IEnumerable{T}.GetEnumerator"/></summary>
            public CodePointEnumerator GetEnumerator() => new CodePointEnumerator(_buffer);
            IEnumerator<uint> IEnumerable<uint>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// <summary>
        /// コードポイント列挙用の enumerator
        /// </summary>
        public struct CodePointEnumerator : IEnumerator<uint>
        {
            private readonly ArraySegment<byte> _buffer;
            private int _index;
            private byte _count;

            /// <summary></summary>
            /// <param name="buffer">UTF8 が入った byte 列。</param>
            public CodePointEnumerator(ArraySegment<byte> buffer)
            {
                _buffer = buffer;
                _index = 0;
                _count = 0;
                Current = 0;
            }

            internal int PositionInCodeUnits => _index;

            /// <summary><see cref="IEnumerator{T}.Current"/></summary>
            public uint Current { get; private set; }

            /// <summary><see cref="IEnumerator.MoveNext"/></summary>
            public bool MoveNext()
            {
                _index += _count;
                var count = Utf8Decoder.TryDecode(_buffer, _index, out var cp);
                if (count == Utf8Decoder.InvalidCount) return false;
                _count = count;
                Current = cp;
                return true;
            }

            /// <summary><see cref="IEnumerator.Reset"/></summary>
            public void Reset() { _index = 0; _count = 0; }

            void IDisposable.Dispose() { }
            object IEnumerator.Current => Current;
        }

        /// <summary>
        /// string からコードポイントを列挙するための enumerator
        /// </summary>
        public struct Utf16Enumerator : IEnumerator<uint>
        {
            private readonly string _str;
            private int _index;
            private byte _count;

            /// <summary>
            /// </summary>
            /// <param name="s">コードポイントを読み出す元。</param>
            public Utf16Enumerator(string s)
            {
                _str = s;
                _index = 0;
                _count = 0;
                Current = 0;
            }

            internal int PositionInCodeUnits => _index;

            /// <summary><see cref="IEnumerator{T}.Current"/></summary>
            public uint Current { get; private set; }

            /// <summary><see cref="IEnumerator.MoveNext"/></summary>
            public bool MoveNext()
            {
                _index += _count;

                if (_index >= _str.Length) return false;

                var c = _str[_index];
                if (char.IsHighSurrogate(c))
                {
                    if (_index + 1 >= _str.Length) return false;

                    var x = (c & 0b00000011_11111111U) + 0b100_0000;
                    x <<= 10;
                    c = _str[_index + 1];
                    x |= (c & 0b00000011_11111111U);

                    Current = x;
                    _count = 2;
                }
                else
                {
                    Current = c;
                    _count = 1;
                }
                return true;
            }

            /// <summary><see cref="IEnumerator.Reset"/></summary>
            public void Reset() { _index = 0; _count = 0; }

            void IDisposable.Dispose() { }
            object IEnumerator.Current => Current;
        }
    }
}