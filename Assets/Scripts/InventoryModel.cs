using System;
using System.Collections.Generic;

namespace Systems.Inventory
{
    public class InventoryModel
    {
        // 只读属性：对外暴露底层的可观察数组，供外界读取物品数据
        public ObservableArray<Item> Items { get; set; }

        // 事件转发器：将底层 ObservableArray 的数据变化事件，优雅地转发给外界（比如 Controller）
        public event Action<Item[]> OnModelChanged
        {
            add => Items.AnyValueChanged += value;
            remove => Items.AnyValueChanged -= value;
        }

        // 构造函数：接收初始物品列表和背包容量，完成数据仓库的初始化
        public InventoryModel(IEnumerable<ItemDetails> itemDetails, int capacity)
        {
            // 根据指定的容量，实例化底层的可观察数组
            Items = new ObservableArray<Item>(capacity);
            // 遍历开局预设的物品配置，将它们转化为真实的物品实例并放入背包
            foreach (var itemDetail in itemDetails)
            {
                // itemDetail.Create(1) 表示根据物品配置创建 1 个该物品
                Items.TryAdd(itemDetail.Create(1));
            }
        }

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