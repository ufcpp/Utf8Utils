using Utf8Utils.Collections;
using System;
using System.Collections.Generic;

namespace Utf8Utils.Text
{
    /// <summary>
    /// 普通に <see cref="string.GetHashCode"/> を使う <see cref="IEqualityComparer{T}"/>。
    /// </summary>
    public struct StringEqualityComparer : IEqualityComparer<string>
    {
        /// <summary><see cref="IEqualityComparer{T}"/></summary>
        public bool Equals(string x, string y) => x == y;
        /// <summary><see cref="IEqualityComparer{T}"/></summary>
        public int GetHashCode(string obj) => obj.GetHashCode();
    }

    /// <summary>
    /// Farm Hash を使う <see cref="IEqualityComparer{T}"/>。
    /// </summary>
    /// <remarks>
    /// UTF8 を UTF8 のまま辞書にしたいので、byte[] 向けのハッシュ値計算が必要。
    /// </remarks>
    public struct FarmHashEqualityComparer : IEqualityComparer<byte[]>
    {
        /// <summary><see cref="IEqualityComparer{T}"/></summary>
        public bool Equals(byte[] x, byte[] y) => ArraySegmentExtensions.SequenceEqual(x, y);
        /// <summary><see cref="IEqualityComparer{T}"/></summary>
        public int GetHashCode(byte[] obj) => FarmHash.GetHashCode(obj);
    }

    /// <summary>
    /// Farm Hash を使う <see cref="IAsymmetricEqualityComparer{T1, T2}"/>。
    /// </summary>
    /// <remarks>
    /// 辞書のキーと格納するのには byte[] を使うだろうけど、
    /// インデクサーの引数には <see cref="ArraySegment{T}"/> が想定される。
    /// 例えば、JSON 文字列の中からキーにあたる部分を slice したもの使って検索したいときとか。
    /// </remarks>
    public struct FarmHashAsymmetricEqualityComparer : IAsymmetricEqualityComparer<byte[], ArraySegment<byte>>
    {
        /// <summary><see cref="IAsymmetricEqualityComparer{T1, T2}"/></summary>
        public bool Equals(byte[] x, ArraySegment<byte> y) => ArraySegmentExtensions.SequenceEqual(x, y);
        /// <summary><see cref="IAsymmetricEqualityComparer{T1, T2}"/></summary>
        public int GetHashCode1(byte[] obj) => FarmHash.GetHashCode(obj);
        /// <summary><see cref="IAsymmetricEqualityComparer{T1, T2}"/></summary>
        public int GetHashCode2(ArraySegment<byte> obj) => FarmHash.GetHashCode(obj);
    }

    /// <summary>
    /// UTF8 な byte 列と string の比較。
    /// </summary>
    /// <remarks>
    /// string からの GetBytes が挟まるのであんまり使いたくはない。
    /// とはいえ、
    /// </remarks>
    public struct FarmHashStringEqualityComparer : IAsymmetricEqualityComparer<byte[], string>
    {
        /// <summary><see cref="IAsymmetricEqualityComparer{T1, T2}"/></summary>
        public bool Equals(byte[] x, string y) => ArraySegmentExtensions.SequenceEqual(x, y);
        /// <summary><see cref="IAsymmetricEqualityComparer{T1, T2}"/></summary>
        public int GetHashCode1(byte[] obj) => FarmHash.GetHashCode(obj);
        /// <summary><see cref="IAsymmetricEqualityComparer{T1, T2}"/></summary>
        public int GetHashCode2(string obj) => FarmHash.GetHashCode(obj);
    }
}
