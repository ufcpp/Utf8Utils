using System;

namespace Utf8Utils.Text.DoubleConversion
{
    using Debug = System.Diagnostics.Debug;

    internal class PowersOfTenCache
    {
        struct CachedPower
        {
            public ulong significand;
            public int binary_exponent;
            public int decimal_exponent;

            public CachedPower(ulong significand, int binary_exponent, int decimal_exponent)
            {
                this.significand = significand;
                this.binary_exponent = binary_exponent;
                this.decimal_exponent = decimal_exponent;
            }
        }

        private static readonly CachedPower[] kCachedPowers = new[]
        {
            new CachedPower(0xfa8fd5a0_081c0288UL, -1220, -348),
            new CachedPower(0xbaaee17f_a23ebf76UL, -1193, -340),
            new CachedPower(0x8b16fb20_3055ac76UL, -1166, -332),
            new CachedPower(0xcf42894a_5dce35eaUL, -1140, -324),
            new CachedPower(0x9a6bb0aa_55653b2dUL, -1113, -316),
            new CachedPower(0xe61acf03_3d1a45dfUL, -1087, -308),
            new CachedPower(0xab70fe17_c79ac6caUL, -1060, -300),
            new CachedPower(0xff77b1fc_bebcdc4fUL, -1034, -292),
            new CachedPower(0xbe5691ef_416bd60cUL, -1007, -284),
            new CachedPower(0x8dd01fad_907ffc3cUL, -980, -276),
            new CachedPower(0xd3515c28_31559a83UL, -954, -268),
            new CachedPower(0x9d71ac8f_ada6c9b5UL, -927, -260),
            new CachedPower(0xea9c2277_23ee8bcbUL, -901, -252),
            new CachedPower(0xaecc4991_4078536dUL, -874, -244),
            new CachedPower(0x823c1279_5db6ce57UL, -847, -236),
            new CachedPower(0xc2109436_4dfb5637UL, -821, -228),
            new CachedPower(0x9096ea6f_3848984fUL, -794, -220),
            new CachedPower(0xd77485cb_25823ac7UL, -768, -212),
            new CachedPower(0xa086cfcd_97bf97f4UL, -741, -204),
            new CachedPower(0xef340a98_172aace5UL, -715, -196),
            new CachedPower(0xb23867fb_2a35b28eUL, -688, -188),
            new CachedPower(0x84c8d4df_d2c63f3bUL, -661, -180),
            new CachedPower(0xc5dd4427_1ad3cdbaUL, -635, -172),
            new CachedPower(0x936b9fce_bb25c996UL, -608, -164),
            new CachedPower(0xdbac6c24_7d62a584UL, -582, -156),
            new CachedPower(0xa3ab6658_0d5fdaf6UL, -555, -148),
            new CachedPower(0xf3e2f893_dec3f126UL, -529, -140),
            new CachedPower(0xb5b5ada8_aaff80b8UL, -502, -132),
            new CachedPower(0x87625f05_6c7c4a8bUL, -475, -124),
            new CachedPower(0xc9bcff60_34c13053UL, -449, -116),
            new CachedPower(0x964e858c_91ba2655UL, -422, -108),
            new CachedPower(0xdff97724_70297ebdUL, -396, -100),
            new CachedPower(0xa6dfbd9f_b8e5b88fUL, -369, -92),
            new CachedPower(0xf8a95fcf_88747d94UL, -343, -84),
            new CachedPower(0xb9447093_8fa89bcfUL, -316, -76),
            new CachedPower(0x8a08f0f8_bf0f156bUL, -289, -68),
            new CachedPower(0xcdb02555_653131b6UL, -263, -60),
            new CachedPower(0x993fe2c6_d07b7facUL, -236, -52),
            new CachedPower(0xe45c10c4_2a2b3b06UL, -210, -44),
            new CachedPower(0xaa242499_697392d3UL, -183, -36),
            new CachedPower(0xfd87b5f2_8300ca0eUL, -157, -28),
            new CachedPower(0xbce50864_92111aebUL, -130, -20),
            new CachedPower(0x8cbccc09_6f5088ccUL, -103, -12),
            new CachedPower(0xd1b71758_e219652cUL, -77, -4),
            new CachedPower(0x9c400000_00000000UL, -50, 4),
            new CachedPower(0xe8d4a510_00000000UL, -24, 12),
            new CachedPower(0xad78ebc5_ac620000UL, 3, 20),
            new CachedPower(0x813f3978_f8940984UL, 30, 28),
            new CachedPower(0xc097ce7b_c90715b3UL, 56, 36),
            new CachedPower(0x8f7e32ce_7bea5c70UL, 83, 44),
            new CachedPower(0xd5d238a4_abe98068UL, 109, 52),
            new CachedPower(0x9f4f2726_179a2245UL, 136, 60),
            new CachedPower(0xed63a231_d4c4fb27UL, 162, 68),
            new CachedPower(0xb0de6538_8cc8ada8UL, 189, 76),
            new CachedPower(0x83c7088e_1aab65dbUL, 216, 84),
            new CachedPower(0xc45d1df9_42711d9aUL, 242, 92),
            new CachedPower(0x924d692c_a61be758UL, 269, 100),
            new CachedPower(0xda01ee64_1a708deaUL, 295, 108),
            new CachedPower(0xa26da399_9aef774aUL, 322, 116),
            new CachedPower(0xf209787b_b47d6b85UL, 348, 124),
            new CachedPower(0xb454e4a1_79dd1877UL, 375, 132),
            new CachedPower(0x865b8692_5b9bc5c2UL, 402, 140),
            new CachedPower(0xc83553c5_c8965d3dUL, 428, 148),
            new CachedPower(0x952ab45c_fa97a0b3UL, 455, 156),
            new CachedPower(0xde469fbd_99a05fe3UL, 481, 164),
            new CachedPower(0xa59bc234_db398c25UL, 508, 172),
            new CachedPower(0xf6c69a72_a3989f5cUL, 534, 180),
            new CachedPower(0xb7dcbf53_54e9beceUL, 561, 188),
            new CachedPower(0x88fcf317_f22241e2UL, 588, 196),
            new CachedPower(0xcc20ce9b_d35c78a5UL, 614, 204),
            new CachedPower(0x98165af3_7b2153dfUL, 641, 212),
            new CachedPower(0xe2a0b5dc_971f303aUL, 667, 220),
            new CachedPower(0xa8d9d153_5ce3b396UL, 694, 228),
            new CachedPower(0xfb9b7cd9_a4a7443cUL, 720, 236),
            new CachedPower(0xbb764c4c_a7a44410UL, 747, 244),
            new CachedPower(0x8bab8eef_b6409c1aUL, 774, 252),
            new CachedPower(0xd01fef10_a657842cUL, 800, 260),
            new CachedPower(0x9b10a4e5_e9913129UL, 827, 268),
            new CachedPower(0xe7109bfb_a19c0c9dUL, 853, 276),
            new CachedPower(0xac2820d9_623bf429UL, 880, 284),
            new CachedPower(0x80444b5e_7aa7cf85UL, 907, 292),
            new CachedPower(0xbf21e440_03acdd2dUL, 933, 300),
            new CachedPower(0x8e679c2f_5e44ff8fUL, 960, 308),
            new CachedPower(0xd433179d_9c8cb841UL, 986, 316),
            new CachedPower(0x9e19db92_b4e31ba9UL, 1013, 324),
            new CachedPower(0xeb96bf6e_badf77d9UL, 1039, 332),
            new CachedPower(0xaf87023b_9bf0ee6bUL, 1066, 340),
        };

