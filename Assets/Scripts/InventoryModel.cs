using System;
using System.Collections.Generic;

namespace Systems.Inventory
{
    public class InventoryModel
    {
        // 只读属性：对外暴露底层的可观察数组，供外界（如 ViewModel 或 Controller）读取物品数据
        public ObservableArray<Item> Items { get; set; }
        // 缓存当前的存档数据对象，作为金币和物品数组的底层存储
        private InventoryData inventoryData = new InventoryData();
        private int capacity; // 记录背包的最大容量

        // 金币属性的封装：将金币数据直接映射到 inventoryData 的 Coins 字段上
        public int Coins
        {
            get => inventoryData.Coins;
            set => inventoryData.Coins = value;
        }

        // 事件转发器：将底层 ObservableArray 的数据变化事件，优雅地转发给外界（比如 Controller）
        public event Action<Item[]> OnModelChanged
        {
            add => Items.AnyValueChanged += value;   // 当外界订阅 OnModelChanged 时，实际是订阅了 Items 的变化
            remove => Items.AnyValueChanged -= value; // 当外界取消订阅时，同步取消对 Items 的订阅
        }

        // 构造函数：接收初始物品列表和背包容量，完成数据仓库的初始化
        public InventoryModel(IEnumerable<ItemDetails> itemDetails, int capacity)
        {
            this.capacity = capacity;
            // 根据指定的容量，实例化底层的可观察数组
            Items = new ObservableArray<Item>(capacity);
            // 遍历开局预设的物品配置，将它们转化为真实的物品实例并放入背包
            foreach (var itemDetail in itemDetails)
            {
                // itemDetail.Create(1) 表示根据物品配置创建 1 个该物品
                Items.TryAdd(itemDetail.Create(1));
            }
        }

        // 接入存档系统的核心方法：将外部传入的存档数据与当前 Model 进行绑定和同步
        public void Bind(InventoryData data)
        {
            inventoryData = data; // 替换当前的底层数据源
            inventoryData.Capacity = capacity; // 确保存档中的容量与当前设定一致

            // 判断是否为全新的空存档（Items 为 null 或长度为 0）
            bool isNew = inventoryData.Items == null || inventoryData.Items.Length == 0;

            if (isNew)
            {
                // 如果是新存档，初始化一个空的物品数组
                inventoryData.Items = new Item[capacity];
            }

            // 如果是新存档，且当前内存中已经有初始物品（开局赠送的物品），则将它们同步写入存档数据中
            if (isNew && Items.Count != 0)
            {
                for (int i = 0; i < capacity; i++)
                {
                    if (Items[i] == null) continue;
                    inventoryData.Items[i] = Items[i];
                }
            }
            
            // 核心步骤：将 Model 的底层数组指向存档数据中的数组，实现数据与存档的实时联动
            Items.items = inventoryData.Items;
        }
        
        // 增加金币的底层方法（供 Controller 调用）
        public void AddCoins(int amount) => Coins += amount;

        // 获取指定索引位置的物品
        public Item Get(int index) => Items[index];

        // 清空整个背包数据
        public void Clear() => Items.Clear();

        // 向背包中添加一个新物品
        public bool Add(Item item) => Items.TryAdd(item);

        // 从背包中移除一个指定的物品
        public bool Remove(Item item) => Items.TryRemove(item);

        // 交换两个索引位置的物品（背包拖拽换位的核心底层逻辑）
        public void Swap(int source, int target) => Items.Swap(source, target);

        // 合并两个格子的物品（比如将两堆药水合并成一堆）
        public int Combine(int source, int target)
        {
            // 计算两个格子物品的总数量
            var total = Items[source].quantity + Items[target].quantity;
            // 将总数量全部赋予目标格子
            Items[target].quantity = total;
            // 移除源格子的物品（因为已经全部合并到目标格子了）
            Remove(Items[source]);
            return total; // 返回合并后的总数量
        }
    }
}