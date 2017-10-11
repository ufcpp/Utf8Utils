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
    }
}
