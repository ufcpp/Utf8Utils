using System;
using System.Linq;
using Utf8Utils.Text;
using Utf8Utils.Text.FloatConversion;
using Xunit;

namespace Utf8UtilsTest
{
    public class FloatConversionTest
    {
        [Fact]
        public void ConvertParse()
        {
            foreach (var x in FloatTestData.DoubleValues) Test(x);
            foreach (var x in FloatTestData.SingleValues) Test(x);
        }

        static void Equal(double a, double b, double Epsilon)
        {
            var diff = Math.Abs((a - b) / a);
            Assert.True(diff < Epsilon);
        }

        private static void Test(double x)
        {
            const double Epsilon = 1E-15;
            FloatConversion.ToString(x, out var buffer);

            var xs = x.ToString();
            var ys = buffer.ToString();

            Assert.Equal(xs.Contains('E'), ys.Contains('E'));

            var y = double.Parse(ys);
            Equal(x, y, Epsilon);

            var zs = new Utf8Array(buffer.GetUtf8Bytes());
            var z = zs.ParseFloat();

            // 精度がそこまで高くないので、小さい値の時は結構誤差が大きいので別判定
            if (Math.Abs(z) < Epsilon) Assert.True(Math.Abs(x) < Epsilon);
            else Equal(x, z, Epsilon);
        }

        private static void Test(float x)
        {
            const double Epsilon = 2E-7;
            FloatConversion.ToString(x, out var buffer);

            var xs = x.ToString();
            var ys = buffer.ToString();

            Assert.Equal(xs.Contains('E'), ys.Contains('E'));

            var y = double.Parse(ys);
            Equal(x, y, Epsilon);

            var zs = new Utf8Array(buffer.GetUtf8Bytes());
            var z = zs.ParseFloat();

            // 精度がそこまで高くないので、小さい値の時は結構誤差が大きいので別判定
            if (Math.Abs(z) < Epsilon) Assert.True(Math.Abs(x) < Epsilon);
            else Equal(x, z, Epsilon);
        }
    }
}
