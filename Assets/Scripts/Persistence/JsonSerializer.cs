using UnityEngine;

namespace Systems.Persistence
{
    // 继承 ISerializer 接口，提供基于 Unity 原生 JsonUtility 的具体序列化实现
    public class JsonSerializer : ISerializer
    {
        // 序列化方法：将任意类型的对象（T obj）转换为格式化的 JSON 字符串
        public string Serialize<T>(T obj)
        {
            // 第二个参数 true 表示生成带有缩进和换行的“美化版” JSON，方便开发阶段人工排查存档文件
            return JsonUtility.ToJson(obj, true);
        }

        // 反序列化方法：将 JSON 字符串还原为指定类型的对象（T）
        public T Deserialize<T>(string json)
        {
            // 将字符串解析并映射回 C# 对象实例
            return JsonUtility.FromJson<T>(json);
        }
    }
}