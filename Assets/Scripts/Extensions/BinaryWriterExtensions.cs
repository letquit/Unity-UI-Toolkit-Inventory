using System.IO; // 引入 IO 命名空间，BinaryWriter 定义在这里

namespace Systems.Inventory
{
    // 这是一个静态工具类，专门用来存放 BinaryWriter 的扩展方法
    public static class BinaryWriterExtensions
    {
        // 扩展方法：给 BinaryWriter 增加一个名为 Write 的新方法，专门用来写入 SerializableGuid
        // 参数 guid 就是你要打包写入的那个自定义 GUID 对象
        public static void Write(this BinaryWriter writer, SerializableGuid guid)
        {
            // 核心逻辑：将 guid 拆解成 4 个部分（Part1 到 Part4），并依次写入二进制流中
            // 这里的写入顺序至关重要，必须和读取（反序列化）时的顺序完全一致！
            writer.Write(guid.Part1);
            writer.Write(guid.Part2);
            writer.Write(guid.Part3);
            writer.Write(guid.Part4);
        }
    }
}