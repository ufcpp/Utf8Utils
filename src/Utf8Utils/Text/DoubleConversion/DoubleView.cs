﻿namespace Utf8Utils.Text.DoubleConversion
{
    using Debug = System.Diagnostics.Debug;

    /// <summary>
    /// https://github.com/google/double-conversion/blob/master/double-conversion/ieee.h
    /// </summary>
    internal struct DoubleView
    {
        private static unsafe ulong BitCast(double x) => *(ulong*)&x;
        private static unsafe double BitCast(ulong x) => *(double*)&x;

        private const ulong kSignMask = 0x80000000_00000000UL;
        private const ulong kExponentMask = 0x7FF00000_00000000UL;
        private const ulong kSignificandMask = 0x000FFFFF_FFFFFFFFUL;
        private const ulong kHiddenBit = 0x00100000_00000000UL;
        private const int kPhysicalSignificandSize = 52;  // Excludes the hidden bit.
        private const int kSignificandSize = 53;
        private const int kExponentBias = 0x3FF + kPhysicalSignificandSize;
        private const int kDenormalExponent = -kExponentBias + 1;
        private const int kMaxExponent = 0x7FF - kExponentBias;
        private const ulong kInfinity = 0x7FF00000_00000000UL;
        private const ulong kNaN = 0x7FF80000_00000000UL;

        private ulong _v;

        public DoubleView(double v) => _v = BitCast(v);

        public double Value => BitCast(_v);

        public ulong Significand
        {
            get
            {
                ulong significand = _v & kSignificandMask;
                if (!IsDenormal)
                {
                    return significand + kHiddenBit;
                }
                else
                {
                    return significand;
                }
            }
        }

        public int Exponent
        {
            get
            {
                if (IsDenormal) return kDenormalExponent;

                int biased_e =
                    (int)((_v & kExponentMask) >> kPhysicalSignificandSize);
                return biased_e - kExponentBias;
            }
        }

        public bool IsDenormal => (_v & kExponentMask) == 0;

        public bool IsSpecial => (_v & kExponentMask) == kExponentMask;

        public bool IsNan => ((_v & kExponentMask) == kExponentMask) && ((_v & kSignificandMask) != 0);

        public bool IsInfinite => ((_v & kExponentMask) == kExponentMask) && ((_v & kSignificandMask) == 0);

        public int Sign => (_v & kSignMask) == 0 ? 1 : -1;

        public DiyFp AsNormalizedDiyFp()
        {
            Debug.Assert(Value > 0.0);
            ulong f = Significand;
            int e = Exponent;

            // The current double could be a denormal.
            while ((f & kHiddenBit) == 0)
            {
                f <<= 1;
                e--;
            }
            // Do the final shifts in one go.
            f <<= DiyFp.SignificandSize - kSignificandSize;
            e -= DiyFp.SignificandSize - kSignificandSize;
            return new DiyFp(f, e);
        }

        public DiyFp AsDiyFp()
        {
            Debug.Assert(Sign > 0);
            Debug.Assert(!IsSpecial);
            return new DiyFp(Significand, Exponent);
        }

        public void NormalizedBoundaries(out DiyFp out_m_minus, out DiyFp out_m_plus)
        {
            Debug.Assert(Value > 0.0);
            DiyFp v = AsDiyFp();
            DiyFp m_plus = DiyFp.Normalize(new DiyFp((v.F << 1) + 1, v.E - 1));
            DiyFp m_minus;
            if (LowerBoundaryIsCloser())
            {
                m_minus = new DiyFp((v.F << 2) - 1, v.E - 2);
            }
            else
            {
                m_minus = new DiyFp((v.F << 1) - 1, v.E - 1);
            }

            m_minus.F = m_minus.F << (m_minus.E - m_plus.E);
            m_minus.E = m_plus.E;

            out_m_plus = m_plus;
            out_m_minus = m_minus;
        }

        bool LowerBoundaryIsCloser()
        {
            bool physical_significand_is_zero = ((_v & kSignificandMask) == 0);
            return physical_significand_is_zero && (Exponent != kDenormalExponent);
        }
    }
}
