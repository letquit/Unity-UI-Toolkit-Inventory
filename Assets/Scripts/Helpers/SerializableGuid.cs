using System;
using UnityEngine;

namespace Systems.Inventory
{
    // 加上 [Serializable] 特性，让 Unity 的序列化系统能够识别并保存这个结构体
    [Serializable]
    public struct SerializableGuid : IEquatable<SerializableGuid>
    {
        // 将 128 位的 Guid 拆分成 4 个 32 位的 uint（无符号整数）。
        // [SerializeField] 让私有字段能在 Inspector 中显示，[HideInInspector] 将其隐藏（保持面板整洁）。
        // 这样既保证了数据能被 Unity 完美序列化，又不会在编辑器里显得杂乱。
        [SerializeField, HideInInspector] public uint Part1;
        [SerializeField, HideInInspector] public uint Part2;
        [SerializeField, HideInInspector] public uint Part3;
        [SerializeField, HideInInspector] public uint Part4;

        // 提供一个全零的空值，常用于表示“无物品”或“未初始化”状态
        public static SerializableGuid Empty => new(0, 0, 0, 0);

        // 构造函数：手动指定 4 个部分的值
        public SerializableGuid(uint p1, uint p2, uint p3, uint p4)
        {
            Part1 = p1;
            Part2 = p2;
            Part3 = p3;
            Part4 = p4;
        }

        // 生成一个新的全局唯一标识符
        public static SerializableGuid NewGuid()
        {
            return FromGuid(Guid.NewGuid());
        }

        // 将 C# 原生的 System.Guid 转换为我们自定义的 SerializableGuid
        public static SerializableGuid FromGuid(Guid guid)
        {
            var bytes = guid.ToByteArray(); // 获取原生 Guid 的 16 字节数组
            // 将 16 字节拆分为 4 个 uint 并赋值
            return new SerializableGuid(
                BitConverter.ToUInt32(bytes, 0),
                BitConverter.ToUInt32(bytes, 4),
                BitConverter.ToUInt32(bytes, 8),
                BitConverter.ToUInt32(bytes, 12)
            );
        }

        // 将自定义的 SerializableGuid 转换回 C# 原生的 System.Guid
        public Guid ToGuid()
        {
            var bytes = new byte[16];
            Buffer.BlockCopy(BitConverter.GetBytes(Part1), 0, bytes, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Part2), 0, bytes, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Part3), 0, bytes, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Part4), 0, bytes, 12, 4);
            return new Guid(bytes);
        }

        // 重写 ToString，方便在 Debug 调试时直接看到标准的 Guid 字符串格式
        public override string ToString()
        {
            return ToGuid().ToString();
        }

        // 从标准 Guid 字符串（如 "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"）解析出 SerializableGuid
        public static SerializableGuid FromString(string value)
        {
            if (Guid.TryParse(value, out var guid))
            {
                return FromGuid(guid);
            }
            return Empty;
        }

        // 从 32 位的纯十六进制字符串（无横杠）解析出 SerializableGuid
        public static SerializableGuid FromHexString(string hex)
        {
            if (hex.Length != 32) return Empty;
            return new SerializableGuid(
                Convert.ToUInt32(hex.Substring(0, 8), 16),
                Convert.ToUInt32(hex.Substring(8, 8), 16),
                Convert.ToUInt32(hex.Substring(16, 8), 16),
                Convert.ToUInt32(hex.Substring(24, 8), 16)
            );
        }

        // 将当前的 Guid 转换为 32 位的纯十六进制字符串
        public string ToHexString()
        {
            return $"{Part1:X8}{Part2:X8}{Part3:X8}{Part4:X8}";
        }

        // 实现 IEquatable 接口的强类型 Equals 方法，用于高性能的值比较
        public bool Equals(SerializableGuid other)
        {
            return Part1 == other.Part1 && Part2 == other.Part2 &&
                   Part3 == other.Part3 && Part4 == other.Part4;
        }

        // 重写基类的 Equals 方法，确保与其他 object 比较时的逻辑一致
        public override bool Equals(object obj)
        {
            return obj is SerializableGuid other && Equals(other);
        }

        // 重写 GetHashCode，确保在 Dictionary 或 HashSet 中作为键值时的哈希计算正确
        public override int GetHashCode()
        {
            return HashCode.Combine(Part1, Part2, Part3, Part4);
        }

        // 重载 == 和 != 运算符，让代码中可以直接使用 if (guid1 == guid2) 这样的直观写法
        public static bool operator ==(SerializableGuid left, SerializableGuid right) => left.Equals(right);
        public static bool operator !=(SerializableGuid left, SerializableGuid right) => !left.Equals(right);

        // 隐式转换：允许直接将 SerializableGuid 赋值给 string 变量（自动调用 ToString）
        public static implicit operator string(SerializableGuid guid) => guid.ToString();
        // 显式转换：允许通过 (SerializableGuid)"xxx-xxx..." 的强制转换语法将字符串转回 Guid
        public static explicit operator SerializableGuid(string value) => FromString(value);
    }
}