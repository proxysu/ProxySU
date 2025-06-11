using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
}
