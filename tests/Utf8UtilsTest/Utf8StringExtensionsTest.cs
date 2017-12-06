using System;
using Utf8Utils.Text;
using Xunit;

namespace Utf8UtilsTest
{
    public class Utf8StringExtensionsTest
    {
        /// <summary>
        /// <see cref="StringTestData.Data"/>からエスケープ文字でエラーになってしまう部分(正常動作)を排除したもの
        /// </summary>
        private static string[] Data = new[]
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
            "ascii string !\"#$%&'() 1234567890 AQWSEDRFTGYHUJIKOLP+@,./<>?_", // エスケープ文字は消している
            "latin1 string °±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ",
        };

        [Fact]
        public void UnescapeToStringTest()
        {
            foreach (var expect in Data)
            {
                var utf8 = System.Text.Encoding.UTF8.GetBytes(expect);
                var segment = new ArraySegment<byte>(utf8);
                var actual = segment.UnescapeToString();
                Assert.Equal(expect, actual);
            }
        }
    }
}
