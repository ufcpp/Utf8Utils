#pragma warning disable 1591

using System;
using System.Runtime.InteropServices;

namespace Utf8Utils
{
    public enum NumberType : byte
    {
        _null,
        _bool,
        _byte,
        _sbyte,
        _short,
        _ushort,
        _int,
        _uint,
        _long,
        _ulong,
        _float,
        _double,
        _DateTime,
        _DateTimeOffset,
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Number
    {
        [FieldOffset(0)] bool _bool;
        [FieldOffset(0)] byte _byte;
        [FieldOffset(0)] sbyte _sbyte;
        [FieldOffset(0)] short _short;
        [FieldOffset(0)] ushort _ushort;
        [FieldOffset(0)] int _int;
        [FieldOffset(0)] uint _uint;
        [FieldOffset(0)] long _long;
        [FieldOffset(0)] ulong _ulong;
        [FieldOffset(0)] float _float;
        [FieldOffset(0)] double _double;

        [FieldOffset(8)]
        NumberType _type;

        [FieldOffset(10)]
        short _offset;

        internal Number(NumberType type) : this() { _type = type; }

        public bool Bool => _bool;
        public byte Byte => _byte;
        public sbyte Sbyte => _sbyte;
        public short Short => _short;
        public ushort Ushort => _ushort;
        public int Int => _int;
        public uint Uint => _uint;
        public long Long => _long;
        public ulong Ulong => _ulong;
        public float Float => _float;
        public double Double => _double;

        public Number(bool b) : this() { _bool = b; _type = NumberType._bool; }
        public Number(float b) : this() { _float = b; _type = NumberType._float; }
        public Number(double b) : this() { _double = b; _type = NumberType._double; }
        public Number(sbyte b) : this() { _long = b; _type = NumberType._sbyte; }
        public Number(short b) : this() { _long = b; _type = NumberType._short; }
        public Number(int b) : this() { _long = b; _type = NumberType._int; }
        public Number(long b) : this() { _long = b; _type = NumberType._long; }
        public Number(byte b) : this() { _ulong = b; _type = NumberType._byte; }
        public Number(ushort b) : this() { _ulong = b; _type = NumberType._ushort; }
        public Number(uint b) : this() { _ulong = b; _type = NumberType._uint; }
        public Number(ulong b) : this() { _ulong = b; _type = NumberType._ulong; }

        public static explicit operator Number (bool n) => new Number(n);
        public static explicit operator Number (byte n) => new Number(n);
        public static explicit operator Number (sbyte n) => new Number(n);
        public static explicit operator Number (short n) => new Number(n);
        public static explicit operator Number (ushort n) => new Number(n);
        public static explicit operator Number (int n) => new Number(n);
        public static explicit operator Number (uint n) => new Number(n);
        public static explicit operator Number (long n) => new Number(n);
        public static explicit operator Number (ulong n) => new Number(n);
        public static explicit operator Number (float n) => new Number(n);
        public static explicit operator Number (double n) => new Number(n);

        public static explicit operator Number (bool? n) => n.HasValue ? new Number(n.GetValueOrDefault()) : default(Number);
        public static explicit operator Number (byte? n) => n.HasValue ? new Number(n.GetValueOrDefault()) : default(Number);
        public static explicit operator Number (sbyte? n) => n.HasValue ? new Number(n.GetValueOrDefault()) : default(Number);
        public static explicit operator Number (short? n) => n.HasValue ? new Number(n.GetValueOrDefault()) : default(Number);
        public static explicit operator Number (ushort? n) => n.HasValue ? new Number(n.GetValueOrDefault()) : default(Number);
        public static explicit operator Number (int? n) => n.HasValue ? new Number(n.GetValueOrDefault()) : default(Number);
        public static explicit operator Number (uint? n) => n.HasValue ? new Number(n.GetValueOrDefault()) : default(Number);
        public static explicit operator Number (long? n) => n.HasValue ? new Number(n.GetValueOrDefault()) : default(Number);
        public static explicit operator Number (ulong? n) => n.HasValue ? new Number(n.GetValueOrDefault()) : default(Number);
        public static explicit operator Number (float? n) => n.HasValue ? new Number(n.GetValueOrDefault()) : default(Number);
        public static explicit operator Number (double? n) => n.HasValue ? new Number(n.GetValueOrDefault()) : default(Number);

        public static explicit operator byte (Number n)
        {
            switch (n._type)
            {
            case NumberType._byte:
            case NumberType._sbyte:
            case NumberType._short:
            case NumberType._ushort:
            case NumberType._int:
            case NumberType._uint:
            case NumberType._long:
            case NumberType._ulong:
                return n._byte;
            }
            throw new InvalidCastException();
        }

        public static explicit operator byte? (Number n)
        {
            switch (n._type)
            {
            case NumberType._null: return null;
            case NumberType._byte:
            case NumberType._sbyte:
            case NumberType._short:
            case NumberType._ushort:
            case NumberType._int:
            case NumberType._uint:
            case NumberType._long:
            case NumberType._ulong:
                return n._byte;
            }
            throw new InvalidCastException();
        }

        public static explicit operator sbyte (Number n)
        {
            switch (n._type)
            {
            case NumberType._byte:
            case NumberType._sbyte:
            case NumberType._short:
            case NumberType._ushort:
            case NumberType._int:
            case NumberType._uint:
            case NumberType._long:
            case NumberType._ulong:
                return n._sbyte;
            }
            throw new InvalidCastException();
        }

        public static explicit operator sbyte? (Number n)
        {
            switch (n._type)
            {
            case NumberType._null: return null;
            case NumberType._byte:
            case NumberType._sbyte:
            case NumberType._short:
            case NumberType._ushort:
            case NumberType._int:
            case NumberType._uint:
            case NumberType._long:
            case NumberType._ulong:
                return n._sbyte;
            }
            throw new InvalidCastException();
        }

        public static explicit operator short (Number n)
        {
            switch (n._type)
            {
            case NumberType._byte:
            case NumberType._sbyte:
            case NumberType._short:
            case NumberType._ushort:
            case NumberType._int:
            case NumberType._uint:
            case NumberType._long:
            case NumberType._ulong:
                return n._short;
            }
            throw new InvalidCastException();
        }

        public static explicit operator short? (Number n)
        {
            switch (n._type)
            {
            case NumberType._null: return null;
            case NumberType._byte:
            case NumberType._sbyte:
            case NumberType._short:
            case NumberType._ushort:
            case NumberType._int:
            case NumberType._uint:
            case NumberType._long:
            case NumberType._ulong:
                return n._short;
            }
            throw new InvalidCastException();
        }

        public static explicit operator ushort (Number n)
        {
            switch (n._type)
            {
            case NumberType._byte:
            case NumberType._sbyte:
            case NumberType._short:
            case NumberType._ushort:
            case NumberType._int:
            case NumberType._uint:
            case NumberType._long:
            case NumberType._ulong:
                return n._ushort;
            }
            throw new InvalidCastException();
        }

        public static explicit operator ushort? (Number n)
        {
            switch (n._type)
            {
            case NumberType._null: return null;
            case NumberType._byte:
            case NumberType._sbyte:
            case NumberType._short:
            case NumberType._ushort:
            case NumberType._int:
            case NumberType._uint:
            case NumberType._long:
            case NumberType._ulong:
                return n._ushort;
            }
            throw new InvalidCastException();
        }

        public static explicit operator int (Number n)
        {
            switch (n._type)
            {
            case NumberType._byte:
            case NumberType._sbyte:
            case NumberType._short:
            case NumberType._ushort:
            case NumberType._int:
            case NumberType._uint:
            case NumberType._long:
            case NumberType._ulong:
                return n._int;
            }
            throw new InvalidCastException();
        }

        public static explicit operator int? (Number n)
        {
            switch (n._type)
            {
            case NumberType._null: return null;
            case NumberType._byte:
            case NumberType._sbyte:
            case NumberType._short:
            case NumberType._ushort:
            case NumberType._int:
            case NumberType._uint:
            case NumberType._long:
            case NumberType._ulong:
                return n._int;
            }
            throw new InvalidCastException();
        }

        public static explicit operator uint (Number n)
        {
            switch (n._type)
            {
            case NumberType._byte:
            case NumberType._sbyte:
            case NumberType._short:
            case NumberType._ushort:
            case NumberType._int:
            case NumberType._uint:
            case NumberType._long:
            case NumberType._ulong:
                return n._uint;
            }
            throw new InvalidCastException();
        }

        public static explicit operator uint? (Number n)
        {
            switch (n._type)
            {
            case NumberType._null: return null;
            case NumberType._byte:
            case NumberType._sbyte:
            case NumberType._short:
            case NumberType._ushort:
            case NumberType._int:
            case NumberType._uint:
            case NumberType._long:
            case NumberType._ulong:
                return n._uint;
            }
            throw new InvalidCastException();
        }

        public static explicit operator long (Number n)
        {
            switch (n._type)
            {
            case NumberType._byte:
            case NumberType._sbyte:
            case NumberType._short:
            case NumberType._ushort:
            case NumberType._int:
            case NumberType._uint:
            case NumberType._long:
            case NumberType._ulong:
                return n._long;
            }
            throw new InvalidCastException();
        }

        public static explicit operator long? (Number n)
        {
            switch (n._type)
            {
            case NumberType._null: return null;
            case NumberType._byte:
            case NumberType._sbyte:
            case NumberType._short:
            case NumberType._ushort:
            case NumberType._int:
            case NumberType._uint:
            case NumberType._long:
            case NumberType._ulong:
                return n._long;
            }
            throw new InvalidCastException();
        }

        public static explicit operator ulong (Number n)
        {
            switch (n._type)
            {
            case NumberType._byte:
            case NumberType._sbyte:
            case NumberType._short:
            case NumberType._ushort:
            case NumberType._int:
            case NumberType._uint:
            case NumberType._long:
            case NumberType._ulong:
                return n._ulong;
            }
            throw new InvalidCastException();
        }

        public static explicit operator ulong? (Number n)
        {
            switch (n._type)
            {
            case NumberType._null: return null;
            case NumberType._byte:
            case NumberType._sbyte:
            case NumberType._short:
            case NumberType._ushort:
            case NumberType._int:
            case NumberType._uint:
            case NumberType._long:
            case NumberType._ulong:
                return n._ulong;
            }
            throw new InvalidCastException();
        }

        public static explicit operator float (Number n)
        {
            switch (n._type)
            {
            case NumberType._float: return unchecked((float)n._float);
            case NumberType._double: return unchecked((float)n._double);
            }
            throw new InvalidCastException();
        }

        public static explicit operator float? (Number n)
        {
            switch (n._type)
            {
            case NumberType._null: return null;
            case NumberType._float: return unchecked((float)n._float);
            case NumberType._double: return unchecked((float)n._double);
            }
            throw new InvalidCastException();
        }

        public static explicit operator double (Number n)
        {
            switch (n._type)
            {
            case NumberType._float: return unchecked((double)n._float);
            case NumberType._double: return unchecked((double)n._double);
            }
            throw new InvalidCastException();
        }

        public static explicit operator double? (Number n)
        {
            switch (n._type)
            {
            case NumberType._null: return null;
            case NumberType._float: return unchecked((double)n._float);
            case NumberType._double: return unchecked((double)n._double);
            }
            throw new InvalidCastException();
        }


        public static explicit operator bool (Number n)
        {
            if (n._type != NumberType._bool)
                throw new InvalidCastException();
            return n._bool;
        }

        public static explicit operator bool? (Number n)
        {
            if (n._type == NumberType._null) return null;

            if (n._type != NumberType._bool)
                throw new InvalidCastException();
            return n._bool;
        }

        public Number(DateTime b) : this() { _long = b.Ticks; _type = NumberType._DateTime; }
        public static explicit operator Number(DateTime n) => new Number(n);
        public static explicit operator DateTime(Number n)
            => n._type == NumberType._DateTime ? new DateTime(n._long) : throw new InvalidCastException();
        public static explicit operator DateTime?(Number n)
            => n._type == NumberType._null ? default(DateTime?)
                : n._type == NumberType._DateTime ? new DateTime(n._long)
                : throw new InvalidCastException();

        public Number(DateTimeOffset b) : this() { _long = b.Ticks; _offset = (short)b.Offset.TotalMinutes; _type = NumberType._DateTimeOffset; }
        public static explicit operator Number(DateTimeOffset n) => new Number(n);
        public static explicit operator DateTimeOffset(Number n)
            => n._type == NumberType._DateTimeOffset ? new DateTimeOffset(n._long, TimeSpan.FromMinutes(n._offset)) : throw new InvalidCastException();
        public static explicit operator DateTimeOffset?(Number n)
            => n._type == NumberType._null ? default(DateTimeOffset?)
                : n._type == NumberType._DateTimeOffset ? new DateTimeOffset(n._long, TimeSpan.FromMinutes(n._offset))
                : throw new InvalidCastException();

        public override string ToString()
        {
            switch (_type)
            {
            default:
            case NumberType._null: return "";
            case NumberType._bool: return _bool.ToString();
            case NumberType._byte: return _byte.ToString();
            case NumberType._sbyte: return _sbyte.ToString();
            case NumberType._short: return _short.ToString();
            case NumberType._ushort: return _ushort.ToString();
            case NumberType._int: return _int.ToString();
            case NumberType._uint: return _uint.ToString();
            case NumberType._long: return _long.ToString();
            case NumberType._ulong: return _ulong.ToString();
            case NumberType._float: return _float.ToString();
            case NumberType._double: return _double.ToString();
            case NumberType._DateTime: return ((DateTime)this).ToString();
            case NumberType._DateTimeOffset: return ((DateTimeOffset)this).ToString();
            }
        }

        public NumberType Type => _type;

        public bool IsNull => _type == NumberType._null;
    }
}
