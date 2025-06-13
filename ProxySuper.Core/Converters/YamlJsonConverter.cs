using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ProxySuper.Core.Converters
{
    public class YamlJsonConverter
    {
        private readonly IDeserializer _yamlDeserializer;
        private readonly ISerializer _yamlSerializer;

        public YamlJsonConverter()
        {
            _yamlDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            _yamlSerializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .DisableAliases()  
                .WithIndentedSequences() 
                .Build();
        }

        /// <summary>
        /// YAML 字符串转 JSON 字符串
        /// </summary>
        public string YamlToJson(string yaml, bool indentJson = true)
        {
            var obj = _yamlDeserializer.Deserialize<object>(yaml);
            var formatting = indentJson ? Formatting.Indented : Formatting.None;
            return JsonConvert.SerializeObject(obj, formatting);
        }

        /// <summary>
        /// JSON 字符串转 YAML 字符串
        /// </summary>
        public string JsonToYaml(string json)
        {
            var obj = JsonConvert.DeserializeObject<object>(json);
            return _yamlSerializer.Serialize(obj);
        }
    }

    public class JsonYamlConverters
    {
        /// <summary>
        /// 将JSON字符串转换为YAML字符串。
        /// </summary>
        /// <param name="jsonString">要转换的JSON字符串。</param>
        /// <returns>转换后的YAML字符串。</returns>
        /// <exception cref="ArgumentNullException">如果jsonString为空或null。</exception>
        /// <exception cref="JsonSerializationException">如果JSON字符串无效。</exception>
        public static string JsonToYaml(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                throw new ArgumentNullException(nameof(jsonString), "JSON字符串不能为空或null。");
            }

            // 使用自定义转换器将JSON反序列化为ExpandoObject
            // ExpandoObject 实现了 IDictionary<string, object>，这通常更受 YamlDotNet 欢迎
            var settings = new JsonSerializerSettings
            {
                Converters = { new ExpandoObjectConverter() }
            };
            ExpandoObject expandoObject = JsonConvert.DeserializeObject<ExpandoObject>(jsonString, settings);


            // 创建一个序列化器将对象转换为YAML
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance) // 可选：设置命名约定
                .Build();

            // 将对象序列化为YAML字符串
            return serializer.Serialize(expandoObject);
        }

        /// <summary>
        /// 将YAML字符串转换为JSON字符串。
        /// </summary>
        /// <param name="yamlString">要转换的YAML字符串。</param>
        /// <returns>转换后的JSON字符串。</returns>
        /// <exception cref="ArgumentNullException">如果yamlString为空或null。</exception>
        /// <exception cref="YamlException">如果YAML字符串无效。</exception>
        public static string YamlToJson(string yamlString)
        {
            if (string.IsNullOrWhiteSpace(yamlString))
            {
                throw new ArgumentNullException(nameof(yamlString), "YAML字符串不能为空或null。");
            }

            // 创建一个反序列化器将YAML转换为对象
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance) // 可选：设置命名约定
                .Build();

            // 将YAML字符串反序列化为动态对象或JObject
            // 使用object可以适应各种YAML结构
            object yamlObject = deserializer.Deserialize<object>(new StringReader(yamlString));

            // 将对象序列化为美化（格式化）的JSON字符串
            return JsonConvert.SerializeObject(yamlObject, Formatting.Indented);
        }
    }

    public class ExpandoObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ExpandoObject);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ReadValue(reader);
        }

        private object ReadValue(JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    var expando = new ExpandoObject() as IDictionary<string, object>;
                    while (reader.Read())
                    {
                        switch (reader.TokenType)
                        {
                            case JsonToken.PropertyName:
                                var propertyName = reader.Value.ToString();
                                reader.Read();
                                expando[propertyName] = ReadValue(reader);
                                break;
                            case JsonToken.EndObject:
                                return expando;
                        }
                    }
                    break;
                case JsonToken.StartArray:
                    var list = new List<object>();
                    while (reader.Read())
                    {
                        switch (reader.TokenType)
                        {
                            case JsonToken.EndArray:
                                return list;
                            case JsonToken.Comment: // Skip comments
                                break;
                            default:
                                list.Add(ReadValue(reader));
                                break;
                        }
                    }
                    break;
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Boolean:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    return reader.Value;
                case JsonToken.Null:
                    return null;
            }
            return null; // Should not happen with valid JSON
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // ExpandoObject 实际上会由 JsonSerializer 自行处理，
            // 但为了完整性，我们也可以在此处实现更严格的转换
            if (value is IDictionary<string, object> dictionary)
            {
                writer.WriteStartObject();
                foreach (var kvp in dictionary)
                {
                    writer.WritePropertyName(kvp.Key);
                    serializer.Serialize(writer, kvp.Value);
                }
                writer.WriteEndObject();
            }
            else if (value is IEnumerable<object> enumerable)
            {
                writer.WriteStartArray();
                foreach (var item in enumerable)
                {
                    serializer.Serialize(writer, item);
                }
                writer.WriteEndArray();
            }
            else
            {
                writer.WriteValue(value);
            }
        }
    }



}
