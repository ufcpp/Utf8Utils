using Utf8Utils.Collections;
using System;

namespace Utf8Utils.Text
{
    internal static class Utf8Decoder
    {
        public const byte InvalidCount = 0xff;
        public const uint EoS = 0xffff_ffff;

        public static int GetLength(ArraySegment<byte> buffer)
        {
            var count = 0;
            for (int i = 0; i < buffer.Count; i++)
            {
                var x = buffer.At(i);
                if ((x & 0b1100_0000) != 0b1000_0000)
                    count++;
            }
            return count;
        }

        public static byte TyrGetCount(ArraySegment<byte> buffer, int index)
        {
            if (index >= buffer.Count) return InvalidCount;

            uint x = buffer.At(index);

            var byteCount =
                (x < 0b1100_0000U) ? (byte)1 :
                (x < 0b1110_0000U) ? (byte)2 :
                (x < 0b1111_0000U) ? (byte)3 :
                (byte)4;

            if (index + byteCount > buffer.Count) return InvalidCount;

            return byteCount;
        }

        public static byte TryDecode(ArraySegment<byte> buffer, int index, out uint codePoint)
        {
            if (index >= buffer.Count) { codePoint = EoS; return InvalidCount; }

            uint code = buffer.At(index);

            if (code < 0b1100_0000)
            {
                // ASCII 文字
                codePoint = code;
                return 1;
            }
            if (code < 0b1110_0000)
            {
                // 2バイト文字
                if (index + 1 >= buffer.Count) { codePoint = EoS; return InvalidCount; }
                code &= 0b1_1111;
                code = (code << 6) | (uint)(buffer.At(++index) & 0b0011_1111);
                codePoint = code;
                return 2;
            }
            if (code < 0b1111_0000)
            {
                // 3バイト文字
                if (index + 2 >= buffer.Count) { codePoint = EoS; return InvalidCount; }
                code &= 0b1111;
                code = (code << 6) | (uint)(buffer.At(++index) & 0b0011_1111);
                code = (code << 6) | (uint)(buffer.At(++index) & 0b0011_1111);
                codePoint = code;
                return 3;
            }

            // 4バイト文字
            if (index + 3 >= buffer.Count) { codePoint = EoS; return InvalidCount; }
            code &= 0b0111;
            code = (code << 6) | (uint)(buffer.At(++index) & 0b0011_1111);
            code = (code << 6) | (uint)(buffer.At(++index) & 0b0011_1111);
            code = (code << 6) | (uint)(buffer.At(++index) & 0b0011_1111);
            codePoint = code;
            return 4;
        }
    }
}
