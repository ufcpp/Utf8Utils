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
            SegmentItem(DataLength, 1);
            SegmentItem(DataLength, 2);
            SegmentItem(DataLength, 5);
            SegmentItem(DataLength, 10);
            SegmentItem(DataLength, 11);
            SegmentItem(DataLength, 17);
            SegmentItem(DataLength, 19);

            Assert.Throws<ArgumentException>(() => SegmentItem(DataLength, 0));
            Assert.Throws<ArgumentException>(() => SegmentItem(DataLength, -1));
        }

        private static void SegmentItem(int dataLength, int windowSize)
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
