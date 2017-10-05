using System;
using System.Linq;
using Utf8Utils.Text;
using Xunit;

namespace Utf8UtilsTest
{
    public class SearchTest
    {
        [Fact]
        public void IndexOf()
        {
            var text = StringTestData.LongString;
            var utf8text = new Utf8Array(text.Utf8);

            foreach (var s in StringTestData.SubStringsInLongText
                .Concat(StringTestData.RandomStrings)
                .Concat(StringTestData.Data)
                )
            {
                var utf8 = new Utf8Array(s.Utf8);

                var i1 = utf8text.IndexOf(utf8);
                var i2 = text.String.IndexOf(s.String, StringComparison.Ordinal);

                Assert.Equal(i1 < 0, i2 < 0);

                if (i1 < 0) continue;

                var s1 = utf8text.Substring(i1, utf8.Length);
                var s2 = text.String.Substring(i2, s.String.Length);

                Assert.Equal(s1.ToString(), s2);
            }
        }
    }
}
