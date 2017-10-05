using System;
using System.Collections;
using System.Collections.Generic;

namespace Utf8Utils.Text
{
    /// <summary>
    /// コードポイント列挙用の enumerator
    /// </summary>
    public struct CodePointEnumerator : IEnumerator<uint>
    {
        private readonly ArraySegment<byte> _buffer;
        private int _index;
        private byte _count;

        /// <summary></summary>
        /// <param name="buffer">UTF8 が入った byte 列。</param>
        public CodePointEnumerator(ArraySegment<byte> buffer)
        {
            _buffer = buffer;
            _index = 0;
            _count = 0;
            Current = 0;
        }

        internal int PositionInCodeUnits => _index;

        /// <summary><see cref="IEnumerator{T}.Current"/></summary>
        public uint Current { get; private set; }

        /// <summary><see cref="IEnumerator.MoveNext"/></summary>
        public bool MoveNext()
        {
            _index += _count;
            var count = Utf8Decoder.TryDecode(_buffer, _index, out var cp);
            if (count == Utf8Decoder.InvalidCount) return false;
            _count = count;
            Current = cp;
            return true;
        }

        /// <summary><see cref="IEnumerator.Reset"/></summary>
        public void Reset() { _index = 0; _count = 0; }

        void IDisposable.Dispose() { }
        object IEnumerator.Current => Current;
    }
}