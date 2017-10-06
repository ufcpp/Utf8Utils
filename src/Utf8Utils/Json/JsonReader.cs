// Writer の方と違って、こっちはかなり corefxlab の JsonReader と同じ実装。
// 数値の判定とかはもうちょっとさぼりたい。
// int と float は JsonValueType のレベルで区別したい。

#pragma warning disable 1591

using Utf8Utils.Text;
using System;

namespace Utf8Utils.Json
{
    public enum JsonTokenType
    {
        None,
        StartObject,
        EndObject,
        StartArray,
        EndArray,
        Key,
        Comment,
        Value,
        Null,
        Undefined,
    };

    public enum JsonValueType
    {
        Unknown,
        Object,
        Array,
        Number,
        String,
        True,
        False,
        Null,
        Undefined,
        NaN,
        Infinity,
        NegativeInfinity,
    }

    public class JsonReader
    {
        Utf8ArraySegment _str;
        private int _index;
        private int _insideObject;
        private int _insideArray;
        public JsonTokenType TokenType;
        private bool _jsonStartIsObject;

        public JsonReader(Utf8ArraySegment str)
        {
            _str = str.TrimStart();
            _index = 0;
            _insideObject = 0;
            _insideArray = 0;
            TokenType = 0;
            _jsonStartIsObject = (byte)_str[0] == '{';
        }

        public JsonReader(string str)
        {
            _str = new Utf8ArraySegment(str).TrimStart();
            _index = 0;
            _insideObject = 0;
            _insideArray = 0;
            TokenType = 0;
            _jsonStartIsObject = (byte)_str[0] == '{';
        }

        public bool Read()
        {
            var canRead = _index < _str.Length;
            if (canRead) MoveToNextTokenType();
            return canRead;
        }

        public Utf8ArraySegment GetKey()
        {
            SkipEmpty();
            var str = ReadStringValue();
            _index++;
            return str;
        }

        /// <summary>
        /// 空白文字かどうかを判定。
        /// JSON 的には Space, Horizontal Tab, Line Feed, Carriage Return のみ。
        /// </summary>
        /// <param name="ascii"></param>
        /// <returns></returns>
        public static bool IsWhitespace(byte c)
        {
            return c == 0x20 || c == 0x09 || c == 0x0A || c == 0x0D;
        }

        public JsonValueType GetJsonValueType()
        {
            var nextByte = (byte)_str[_index];

            while (IsWhitespace(nextByte))
            {
                _index++;
                nextByte = (byte)_str[_index];
            }

            if (nextByte == '"')
            {
                return JsonValueType.String;
            }

            if (nextByte == '{')
            {
                return JsonValueType.Object;
            }

            if (nextByte == '[')
            {
                return JsonValueType.Array;
            }

            if (nextByte == 't')
            {
                return JsonValueType.True;
            }

            if (nextByte == 'f')
            {
                return JsonValueType.False;
            }

            if (nextByte == 'n')
            {
                return JsonValueType.Null;
            }

            if (nextByte == '-' || (nextByte >= '0' && nextByte <= '9'))
            {
                return JsonValueType.Number;
            }

            throw new FormatException("Invalid json, tried to read char '" + nextByte + "'.");
        }

        public Utf8ArraySegment GetValue()
        {
            var type = GetJsonValueType();
            SkipEmpty();
            switch (type)
            {
                case JsonValueType.String:
                    return ReadStringValue();
                case JsonValueType.Number:
                case JsonValueType.True:
                case JsonValueType.False:
                case JsonValueType.Null:
                    return ReadWord();
                case JsonValueType.Object:
                case JsonValueType.Array:
                    return default(Utf8ArraySegment);
                default:
                    throw new ArgumentException("Invalid json value type '" + type + "'.");
            }
        }

        private Utf8ArraySegment ReadStringValue()
        {
            _index++;
            var count = _index;
            do
            {
                while ((byte)_str[count] != '"')
                {
                    count++;
                }
                count++;
            } while (AreNumOfBackSlashesAtEndOfStringOdd(count - 2));

            var strLength = count - _index;
            var resultString = _str.Substring(_index, strLength - 1);
            _index += strLength;

            SkipEmpty();
            return resultString;
        }

