using System;
using Systems.Persistence;
using UnityEngine;

namespace Systems.Inventory
{
    // 加上 [Serializable] 特性，让 Unity 的 JsonUtility 能够识别并序列化这个类
    [Serializable]
    // 实现 ISaveable 接口，正式接入你的自动化存档系统，获得唯一的身份标识
    public class InventoryData : ISaveable
    {
        // 使用 [field: SerializeField] 让自动实现的 Id 属性也能被 Unity 序列化
        // 这个 Id 将由 Inventory 脚本提供，确保存档数据与场景中的具体背包物体精准对应
        [field: SerializeField] public SerializableGuid Id { get; set; }
        
        public Item[] Items;  // 存储背包里所有物品的数组
        public int Capacity;  // 记录背包的最大容量
        public int Coins;     // 记录玩家当前拥有的金币数量
    }
}