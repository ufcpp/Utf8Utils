using System;
using Utf8Utils.Text;
using Xunit;

namespace Utf8UtilsTest
{
    public class Allocation
    {
        // https://github.com/Microsoft/xunit-performance ← これとかが簡単に使えるようになったらまた改めて考える
        [Fact(Skip = "他のテストと並列で動くと正しくテスト判定できない")]
        public void NoAllocationWithForeach()
        {
            const int N = 1000;

            foreach (var s in StringTestData.Data)
            {
                NoAllocationWithForeachItem(new Utf8ArraySegment(s.Utf8), N);
            }
        }

        private static void NoAllocationWithForeachItem(Utf8ArraySegment s, int n)
        {
            var start = GC.GetTotalMemory(false);

            for (int i = 0; i < n; i++)
            {
                foreach (var x in s.CodePoints)
                    ;

                if (s.Length > 3)
                {
                    var sub1 = s.Substring(1, 1);
                    var sub2 = s.Substring(2);
                    var sub3 = s.Substring(3);
                }
            }

            var end = GC.GetTotalMemory(false);

            Assert.Equal(start, end);
        }
    }
}