        private bool AreNumOfBackSlashesAtEndOfStringOdd(int count)
        {
            var length = count - _index;
            if (length < 0) return false;
            var nextByte = (byte)_str[count];
            if (nextByte != '\\') return false;
            var numOfBackSlashes = 0;
            while (nextByte == '\\')
            {
                numOfBackSlashes++;
                if ((length - numOfBackSlashes) < 0) return numOfBackSlashes % 2 != 0;
                nextByte = (byte)_str[count - numOfBackSlashes];
            }
            return numOfBackSlashes % 2 != 0;
        }

        private static bool IsWordBreak(byte c)
        {
            switch (c)
            {
                case (byte)' ':
                case (byte)'\t':
                case (byte)'\n':
                case (byte)'\r':
                case (byte)'{':
                case (byte)'}':
                case (byte)'[':
                case (byte)']':
                case (byte)',':
                case (byte)':':
                case (byte)'\"':
                    return true;
                default:
                    return false;
            }
        }

        private Utf8ArraySegment ReadWord()
        {
            var len = _str.Length;
            var i = _index;

            while (i < len && !IsWordBreak(_str[i])) ++i;

            var length = i - _index;
            var resultStr = _str.Substring(_index, length);
            _index = i;
            SkipEmpty();
            return resultStr;
        }

        private void SkipEmpty()
        {
            var len = _str.Length;
            var i = _index;
            while (i < len && IsWhitespace(_str[i])) ++i;
            _index = i;
        }

        private void MoveToNextTokenType()
        {
            var nextByte = (byte)_str[_index];
            while (IsWhitespace(nextByte))
            {
                _index++;
                nextByte = (byte)_str[_index];
            }

            switch (TokenType)
            {
                case JsonTokenType.StartObject:
                    if (nextByte != '}')
                    {
                        TokenType = JsonTokenType.Key;
                        return;
                    }
                    break;
                case JsonTokenType.EndObject:
                    if (nextByte == ',')
                    {
                        _index++;
                        if (_insideObject == _insideArray)
                        {
                            TokenType = !_jsonStartIsObject ? JsonTokenType.Key : JsonTokenType.Value;
                            return;
                        }
                        TokenType = _insideObject > _insideArray ? JsonTokenType.Key : JsonTokenType.Value;
                        return;
                    }
                    break;
                case JsonTokenType.StartArray:
                    if (nextByte != ']')
                    {
                        TokenType = JsonTokenType.Value;
                        return;
                    }
                    break;
                case JsonTokenType.EndArray:
                    if (nextByte == ',')
                    {
                        _index++;
                        if (_insideObject == _insideArray)
                        {
                            TokenType = !_jsonStartIsObject ? JsonTokenType.Key : JsonTokenType.Value;
                            return;
                        }
                        TokenType = _insideObject > _insideArray ? JsonTokenType.Key : JsonTokenType.Value;
                        return;
                    }
                    break;
                case JsonTokenType.Key:
                    if (nextByte == ',')
                    {
                        _index++;
                        return;
                    }
                    break;
                case JsonTokenType.Value:
                    if (nextByte == ',')
                    {
                        _index++;
                        return;
                    }
                    break;
            }

            switch (nextByte)
            {
                case (byte)'{':
                    _index++;
                    _insideObject++;
                    TokenType = JsonTokenType.StartObject;
                    return;
                case (byte)'}':
                    _index++;
                    _insideObject--;
                    TokenType = JsonTokenType.EndObject;
                    return;
                case (byte)'[':
                    _index++;
                    _insideArray++;
                    TokenType = JsonTokenType.StartArray;
                    return;
                case (byte)']':
                    _index++;
                    _insideArray--;
                    TokenType = JsonTokenType.EndArray;
                    return;
                case (byte)'0':
                case (byte)'1':
                case (byte)'2':
                case (byte)'3':
                case (byte)'4':
                case (byte)'5':
                case (byte)'6':
                case (byte)'7':
                case (byte)'8':
                case (byte)'9':
                case (byte)'\"':
                case (byte)'t': // true
                case (byte)'f': // false
                case (byte)'n': // null
                    TokenType = JsonTokenType.Value;
                    break;

                default:
                    throw new FormatException("Unable to get next token type. Check json format.");
            }
        }
    }
}
