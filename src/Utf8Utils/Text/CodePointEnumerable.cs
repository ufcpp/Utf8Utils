using System;
using System.Collections;
using System.Collections.Generic;

namespace Utf8Utils.Text
{
    /// <summary>
    /// コードポイント列挙用の enumerable
    /// </summary>
    public struct CodePointEnumerable : IEnumerable<uint>
    {
        private readonly ArraySegment<byte> _buffer;

        /// <summary></summary>
        /// <param name="buffer">UTF8 が入った byte 列。</param>
        public CodePointEnumerable(ArraySegment<byte> buffer) => _buffer = buffer;

        /// <summary></summary>
        /// <param name="buffer">UTF8 が入った byte 列。</param>
        public CodePointEnumerable(byte[] buffer) => _buffer = new ArraySegment<byte>(buffer);

        /// <summary><see cref="IEnumerable{T}.GetEnumerator"/></summary>
        public CodePointEnumerator GetEnumerator() => new CodePointEnumerator(_buffer);
        IEnumerator<uint> IEnumerable<uint>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}