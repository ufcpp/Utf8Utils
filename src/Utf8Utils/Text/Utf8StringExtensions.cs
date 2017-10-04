using System;

namespace Utf8Utils.Text
{
    /// <summary>
    /// <see cref="Utf8String"/>本体に入れたくなかったメソッドをいくつか。
    /// </summary>
    public static class Utf8StringExtensions
    {
        /// <summary>
        /// <see cref="Utf8String"/> 中の文字列を整数化。
        /// </summary>
        public static long GetInt(this Utf8String s)
        {
            var neg = false;
            var x = 0L;

            var begin = s.Buffer.Offset;
            var end = s.Buffer.Offset + s.Buffer.Count;

            if (begin == end) return 0; // 例外の方がいい？

            var a = s.Buffer.Array;

            {
                var c = a[begin];
                if (c == '-') neg = true;
                else x = c - '0';
            }

            for (int i = begin + 1; i < end; i++)
            {
                var c = a[i];
                x *= 10;
                x += c - '0';
            }
            if (neg) x = -x;
            return x;
        }

        /// <summary>
        /// <see cref="Utf8String"/> 中の文字列を浮動小数点数化。
        /// </summary>
        public static double GetFloat(this Utf8String s)
        {
            var neg = false;
            var y = -1;
            var x = 0L;

            var begin = s.Buffer.Offset;
            var end = s.Buffer.Offset + s.Buffer.Count;

            if (begin == end) return 0; // 例外の方がいい？

            var a = s.Buffer.Array;

            {
                var c = a[begin];
                if (c == '-') neg = true;
                else if (c == '.') y = 0;
                else x = c - '0';
            }

            int i = begin + 1;
            for (; i < end; i++)
            {
                var c = a[i];
                if (c == '.')
                {
                    y = 0;
                    continue;
                }
                else if(c == 'E' || c == 'e')
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
                var c = a[i];
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
            if (exp != 0) d = Math.Round(d * Math.Pow(10, exp), 15);

            return d;
        }

        /// <summary>
        /// == "true"
        /// </summary>
        public static unsafe bool EqualsTrue(this Utf8String s)
        {
            if (s.Buffer.Count != 4) return false;
            const uint True = (byte)'t' | (byte)'r' << 8 | (byte)'u' << 16 | (byte)'e' << 24;

            fixed (byte* a = s.Buffer.Array)
            {
                var p = (uint*)(a + s.Buffer.Offset);
                return *p == True;
            }
        }

        /// <summary>
        /// == "false"
        /// </summary>
        public static unsafe bool EqualsFalse(this Utf8String s)
        {
            if (s.Buffer.Count != 5) return false;
            const uint Fals = (byte)'f' | (byte)'a' << 8 | (byte)'l' << 16 | (byte)'s' << 24;

            fixed (byte* a = s.Buffer.Array)
            {
                var p = (uint*)(a + s.Buffer.Offset);
                if (*p != Fals) return false;
            }
            return s.Buffer.Array[s.Buffer.Offset + 4] == (byte)'e';
        }

        /// <summary>
        /// == "null"
        /// </summary>
        /// <remarks>
        /// little endian しか考えてない。
        /// <see cref="EqualsTrue(Utf8String)"/>, <see cref="EqualsFalse(Utf8String)"/> も同様。
        /// </remarks>
        public static unsafe bool EqualsNull(this Utf8String s)
        {
            if (s.Buffer.Count != 4) return false;
            const uint Null = (byte)'n' | (byte)'u' << 8 | (byte)'l' << 16 | (byte)'l' << 24;

            fixed (byte* a = s.Buffer.Array)
            {
                var p = (uint*)(a + s.Buffer.Offset);
                return *p == Null;
            }
        }
    }
}
