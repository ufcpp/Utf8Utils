using System.Linq;
using Utf8Utils.Text;
using Xunit;

namespace Utf8UtilsTest
{
    public class Equality
    {
        [Fact]
        public void EqualOperator()
        {
            foreach (var (a, b) in TestData.Pairs)
            {
                var strEquals = a.String == b.String;
                var ut8Equals = new Utf8String(a.Utf8) == new Utf8String(b.Utf8);

                Assert.Equal(strEquals, ut8Equals);
            }
        }

        [Fact]
        public void Equals()
        {
            foreach (var (a, b) in TestData.Pairs)
            {
                var strEquals = a.String.Equals(b.String);
                var ut8Equals = new Utf8String(a.Utf8).Equals(new Utf8String(b.Utf8));
                var ut8StrEquals = new Utf8String(a.Utf8).Equals(b.String);

                Assert.Equal(strEquals, ut8Equals);
                Assert.Equal(strEquals, ut8StrEquals);
            }
        }

        [Fact]
        public void ShouldBeIdentical()
        {
            foreach (var s in TestData.Data)
            {
                var utf8 = new Utf8String(s.Utf8);
                EqualAsBytes(utf8, s.Utf8);
                EqualAsCodePoints(utf8, s.Utf32I);

                Assert.True(utf8.Equals(s.String));
            }
        }

        private static void EqualAsCodePoints(Utf8String s, uint[] expected)
        {
            var codePoints = s.CodePoints.ToArray();

            Assert.Equal(expected.Length, codePoints.Length);

            for (int i = 0; i < codePoints.Length; i++)
            {
                Assert.Equal(expected[i], codePoints[i]);
            }
        }

        private static void EqualAsBytes(Utf8String s, byte[] expected)
        {
            var bytes = s.ToArray();

            Assert.Equal(expected.Length, bytes.Length);

            for (int i = 0; i < bytes.Length; i++)
            {
                Assert.Equal(expected[i], bytes[i]);
            }
        }
    }
}
