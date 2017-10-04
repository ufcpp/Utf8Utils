using System;
using System.Collections.Generic;

namespace Utf8Utils.Collections
{
    /// <summary>
    /// <see cref="ArraySegment{T}"/>を簡易 Span として使うことにしたので、それに関連していくつか拡張メソッドを用意。
    /// </summary>
    public static class ArraySegmentExtensions
    {
        /// <summary>
        /// 指定インデックスの要素を取得。
        /// </summary>
        public static byte At(this ArraySegment<byte> array, int index) => array.Array[array.Offset + index];

        /// <summary>
        /// <see cref="ArraySegment{T}"/>の分解。
        /// </summary>
        public static void Deconstruct<T>(this ArraySegment<T> segment, out T[] array, out int offset, out int count)
        {
            array = segment.Array;
            offset = segment.Offset;
            count = segment.Count;
        }

        /// <summary>
        /// <see cref="ArraySegment{T}"/>の一部分を切り出す。
        /// </summary>
        public static ArraySegment<T> Slice<T>(this ArraySegment<T> segment, int offset, int count) => new ArraySegment<T>(segment.Array, segment.Offset + offset, count);

        /// <summary>
        /// <see cref="ArraySegment{T}"/>の一部分を切り出す。
        /// </summary>
        public static ArraySegment<T> Slice<T>(this ArraySegment<T> segment, int offset) => new ArraySegment<T>(segment.Array, segment.Offset + offset, segment.Count - offset);

        /// <summary>
        /// 配列化。
        /// </summary>
        public static byte[] ToArray(this ArraySegment<byte> segment)
        {
            if (segment.Array == null) return null;
            var a = new byte[segment.Count];
            if (a.Length != 0)
                Copy(segment.Array, segment.Offset, a, 0, a.Length);
            return a;
        }

        /// <summary>
        /// <see cref="ArraySegment{T}"/>中の要素が全て一致するかどうかを判定。
        /// </summary>
        public static bool SequenceEqual<T>(this ArraySegment<T> first, ArraySegment<T> second)
        {
            if (first.Count != second.Count) return false;

            var i = first.Offset;
            var ei = i + first.Count;
            var j = second.Offset;
            var ej = j + second.Count;
            var a = first.Array;
            var b = second.Array;
            for (; i < ei && j < ej; i++, j++)
            {
                if (!EqualityComparer<T>.Default.Equals(a[i], b[j])) return false;
            }

            return true;
        }

        /// <summary>
        /// <see cref="ArraySegment{T}"/>中の要素が全て一致するかどうかを判定。
        /// </summary>
        public static bool SequenceEqual(this ArraySegment<byte> first, ArraySegment<byte> second)
        {
            if (first.Count != second.Count) return false;
            return SequenceEqual(first.Array, first.Offset, second.Array, second.Offset, first.Count);
        }

        /// <summary>
        /// 配列中の要素が全て一致するかどうかを判定。
        /// </summary>
        public static bool SequenceEqual(this byte[] first, ArraySegment<byte> second)
        {
            if (first.Length != second.Count) return false;
            return SequenceEqual(first, 0, second.Array, second.Offset, first.Length);
        }

        /// <summary>
        /// 配列中の要素が全て一致するかどうかを判定。
        /// </summary>
        public static bool SequenceEqual(this ArraySegment<byte> first, byte[] second)
        {
            if (first.Count != second.Length) return false;
            return SequenceEqual(first.Array, first.Offset, second, 0, second.Length);
        }

        /// <summary>
        /// 配列中の要素が全て一致するかどうかを判定。
        /// </summary>
        public static bool SequenceEqual(this byte[] first, byte[] second)
        {
            if (first.Length != second.Length) return false;
            return SequenceEqual(first, 0, second, 0, first.Length);
        }

        /// <summary>
        /// UTF8 な byte[] と string を比較。
        /// (string の方を GetBytes したうえで <see cref="SequenceEqual(byte*, byte*, int)"/>。)
        /// </summary>
        public static bool SequenceEqual(this byte[] utf8, string utf16) => SequenceEqual(utf8, 0, utf8.Length, utf16);

        /// <summary>
        /// UTF8 な byte[] と string を比較。
        /// (string の方を GetBytes したうえで <see cref="SequenceEqual(byte*, byte*, int)"/>。)
        /// </summary>
        public static unsafe bool SequenceEqual(this byte[] utf8, int offset, int count, string utf16)
        {
            var len = utf16.Length * 3;
            var buf = stackalloc byte[len];
            fixed (char* p = utf16)
            {
                len = System.Text.Encoding.UTF8.GetBytes(p, utf16.Length, buf, len);
            }

            if (count != len) return false;

            fixed (byte* p = utf8)
            {
                return SequenceEqual(p + offset, buf, len);
            }
        }

        /// <summary>
        /// <see cref="ArraySegment{T}"/>中の要素が全て一致するかどうかを判定。
        /// </summary>
        public static bool SequenceEqual(byte[] first, int firstOffset, byte[] second, int secondOffset, int length)
        {
            unsafe
            {
                fixed (byte* pa = first)
                fixed (byte* pb = second)
                    return SequenceEqual(pa + firstOffset, pb + secondOffset, length);
            }
        }

