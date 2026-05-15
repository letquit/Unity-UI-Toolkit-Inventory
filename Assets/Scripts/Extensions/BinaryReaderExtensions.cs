using System.IO; // 引入 IO 命名空间，因为 BinaryReader 定义在这里

namespace Systems.Inventory
{
    // 这是一个静态工具类，专门用来存放 BinaryReader 的扩展方法
    public static class BinaryReaderExtensions
    {
        // 扩展方法：给 BinaryReader 增加一个名为 Read 的新方法，专门用来读取 SerializableGuid
        // 返回值是还原好的 SerializableGuid 对象
        public static SerializableGuid Read(this BinaryReader reader)
        {
            // 核心逻辑：按照写入时的顺序，连续读取 4 个无符号 32 位整数（uint）
            // 并将它们作为参数，传入 SerializableGuid 的构造函数中，重新组装成一个完整的 GUID
            return new SerializableGuid(
                reader.ReadUInt32(), 
                reader.ReadUInt32(), 
                reader.ReadUInt32(),
                reader.ReadUInt32()
            );
        }
    }
}