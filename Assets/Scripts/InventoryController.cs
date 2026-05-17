using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.Inventory {
    // 新增的 ViewModel 类：作为 View（视图）和 Model（数据）之间的“翻译官”和“中间人”
    public class ViewModel
    {
        public readonly int Capacity; // 暴露背包容量给 View 层
        // 暴露一个可绑定的金币字符串属性。View 层只需要监听它，它一变，UI 就自动更新
        public readonly BindableProperty<string> Coins;

        public ViewModel(InventoryModel model, int capacity)
        {
            Capacity = capacity;
            // 核心逻辑：将 ViewModel 的 Coins 属性，与 Model 层的 model.Coins 数据进行绑定
            // 只要 model.Coins 发生变化，这个 BindableProperty 就会自动通知 View 刷新
            Coins = BindableProperty<string>.Bind(() => model.Coins.ToString());
        }
    }

    public class InventoryController {
        // 只读字段：确保在控制器创建后，视图、模型和容量不能被外部篡改
        private readonly InventoryView view;
        private readonly InventoryModel model;
        private readonly int capacity;

        // 私有构造函数：强制外界必须通过 Builder 来创建控制器实例
        private InventoryController(InventoryView view, InventoryModel model, int capacity) {
            // 使用断言进行防御性编程，确保传入的核心组件不为空且参数合法
            Debug.Assert(view != null, "View is null");
            Debug.Assert(model != null, "Model is null");
            Debug.Assert(capacity > 0, "Capacity is less than 1");
            this.view = view;
            this.model = model;
            this.capacity = capacity;
            
            // 启动一个协程来异步初始化系统（等待视图准备就绪）
            view.StartCoroutine(Initialize());
        }

        // 接入存档系统的核心方法：将存档数据注入底层的 Model
        public void Bind(InventoryData data) => model.Bind(data);
        
        // 对外暴露的增加金币的公共方法（业务逻辑入口）
        public void AddCoins(int amount) => model.Coins += amount;
        
        // 初始化协程：负责绑定事件和首次刷新界面
        private IEnumerator Initialize() {
            // 架构升级点：将新创建的 ViewModel 实例传递给 View 进行初始化
            yield return view.InitializeView(new ViewModel(model, capacity));
            yield return null; // 等待一帧，确保 UI 元素已经生成完毕

            // 核心逻辑：订阅 View 和 Model 的事件，实现双向绑定
            view.OnDrop += HandleDrop; // 监听 UI 的拖拽掉落事件
            model.OnModelChanged += HandleModelChanged; // 监听底层数据的变化事件
            
            RefreshView(); // 初始化完成后，立刻根据模型数据刷新一次视图
        }

        // 处理 UI 上的物品拖拽掉落逻辑（背包系统的核心交互）
        private void HandleDrop(Slot originalSlot, Slot closestSlot) {
            // 情况1：拖拽到了一个空的格子上 -> 直接交换位置（即把物品移动过去）
            if (closestSlot.ItemId.Equals(SerializableGuid.Empty)) {
                model.Swap(originalSlot.Index, closestSlot.Index);
                return;
            }
        
            // TODO world drops (拖出背包丢到游戏世界)
            // TODO Cross Inventory drops (跨背包拖拽，比如从背包拖到快捷栏)
            // TODO Hotbar drops (快捷栏拖拽)

            // 情况2：拖拽到了一个非空的格子上
            // 获取源格子和目标格子的物品 ID
            var sourceItemId = model.Items[originalSlot.Index].details.Id;
            var targetItemId = model.Items[closestSlot.Index].details.Id;
            
            // 情况2.1：如果两个物品 ID 相同，且目标物品支持堆叠（maxStack > 1） -> 执行合并逻辑
            if (sourceItemId.Equals(targetItemId) && model.Items[closestSlot.Index].details.maxStack > 1) { 
                // TODO improve this handling max stack, consider options (后续可优化堆叠数量的计算逻辑)
                model.Combine(originalSlot.Index, closestSlot.Index);
            } else {
                // 情况2.2：物品不同或不支持堆叠 -> 直接交换两个格子的物品
                model.Swap(originalSlot.Index, closestSlot.Index);
            }
        }

        // 当底层模型数据发生变化时，自动触发视图刷新
        private void HandleModelChanged(IList<Item> items) => RefreshView();
        
        // 遍历模型中的所有数据，同步更新到 UI 视图上
        private void RefreshView() {
            for (int i = 0; i < capacity; i++) {
                var item = model.Get(i);
                if (item == null || item.details == null) {
                    view.Slots[i].Set(SerializableGuid.Empty, null);
                } else {
                    view.Slots[i].Set(item.Id, item.details.Icon, item.quantity);
                }
            }
        }

        #region Builder
        
        // 嵌套的建造者类：用于优雅地组装 InventoryController 所需的依赖
        public class Builder {
            InventoryView view;
            IEnumerable<ItemDetails> itemDetails; // 开局初始物品列表
            int capacity = 20; // 默认容量
            
            public Builder(InventoryView view) {
                this.view = view;
            }

            // 链式调用方法：设置初始物品
            public Builder WithStartingItems(IEnumerable<ItemDetails> itemDetails) {
                this.itemDetails = itemDetails;
                return this;
            }

            // 链式调用方法：设置背包容量
            public Builder WithCapacity(int capacity) {
                this.capacity = capacity;
                return this;
            }

            // 最终构建方法：根据配置创建 Model 和 Controller
            public InventoryController Build() {
                // 根据是否有初始物品，来决定如何实例化 InventoryModel
                InventoryModel model = itemDetails != null 
                    ? new InventoryModel(itemDetails, capacity) 
                    : new InventoryModel(Array.Empty<ItemDetails>(), capacity);

                // 将组装好的 View、Model 和 Capacity 传入私有构造函数，返回完整的控制器
                return new InventoryController(view, model, capacity);
            }
        }
        
        #endregion Builder
    }
}