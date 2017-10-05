namespace Utf8Utils.Text.FloatConversion
{
    using Debug = System.Diagnostics.Debug;

    /// <summary>
    /// https://github.com/google/double-conversion/blob/master/double-conversion/diy-fp.cc
    /// </summary>
    internal struct DiyFp
    {
        public ulong F;
        public int E;

        private const ulong Uint64MSB = 0x80000000_00000000;
        public const int SignificandSize = 64;

        public DiyFp(ulong significand, int exponent)
        {
            F = significand;
            E = exponent;
        }

        void Subtract(DiyFp other)
        {
            Debug.Assert(E == other.E);
            Debug.Assert(F >= other.F);
            F -= other.F;
        }

        public static DiyFp operator -(DiyFp a, DiyFp b)
        {
            DiyFp result = a;
            result.Subtract(b);
            return result;
        }

        void Multiply(DiyFp other)
        {
            const ulong kM32 = 0xFFFFFFFFU;
            ulong a = F >> 32;
            ulong b = F & kM32;
            ulong c = other.F >> 32;
            ulong d = other.F & kM32;
            ulong ac = a * c;
            ulong bc = b * c;
            ulong ad = a * d;
            ulong bd = b * d;
            ulong tmp = (bd >> 32) + (ad & kM32) + (bc & kM32);
            tmp += 1U << 31;
            ulong result_f = ac + (ad >> 32) + (bc >> 32) + (tmp >> 32);
            E += other.E + 64;
            F = result_f;
        }

        public static DiyFp operator *(DiyFp a, DiyFp b)
        {
            DiyFp result = a;
            result.Multiply(b);
            return result;
        }

        public void Normalize()
        {
            Debug.Assert(F != 0);
            ulong significand = F;
            int exponent = E;

            const ulong k10MSBits = 0xFFC00000_00000000;
            while ((significand & k10MSBits) == 0)
            {
                significand <<= 10;
                exponent -= 10;
            }
            while ((significand & Uint64MSB) == 0)
            {
                significand <<= 1;
                exponent--;
            }
            F = significand;
            E = exponent;
        }

        public static DiyFp Normalize(DiyFp a)
        {
            DiyFp result = a;
            result.Normalize();
            return result;
        }
    }
}
