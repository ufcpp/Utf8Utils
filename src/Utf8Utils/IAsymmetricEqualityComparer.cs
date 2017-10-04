using System;

namespace Utf8Utils
{
    /// <summary>
    /// 同じロジックでハッシュ値計算・等値判定ができるような2つの型(例えば T[] と <see cref="ArraySegment{T}"/> とか、string と UTF8 byte 列とか)の比較に使うインターフェイス。
    /// </summary>
    public interface IAsymmetricEqualityComparer<T1, T2>
    {
        /// <summary>
        /// x == y
        /// </summary>
        bool Equals(T1 x, T2 y);

        /// <summary>
        /// T1 に関するハッシュ値計算。
        /// </summary>
        int GetHashCode1(T1 obj);

        /// <summary>
        /// T2 に関するハッシュ値計算。
        /// </summary>
        int GetHashCode2(T2 obj);
    }
}
