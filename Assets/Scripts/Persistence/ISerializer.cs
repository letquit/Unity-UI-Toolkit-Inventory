using UnityEngine;

namespace Systems.Persistence
{
    // 序列化器接口：定义了数据“存”与“取”的标准契约
    public interface ISerializer
    {
        // 序列化方法：将任意类型的对象（T obj）转换为字符串（通常是 JSON 格式），用于存档或网络传输
        string Serialize<T>(T obj);
        
        // 反序列化方法：将字符串（json）还原为指定类型的对象（T），用于读档或接收网络数据
        T Deserialize<T>(string json);
    }
}