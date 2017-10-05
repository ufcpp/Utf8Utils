using Utf8Utils.Collections;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Utf8Utils.Text
{
    /// <summary>
    /// UTF8 を直接読み書きする構造体。
    /// 配列版。
    /// </summary>
    /// <remarks>
    /// <see cref="Utf8ArraySegment"/> と2重実装なところが結構あるものの、
    /// offset/count のサイズ分の負担が案外馬鹿にならないので、配列だけを持つバージョンを用意。
    /// </remarks>
    public struct Utf8Array : IUtf8String, IEquatable<Utf8Array>
    {
        private readonly byte[] _buffer;

        /// <summary>
        /// string から初期化。
        /// </summary>
        public Utf8Array(string s) : this(System.Text.Encoding.UTF8.GetBytes(s)) { }

        /// <summary>
        /// UTF8 文字列が入った byte 配列から初期化。
        /// </summary>
        public Utf8Array(byte[] encodedBytes) => _buffer = encodedBytes;

        public ArraySegment<byte> Utf8 => new ArraySegment<byte>(_buffer);

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
        public override string ToString() => _buffer == null ? null : System.Text.Encoding.UTF8.GetString(_buffer);

        /// <summary>
        /// i byte 目を取得。
        /// </summary>
        public byte this[int i] => _buffer[i];

        /// <summary>
        /// UTF8 エンコード済みの byte 数。
        /// </summary>
        public int Length => _buffer.Length;

        /// <summary>
        /// コードポイントで数えた文字数。
        /// </summary>
        public int CodePointLength => Utf8Decoder.GetLength(new ArraySegment<byte>(_buffer));

        #region equality
#pragma warning disable 1591

        public static bool operator ==(Utf8Array x, Utf8Array y) => x.Equals(y);
        public static bool operator !=(Utf8Array x, Utf8Array y) => x.Equals(y);

        public bool Equals(Utf8Array other) => _buffer.SequenceEqual(other._buffer);
        public bool Equals(string other) => new Utf8ArraySegment(_buffer).Equals(other);
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
        public Utf8ArraySegment Substring(int index, int length) => new Utf8ArraySegment(_buffer).Substring(index, length);

        /// <summary>
        /// 先頭の空白文字を除去。
        /// </summary>
        public Utf8ArraySegment TrimStart() => new Utf8ArraySegment(_buffer).TrimStart();

        /// <summary>
        /// 文字列の中から特定のパターンを探す。
        /// </summary>
        /// <param name="pattern">パターン。</param>
        /// <returns>見つかった場合その開始位置。見つからない場合 -1。</returns>
        public int IndexOf(Utf8ArraySegment pattern) => BoyerMoore.IndexOf(new ArraySegment<byte>(_buffer), pattern.Utf8);

        /// <summary>
        /// 文字列の中から特定のパターンを探す。
        /// </summary>
        /// <param name="startIndex">検索開始位置。</param>
        /// <param name="pattern">パターン。</param>
        /// <returns>見つかった場合その開始位置。見つからない場合 -1。</returns>
        /// <summary>
        public int IndexOf(Utf8ArraySegment pattern, int startIndex) => BoyerMoore.IndexOf(_buffer.Slice(startIndex), pattern.Utf8);

        /// <summary>
        /// <see cref="Utf8ArraySegment"/>のバイト列を列挙するための enumerator。
        /// 実際のところ、<see cref="ArraySegment{T}"/>列挙子。
        /// </summary>
        public struct Enumerator : IEnumerator<byte>
        {
            private readonly byte[] _buffer;
            private int _index;

            internal Enumerator(byte[] buffer)
            {
                _buffer = buffer;
                _index = -1;
            }

            /// <summary><see cref="IEnumerator{T}.Current"/></summary>
            public byte Current => _buffer[_index];

            /// <summary><see cref="IEnumerator.MoveNext"/></summary>
            public bool MoveNext() => ++_index < _buffer.Length;

            /// <summary><see cref="IEnumerator.Reset"/></summary>
            public void Reset() => _index = -1;

            object IEnumerator.Current => Current;
            void IDisposable.Dispose() { }
        }

        public static implicit operator Utf8ArraySegment(Utf8Array s) => new Utf8ArraySegment(s._buffer);
    }
}