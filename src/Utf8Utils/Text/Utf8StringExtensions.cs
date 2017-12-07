using System;

namespace Utf8Utils.Text
{
    /// <summary>
    /// <see cref="Utf8ArraySegment"/>本体に入れたくなかったメソッドをいくつか。
    /// 数値のフォーマットとか文字列のエスケープとか、C# や JSON (を含むたいていのC由来言語)の構文であって他の形式がなくもないものなので、本体には含めづらい。
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

        /// <summary>
        /// 配列を作ってそこに<see cref="Unescape(ArraySegment{byte}, byte*)"/>。
        /// </summary>
        /// <remarks>
        /// 「エスケープした文字列よりも、復元結果が長くなるはずはない」という前提で、エスケープした文字列を同じ長さで配列を確保してしまう。
        /// 2パス走査でちゃんとした長さを測ってから配列確保するよりもその方が軽そうなので。
        ///
        /// ワーストケースとしては \u0061 みたいなUnicodeエスケープが続く場合だけども、その場合は3倍ほど無駄に領域確保することになる。
        /// まあ、エスケープ文字だらけな文字列とかそんなに来ないだろうという想定。
        /// </remarks>
        /// <param name="s">エスケープした文字列。</param>
        /// <returns>復元結果。</returns>
        public static unsafe ArraySegment<byte> Unescape(this ArraySegment<byte> s)
        {
            var buffer = new byte[s.Count];
            int len;

            fixed(byte* to = buffer)
            {
                len = Unescape(s, to);
            }

            return new ArraySegment<byte>(buffer, 0, len);
        }

        public static unsafe Utf8ArraySegment Unescape<TUtf8>(this TUtf8 s) where TUtf8 : IUtf8String => new Utf8ArraySegment(Unescape(s.Utf8));

        /// <summary>
        /// <see cref="Unescape(ArraySegment{byte}, byte*)"/>してから string 化。
        /// </summary>
        /// <param name="s">エスケープした文字列。</param>
        /// <returns>復元結果。</returns>
        /// <remarks>
        /// <code><![CDATA[ new Utf8ArraySegment(Unescape(s)).ToString() ]]></code> で同じことができるものの。
        /// <see cref="Unescape(ArraySegment{byte})"/>が内部で配列を1個ヒープ確保しちゃうのがもったいないので。
        /// stackalloc した領域に<see cref="Unescape(ArraySegment{byte}, byte*)"/>して、それを直接<see cref="System.Text.Encoding.GetChars(byte*, int, char*, int)"/>する。
        /// </remarks>
        public static unsafe string UnescapeToString(this ArraySegment<byte> s)
        {
            if (s.Count == 0) return "";

            // Count が大きい時に stackalloc でいいのかという問題はあり
            // Span が使えるなら count <= 1024 ? (Span<byte>)stackalloc byte[count] : (Span<byte>)new byte[count] とかやるんだけど。
            var buffer = stackalloc byte[s.Count];
            var len = Unescape(s, buffer);

            var utf8 = System.Text.Encoding.UTF8;

#if NET35
            var count = utf8.GetCharCount(buffer, len);
            var str = new string('\0', count);
            fixed(char* p = str)
            {
                utf8.GetChars(buffer, len, p, count);
            }
            return str;
#else
            return utf8.GetString(buffer, len);
#endif
        }

        public static unsafe string UnescapeToString<TUtf8>(this TUtf8 s) where TUtf8 : IUtf8String => UnescapeToString(s.Utf8);