        private const int kCachedPowersOffset = 348;  // -1 * the first decimal_exponent.
        private const double kD_1_LOG2_10 = 0.30102999566398114;  //  1 / lg(10)
                                                                  // Difference between the decimal exponents in the table above.
        const int kDecimalExponentDistance = 8;
        const int kMinDecimalExponent = -348;
        const int kMaxDecimalExponent = 340;

        public static void GetCachedPowerForBinaryExponentRange(
            int min_exponent,
            int max_exponent,
            out DiyFp power,
            out int decimal_exponent)
        {
            int kQ = DiyFp.SignificandSize;
            double k = Math.Ceiling((min_exponent + kQ - 1) * kD_1_LOG2_10);
            int foo = kCachedPowersOffset;
            int index =
                (foo + (int)(k) - 1) / kDecimalExponentDistance + 1;
            Debug.Assert(0 <= index && index < kCachedPowers.Length);
            var cached_power = kCachedPowers[index];
            Debug.Assert(min_exponent <= cached_power.binary_exponent);
            //(void)max_exponent;  // Mark variable as used.
            Debug.Assert(cached_power.binary_exponent <= max_exponent);
            decimal_exponent = cached_power.decimal_exponent;
            power = new DiyFp(cached_power.significand, cached_power.binary_exponent);
        }
    }
}
