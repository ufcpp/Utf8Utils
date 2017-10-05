using System;
using System.Collections.Generic;

namespace Utf8Utils.Text
{
    public interface IUtf8String : IEquatable<IUtf8String>, IEquatable<string>, IEnumerable<byte>
    {
        byte this[int i] { get; }

        int Length { get; }
        ArraySegment<byte> Utf8 { get; }

        int CodePointLength { get; }
        CodePointEnumerable CodePoints { get; }

        Utf8ArraySegment Substring(int index);
        Utf8ArraySegment Substring(int index, int length);
    }
}
