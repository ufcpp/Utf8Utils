namespace Utf8Utils.Text
{
    internal static class Utf8Encoder
    {
        public static unsafe int Encode(uint codePoint, byte* to)
        {
            if (codePoint < 0x80)
            {
                to[0] = (byte)codePoint;
                return 1;
            }
            else if (codePoint < 0x800)
            {
                to[0] = (byte)(0b1100_0000 | ((codePoint >> 6) & 0b01_1111));
                to[1] = (byte)(0b1000_0000 | (codePoint & 0b11_1111));
                return 2;
            }
            else if (codePoint < 0x1_0000)
            {
                to[0] = (byte)(0b1110_0000 | ((codePoint >> 12) & 0b1111));
                to[1] = (byte)(0b1000_0000 | ((codePoint >> 6) & 0b11_1111));
                to[2] = (byte)(0b1000_0000 | (codePoint & 0b11_1111));
                return 3;
            }
            else
            {
                to[0] = (byte)(0b1111_0000 | ((codePoint >> 18) & 0b0111));
                to[1] = (byte)(0b1000_0000 | ((codePoint >> 12) & 0b11_1111));
                to[2] = (byte)(0b1000_0000 | ((codePoint >> 6) & 0b11_1111));
                to[3] = (byte)(0b1000_0000 | (codePoint & 0b11_1111));
                return 4;
            }
        }
    }
}
