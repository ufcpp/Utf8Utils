namespace Utf8Utils.Text
{
    /// <summary>
    /// string か<see cref="Utf8String"/>かを格納している値。
    /// </summary>
    /// <remarks>
    /// パフォーマンスを考えると string 用と <see cref="Utf8String"/> 用のオーバーロードをそれぞれ用意すべきなんだけど。
    /// オーバーロードが掛け算的に増えちゃって収集つかないので妥協したのがこの型。
    /// </remarks>
    public struct UnionString
    {
        /// <summary>
        /// string か byte[] を入れる。
        /// これの型を見て string か <see cref="Utf8String"/> かを分岐する。
        /// </summary>
        object _obj;
        int _offset;
        int _count;

        /// <summary>
        /// <see cref="Utf8String"/>から初期化。
        /// </summary>
        public UnionString(Utf8String str)
        {
            _obj = str.Buffer.Array;
            _offset = str.Buffer.Offset;
            _count = str.Buffer.Count;
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
        /// 中身が<see cref="Utf8String"/>かどうか。
        /// </summary>
        public bool IsUtf8 => _obj is byte[];

        /// <summary>
        /// string を取得。
        /// </summary>
        public string String => (string)_obj;

        /// <summary>
        /// <see cref="Utf8String"/>を取得。
        /// </summary>
        public Utf8String Utf8 => new Utf8String((byte[])_obj, _offset, _count);

        /// <summary>
        /// string からのキャスト。
        /// </summary>
        public static implicit operator UnionString(string s) => new UnionString(s);

        /// <summary>
        /// <see cref="Utf8String"/>からのキャスト。
        /// </summary>
        public static implicit operator UnionString(Utf8String s) => new UnionString(s);
    }
}
