using System;
using System.Linq;
using System.Text;

namespace Utf8UtilsTest
{
    internal struct StringTestData
    {
        private static readonly string _longStr = "long text: 1234567890-^\\qwertyuiop@[asdfghjkl;:]zxcvbnm,./\\!\"#$%&'()=~|QWERTYUIOP`{ASDFGHJKL+*}ZXCVBNM<>?_🐁🐂🐅🐇🐉🐍🐎🐑🐒🐔🐕🐗１２３４５６７８９０－＾￥くぇｒちゅいおｐ＠「あｓｄｆｇｈｊｋｌ；：」ｚｘｃｖｂんｍ、。・￥あｑｗせｄｒｆｔｇｙふじこｌｐ；＠：「あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよわをんaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbcccccccccccccccccccccccccccccccccccccccccccccccccccdddddddddddddddddddddddddddddddddddddddddddddddddddddeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeefffffffffffffffffffffffffffffffffffffffffffffffffffffggggggggggggggggg1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890O Romeo, Romeo, wherefore art thou Romeo? Deny thy father and refuse thy name; Or if thou wilt not, be but sworn my love And I'll no longer be a Capulet. 国破山河在 城春草木深 感時花濺涙 恨別鳥驚心 烽火連三月 家書抵萬金 白頭掻更短 渾欲不勝簪 春はあけぼの。やうやう白くなりゆく山際、少し明かりて、紫だちたる雲の細くたなびきたる。 夏は夜。月のころはさらなり、闇もなほ、蛍の多く飛びちがひたる。また、ただ一つ二つなど、ほかにうち光て行くもをかし。雨など降るもをかし。 秋は夕暮れ。夕日の差して山の端いと近うなりたるに、烏の寝所へ行くとて、三つ四つ、二つ三つなど飛び急ぐさへあはれなり。まいて雁などの連ねたるが、いと小さく見ゆるは、いとをかし。日入り果てて、風の音、虫の音など、はた言ふべきにあらず。 冬はつとめて。雪の降りたるは言ふべきにもあらず、霜のいと白きも、またさらでもいと寒きに、火など急ぎおこして、炭持て渡るも、いとつきづきし。昼になりて、ぬるくゆるびもていけば、火桶の火も、白き灰がちになりてわろし。";

        public static readonly StringTestData LongString = new StringTestData(_longStr);

        public static readonly StringTestData[] Data = new[]
        {
            "abcdefg",
            "aáαあ😀",
            "aáαℵあáあ゙亜👩👩🏽",
            "아조선글",
            "👨‍👨‍👨‍👨‍👨‍👨‍👨",
            "👨‍👩‍👦‍👦",
            "👨🏻‍👩🏿‍👦🏽‍👦🏼",
            "́",
            "♢♠♤",
            "🀄♔",
            "☀☂☁",
            "∀∂∋",
            "ᚠᛃᚻ",
            "𩸽",
            "",
            "\0\0\0",
            "\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\u0008\u0009\u000A\u000B\u000C\u000D\u000E\u000F\u0010\u0011\u0012\u0013\u0014\u0015",
            "ascii string !\"#$%&'() 1234567890 AQWSEDRFTGYHUJIKOLP+@,./\\<>?_",
            "latin1 string °±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ",
            _longStr,
        }.Select(s => new StringTestData(s)).ToArray();

        public static readonly (StringTestData a, StringTestData b)[] Pairs;

        public static StringTestData[] SubStringsInLongText;

        public static StringTestData[] RandomStrings;

        static StringTestData()
        {
            var r = new Random(1);

            Pairs = new(StringTestData, StringTestData)[300];
            for (int i = 0; i < Pairs.Length; i++)
            {
                Pairs[i].a = Data[r.Next(Data.Length)];
                Pairs[i].b = Data[r.Next(Data.Length)];
            }

            SubStringsInLongText = new StringTestData[300];
            for (int i = 0; i < SubStringsInLongText.Length; i++)
            {
                string s;
                do
                {
                    // サロゲートペアの途中で切れるのを排除
                    var start = r.Next(_longStr.Length);
                    var len = r.Next(_longStr.Length - start);
                    s = _longStr.Substring(start, len);
                }
                while (s.Length == 0 || (char.IsLowSurrogate(s[0]) || char.IsHighSurrogate(s[s.Length - 1])));

                SubStringsInLongText[i] = new StringTestData(s);
            }

            RandomStrings = new StringTestData[300];
            for (int i = 0; i < RandomStrings.Length; i++)
            {
                var start = r.Next(_longStr.Length);
                var len = r.Next(_longStr.Length - start);
                RandomStrings[i] = new StringTestData(RandomString(r, 0, 100));
            }
        }

