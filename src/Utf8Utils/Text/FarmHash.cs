using System;
using System.Text;

namespace Utf8Utils.Text
{
    internal static class FarmHash
    {
        public static unsafe int GetHashCode(string s)
        {
            var len = s.Length * 3;
            var buf = stackalloc byte[len];
            fixed (char* p = s)
            {
                len = Encoding.UTF8.GetBytes(p, s.Length, buf, len);
            }

            return (int)Hash32(buf, (uint)len);
        }

        public static unsafe int GetHashCode(byte[] s, int offset, int length)
        {
            fixed (byte* p = s)
            {
                return (int)Hash32(p + offset, (uint)length);
            }
        }

        public static int GetHashCode(byte[] s) => GetHashCode(s, 0, s.Length);
        public static int GetHashCode(ArraySegment<byte> s) => GetHashCode(s.Array, s.Offset, s.Count);

        #region Hash32

        public static unsafe uint Hash32(byte[] bytes, int offset, int count)
        {
            if (count <= 4)
            {
                return Hash32Len0to4(bytes, offset, (uint)count);
            }

            fixed (byte* p = &bytes[offset])
            {
                return Hash32(p, (uint)count);
            }
        }

        // port of farmhash.cc, 32bit only

        // Magic numbers for 32-bit hashing.  Copied from Murmur3.
        const uint c1 = 0xcc9e2d51;
        const uint c2 = 0x1b873593;

        static unsafe uint Fetch32(byte* p)
        {
            return *(uint*)p;
        }

        static uint Rotate32(uint val, int shift)
        {
            return shift == 0 ? val : ((val >> shift) | (val << (32 - shift)));
        }

        // A 32-bit to 32-bit integer hash copied from Murmur3.
        static uint fmix(uint h)
        {
            unchecked
            {
                h ^= h >> 16;
                h *= 0x85ebca6b;
                h ^= h >> 13;
                h *= 0xc2b2ae35;
                h ^= h >> 16;
                return h;
            }
        }

        static uint Mur(uint a, uint h)
        {
            unchecked
            {
                // Helper from Murmur3 for combining two 32-bit values.
                a *= c1;
                a = Rotate32(a, 17);
                a *= c2;
                h ^= a;
                h = Rotate32(h, 19);
                return h * 5 + 0xe6546b64;
            }
        }

        // 0-4
        static unsafe uint Hash32Len0to4(byte[] s, int offset, uint len)
        {
            unchecked
            {
                uint b = 0;
                uint c = 9;
                var max = offset + len;
                for (int i = offset; i < max; i++)
                {
                    b = b * c1 + s[i];
                    c ^= b;
                }
                return fmix(Mur(b, Mur(len, c)));
            }
        }

        static unsafe uint Hash32Len0to4(byte* s, uint len)
        {
            unchecked
            {
                uint b = 0;
                uint c = 9;
                var max = s + len;
                for (byte* p = s; p < max; p++)
                {
                    b = b * c1 + *p;
                    c ^= b;
                }
                return fmix(Mur(b, Mur(len, c)));
            }
        }

        // 5-12
        static unsafe uint Hash32Len5to12(byte* s, uint len)
        {
            unchecked
            {
                uint a = len, b = len * 5, c = 9, d = b;
                a += Fetch32(s);
                b += Fetch32(s + len - 4);
                c += Fetch32(s + ((len >> 1) & 4));
                return fmix(Mur(c, Mur(b, Mur(a, d))));
            }
        }

        // 13-24
        static unsafe uint Hash32Len13to24(byte* s, uint len)
        {
            unchecked
            {
                uint a = Fetch32(s - 4 + (len >> 1));
                uint b = Fetch32(s + 4);
                uint c = Fetch32(s + len - 8);
                uint d = Fetch32(s + (len >> 1));
                uint e = Fetch32(s);
                uint f = Fetch32(s + len - 4);
                uint h = d * c1 + len;
                a = Rotate32(a, 12) + f;
                h = Mur(c, h) + a;
                a = Rotate32(a, 3) + c;
                h = Mur(e, h) + a;
                a = Rotate32(a + f, 12) + d;
                h = Mur(b, h) + a;
                return fmix(h);
            }
        }

        static unsafe uint Hash32(byte* s, uint len)
        {
            if (len <= 4)
            {
                return Hash32Len0to4(s, len);
            }

            if (len <= 24)
            {
                return len <= 12 ? Hash32Len5to12(s, len) : Hash32Len13to24(s, len);
            }

            unchecked
            {
                // len > 24
                uint h = len, g = c1 * len, f = g;
                uint a0 = Rotate32(Fetch32(s + len - 4) * c1, 17) * c2;
                uint a1 = Rotate32(Fetch32(s + len - 8) * c1, 17) * c2;
                uint a2 = Rotate32(Fetch32(s + len - 16) * c1, 17) * c2;
                uint a3 = Rotate32(Fetch32(s + len - 12) * c1, 17) * c2;
                uint a4 = Rotate32(Fetch32(s + len - 20) * c1, 17) * c2;
                h ^= a0;
                h = Rotate32(h, 19);
                h = h * 5 + 0xe6546b64;
                h ^= a2;
                h = Rotate32(h, 19);
                h = h * 5 + 0xe6546b64;
                g ^= a1;
                g = Rotate32(g, 19);
                g = g * 5 + 0xe6546b64;
                g ^= a3;
                g = Rotate32(g, 19);
                g = g * 5 + 0xe6546b64;
                f += a4;
                f = Rotate32(f, 19) + 113;
                uint iters = (len - 1) / 20;
                do
                {
                    uint a = Fetch32(s);
                    uint b = Fetch32(s + 4);
                    uint c = Fetch32(s + 8);
                    uint d = Fetch32(s + 12);
                    uint e = Fetch32(s + 16);
                    h += a;
                    g += b;
                    f += c;
                    h = Mur(d, h) + e;
                    g = Mur(c, g) + a;
                    f = Mur(b + e * c1, f) + d;
                    f += g;
                    g += f;
                    s += 20;
                } while (--iters != 0);
                g = Rotate32(g, 11) * c1;
                g = Rotate32(g, 17) * c1;
                f = Rotate32(f, 11) * c1;
                f = Rotate32(f, 17) * c1;
                h = Rotate32(h + g, 19);
                h = h * 5 + 0xe6546b64;
                h = Rotate32(h, 17) * c1;
                h = Rotate32(h + f, 19);
                h = h * 5 + 0xe6546b64;
                h = Rotate32(h, 17) * c1;
                return h;
            }
        }

#endregion
    }
}
