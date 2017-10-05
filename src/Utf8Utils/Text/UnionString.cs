using System;

namespace Utf8Utils.Text
{
    /// <summary>
    /// string か<see cref="Utf8ArraySegment"/>かを格納している値。
    /// </summary>
    /// <remarks>
    /// パフォーマンスを考えると string 用と <see cref="Utf8ArraySegment"/> 用のオーバーロードをそれぞれ用意すべきなんだけど。
    /// オーバーロードが掛け算的に増えちゃって収集つかないので妥協したのがこの型。
    /// </remarks>
    public struct UnionString
    {
        /// <summary>
        /// string か byte[] を入れる。
        /// これの型を見て string か <see cref="Utf8ArraySegment"/> かを分岐する。
        /// </summary>
        object _obj;
        int _offset;
        int _count;

        /// <summary>
        /// <see cref="Utf8ArraySegment"/>から初期化。
        /// </summary>
        public UnionString(Utf8ArraySegment str) : this(str.Utf8) { }

        /// <summary>
        /// <see cref="Utf8Array"/>から初期化。
        /// </summary>
        public UnionString(Utf8Array str) : this(str.Utf8) { }

        /// <summary>
        /// <see cref="ArraySegment{Byte}"/>から初期化。
        /// </summary>
        public UnionString(ArraySegment<byte> str)
        {
            _obj = str.Array;
            _offset = str.Offset;
            _count = str.Count;
        }

        /// <summary>
        /// string から初期化。
        /// </summary>
        public UnionString(string str)
        {
            _obj = str;
            _offset = 0;
            _count = 0;
        }

        /// <summary>
        /// null かどうか。
        /// </summary>
        public bool IsNull => _obj == null;

        /// <summary>
        /// 中身が string かどうか。
        /// </summary>
        public bool IsString => _obj is string;

        /// <summary>
        /// 中身が<see cref="Utf8ArraySegment"/>かどうか。
        /// </summary>
        public bool IsUtf8 => _obj is byte[];

        /// <summary>
        /// string を取得。
        /// </summary>
        public string String => (string)_obj;

        /// <summary>
        /// <see cref="Utf8ArraySegment"/>を取得。
        /// </summary>
        public Utf8ArraySegment Utf8 => new Utf8ArraySegment((byte[])_obj, _offset, _count);

        /// <summary>
        /// <see cref="Utf8Array"/>を取得。
        /// </summary>
        public Utf8Array Utf8Array
        {
            get
            {
                var array = (byte[])_obj;
                if (_offset != 0 || _count != array.Length) throw new InvalidCastException();
                return new Utf8Array(array);
            }
        }

        /// <summary>
        /// string からのキャスト。
        /// </summary>
        public static implicit operator UnionString(string s) => new UnionString(s);

        /// <summary>
        /// <see cref="Utf8ArraySegment"/>からのキャスト。
        /// </summary>
        public static implicit operator UnionString(Utf8ArraySegment s) => new UnionString(s);
    }
}