    public string String { get; }
        public byte[] Utf8 { get; }
        public byte[] Utf16B { get; }
        public ushort[] Utf16S { get; }
        public byte[] Utf32B { get; }
        public uint[] Utf32I { get; }
        public byte[] Latin1 { get; }

        public StringTestData(string s)
        {
            String = s;
            Utf8 = Encoding.UTF8.GetBytes(s);
            Utf16B = Encoding.Unicode.GetBytes(s);
            Utf16S = Copy8To16(Utf16B);
            Utf32B = Encoding.UTF32.GetBytes(s);
            Utf32I = Copy8To32(Utf32B);

            if (s.All(c => c < 0x100))
                Latin1 = Encoding.GetEncoding("iso-8859-1").GetBytes(s);
            else
                Latin1 = null;
        }

        private static ushort[] Copy8To16(byte[] encodedBytes)
        {
            if ((encodedBytes.Length % 2) != 0) throw new ArgumentException();
            var output = new ushort[encodedBytes.Length / 2];
            Buffer.BlockCopy(encodedBytes, 0, output, 0, encodedBytes.Length);
            return output;
        }

        private static uint[] Copy8To32(byte[] encodedBytes)
        {
            if ((encodedBytes.Length % 4) != 0) throw new ArgumentException();
            var output = new uint[encodedBytes.Length / 4];
            Buffer.BlockCopy(encodedBytes, 0, output, 0, encodedBytes.Length);
            return output;
        }

        private static string RandomString(Random r, int minLen = 0, int maxLen = 1024)
        {
            var len = r.Next(minLen, maxLen);
            var sb = new System.Text.StringBuilder();

            for (int i = 0; i < len; i++)
            {
                var x = r.NextDouble();
                if (x < 0.1) sb.Append((char)r.Next('a', 'z' + 1));
                else if (x < 0.2) sb.Append((char)r.Next('A', 'Z' + 1));
                else if (x < 0.3) sb.Append("\"\t !#$%&'()=-+*;:{}[]<>?/\\_`@"[r.Next(0, 29)]);
                else if (x < 0.4) sb.Append((char)r.Next('ぁ', 'ゖ' + 1));
                else if (x < 0.5) sb.Append((char)r.Next('ァ', 'ヺ' + 1));
                else if (x < 0.6) sb.Append((char)r.Next('一', '鿕' + 1));
                else if (x < 0.7) sb.Append(char.ConvertFromUtf32(r.Next(0x1F300, 0x1F600)));
                else if (x < 0.8) sb.Append((char)r.Next('α', 'ω' + 1));
                else if (x < 0.9) sb.Append((char)r.Next('А', 'я' + 1));
                else sb.Append((char)r.Next('Ā', 'ſ' + 1));
            }

            return sb.ToString();
        }
    }

    internal class FloatTestData
    {
        public static double[] DoubleValues;
        public static float[] SingleValues;

        static FloatTestData()
        {
            const int n = 100000;

            var r = new Random();

            var x = new double[3 * n + 1];
            x[0] = 2e15;
            var i = 1;
            for (; i <= n; i++) x[i] = r.Next() * Math.Pow(10, r.Next(1, 15));
            for (; i <= 2 * n; i++) x[i] = (2 * r.NextDouble() - 1) * Math.Pow(10, r.Next(-300, 300));
            for (; i <= 3 * n; i++) x[i] = (2 * r.NextDouble() - 1) * Math.Pow(10, r.Next(-5, 15));
            DoubleValues = x;

            var y = new float[3 * n + 1];
            y[0] = 2e7f;
            i = 1;
            for (; i <= n; i++) y[i] = (float)(r.Next() * Math.Pow(10, r.Next(1, 7)));
            for (; i <= 2 * n; i++) y[i] = (float)((2 * r.NextDouble() - 1) * Math.Pow(10, r.Next(-35, 35)));
            for (; i <= 3 * n; i++) y[i] = (float)((2 * r.NextDouble() - 1) * Math.Pow(10, r.Next(-5, 7)));
            SingleValues = y;
        }
    }
}
