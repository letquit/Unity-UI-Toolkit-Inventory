using System;
using System.Collections.Generic;
using Systems.Persistence;
using UnityEngine;

namespace Systems.Inventory
{
    // 继承 MonoBehaviour，使其能作为组件挂载到 Unity 的游戏物体（GameObject）上
    // 同时实现 IBind<InventoryData> 接口，正式接入你的自动化存档系统
    public class Inventory : MonoBehaviour, IBind<InventoryData>
    {
        // 序列化字段：在 Inspector 面板中拖拽绑定你写好的 UI 视图（InventoryView）
        [SerializeField] private InventoryView view;

        // 序列化字段：在面板中直观地设置背包的最大容量（默认 20）
        [SerializeField] private int capacity = 20;

        // 序列化字段：允许在面板中预设游戏开局时自带的物品列表
        [SerializeField] private List<ItemDetails> startingItems = new List<ItemDetails>();
        
        // 使用 [field: SerializeField] 让自动实现的 Id 属性也能被 Unity 序列化
        // 默认为每个背包生成一个全局唯一的 GUID，供存档系统精准识别
        [field: SerializeField] public SerializableGuid Id { get; set; } = SerializableGuid.NewGuid();

        // 核心控制器：负责处理背包所有的增删改查业务逻辑
        private InventoryController controller;

        // Awake 是 Unity 的生命周期方法，在脚本实例被加载时调用（早于 Start）
        private void Awake()
        {
            // 使用建造者模式（Builder Pattern）优雅地组装并创建背包控制器
            controller = new InventoryController.Builder(view)
                .WithStartingItems(startingItems) // 链式调用：传入开局物品
                .WithCapacity(capacity) // 链式调用：传入背包容量
                .Build(); // 最终构建出配置好的 InventoryController 实例
        }
        
        // 实现 IBind 接口的核心方法：存档系统加载数据后，会调用此方法将数据注入背包
        public void Bind(InventoryData data) => controller.Bind(data);

        // Update 是 Unity 的帧更新方法，每渲染一帧都会调用一次
        private void Update()
        {
            // 每帧调用一次增加金币的方法（用于测试金币增加逻辑是否正常）
            controller.AddCoins(1);
        }
    }
}