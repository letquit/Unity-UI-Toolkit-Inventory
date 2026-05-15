using System;

namespace Systems.Inventory
{
    // 标记为可序列化，使其能够被 Unity 的序列化系统识别（方便后续存档或网络同步）
    [Serializable]
    public class Item
    {
        // 物品的唯一身份标识（你之前封装的 SerializableGuid）
        public SerializableGuid Id;

        // 物品的详细配置信息（指向 ItemDetails 资源，包含图标、名称、最大堆叠数等静态属性）
        public ItemDetails details;

        // 当前物品的堆叠数量
        public int quantity;

        // 构造函数：在创建物品实例时，必须传入物品配置，并可选择初始数量（默认为 1）
        public Item(ItemDetails details, int quantity = 1)
        {
            // 核心逻辑：每次实例化一个物品，都会自动生成一个全新的全局唯一 ID
            Id = SerializableGuid.NewGuid();
            this.details = details;
            this.quantity = quantity;
        }

        // TODO Serialize and Deserialize
        // （后续可以在此处补充将物品数据转换为字节流或 JSON 的具体方法）
    }
}