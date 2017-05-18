using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json;

namespace N2.Common
{
    internal struct DatConvertor
    {
        public Func<JsonReader, object> Read { get; set; }
        public Func<JsonToken, object, object> ReadElemental { get; set; }
    }

    /// <summary>
    /// 게임내에서 사용하는 기본 타입 객체를 문자열 혹은 숫자로 변환 또는 복원합니다.
    /// 문자열로 입력받은 값을 enum 타입으로 변환 혹은 복원합니다.
    /// </summary>
    public class JsonConverterList
    {
        Dictionary<Type, DatConvertor> mParsers = new Dictionary<Type, DatConvertor>();
        Dictionary<Type, Func<object, string>> mStrConvertes = new Dictionary<Type, Func<object, string>>();

        public JsonConverterList(Action<JsonConverterList> Init)
        {
            Init(this);
        }        

        public void AddParser<T>(System.Func<long, T> creator, T nullObj)
        {
            mParsers.Add(typeof(T), new DatConvertor()
            {
                Read = (reader) =>
                {
                    return ParseStructure(reader, creator, nullObj);
                },
                ReadElemental = (tokenType, value) =>
                {
                    return ParseStructure(tokenType, value, creator, nullObj);
                }
            });
        }

        public void AddParser<T>(Func<string, T> parser, T nullObj)
        {
            mParsers.Add(typeof(T), new DatConvertor()
            {
                Read = (reader) =>
                {
                    return ParseCustomParser(reader.TokenType, reader.Value, parser, nullObj);
                },
                ReadElemental = (tokenType, value) =>
                {
                    return ParseCustomParser(tokenType, value, parser, nullObj);
                }
            });
        }

        public void AddStrConverter<T>(Func<object, string> func)
        {
            mStrConvertes.Add(typeof(T), func);
        }
        
        public bool ContainsKey(Type type)
        {
            return mParsers.ContainsKey(type);
        }

        internal bool TryGetValue(Type type, out DatConvertor converter)
        {
            return mParsers.TryGetValue(type, out converter);
        }

        public void Write(JsonWriter writer, Type type, object value)
        {
            if (writer == null)
                return;
            try
            {
                Func<object, string> toString = null;

                if (mStrConvertes.TryGetValue(type, out toString))
                {
                    var str = toString(value);
                    writer.WriteValue(str);
                }
                else
                {
                    if (type.IsEnum)
                    {
                        var text = Enum.GetName(type, value);
                        writer.WriteValue(text);
                    }
                    else
                    {
                        writer.WriteValue(value.ToString());
                    }
                }
            }
            catch (System.Exception e)
            {
                Dev.Logger.LogError("[TypeConverter] Write(\"" + value.ToString() + "\") : error => " + e);
            }
        }

        private T ParseCustomParser<T>(JsonReader reader, System.Func<string, T> parser, T nullObj)
        {
            if (reader != null)
                return ParseCustomParser(reader.TokenType, reader.Value, parser, nullObj);
            else
                return nullObj;
        }

        private T ParseCustomParser<T>(JsonToken tokenType, object value, System.Func<string, T> parser, T nullObj)
        {
            try
            {
                if (tokenType == JsonToken.Null || value == null)
                {
                    return nullObj;
                }
                else if(tokenType == JsonToken.Date)
                {
                    // [2017-02-18] narlamy
                    // Date이더라도 value는 문자열로 들어올 줄 알았습니다.
                    // 그래야 Parsing을 할 수 있죠.
                    // 그런데 이미 파싱이 끝난 DateTime 객체로 들어옵니다.

                    if (value is DateTime && typeof(T)== typeof(DateTime))
                        return (T)value;
                }

                var s = value is string ? (string)value : value.ToString();
                if (string.IsNullOrEmpty(s))
                {
                    return nullObj;
                }
                else
                {
                    return parser(s);
                }
            }
            catch (System.Exception e)
            {
                Dev.Logger.LogError("[TypeConverter] Parse(\"" + value.ToString() + "\") : error => " + e);
                return nullObj;
            }
        }

        private object ParseStructure<T>(JsonReader reader, System.Func<long, T> creator, T nullObj)
        {
            return ParseStructure(reader.TokenType, reader.Value, creator, nullObj);
        }

        private object ParseStructure<T>(JsonToken tokenType, object value, System.Func<long, T> creator, T nullObj)
        {
            if (value == null)
                return nullObj;
            try
            {
                switch (tokenType)
                {
                case JsonToken.String:
                    long key = 0;
                    long.TryParse(value.ToString(), out key);
                    return creator(key);

                case JsonToken.Integer:
                    long num = 0;

                    if (value is uint) num = (long)(uint)value;
                    else if (value is int) num = (long)(int)value;
                    else num = Convert.ToInt64(value);
                    return creator(num);

                case JsonToken.Float:
                    if (value is float) num = (long)(float)value;
                    else if (value is double) num = (long)(double)value;
                    else num = Convert.ToInt64(value);
                    return creator(num);

                case JsonToken.Null:
                    return nullObj;

                default:
                    return value;
                }
            }
            catch (System.Exception e)
            {
                Dev.Logger.LogError("[TypeConverter] ParseeValue(\"" + value.ToString() + "\") : error => " + e);
                return nullObj;
            }
        }
    }    
}
