using System.Collections.Generic;
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
                var ut8Equals = new Utf8Array(a.Utf8) == new Utf8Array(b.Utf8);

                Assert.Equal(strEquals, ut8Equals);
            }

            foreach (var (a, b) in TestData.Pairs)
            {
                var strEquals = a.String == b.String;
                var ut8Equals = new Utf8ArraySegment(a.Utf8) == new Utf8ArraySegment(b.Utf8);

                Assert.Equal(strEquals, ut8Equals);
            }
        }

        [Fact]
        public void Equals()
        {
            foreach (var (a, b) in TestData.Pairs)
            {
                var strEquals = a.String.Equals(b.String);
                var ut8Equals = new Utf8ArraySegment(a.Utf8).Equals(new Utf8ArraySegment(b.Utf8));
                var ut8StrEquals = new Utf8ArraySegment(a.Utf8).Equals(b.String);

                Assert.Equal(strEquals, ut8Equals);
                Assert.Equal(strEquals, ut8StrEquals);
            }

            foreach (var (a, b) in TestData.Pairs)
            {
                var strEquals = a.String.Equals(b.String);
                var ut8Equals = new Utf8Array(a.Utf8).Equals(new Utf8Array(b.Utf8));
                var ut8StrEquals = new Utf8Array(a.Utf8).Equals(b.String);

                Assert.Equal(strEquals, ut8Equals);
                Assert.Equal(strEquals, ut8StrEquals);
            }
        }

        [Fact]
        public void CodePointsSequenceEqual()
        {
            foreach (var s in TestData.Data)
            {
                var utf8 = new Utf8ArraySegment(s.Utf8);
                Assert.True(((IEnumerable<uint>)utf8.CodePoints).SequenceEqual(s.String.GetCodePoints()));
            }

            foreach (var s in TestData.Data)
            {
                var utf8 = new Utf8Array(s.Utf8);
                Assert.True(((IEnumerable<uint>)utf8.CodePoints).SequenceEqual(s.String.GetCodePoints()));
            }
        }

        [Fact]
        public void ShouldBeIdentical()
        {
            foreach (var s in TestData.Data)
            {
                var utf8 = new Utf8ArraySegment(s.Utf8);
                EqualAsBytes(utf8, s.Utf8);
                EqualAsCodePoints(utf8, s.Utf32I);

                Assert.True(utf8.Equals(s.String));
            }

            foreach (var s in TestData.Data)
            {
                var utf8 = new Utf8Array(s.Utf8);
                EqualAsBytes(utf8, s.Utf8);
                EqualAsCodePoints(utf8, s.Utf32I);

                Assert.True(utf8.Equals(s.String));
            }
        }

        private static void EqualAsCodePoints<TUtf8>(TUtf8 s, uint[] expected)
            where TUtf8 : IUtf8String
        {
            var codePoints = s.CodePoints.ToArray();

            Assert.Equal(expected.Length, codePoints.Length);

            for (int i = 0; i < codePoints.Length; i++)
            {
                Assert.Equal(expected[i], codePoints[i]);
            }
        }

        private static void EqualAsBytes<TUtf8>(TUtf8 s, byte[] expected)
            where TUtf8 : IUtf8String
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
