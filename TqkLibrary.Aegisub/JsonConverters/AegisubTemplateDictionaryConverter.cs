using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.Aegisub.JsonConverters
{
    public class AegisubTemplateDictionaryConverter : JsonConverter<Dictionary<string, object>>
    {
        public AegisubTemplateDictionaryConverter()
        {
        }

        public override Dictionary<string, object> ReadJson(
            JsonReader reader,
            Type objectType,
            Dictionary<string, object>? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
            )
        {
            var result = new Dictionary<string, object>();
            var jObject = JObject.Load(reader);

            foreach (var prop in jObject.Properties())
            {
                Type type = GetTypeHelper(prop.Name);
                var value = prop.Value.ToObject(type, serializer)!;
                result[prop.Name] = value;
            }

            return result;
        }

        public override void WriteJson(
            JsonWriter writer,
            Dictionary<string, object>? value,
            JsonSerializer serializer
            )
        {
            if (value is not null)
            {
                writer.WriteStartObject();

                foreach (var kvp in value)
                {
                    writer.WritePropertyName(kvp.Key.ToString());
                    serializer.Serialize(writer, kvp.Value);
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
            //if (name.EndsWith("Int8"))
            //{
            //    return typeof(byte);
            //}
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
    }
}
