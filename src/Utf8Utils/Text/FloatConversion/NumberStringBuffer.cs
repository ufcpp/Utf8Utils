using Utf8Utils.Collections;
using System.Text;

namespace Utf8Utils.Text.FloatConversion
{
    /// <summary>
    /// <see cref="FloatConversion"/>の結果を受け取るためのバッファー構造体。
    /// </summary>
    public unsafe struct NumberStringBuffer
    {
        /// <summary>
        /// <see cref="Digits"/>の長さ。
        /// double の ToString 結果が最大で17ケタなのでこの数字。
        /// </summary>
        public const int MaxDigits = 17;

        /// <summary>
        /// <see cref="GetUtf8Bytes"/>で返ってくる最大の長さ。
        /// 一番長くなる状況が
        /// '-' "Digits"(max = 17) '.' 'E' '-' "Exponent"(max = 3)
        /// みたいな感じなので、<see cref="MaxDigits"/> + 7 が最大のはず。
        /// </summary>
        public const int MaxChars = MaxDigits + 7;

        /// <summary>
        /// 指数部。
        /// <see cref="Digits"/> の頭に "0." を付けたとして、0.######E*** の *** の部分に来る数値。
        /// </summary>
        public short DecimalExponent;

        /// <summary>
        /// <see cref="Digits"/>のうち、有効な桁数。
        /// </summary>
        public byte Length;

        /// <summary>
        /// 負の数。
        /// </summary>
        public bool IsNegative;

        /// <summary>
        /// float か double。
        /// float の時に true。
        /// </summary>
        public bool IsSinglePrecision;

        /// <summary>
        /// <see cref="double.IsInfinity(double)"/>
        /// </summary>
        public bool IsInfinity;

        /// <summary>
        /// <see cref="double.IsNaN(double)"/>
        /// </summary>
        public bool IsNaN;

        /// <summary>
        /// <see cref="double.PositiveInfinity"/>
        /// </summary>
        public static NumberStringBuffer PositiveInfinity => new NumberStringBuffer { IsInfinity = true };

        /// <summary>
        /// <see cref="double.NegativeInfinity"/>
        /// </summary>
        public static NumberStringBuffer NegativeInfinity => new NumberStringBuffer { IsNegative = true, IsInfinity = true };

        /// <summary>
        /// <see cref="double.NaN"/>
        /// </summary>
        public static NumberStringBuffer NaN => new NumberStringBuffer { IsNaN = true };

        /// <summary>
        /// 0
        /// </summary>
        public static NumberStringBuffer Zero
        {
            get
            {
                var buf = default(NumberStringBuffer);
                buf.Digits[0] = (byte)'0';
                buf.Length = 1;
                return buf;
            }
        }

        /// <summary>
        /// 数字。
        /// </summary>
        public fixed byte Digits[MaxDigits];

        /// <summary>
        /// UTF8 なバイト列を配列で取得。
        /// </summary>
        public byte[] GetUtf8Bytes()
        {
            var buffer = stackalloc byte[MaxChars];
            var length = Format(buffer);

            var a = new byte[length];
            ArraySegmentExtensions.Copy(buffer, length, a, 0, length);
            return a;
        }

        /// <summary>
        /// string 化。
        /// </summary>
        public override string ToString()
        {
            var buffer = stackalloc byte[MaxChars];
            var length = Format(buffer);

            var cLen = Encoding.UTF8.GetCharCount(buffer, length);
            var s = new string('\0', cLen);
            fixed (char* p = s) Encoding.UTF8.GetChars(buffer, length, p, cLen);
            return s;
        }

        /// <summary>
        /// UTF8 なバイト列を<paramref name="buffer"/>に書き込む。
        /// </summary>
        public int Format(byte* buffer)
        {
            var exp = Length + DecimalExponent;

            if (exp > 0 && exp <= (IsSinglePrecision ? 7 : 15))
            {
                return FormatPositiveExp(buffer);
            }
            else if (exp <= 0 && exp > -4)
            {
                return FormatNegativeExp(buffer);
            }

            return FormatScientific(buffer);
        }

        private int FormatPositiveExp(byte* buffer)
        {
            var pb = buffer;

            if (IsNegative)
            {
                *(pb++) = (byte)'-';
            }

            fixed (byte* fd = Digits)
            {
                var pd = fd;

                int posLength = Length;
                if (DecimalExponent < 0) posLength += DecimalExponent;

                for (int i = 0; i < posLength; i++) *(pb++) = *(pd++);
                for (int i = 0; i < DecimalExponent; i++) *(pb++) = (byte)'0';

                if (DecimalExponent < 0)
                {
                    *(pb++) = (byte)'.';
                    for (int i = DecimalExponent; i < 0; i++) *(pb++) = *(pd++);
                }
            }

            int length = (int)(pb - buffer);
            return length;
        }

        private int FormatNegativeExp(byte* buffer)
        {
            var pb = buffer;

            if (IsNegative)
            {
                *(pb++) = (byte)'-';
            }

            *(pb++) = (byte)'0';
            *(pb++) = (byte)'.';

            var exp = Length + DecimalExponent;

            for (int i = 0; i < -exp; i++)
            {
                *(pb++) = (byte)'0';
            }

            fixed (byte* fd = Digits)
            {
                var pd = fd;

                for (int i = 0; i < Length; i++)
                {
                    *(pb++) = *(pd++);
                }
            }

            int length = (int)(pb - buffer);
            return length;
        }

        private int FormatScientific(byte* buffer)
        {
            var pb = buffer;

            if (IsNegative)
            {
                *(pb++) = (byte)'-';
            }

            if(IsInfinity)
            {
                *(pb++) = 226;
                *(pb++) = 136;
                *(pb++) = 158;
                goto END;
            }
            else if (IsNaN)
            {
                *(pb++) = 78;
                *(pb++) = 97;
                *(pb++) = 78;
                goto END;
            }

            fixed (byte* fd = Digits)
            {
                var pd = fd;
                var last = fd + Length;

                *(pb++) = *(pd++);

                if (Length > 1)
                {
                    *(pb++) = (byte)'.';

                    while (pd != last) *(pb++) = *(pd++);
                }

                var exp = Length + DecimalExponent - 1;
                if (exp != 0)
                {
                    *(pb++) = (byte)'E';

                    if (exp < 0)
                    {
                        *(pb++) = (byte)'-';
                        exp = -exp;
                    }
                    else
                    {
                        *(pb++) = (byte)'+';
                    }

                    if (exp >= 100)
                    {
                        *(pb++) = (byte)(exp / 100 + '0');
                        exp %= 100;
                    }

                    *(pb++) = (byte)(exp / 10 + '0');
                    exp %= 10;
                    *(pb++) = (byte)(exp + '0');
                }
            }

            END:
            int length = (int)(pb - buffer);
            return length;
        }
    }
}
