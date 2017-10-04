using System;

namespace Utf8Utils.Collections
{
    /// <summary>
    /// <see cref="System.Collections.Generic.List{T}"/> みたいなやつ。
    /// 他のクラスに埋め込んで使う前提で struct 化したもの。
    /// ジェネリックにするとあんまりパフォーマンス的においしくなかったんで、パフォーマンスを必要としている byte は単独で提供。
    /// </summary>
    public struct ResizableArray
    {
        private byte[] _array;
        private int _count;

        /// <summary>
        /// </summary>
        /// <param name="capacity">初期サイズ。</param>
        public ResizableArray(int capacity)
        {
            _array = new byte[capacity];
            _count = 0;
        }

        /// <summary>
        /// </summary>
        /// <param name="array">初期に使う配列。</param>
        /// <param name="count">あらかじめ入っている想定にしたい要素数。</param>
        public ResizableArray(byte[] array, int count = 0)
        {
            _array = array;
            _count = count;
        }

        /// <summary>
        /// 配列を直接読み書き。
        /// </summary>
        public byte[] Items
        {
            get { return _array; }
            set { _array = value; }
        }

        /// <summary>
        /// 要素数を直接読み書き。
        /// </summary>
        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        /// <summary>
        /// 容量。
        /// </summary>
        public int Capacity => _array?.Length ?? 0;

        /// <summary>
        /// 要素の読み書き。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ref byte this[int index] => ref _array[index];

        /// <summary>
        /// 要素を追加。
        /// </summary>
        public void Add(byte item)
        {
            if (_array.Length == _count)
            {
                ResizeCapacity();
            }
            _array[_count++] = item;
        }

        /// <summary>
        /// 複数の要素を追加。
        /// </summary>
        public void AddAll(byte[] items)
        {
            Reserve(items.Length);
            ArraySegmentExtensions.Copy(items, 0, _array, _count, items.Length);
            _count += items.Length;
        }

        /// <summary>
        /// 複数の要素を追加。
        /// </summary>
        public unsafe void AddAll(byte* p, int length)
        {
            Reserve(length);
            ArraySegmentExtensions.Copy(p, length, _array, _count, length);
            _count += length;
        }

        /// <summary>
        /// 事前に<see cref="Capacity"/>を増やす。
        /// </summary>
        /// <param name="length">要素数の増分。</param>
        public void Reserve(int length)
        {
            if (length > _array.Length - _count)
            {
                ResizeCapacity(_array.Length + length);
            }
        }

        /// <summary>
        /// <see cref="Capacity"/>確認せずに要素を追加。
        /// 事前に<see cref="Reserve(int)"/>を呼ぶ想定。
        /// </summary>
        public void UnsafeAdd(byte item)
        {
            _array[_count++] = item;
        }

        /// <summary>
        /// 複数の要素を追加。
        /// </summary>
        public void AddAll(ArraySegment<byte> items)
        {
            if (items.Count > _array.Length - _count)
            {
                ResizeCapacity(items.Count + _count);
            }

            for (int i = 0; i < items.Count; i++) _array[i] = items.Array[items.Offset + i];
            _count += items.Count;
        }

        /// <summary>
        /// 空にする。
        /// </summary>
        public void Clear()
        {
            _count = 0;
        }

        /// <summary>
        /// 要素数を増やす。
        /// </summary>
        /// <param name="size">増分。</param>
        public void Extend(int size)
        {
            Reserve(size);
            _count += size;
        }

        /// <summary>
        /// 容量変更。
        /// </summary>
        /// <param name="newSize">変更後のサイズ。</param>
        /// <returns>容量変更前に使っていた配列。</returns>
        public byte[] ResizeCapacity(int newSize = -1)
        {
            if (newSize == -1)
            {
                // 2倍に拡張。
                newSize = Capacity << 1;
            }
            else
            {
                // あんまり細かくリサイズすると効率が悪いので、最低でも2倍に拡張。
                newSize = Math.Max(Capacity << 1, newSize);
            }

            var oldArray = _array;
            var newArray = new byte[newSize];
            ArraySegmentExtensions.Copy(_array, newArray);
            _array = newArray;
            return oldArray;
        }

        /// <summary>
        /// <see cref="Items"/>のうち、埋まっている部分。
        /// </summary>
        public ArraySegment<byte> Full => new ArraySegment<byte>(_array, 0, _count);

        /// <summary>
        /// <see cref="Items"/>のうち、埋まっていない部分。
        /// </summary>
        public ArraySegment<byte> Free => new ArraySegment<byte>(_array, _count, _array.Length - _count);
    }
}