        private unsafe static bool SequenceEqual(byte* first, byte* second, int length)
        {
            if (length < 0) throw new ArgumentException();

            if (length < 4) goto LT4;
            if (length < 8) goto LT8;

            while (length >= 64)
            {
                if (*(ulong*)first != *(ulong*)second
                    || *(ulong*)(first + 8) != *(ulong*)(second + 8)
                    || *(ulong*)(first + 16) != *(ulong*)(second + 16)
                    || *(ulong*)(first + 24) != *(ulong*)(second + 24)
                    || *(ulong*)(first + 32) != *(ulong*)(second + 32)
                    || *(ulong*)(first + 40) != *(ulong*)(second + 40)
                    || *(ulong*)(first + 48) != *(ulong*)(second + 48)
                    || *(ulong*)(first + 56) != *(ulong*)(second + 56)
                    ) return false;
                length -= 64;
                first += 64;
                second += 64;
            }
            while (length >= 8)
            {
                if (*(ulong*)first != *(ulong*)second) return false;
                length -= 8;
                first += 8;
                second += 8;
            }
            LT8:
            while (length >= 4)
            {
                if (*(uint*)first != *(uint*)second) return false;
                length -= 4;
                first += 4;
                second += 4;
            }
            LT4:
            while (length > 0)
            {
                if (*first != *second) return false;
                --length;
                first += 1;
                second += 1;
            }
            return true;
        }

        /// <summary>
        /// <paramref name="source"/>から<paramref name="destination"/>にデータをコピー。
        /// </summary>
        public static void Copy(char[] source, char[] destination) => Copy(source, destination, source.Length);

        /// <summary>
        /// <paramref name="source"/>から<paramref name="destination"/>にデータをコピー。
        /// </summary>
        public static void Copy(char[] source, char[] destination, int length)
        {
            if (source.Length < length) throw new ArgumentException();
            if (destination.Length < length) throw new ArgumentException();

            unsafe
            {
                fixed (char* pa = &source[0])
                fixed (char* pb = &destination[0])
                    CopyTo((byte*)pa, (byte*)pb, length * 2);
            }
        }

        /// <summary>
        /// <paramref name="source"/>から<paramref name="destination"/>にデータをコピー。
        /// </summary>
        public static void Copy(this byte[] source, byte[] destination) => Copy(source, 0, destination, 0, source.Length);

        /// <summary>
        /// <paramref name="source"/>から<paramref name="destination"/>にデータをコピー。
        /// </summary>
        /// <remarks>
        /// <see cref="Array.Copy(Array, Array, int)"/>とかが遅すぎてやってられないので自作。
        /// .NET 4.5 であれば、Buffer.MemoryCopy が速いんだけど。
        /// </remarks>
        public static void Copy(this byte[] source, int sourceOffset, byte[] destination, int destinationOffset, int length)
        {
            if (source.Length < length) throw new ArgumentException();
            if (destination.Length < length) throw new ArgumentException();

            unsafe
            {
                fixed (byte* pa = &source[0])
                fixed (byte* pb = &destination[0])
                    CopyTo(pa + sourceOffset, pb + destinationOffset, length);
            }
        }

        /// <summary>
        /// <paramref name="source"/>から<paramref name="destination"/>にデータをコピー。
        /// </summary>
        public static unsafe void Copy(byte* source, int sourceLength, byte[] destination, int destinationOffset, int length)
        {
            if (sourceLength < length) throw new ArgumentException();
            if (destination.Length < length) throw new ArgumentException();

            unsafe
            {
                fixed (byte* pb = &destination[0])
                    CopyTo(source, pb + destinationOffset, length);
            }
        }

        private static unsafe void CopyTo(byte* a, byte* b, int length)
        {
            if (length < 4) goto LT4;
            if (length < 8) goto LT8;

            while (length >= 64)
            {
                *(ulong*)b = *(ulong*)a;
                *(ulong*)(b + 8) = *(ulong*)(a + 8);
                *(ulong*)(b + 16) = *(ulong*)(a + 16);
                *(ulong*)(b + 24) = *(ulong*)(a + 24);
                *(ulong*)(b + 32) = *(ulong*)(a + 32);
                *(ulong*)(b + 40) = *(ulong*)(a + 40);
                *(ulong*)(b + 48) = *(ulong*)(a + 48);
                *(ulong*)(b + 56) = *(ulong*)(a + 56);
                length -= 64;
                a += 64;
                b += 64;
            }
            while (length >= 8)
            {
                *(ulong*)b = *(ulong*)a;
                length -= 8;
                a += 8;
                b += 8;
            }
            LT8:
            if (length >= 4)
            {
                *(uint*)b = *(uint*)a;
                length -= 4;
                a += 4;
                b += 4;
            }
            LT4:
            while (length > 0)
            {
                *b = *a;
                --length;
                a += 1;
                b += 1;
            }
        }
    }
}
