using System; // 引入 System 命名空间，因为要用到原生的 Guid 和 BitConverter

namespace Systems.Inventory
{
    public static class GuidExtensions
    {
        // 扩展方法 1：将 C# 原生的 Guid 转换成你自定义的可序列化 Guid
        // systemGuid 是被转换的原生 GUID 对象
        public static SerializableGuid ToSerializableGuid(this Guid systemGuid)
        {
            // 1. 将原生的 128 位 GUID 拆解成一个包含 16 个字节的数组
            byte[] bytes = systemGuid.ToByteArray();

            // 2. 将这 16 个字节按顺序切成 4 块（每块 4 字节），分别转换成 uint
            // 并传入 SerializableGuid 的构造函数中
            return new SerializableGuid(
                BitConverter.ToUInt32(bytes, 0), // 读取第 0-3 字节
                BitConverter.ToUInt32(bytes, 4), // 读取第 4-7 字节
                BitConverter.ToUInt32(bytes, 8), // 读取第 8-11 字节
                BitConverter.ToUInt32(bytes, 12) // 读取第 12-15 字节
            );
        }

        // 扩展方法 2：将你自定义的 SerializableGuid 还原成 C# 原生的 Guid
        // serializableGuid 是你自定义的、包含 4 个 int 部分的对象
        public static Guid ToSystemGuid(this SerializableGuid serializableGuid)
        {
            // 1. 准备一个长度为 16 的空字节数组（因为 1 个 Guid 正好是 16 字节）
            byte[] bytes = new byte[16];

            // 2. 使用 Buffer.BlockCopy 进行高效的内存拷贝
            // 将 4 个部分（Part1 到 Part4）分别转换成字节，并精准地填入字节数组的对应位置
            Buffer.BlockCopy(BitConverter.GetBytes(serializableGuid.Part1), 0, bytes, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(serializableGuid.Part2), 0, bytes, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(serializableGuid.Part3), 0, bytes, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(serializableGuid.Part4), 0, bytes, 12, 4);

            // 3. 调用原生 Guid 的构造函数，用拼接好的 16 字节数组还原出标准 Guid
            return new Guid(bytes);
        }
    }
}