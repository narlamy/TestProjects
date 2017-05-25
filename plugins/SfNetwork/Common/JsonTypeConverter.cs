using System;

#if NEWTON_JSON
using System.Globalization;
using Newtonsoft.Json;

namespace SF.Common
{
    public abstract class JsonTypeConverter : JsonConverter
    {
        private JsonConverterList mConvertList = null;

        JsonConverterList ConverterList
        {
            set { mConvertList = value; }
            get { return mConvertList; }
        }

        protected JsonTypeConverter(JsonConverterList converterList)
        {
            ConverterList = converterList;
        }

        public bool IsSupportedEnum(Type objectType)
        {
            return objectType.IsEnum;
        }

        public override bool CanConvert(Type objectType)
        {
            bool nullable, isArray;
            var t = GetConvertingType(objectType, out nullable, out isArray);

            return ConverterList.ContainsKey(t) || IsSupportedEnum(t);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                if (reader == null)
                    return null;

                bool isNullable, isArray;
                var elementalType = GetConvertingType(objectType, out isNullable, out isArray);

                if (isNullable)
                {
                    if (reader.TokenType == JsonToken.Null)
                    {
                        var jsonProp = Attribute.GetCustomAttribute(objectType, typeof(JsonPropertyAttribute)) as JsonPropertyAttribute;
                        if (jsonProp == null || jsonProp.NullValueHandling == NullValueHandling.Include)
                            return null;
                    }
                }

                DatConvertor converter;

                if (ConverterList.TryGetValue(elementalType, out converter))
                {
                    if (isArray)
                    {
                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            var values = new System.Collections.ArrayList();

                            while (reader.Read())
                            {
                                if (reader.TokenType == JsonToken.EndArray)
                                {
                                    return values.ToArray(elementalType);
                                }
                                else
                                {
                                    values.Add(converter.Read(reader));
                                }
                            }
                        }
                    }
                    else
                    {
                        return converter.Read(reader);
                    }
                }
                else
                {
                    if (IsSupportedEnum(elementalType))
                    {
                        try
                        {
                            if (isArray)
                            {
                                if (reader.TokenType == JsonToken.StartArray)
                                {
                                    var values = new System.Collections.ArrayList();

                                    while (reader.Read())
                                    {
                                        if (reader.TokenType == JsonToken.EndArray)
                                        {
                                            return values.ToArray(elementalType);
                                        }
                                        else
                                        {
                                            var e = System.Enum.Parse(elementalType, reader.Value.ToString());
                                            values.Add(e);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                return System.Enum.Parse(elementalType, reader.Value.ToString());
                            }
                        }
                        catch
                        {
                            if (isNullable) return null;
                            else
                            {
                                if (isArray) return new object[0];
                                else return 0;
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Dev.Logger.LogError("[TypeConverter] " + e);
            }

            return reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            try
            {
                Type objectType = value.GetType();
                bool nullable, isArray;
                var convertType = GetConvertingType(objectType, out nullable, out isArray);

                if (ConverterList.ContainsKey(convertType) || IsSupportedEnum(convertType))
                {
                    if (isArray)
                    {
                        writer.WriteStartArray();

                        var array = value as Array;
                        foreach (var element in array)
                        {
                            ConverterList.Write(writer, convertType, element);
                        }

                        writer.WriteEndArray();
                    }
                    else
                    {
                        ConverterList.Write(writer, convertType, value);
                    }
                }
            }
            catch (System.Exception e)
            {
                Dev.Logger.LogError(e.ToString());
            }
        }

        private Type GetConvertingType(Type objectType, out bool nullable, out bool isArray)
        {
            nullable = objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>);
            isArray = objectType.IsArray;

            if (nullable)
            {
                return Nullable.GetUnderlyingType(objectType);
            }
            else
            {
                if (isArray)
                {
                    return objectType.GetElementType();
                }
                else
                {
                    return objectType;
                }
            }
        }

        public static DateTime ParseTimeText(string timeText)
        {
            if (string.IsNullOrEmpty(timeText))
                return DateTime.MinValue;

            // [2016-07-07] nalramy
            // ISO 형식으로 된 문자열의 경우 TimeZone offset 값을 참조해서 로컬 시간으로 변경해서 사용합니다.
            // 게임 안에서는 대부분 로컬 시간을 사용합니다. 
            // 서버에서 받은 시간은 ISO-8601 형식에 TimeOffset이 포함되어 있습니다. 간혹 UTC 시간대의 경우 Z 표시가
            // 없는 경우도 있긴합니다.
            //
            // ISO-8601 형식이 아닌 경우 서버에서 받은 데이터는 UTC이고 클라이언트는 로컬 시간입니다.

            try
            {
                var is_ISO_8601 = timeText.Contains("T") || timeText.Contains("Z");

                if (is_ISO_8601)
                    return DateTime.Parse(timeText, null, DateTimeStyles.RoundtripKind | DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault);
                else
                    return DateTime.Parse(timeText, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);
            }
            catch(System.Exception e)
            {
                Dev.Logger.LogException(e);
                return DateTime.Now;
            }
        }

        public static string TypeToStr(object t)
        {
            if (t is Type == false)
                return "";

            Type type = (Type)t;
            var typeName = type.AssemblyQualifiedName;
            var asmNameIndex = typeName.IndexOf(',');
            return asmNameIndex>=0 ? typeName.Remove(typeName.IndexOf(',', asmNameIndex+1)) : typeName;
        }
        
        public static string DateTimeToStr(object k)
        {
            if (k is DateTime == false)
                return "";

            DateTime readTime = (DateTime)k;
            if (readTime == DateTime.MinValue)
                return "";

            // msec는 무시합니다.
            DateTime t = new DateTime(
                readTime.Year, readTime.Month, readTime.Day,
                readTime.Hour, readTime.Minute, readTime.Second, 0,
                readTime.Kind);

            try
            {
                //string FORMAT = "yyyy-mm-ddThh:mm:ssZ";   // ISO-8601
                string FORMAT = "o";

                if (t.Kind == DateTimeKind.Unspecified)
                {
                    t = new DateTime(t.Ticks, DateTimeKind.Local);
                    return t.ToString(FORMAT);
                }
                else if (t.Kind == DateTimeKind.Local)
                {
                    return t.ToString(FORMAT);
                }
                else if (t.Kind == DateTimeKind.Utc)
                {
                    return t.ToLocalTime().ToString(FORMAT);
                }
                else
                {
                    return t.ToString(FORMAT);
                }
            }
            catch(System.Exception e)
            {
                Dev.Logger.LogException(e);
                return "";
            }
        }
    }
}
#endif
namespace SF.Common
{
    public static class JsonTypeUtil
    {
        public static string TypeToString(Type type)
        {
            //return JsonTypeConverter.TypeToStr(type);
            return LitJson.JsonMapper.ToJson(type);
        }
    }
}
