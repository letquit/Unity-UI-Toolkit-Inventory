using System;
using UnityEngine;

namespace Systems.Inventory
{
    /// <summary>
    /// 表示一个全局唯一标识符 (GUID)，它支持 Unity 序列化，并且可以在游戏脚本中正常使用。
    /// </summary>
    [Serializable] // 关键特性：让 Unity 的序列化系统能够识别并保存这个结构体
    public struct SerializableGuid : IEquatable<SerializableGuid>
    {
        // 将 128 位的 GUID 拆分成 4 个 32 位的无符号整数（uint）
        // SerializeField 确保它们能被 Unity 序列化，HideInInspector 避免在 Inspector 中直接显示这 4 个枯燥的数字
        [SerializeField, HideInInspector] public uint Part1;
        [SerializeField, HideInInspector] public uint Part2;
        [SerializeField, HideInInspector] public uint Part3;
        [SerializeField, HideInInspector] public uint Part4;

        // 提供一个全零的空 GUID 静态实例，类似于 System.Guid.Empty
        public static SerializableGuid Empty => new(0, 0, 0, 0);

        // 构造函数：接收 4 个 uint 并组装成一个完整的 SerializableGuid
        public SerializableGuid(uint val0, uint val1, uint val2, uint val3)
        {
            Part1 = val0;
            Part2 = val1;
            Part3 = val2;
            Part4 = val3;
        }

        // 静态方法：调用 C# 原生的 Guid.NewGuid() 生成一个全新的 GUID，并通过你写的扩展方法转换成 SerializableGuid
        public static SerializableGuid NewGuid() => Guid.NewGuid().ToSerializableGuid();

        // 静态方法：从一个 32 位的十六进制字符串（如 "A1B2C3D4..."）还原出 SerializableGuid
        public static SerializableGuid FromHexString(string hexString)
        {
            // 安全检查：标准的 GUID 字符串长度必须是 32 位
            if (hexString.Length != 32)
            {
                return Empty;
            }

            // 将字符串按每 8 位一段切分，并转换回 uint（16 代表按十六进制解析）
            return new SerializableGuid
            (
                Convert.ToUInt32(hexString.Substring(0, 8), 16),
                Convert.ToUInt32(hexString.Substring(8, 8), 16),
                Convert.ToUInt32(hexString.Substring(16, 8), 16),
                Convert.ToUInt32(hexString.Substring(24, 8), 16)
            );
        }

        // 实例方法：将当前的 SerializableGuid 转换回 32 位的十六进制字符串
        // :X8 格式化符确保每个 uint 都输出为 8 位大写十六进制数，不足的前面补零
        public string ToHexString()
        {
            return $"{Part1:X8}{Part2:X8}{Part3:X8}{Part4:X8}";
        }

        // 重写 object 的 Equals 方法，确保在作为 Dictionary 的 Key 或调用 object.Equals 时能正确比较
        public override bool Equals(object obj)
        {
            return obj is SerializableGuid guid && this.Equals(guid);
        }

        // 实现 IEquatable 接口的强类型 Equals 方法，提供最高效的相等性比较
        public bool Equals(SerializableGuid other)
        {
            // 只有当 4 个部分完全相等时，两个 GUID 才被视为相等
            return Part1 == other.Part1 && Part2 == other.Part2 && Part3 == other.Part3 && Part4 == other.Part4;
        }

        // 重写 GetHashCode，确保在使用 Dictionary 或 HashSet 等哈希集合时，相同的 GUID 能映射到相同的哈希桶
        public override int GetHashCode()
        {
            return HashCode.Combine(Part1, Part2, Part3, Part4);
        }

        // 重载 == 和 != 运算符，让你在代码中可以直接用 if (guid1 == guid2) 这样的语法进行直观比较
        public static bool operator ==(SerializableGuid left, SerializableGuid right) => left.Equals(right);
        public static bool operator !=(SerializableGuid left, SerializableGuid right) => !(left == right);
    }
}