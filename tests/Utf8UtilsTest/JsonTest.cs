using Utf8Utils;
using Utf8Utils.Json;
using Utf8Utils.Text;
using Xunit;

namespace Utf8UtilsTest
{
    struct Entry
    {
        public long Id;
        public string Name;
        public byte Code;
        public int Hash;

        public void Serialize(JsonWriter json)
        {
            json.BeginObject();

            json.WriteProperty(nameof(Id), (Number)Id);
            json.WriteProperty(nameof(Name), Name);
            json.WriteProperty(nameof(Code), (Number)Code);
            json.WriteProperty(nameof(Hash), (Number)Hash);

            json.EndObject();
        }

        public void Deserialize(JsonReader json)
        {
            while (json.Read())
            {
                switch (json.TokenType)
                {
                    case JsonTokenType.Key:
                        var name = json.GetKey();

                        if (name.Equals(nameof(Id))) Id = (long)json.GetValue().ParseNumber();
                        else if (name.Equals(nameof(Name))) Name = json.GetValue().ToString();
                        else if (name.Equals(nameof(Code))) Code = (byte)json.GetValue().ParseNumber();
                        else if (name.Equals(nameof(Hash))) Hash = (int)json.GetValue().ParseNumber();
                        break;
                    case JsonTokenType.EndObject:
                        return;
                }
            }
        }
    }

    public class JsonTest
    {
       [Fact]
        public void Int()
        {
            var json = new Utf8ArraySegment("123456789");

            var r = new JsonReader(json);

            var num = ParseInt(r);
            Assert.Equal(123456789, num);
        }

        private static long ParseInt(JsonReader r)
        {
            Assert.True(r.Read());
            Assert.Equal(JsonTokenType.Value, r.TokenType);
            Assert.Equal(JsonValueType.Number, r.GetJsonValueType());

            var v = r.GetValue();
            var num = v.ParseInt();
            return num;
        }

        [Fact]
        public void Float()
        {
            var json = new Utf8ArraySegment("1.23E4");

            var r = new JsonReader(json);

            var num = ParseFloat(r);
            Assert.Equal(1.23E4, num);
        }

        private static double ParseFloat(JsonReader r)
        {
            Assert.True(r.Read());
            Assert.Equal(JsonTokenType.Value, r.TokenType);
            Assert.Equal(JsonValueType.Number, r.GetJsonValueType());

            var v = r.GetValue();
            var num = (double)v.ParseNumber();
            return num;
        }

        [Fact]
        public void String()
        {
            var json = new Utf8ArraySegment("\"abc def\"");

            var r = new JsonReader(json);

            var s = ParseString(r);
            Assert.Equal("abc def", s);
        }

        private static string ParseString(JsonReader r)
        {
            Assert.True(r.Read());
            Assert.Equal(JsonTokenType.Value, r.TokenType);
            Assert.Equal(JsonValueType.String, r.GetJsonValueType());
            var v = r.GetValue().ToString();
            return v;
        }

        [Fact]
        public void Array()
        {
            var json = new Utf8ArraySegment(@"[
    123456789,
    ""abc def"",
    1.23E4,
    {""Id"":123456789123456789,""Name"":""aiueâïùéøoαιυεωあいうえお亜以宇江男🐁🐂🐅"",""Code"":128,""Hash"":123456789}
]");

            var r = new JsonReader(json);

            Assert.True(r.Read());
            Assert.Equal(JsonTokenType.StartArray, r.TokenType);

            var i = ParseInt(r);
            Assert.Equal(123456789, i);

            var s = ParseString(r);
            Assert.Equal("abc def", s);

            var f = ParseFloat(r);
            Assert.Equal(1.23E4, f);

            var e = default(Entry);
            e.Deserialize(r);
            Assert.Equal(123456789123456789L, e.Id);
            Assert.Equal("aiueâïùéøoαιυεωあいうえお亜以宇江男🐁🐂🐅", e.Name);
            Assert.Equal(128, e.Code);
            Assert.Equal(123456789, e.Hash);

            Assert.True(r.Read());
            Assert.Equal(JsonTokenType.EndArray, r.TokenType);
        }

        [Fact]
        public void EntryWriteRead()
        {
            var expectedJson = "{\"Id\":123456789123456789,\"Name\":\"aiueâïùéøoαιυεωあいうえお亜以宇江男🐁🐂🐅\",\"Code\":128,\"Hash\":123456789}";

            var e1 = new Entry
            {
                Id = 123456789123456789,
                Name = "aiueâïùéøoαιυεωあいうえお亜以宇江男🐁🐂🐅",
                Code = 128,
                Hash = 123456789,
            };

            var w = new JsonWriter(1024);
            e1.Serialize(w);

            var json = new Utf8ArraySegment(w.Result);

            Assert.Equal(expectedJson, json.ToString());

            var r = new JsonReader(json);

            var e2 = default(Entry);
            e2.Deserialize(r);

            Assert.Equal(e1.Id, e2.Id);
            Assert.Equal(e1.Name, e2.Name);
            Assert.Equal(e1.Code, e2.Code);
            Assert.Equal(e1.Hash, e2.Hash);
        }
    }
}
