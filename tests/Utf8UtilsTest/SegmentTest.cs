using System;
using System.Linq;
using Utf8Utils.Collections;
using Xunit;

namespace Utf8UtilsTest
{
    public class SegmentTest
    {
        [Fact]
        public void Segment()
        {
            const int DataLength = 1000;
            Segment(DataLength, 1);
            Segment(DataLength, 2);
            Segment(DataLength, 5);
            Segment(DataLength, 10);
            Segment(DataLength, 11);
            Segment(DataLength, 17);
            Segment(DataLength, 19);

            Assert.Throws(typeof(ArgumentException), () => Segment(DataLength, 0));
            Assert.Throws(typeof(ArgumentException), () => Segment(DataLength, -1));
        }

        private static void Segment(int dataLength, int windowSize)
        {
            var data = Enumerable.Range(0, dataLength).ToArray();

            var count = data.Segment(windowSize).Count();

            Assert.Equal((dataLength + windowSize - 1) / windowSize, count);

            foreach (var segment in data.Segment(windowSize))
            {
                for (int i = 0; i < segment.Count; i++)
                {
                    Assert.Equal(i, segment.At(i) % windowSize);
                }
            }
        }
    }
}
