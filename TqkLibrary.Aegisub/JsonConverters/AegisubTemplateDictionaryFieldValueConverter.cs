using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Aegisub.Models;

namespace TqkLibrary.Aegisub.JsonConverters
{
    public class AegisubTemplateDictionaryFieldValueConverter : JsonConverter<Dictionary<string, AegisubTemplateConfigureFieldValue>>
    {
        public AegisubTemplateDictionaryFieldValueConverter()
        {
        }

        public override Dictionary<string, AegisubTemplateConfigureFieldValue>? ReadJson(
            JsonReader reader,
            Type objectType,
            Dictionary<string, AegisubTemplateConfigureFieldValue>? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
            )
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var jsonObject = JObject.Load(reader);
            var result = existingValue ?? new Dictionary<string, AegisubTemplateConfigureFieldValue>();

            foreach (var property in jsonObject.Properties())
            {
                string key = property.Name;
                JObject fieldObject = (JObject)property.Value;
                Type valueType = GetTypeHelper(key);
                var innerSerializer = new JsonSerializer
                {
                    ContractResolver = serializer.ContractResolver,
                    TypeNameHandling = serializer.TypeNameHandling,
                    Converters = { new ValueTypeConverter(valueType) }
                };
                var fieldValue = fieldObject.ToObject<AegisubTemplateConfigureFieldValue>(innerSerializer);
                if (fieldValue != null)
                {
                    result[key] = fieldValue;
                }
            }

            return result;
        }

        public override void WriteJson(
            JsonWriter writer,
            Dictionary<string, AegisubTemplateConfigureFieldValue>? value,
            JsonSerializer serializer
            )
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }
            else
            {
                writer.WriteStartObject();

                foreach (var kvp in value)
                {
                    writer.WritePropertyName(kvp.Key);
                    Type valueType = GetTypeHelper(kvp.Key);
                    var innerSerializer = new JsonSerializer
                    {
                        Formatting = serializer.Formatting,
                        ContractResolver = serializer.ContractResolver,
                        TypeNameHandling = serializer.TypeNameHandling,
                        Converters = { new ValueTypeConverter(valueType) }
                    };
                    innerSerializer.Serialize(writer, kvp.Value);
                }

                writer.WriteEndObject();
            }
        }


        public static Type GetTypeHelper(string name)
        {
            if (name.EndsWith("Color") || name.EndsWith("Colour"))
            {
                return typeof(System.Drawing.Color);
            }
            if (name.EndsWith("Int"))
            {
                return typeof(int);
            }
            if (name.EndsWith("Float"))
            {
                return typeof(float);
            }
            return typeof(string);
        }
        public static object CreateDefaultTypeHelper(Type type, string defaultValue)
        {
            if (type == typeof(System.Drawing.Color))
            {
                uint abgr = uint.Parse(defaultValue.Length == 6 ? $"FF{defaultValue}" : defaultValue, System.Globalization.NumberStyles.HexNumber);
                System.Drawing.Color color = System.Drawing.Color.FromArgb(
                    (int)(abgr >> 24 & 0xFF),  // Alpha
                    (int)(abgr & 0xFF),          // R
                    (int)(abgr >> 8 & 0xFF),   // G
                    (int)(abgr >> 16 & 0xFF)   // B
                );
                return color;
            }
            if (type == typeof(int))
            {
                return int.Parse(defaultValue);
            }
            //if (type == typeof(byte))
            //{
            //    return byte.Parse(defaultValue);
            //}
            if (type == typeof(float))
            {
                return float.Parse(defaultValue);
            }
            return defaultValue;
        }



        public class ValueTypeConverter : JsonConverter
        {
            private readonly Type _targetType;

            public ValueTypeConverter(Type targetType)
            {
                _targetType = targetType;
            }
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(object) || objectType == typeof(AegisubTemplateConfigureFieldValue);
            }
            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteNull();
                    return;
                }

                if (value.GetType() == _targetType)
                {
                    serializer.Serialize(writer, value);
                }
                else
                {
                    try
                    {
                        object convertedValue = Convert.ChangeType(value, _targetType);
                        serializer.Serialize(writer, convertedValue);
                    }
                    catch (InvalidCastException)
                    {
                        serializer.Serialize(writer, value);
                    }
                    catch (FormatException)
                    {
                        serializer.Serialize(writer, value);
                    }
                }
            }
            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                {
                    return null;
                }

                try
                {
                    JToken token = JToken.Load(reader);
                    return token.ToObject(_targetType, serializer);
                }
                catch (Exception ex)
                {
                    throw new JsonSerializationException($"Không thể deserialize giá trị về kiểu {_targetType.FullName}.", ex);
                }
            }
        }
    }
}
