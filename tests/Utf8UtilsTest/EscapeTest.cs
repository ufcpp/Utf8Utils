using System.Text;
using Utf8Utils.Text;
using Xunit;

namespace Utf8UtilsTest
{
    public class EscapeTest
    {
        [Fact]
        public void Unescape()
        {
            var utf8 = Encoding.UTF8;

            // aαℵ🐈🐈
            var unescaped = "\\\"\n\r\u0061\u03B1\u2135\uD83D\uDC08\U0001F408";
            var escaped = @"\\\""\n\r\u0061\u03B1\u2135\uD83D\uDC08\U0001F408";

            var unescapedUtf8 = new Utf8Array(utf8.GetBytes(unescaped));
            var escapedUtf8 = new Utf8Array(utf8.GetBytes(escaped));

            var x = escapedUtf8.Unescape();

            Assert.Equal(unescapedUtf8, x);

            Assert.Equal(unescaped, x.ToString());
            Assert.Equal(unescaped, escapedUtf8.UnescapeToString());
        }

        /// <summary>
        /// 文字列中にエスケープしていない " が入ってきた場合でも
        /// 不正な扱いをせずに変換をしていない結果にする
        /// </summary>
        [Fact]
        public void NotEscapedDoubleQuate()
        {
            var utf8 = Encoding.UTF8;

            var text = @"""";
            var utf8Text = new Utf8Array(utf8.GetBytes(text));
            var actual = utf8Text.UnescapeToString();

            Assert.Equal(text, actual);
        }

        /// <summary>
        /// 複数パターンでのチェック
        /// </summary>
        [Fact]
        public void UnescapeToStringPattern()
        {
            (string escaped, string unescaped)[] Data = new(string escaped, string unescaped)[]
            {
                ("abcdefg","abcdefg"),
                ("aáαあ😀","aáαあ😀"),
                ("aáαℵあáあ゙亜👩👩🏽","aáαℵあáあ゙亜👩👩🏽"),
                ("아조선글","아조선글"),
                ("👨‍👨‍👨‍👨‍👨‍👨‍👨","👨‍👨‍👨‍👨‍👨‍👨‍👨"),
                ("👨‍👩‍👦‍👦","👨‍👩‍👦‍👦"),
                ("👨🏻‍👩🏿‍👦🏽‍👦🏼","👨🏻‍👩🏿‍👦🏽‍👦🏼"),
                ("♢♠♤","♢♠♤"),
                ("🀄♔","🀄♔"),
                ("☀☂☁","☀☂☁"),
                ("∀∂∋","∀∂∋"),
                ("ᚠᛃᚻ","ᚠᛃᚻ"),
                ("𩸽","𩸽"),
                ("́","́"),
                ("\0\0\0",@"\0\0\0"),
                ("\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\u0008\u0009\u000A\u000B\u000C\u000D\u000E\u000F\u0010\u0011\u0012\u0013\u0014\u0015",@"\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\u0008\u0009\u000A\u000B\u000C\u000D\u000E\u000F\u0010\u0011\u0012\u0013\u0014\u0015"),
                ("ascii string !\"#$%&'() 1234567890 AQWSEDRFTGYHUJIKOLP+@,./<>?_",@"ascii string !\""#$%&'() 1234567890 AQWSEDRFTGYHUJIKOLP+@,./<>?_"), // エスケープ文字は消している
                ("latin1 string °±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ","latin1 string °±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ"),
            };

            var utf8 = Encoding.UTF8;

            foreach (var d in Data)
            {
                var bytes = utf8.GetBytes(d.unescaped);
                var segment = new Utf8Array(bytes);
                var actual = segment.UnescapeToString();
                Assert.Equal(d.escaped, actual);
            }
        }
    }
}
