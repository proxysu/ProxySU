using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ProxySuper.Core.Helpers
{
    /// <summary>
    /// 提供静态方法来合并两个 YAML 字符串。
    /// 它将第二个 YAML 字符串的内容合并到第一个 YAML 字符串中，
    /// 遵循深度合并字典和追加列表的策略。
    /// </summary>
    public static class YamlConfigMerger
    {
        /// <summary>
        /// 合并两个 YAML 字符串。
        /// 当键冲突时，第二个 YAML 字符串的值会覆盖第一个字符串的值。
        /// 对于列表，它会尝试进行追加合并。
        /// </summary>
        /// <param name="yamlString1">第一个 YAML 字符串（作为基础配置）。</param>
        /// <param name="yamlString2">第二个 YAML 字符串（其内容将合并到第一个字符串中）。</param>
        /// <param name="namingConvention">用于 YAML 序列化/反序列化的命名约定。默认为 CamelCaseNamingConvention。</param>
        /// <returns>合并后的 YAML 字符串。</returns>
        public static string MergeYamlStrings(string yamlString1, string yamlString2, INamingConvention namingConvention = null)
        {
            // 默认使用 CamelCaseNamingConvention
            namingConvention = namingConvention ?? CamelCaseNamingConvention.Instance;

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(namingConvention)
                .Build();

            var serializer = new SerializerBuilder()
                .WithNamingConvention(namingConvention)
                .Build();

            // 1. 反序列化两个 YAML 字符串为内部字典表示
            IDictionary<object, object> data1 = DeserializeYaml(yamlString1, deserializer);
            IDictionary<object, object> data2 = DeserializeYaml(yamlString2, deserializer);

            // 2. 合并两个字典
            IDictionary<object, object> mergedData = MergeDictionaries(data1, data2);

            // 3. 序列化合并后的数据回 YAML 字符串
            return serializer.Serialize(mergedData);
        }

        /// <summary>
        /// 私有方法：将 YAML 字符串反序列化为 IDictionary<object, object>。
        /// </summary>
        /// <param name="yamlString">要反序列化的 YAML 字符串。</param>
        /// <param name="deserializer">用于反序列化的 YamlDotNet Deserializer 实例。</param>
        /// <returns>表示 YAML 数据的字典。</returns>
        private static IDictionary<object, object> DeserializeYaml(string yamlString, IDeserializer deserializer)
        {
            if (string.IsNullOrWhiteSpace(yamlString))
            {
                return new Dictionary<object, object>();
            }
            try
            {
                var data = deserializer.Deserialize<IDictionary<object, object>>(yamlString);
                return data ?? new Dictionary<object, object>(); // 如果反序列化结果为 null，则返回空字典
            }
            catch (YamlDotNet.Core.YamlException ex)
            {
                // 可以在此处添加更详细的日志记录
                throw new ArgumentException($"Invalid YAML string provided: {ex.Message}", nameof(yamlString), ex);
            }
        }

        /// <summary>
        /// 内部递归合并字典的方法。
        /// sourceDict 将会被 targetDict 中的内容更新或覆盖。
        /// 对于列表，它会尝试进行追加。
        /// </summary>
        private static IDictionary<object, object> MergeDictionaries(IDictionary<object, object> sourceDict, IDictionary<object, object> targetDict)
        {
            if (sourceDict == null && targetDict == null)
            {
                return new Dictionary<object, object>();
            }
            if (sourceDict == null)
            {
                return new Dictionary<object, object>(targetDict);
            }
            if (targetDict == null)
            {
                return new Dictionary<object, object>(sourceDict);
            }

            var result = new Dictionary<object, object>(sourceDict); // 创建 sourceDict 的一个副本

            foreach (var kvp in targetDict)
            {
                var key = kvp.Key;
                var targetValue = kvp.Value;

                if (result.ContainsKey(key))
                {
                    var sourceValue = result[key];

                    if (sourceValue is IDictionary<object, object> sourceSubDict &&
                        targetValue is IDictionary<object, object> targetSubDict)
                    {
                        // 递归合并嵌套的字典
                        result[key] = MergeDictionaries(sourceSubDict, targetSubDict);
                    }
                    else if (sourceValue is IList<object> sourceList &&
                             targetValue is IList<object> targetList)
                    {
                        // 对于列表，简单地追加。
                        // 实际应用中，你可能需要更复杂的列表合并逻辑（例如按 ID 更新、去重等）。
                        var mergedList = new List<object>(sourceList);
                        mergedList.AddRange(targetList);
                        result[key] = mergedList;
                    }
                    else
                    {
                        // 覆盖非字典/非列表的值
                        result[key] = targetValue;
                    }
                }
                else
                {
                    // targetDict 中有而 sourceDict 中没有的键，直接添加
                    result[key] = targetValue;
                }
            }
            return result;
        }
    }
}
