#pragma warning disable 1591

using Utf8Utils.Collections;
using Utf8Utils.Text;
using System;
using Utf8Utils.Text.FloatConversion;

namespace Utf8Utils.Json
{
    public interface IFormatter
    {
        void BeginObject(int? discriminator, uint? nullFlags);
        void EndObject();

        void Separator();

        void Key(UnionString name);

        void Value(Number value);
        void Value(UnionString value);

        void BeginArray(int? length);
        void EndArray();

        ArraySegment<byte> Result { get; }
    }

    public static class FormatterExtensions
    {
        public static void WriteProperty(this IFormatter f, UnionString key, Number value)
        {
            f.Separator();
            f.Key(key);
            f.Value(value);
        }

        public static void WriteProperty(this IFormatter f, UnionString key, UnionString value)
        {
            f.Separator();
            f.Key(key);
            f.Value(value);
        }
    }

    public class JsonWriter : IFormatter
    {
        ResizableArray _buffer;
        bool _itemWrote;

        public JsonWriter(int capacity) => _buffer = new ResizableArray(capacity);
        public JsonWriter(ResizableArray buffer) => _buffer = buffer;

        private static readonly byte[] discriminatorBytes = new byte[] { (byte)'d', (byte)'i', (byte)'s', (byte)'c', (byte)'r', (byte)'i', (byte)'m', (byte)'i', (byte)'n', (byte)'a', (byte)'t', (byte)'o', (byte)'r' };
        public void BeginObject(int? discriminator = null, uint? nullFlags = null)
        {
            WriteAscii('{');
            _itemWrote = false;

            if (discriminator != null)
            {
                Key(new Utf8ArraySegment(discriminatorBytes));
                Value((Number)discriminator.Value);
            }
        }
        public void EndObject() => WriteAscii('}');

        public void BeginArray(int? length = null)
        {
            WriteAscii('[');
            _itemWrote = false;
        }
        public void EndArray() => WriteAscii(']');

        public void Key(UnionString name)
        {
            if (name.IsUtf8) Write(name.Utf8);
            else Write(name.String);
            WriteAscii(':');
        }

        public void Value(Number value)
        {
            switch (value.Type)
            {
                case NumberType._null: WriteNull(); break;
                case NumberType._bool: Write((bool)value); break;
                case NumberType._byte: Write((byte)value); break;
                case NumberType._sbyte: Write((sbyte)value); break;
                case NumberType._short: Write((short)value); break;
                case NumberType._ushort: Write((ushort)value); break;
                case NumberType._int: Write((int)value); break;
                case NumberType._uint: Write((uint)value); break;
                case NumberType._long: Write((long)value); break;
                case NumberType._ulong: Write((long)(ulong)value); break;
                case NumberType._float: Write((float)value); break;
                case NumberType._double: Write((double)value); break;
                //todo: DateTime/DateTimeOffset
            }
        }

        public void Value(UnionString value)
        {
            if (value.IsNull) WriteNull();
            else if (value.IsUtf8) Write(value.Utf8);
            else Write(value.String);
        }

        public ArraySegment<byte> Result => _buffer.Full;

        private static readonly byte[] nullBytes = new byte[] { (byte)'n', (byte)'u', (byte)'l', (byte)'l' };
        private static readonly byte[] trueBytes = new byte[] { (byte)'t', (byte)'r', (byte)'u', (byte)'e' };
        private static readonly byte[] falseBytes = new byte[] { (byte)'f', (byte)'a', (byte)'l', (byte)'s', (byte)'e' };

        internal void WriteAscii(char c)
        {
            // ASCII 文字以外を渡したときの動作保証ない
            _buffer.Add((byte)c);
        }

        public void Separator()
        {
            if (_itemWrote) WriteAscii(',');
            _itemWrote = true;
        }

        public void WriteNull()
        {
            _buffer.AddAll(nullBytes);
        }

        public void Write(bool b)
        {
            if (b) _buffer.AddAll(trueBytes);
            else _buffer.AddAll(falseBytes);
        }

        private unsafe void Write(double x)
        {
            FloatConversion.ToString(x, out var buffer);
            var b = stackalloc byte[NumberStringBuffer.MaxChars];
            var length = buffer.Format(b);
            _buffer.AddAll(b, length);
        }

        private unsafe void Write(float x)
        {
            FloatConversion.ToString(x, out var buffer);
            var b = stackalloc byte[NumberStringBuffer.MaxChars];
            var length = buffer.Format(b);
            _buffer.AddAll(b, length);
        }

        public void Write(string s)
        {
            if (s == null)
            {
                WriteNull();
                return;
            }

            uint high = 0;

            WriteAscii('"');
            foreach (var c in s)
            {
                if (c < 0x80)
                {
                    WriteEscapedAscii((byte)c);
                }
                else if (char.IsHighSurrogate(c))
                {
                    high = (c & 0b00000011_11111111U) + 0b1000000;
                    high <<= 10;
                }
                else if (char.IsLowSurrogate(c))
                {
                    high |= (c & 0b00000011_11111111U);
                    unchecked
                    {
                        _buffer.Add((byte)((high >> 18) | 0b1111_0000));
                        _buffer.Add((byte)((high >> 12) & 0b11_1111 | 0b1000_0000));
                        _buffer.Add((byte)((high >> 6) & 0b11_1111 | 0b1000_0000));
                        _buffer.Add((byte)(high & 0b11_1111 | 0b1000_0000));
                    }
                }
                else if(c < 0x800)
                {
                    unchecked
                    {
                        _buffer.Add((byte)((c >> 6) | 0b1100_0000));
                        _buffer.Add((byte)(c & 0b11_1111 | 0b1000_0000));
                    }
                }
                else
                {
                    unchecked
                    {
                        _buffer.Add((byte)((c >> 12) | 0b1110_0000));
                        _buffer.Add((byte)((c >> 6) & 0b11_1111 | 0b1000_0000));
                        _buffer.Add((byte)(c & 0b11_1111 | 0b1000_0000));
                    }
                }
            }
            WriteAscii('"');
        }

        public void Write(Utf8ArraySegment s)
        {
            WriteAscii('"');
            foreach (var c in s)
            {
                WriteEscapedAscii(c);
            }
            WriteAscii('"');
        }

        private void WriteEscapedAscii(byte c)
        {
            switch ((char)c)
            {
                case '"':
                case '\\':
                case '/':
                    WriteAscii('\\');
                    _buffer.Add(c);
                    break;
                case '\r':
                    WriteAscii('\\');
                    WriteAscii('r');
                    break;
                case '\n':
                    WriteAscii('\\');
                    WriteAscii('n');
                    break;
                case '\t':
                    WriteAscii('\\');
                    WriteAscii('t');
                    break;
                default:
                    _buffer.Add(c);
                    break;
            }
        }

        public void Write(long value)
        {
            if(value == 0)
            {
                WriteAscii('0');
                return;
            }

            if (value < 0)
            {
                WriteAscii('-');
                value = -value;
            }
            var digits = 0;
            for (var i = value; i != 0; i /= 10) digits++;

            _buffer.Extend(digits);

            var index = _buffer.Count - 1;
            for (int i = 0; i < digits; i++, index--)
            {
#if NETSTANDARD1_3
                var div= value / 10;
                var rem = value - (div * 10);
                value = div;
#else
                value = Math.DivRem(value, 10, out var rem);
#endif
                _buffer[index] = unchecked((byte)('0' + rem));
            }
        }
    }
}
