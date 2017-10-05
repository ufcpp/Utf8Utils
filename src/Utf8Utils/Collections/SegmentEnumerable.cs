using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utf8Utils.Collections
{
    /// <summary>
    /// 配列を一定個数ずつ区切って返す enumerable。
    /// </summary>
    /// <typeparam name="T">配列の要素の型。</typeparam>
    public struct SegmentEnumerable<T> : IEnumerable<ArraySegment<T>>
    {
        private readonly T[] _data;
        private readonly int _windowSize;

        public SegmentEnumerable(IEnumerable<T> seq, int windowSize) : this(seq.ToArray(), windowSize) { }

        public SegmentEnumerable(T[] array, int windowSize)
        {
            if (windowSize <= 0) throw new ArgumentException(nameof(windowSize) + " must be greater than 0");

            _data = array;
            _windowSize = windowSize;
        }

        public Enumerator GetEnumerator() => new Enumerator(_data, _windowSize);
        IEnumerator<ArraySegment<T>> IEnumerable<ArraySegment<T>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<ArraySegment<T>>
        {
            private readonly T[] _data;
            private readonly int _count;

            private int _index;

            public Enumerator(T[] data, int count)
            {
                _data = data;
                _count = count;
                _index = -count; // 初回MoveNext用
            }

            public ArraySegment<T> Current => new ArraySegment<T>(_data, _index, Math.Min(_count, _data.Length - _index));
            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                _index += _count;
                return _index < _data.Length;
            }

            public void Reset() => throw new NotSupportedException();
        }
    }
}
