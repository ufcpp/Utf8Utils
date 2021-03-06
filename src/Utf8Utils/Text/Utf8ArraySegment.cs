﻿using Utf8Utils.Collections;
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
    public struct Utf8ArraySegment : IUtf8String, IEquatable<Utf8ArraySegment>
    {
        private readonly ArraySegment<byte> _buffer;

        /// <summary>
        /// string から初期化。
        /// </summary>
        public Utf8ArraySegment(string s) : this(System.Text.Encoding.UTF8.GetBytes(s)) { }

        /// <summary>
        /// UTF8 文字列が入った byte 配列から初期化。
        /// </summary>
        public Utf8ArraySegment(byte[] encodedBytes) => _buffer = encodedBytes == null ? default(ArraySegment<byte>) : new ArraySegment<byte>(encodedBytes);

        /// <summary>
        /// UTF8 文字列が入った byte 配列から初期化。
        /// </summary>
        public Utf8ArraySegment(byte[] data, int offset, int count) => _buffer = new ArraySegment<byte>(data, offset, count);

        /// <summary>
        /// UTF8 文字列が入った <see cref="ArraySegment{T}"/> から初期化。
        /// </summary>
        public Utf8ArraySegment(ArraySegment<byte> encodedBytes) => _buffer = encodedBytes;

        public ArraySegment<byte> Utf8 => _buffer;

        public bool IsNull => _buffer.Array == null;

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
        public override string ToString() => _buffer.Array == null ? "" : System.Text.Encoding.UTF8.GetString(_buffer.Array, _buffer.Offset, _buffer.Count);

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

        public static explicit operator Utf8ArraySegment(string s) => new Utf8ArraySegment(s);
        public static explicit operator Utf8ArraySegment(byte[] s) => new Utf8ArraySegment(s);
        public static explicit operator Utf8ArraySegment(ArraySegment<byte> s) => new Utf8ArraySegment(s);

        #region equality
#pragma warning disable 1591

        public static bool operator ==(Utf8ArraySegment x, Utf8ArraySegment y) => x.Equals(y);
        public static bool operator !=(Utf8ArraySegment x, Utf8ArraySegment y) => x.Equals(y);

        public bool Equals(Utf8ArraySegment other) => _buffer.SequenceEqual(other._buffer);
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
        private bool ShortEquals(string other) => ArraySegmentExtensions.SequenceEqual(_buffer.Array, _buffer.Offset, _buffer.Count, other);

        /// <summary>
        /// 文字列が長い時は1文字ずつコードポイント比較。
        /// </summary>
        /// <remarks>
        /// 正直、<see cref="ShortEquals(string)"/>をループで回した方が速いかも。
        /// </remarks>
        private bool LongEquals(string other)
        {
            var e1 = new CodePointEnumerator(_buffer);
            var e2 = new StringExtensions.CodePointEnumerator(other);

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

        public bool Equals(IUtf8String other) => Utf8.SequenceEqual(other.Utf8);

        public override bool Equals(object obj) => obj is Utf8ArraySegment other && Equals(other) || obj is string s && Equals(s);

        public override int GetHashCode() => FarmHash.GetHashCode(_buffer);

#pragma warning restore
        #endregion

        /// <summary>
        /// 部分文字列を取得。
        /// </summary>
        /// <param name="index">取得したい部分文字列の開始位置。</param>
        public Utf8ArraySegment Substring(int index)
        {
            return Substring(index, Length - index);
        }

        /// <summary>
        /// 部分文字列を取得。
        /// </summary>
        /// <param name="index">取得したい部分文字列の開始位置。</param>
        /// <param name="length">取得したい部分文字列の文字数。</param>
        public Utf8ArraySegment Substring(int index, int length)
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
                return default(Utf8ArraySegment);
            }

            if (length == Length)
            {
                return this;
            }

            if (index + length > Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            return new Utf8ArraySegment(_buffer.Slice(index, length));
        }

        /// <summary>
        /// 先頭の空白文字を除去。
        /// </summary>
        public Utf8ArraySegment TrimStart()
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
        public int IndexOf(Utf8ArraySegment pattern) => BoyerMoore.IndexOf(_buffer, pattern._buffer);

        /// <summary>
        /// 文字列の中から特定のパターンを探す。
        /// </summary>
        /// <param name="startIndex">検索開始位置。</param>
        /// <param name="pattern">パターン。</param>
        /// <returns>見つかった場合その開始位置。見つからない場合 -1。</returns>
        /// <summary>
        public int IndexOf(Utf8ArraySegment pattern, int startIndex) => BoyerMoore.IndexOf(_buffer.Slice(startIndex), pattern._buffer);

        /// <summary>
        /// <see cref="Utf8ArraySegment"/>のバイト列を列挙するための enumerator。
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
        /// <see cref="ArraySegment{T}"/>を配列化。
        /// </summary>
        /// <remarks>
        /// <see cref="ArraySegment{T}"/>の場合、長大なバイト列の中の一部分を指していることがあって、ずっと元の配列を持っていたくないことがある。
        /// なので、<see cref="ArraySegment{T}"/>が参照している範囲だけをコピーした配列を作って返す。
        ///
        /// <see cref="ArraySegment{T}"/>が配列全体を指してる(Offset が 0 で、Count が配列長に一致)ときは、もったいないので<see cref="ArraySegment{T}.Array"/>をそのまま返す。
        /// </remarks>
        /// <returns></returns>
        public Utf8Array ToArray()
        {
            var array = _buffer.Offset == 0 && _buffer.Count == _buffer.Array.Length
                ? _buffer.Array
                : _buffer.ToArray();
            return new Utf8Array(array);
        }
    }
}