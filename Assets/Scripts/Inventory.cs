using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.Inventory
{
    // 继承 MonoBehaviour，使其能作为组件挂载到 Unity 的游戏物体（GameObject）上
    public class Inventory : MonoBehaviour
    {
        // 序列化字段：在 Inspector 面板中拖拽绑定你写好的 UI 视图（InventoryView）
        [SerializeField] private InventoryView view;

        // 序列化字段：在面板中直观地设置背包的最大容量（默认 20）
        [SerializeField] private int capacity = 20;

        // 序列化字段：允许在面板中预设游戏开局时自带的物品列表
        [SerializeField] private List<ItemDetails> startingItems = new List<ItemDetails>();

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

        // Update 是 Unity 的帧更新方法，每渲染一帧都会调用一次
        private void Update()
        {
            // 每帧调用一次增加金币的方法（用于测试金币增加逻辑是否正常）
            controller.AddCoins(1);
        }
    }
}