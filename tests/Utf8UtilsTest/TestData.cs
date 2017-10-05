using System;
using System.Linq;
using System.Text;

namespace Utf8UtilsTest
{
    internal struct StringTestData
    {
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
            "long long text: 1234567890-^\\qwertyuiop@[asdfghjkl;:]zxcvbnm,./\\!\"#$%&'()=~|QWERTYUIOP`{ASDFGHJKL+*}ZXCVBNM<>?_１２３４５６７８９０－＾￥くぇｒちゅいおｐ＠「あｓｄｆｇｈｊｋｌ；：」ｚｘｃｖｂんｍ、。・￥あｑｗせｄｒｆｔｇｙふじこｌｐ；＠：「あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよわをんO Romeo, Romeo, wherefore art thou Romeo? Deny thy father and refuse thy name; Or if thou wilt not, be but sworn my love And I'll no longer be a Capulet. 国破山河在 城春草木深 感時花濺涙 恨別鳥驚心 烽火連三月 家書抵萬金 白頭掻更短 渾欲不勝簪 春はあけぼの。やうやう白くなりゆく山際、少し明かりて、紫だちたる雲の細くたなびきたる。 夏は夜。月のころはさらなり、闇もなほ、蛍の多く飛びちがひたる。また、ただ一つ二つなど、ほかにうち光て行くもをかし。雨など降るもをかし。 秋は夕暮れ。夕日の差して山の端いと近うなりたるに、烏の寝所へ行くとて、三つ四つ、二つ三つなど飛び急ぐさへあはれなり。まいて雁などの連ねたるが、いと小さく見ゆるは、いとをかし。日入り果てて、風の音、虫の音など、はた言ふべきにあらず。 冬はつとめて。雪の降りたるは言ふべきにもあらず、霜のいと白きも、またさらでもいと寒きに、火など急ぎおこして、炭持て渡るも、いとつきづきし。昼になりて、ぬるくゆるびもていけば、火桶の火も、白き灰がちになりてわろし。 🐁🐂🐅🐇🐉🐍🐎🐑🐒🐔🐕🐗",
        }.Select(s => new StringTestData(s)).ToArray();

        public static readonly (StringTestData a, StringTestData b)[] Pairs = (
            from _ in Enumerable.Range(0, 300)
            let r = new Random()
            select (Data[r.Next(Data.Length)], Data[r.Next(Data.Length)])
            ).ToArray();

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