        /// <summary>
        /// \ エスケープ文字列から元の文字列を復元。
        /// </summary>
        /// <param name="s">エスケープした文字列。</param>
        /// <param name="to">復元先。</param>
        /// <returns>復元後の長さ。</returns>
        /// <remarks>
        /// 文字列中にエスケープされていない " など、不正な入力がされた場合の挙動は未定義
        /// </remarks>
        public static unsafe int Unescape(this ArraySegment<byte> s, byte* to)
        {
            var i = 0;
            var len = 0;

            bool TryRead(out byte b)
            {
                var index = i++;
                if (index < s.Count)
                {
                    b = s.Array[index + s.Offset];
                    return true;
                }
                else
                {
                    b = 0;
                    return false;
                }
            }

            void Write(byte c)
            {
                *to = c;
                to++;
                len++;
            }

            uint ParseHex(byte b)
            {
                if ('0' <= b && b <= '9') return (uint)b - (byte)'0';
                if ('a' <= b && b <= 'f') return (uint)b - (byte)'a' + 10;
                if ('A' <= b && b <= 'F') return (uint)b - (byte)'A' + 10;
                return 0;
            }

            while (TryRead(out var c))
            {
                if (c != (byte)'\\')
                {
                    Write(c);
                    continue;
                }

                if (!TryRead(out c)) throw new FormatException();

                switch (c)
                {
                    case (byte)'"':
                    case (byte)'\\':
                    case (byte)'/':
                        Write(c);
                        break;
                    case (byte)'b':
                        Write((byte)'\b');
                        break;
                    case (byte)'f':
                        Write((byte)'\f');
                        break;
                    case (byte)'n':
                        Write((byte)'\n');
                        break;
                    case (byte)'r':
                        Write((byte)'\r');
                        break;
                    case (byte)'t':
                        Write((byte)'\t');
                        break;
                    case (byte)'0':
                        Write((byte)'\0');
                        break;
                    case (byte)'u':
                        {
                            if (!TryRead(out var b1)) throw new FormatException();
                            if (!TryRead(out var b2)) throw new FormatException();
                            if (!TryRead(out var b3)) throw new FormatException();
                            if (!TryRead(out var b4)) throw new FormatException();

                            var cp = (ParseHex(b1) << 12) | (ParseHex(b2) << 8) | (ParseHex(b3) << 4) | ParseHex(b4);

                            if (char.IsHighSurrogate((char)cp))
                            {
                                // サロゲートペアの high surrogate だけが \u エスケープされてる状態とかは考えなくていいよね？
                                if (!TryRead(out c) || c != (byte)'\\') throw new FormatException();
                                if (!TryRead(out c) || c != (byte)'u') throw new FormatException();

                                if (!TryRead(out b1)) throw new FormatException();
                                if (!TryRead(out b2)) throw new FormatException();
                                if (!TryRead(out b3)) throw new FormatException();
                                if (!TryRead(out b4)) throw new FormatException();

                                var low = (ParseHex(b1) << 12) | (ParseHex(b2) << 8) | (ParseHex(b3) << 4) | ParseHex(b4);

                                cp = (cp & 0b00000011_11111111U) + 0b100_0000;
                                cp <<= 10;
                                cp |= (low & 0b00000011_11111111U);
                            }

                            var count = Utf8Encoder.Encode(cp, to);
                            len += count;
                            to += count;
                        }
                        break;
                    case (byte)'U':
                        // C# には \Uxxxxxxxx で32ビットのコードポイントをエスケープする構文あり
                        {
                            if (!TryRead(out var b1)) throw new FormatException();
                            if (!TryRead(out var b2)) throw new FormatException();
                            if (!TryRead(out var b3)) throw new FormatException();
                            if (!TryRead(out var b4)) throw new FormatException();
                            if (!TryRead(out var b5)) throw new FormatException();
                            if (!TryRead(out var b6)) throw new FormatException();
                            if (!TryRead(out var b7)) throw new FormatException();
                            if (!TryRead(out var b8)) throw new FormatException();

                            var cp = (ParseHex(b1) << 28) | (ParseHex(b2) << 24) | (ParseHex(b3) << 20) | (ParseHex(b4) << 16)
                                | (ParseHex(b5) << 12) | (ParseHex(b6) << 8) | (ParseHex(b7) << 4) | ParseHex(b8);

                            var count = Utf8Encoder.Encode(cp, to);
                            len += count;
                            to += count;
                        }
                        break;
                    default:
                        throw new FormatException();
                }
            }

            return len;
        }
    }
}
