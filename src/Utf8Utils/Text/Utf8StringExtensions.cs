using System;

namespace Utf8Utils.Text
{
    /// <summary>
    /// <see cref="Utf8ArraySegment"/>本体に入れたくなかったメソッドをいくつか。
    /// </summary>
    public static class Utf8StringExtensions
    {
        /// <summary>
        /// <see cref="IUtf8String"/> 中の文字列を整数化。
        /// </summary>
        public static long ParseInt<TUtf8>(this TUtf8 s) where TUtf8 : IUtf8String => ParseInt(s.Utf8);

        private static long ParseInt(ArraySegment<byte> seg)
        {
            var array = seg.Array;
            var begin = seg.Offset;
            var end = seg.Offset + seg.Count;
            return ParseInt(array, begin, end);
        }

        private static long ParseInt(byte[] array, int begin, int end)
        {
            if (begin == end) return 0; // 例外の方がいい？

            var neg = false;
            var x = 0L;

            {
                var c = array[begin];
                if (c == '-') neg = true;
                else x = c - '0';
            }

            for (int i = begin + 1; i < end; i++)
            {
                var c = array[i];
                x *= 10;
                x += c - '0';
            }
            if (neg) x = -x;
            return x;
        }

        /// <summary>
        /// <see cref="IUtf8String"/> 中の文字列を浮動小数点数化。
        /// </summary>
        /// <remarks>
        /// このコード、精度はそこまで高くない。
        /// 値が小さい時、結構誤差が出る。
        ///
        /// double → string の方は https://github.com/google/double-conversion/blob/master/double-conversion/fast-dtoa.cc の移植なんだし、
        /// Parse の方も double-conversion から移植してもいいかも。
        /// </remarks>
        public static double ParseFloat<TUtf8>(this TUtf8 s) where TUtf8 : IUtf8String => ParseFloat(s.Utf8);

        private static double ParseFloat(ArraySegment<byte> seg)
        {
            var array = seg.Array;
            var begin = seg.Offset;
            var end = seg.Offset + seg.Count;
            return ParseFloat(array, begin, end);
        }

        private static double ParseFloat(byte[] array, int begin, int end)
        {
            if (begin == end) return 0; // 例外の方がいい？

            var neg = false;
            var y = -1;
            var x = 0L;

            {
                var c = array[begin];
                if (c == '-') neg = true;
                else if (c == '.') y = 0;
                else x = c - '0';
            }

            int i = begin + 1;
            for (; i < end; i++)
            {
                var c = array[i];
                if (c == '.')
                {
                    y = 0;
                    continue;
                }
                else if (c == 'E' || c == 'e')
                {
                    i++;
                    break;
                }
                x *= 10;
                if (y >= 0) y++;
                x += c - '0';
            }

            var expNeg = false;
            var exp = 0;
            for (; i < end; i++)
            {
                var c = array[i];
                if (c == '-')
                {
                    expNeg = true;
                    continue;
                }
                else if (c == '+') continue;
                exp *= 10;
                exp += c - '0';
            }

            if (neg) x = -x;
            if (expNeg) exp = -exp;

            var d = (double)x;

            if (y > 0) exp -= y;
            if (exp != 0) d = d * Math.Pow(10, exp);

            return d;
        }

        public static Number ParseNumber<TUtf8>(this TUtf8 s) where TUtf8 : IUtf8String => ParseNumber(s.Utf8);

        private static Number ParseNumber(ArraySegment<byte> seg)
        {
            var array = seg.Array;
            var begin = seg.Offset;
            var end = seg.Offset + seg.Count;
            return ParseNumber(array, begin, end);
        }

        private static Number ParseNumber(byte[] array, int begin, int end)
        {
            if (begin == end) return default(Number); // 例外の方がいい？

            switch (array[begin])
            {
                case (byte)'t':
                    if (EqualsTrue(new ArraySegment<byte>(array, begin, end))) return (Number)true;
                    break;
                case (byte)'f':
                    if (EqualsFalse(new ArraySegment<byte>(array, begin, end))) return (Number)false;
                    break;
                case (byte)'n':
                    if (EqualsNull(new ArraySegment<byte>(array, begin, end))) return default(Number);
                    break;
            }

            var neg = false;
            var y = -1;
            var x = 0L;

            {
                var c = array[begin];
                if (c == '-') neg = true;
                else if (c == '.') y = 0;
                else x = c - '0';
            }

            int i = begin + 1;
            for (; i < end; i++)
            {
                var c = array[i];
                if (c == '.')
                {
                    y = 0;
                    continue;
                }
                else if (c == 'E' || c == 'e')
                {
                    i++;
                    break;
                }
                x *= 10;
                if (y >= 0) y++;
                x += c - '0';
            }

            var expNeg = false;
            var exp = 0;
            for (; i < end; i++)
            {
                var c = array[i];
                if (c == '-')
                {
                    expNeg = true;
                    continue;
                }
                else if (c == '+') continue;
                exp *= 10;
                exp += c - '0';
            }

            if (neg) x = -x;
            if (expNeg) exp = -exp;

            if (y < 0) return (Number)x;

            var d = (double)x;
            exp -= y;
            if (exp != 0) d = d * Math.Pow(10, exp);

            return (Number)d;
        }

        /// <summary>
        /// == "true"
        /// </summary>
        public static unsafe bool EqualsTrue(this ArraySegment<byte> s)
        {
            if (s.Count != 4) return false;
            const uint True = (byte)'t' | (byte)'r' << 8 | (byte)'u' << 16 | (byte)'e' << 24;

            fixed (byte* a = s.Array)
            {
                var p = (uint*)(a + s.Offset);
                return *p == True;
            }
        }

        /// <summary>
        /// == "false"
        /// </summary>
        public static unsafe bool EqualsFalse(this ArraySegment<byte> s)
        {
            if (s.Count != 5) return false;
            const uint Fals = (byte)'f' | (byte)'a' << 8 | (byte)'l' << 16 | (byte)'s' << 24;

            fixed (byte* a = s.Array)
            {
                var p = (uint*)(a + s.Offset);
                if (*p != Fals) return false;
            }
            return s.Array[s.Offset + 4] == (byte)'e';
        }

        /// <summary>
        /// == "null"
        /// </summary>
        /// <remarks>
        /// little endian しか考えてない。
        /// <see cref="EqualsTrue(Utf8ArraySegment)"/>, <see cref="EqualsFalse(Utf8ArraySegment)"/> も同様。
        /// </remarks>
        public static unsafe bool EqualsNull(this ArraySegment<byte> s)
        {
            if (s.Count != 4) return false;
            const uint Null = (byte)'n' | (byte)'u' << 8 | (byte)'l' << 16 | (byte)'l' << 24;

            fixed (byte* a = s.Array)
            {
                var p = (uint*)(a + s.Offset);
                return *p == Null;
            }
        }
    }
}
