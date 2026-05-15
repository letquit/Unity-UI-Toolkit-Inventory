using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Systems.Inventory
{
    // 继承 VisualElement，让 Slot 成为一个可以被 UIElements 系统识别和渲染的自定义 UI 控件
    public class Slot : VisualElement
    {
        public Image Icon; // 用于显示物品图标的 Image 控件

        public Label StackLabel; // 用于显示物品堆叠数量的 Label 控件

        // 动态计算当前格子在父容器（即背包格子容器）中的索引位置
        public int Index => parent.IndexOf(this);

        // 只读属性：对外暴露当前格子里存放的物品唯一 ID（默认为空 Guid）
        public SerializableGuid ItemId { get; private set; } = SerializableGuid.Empty;
        public Sprite BaseSprite; // 缓存当前格子的物品图标资源

        // 核心事件：当在该格子上按下鼠标准备拖拽时触发，向外传递鼠标位置和当前格子实例
        public event Action<Vector2, Slot> OnStartDrag = delegate { };

        public Slot()
        {
            // 构造函数：在 Slot 被实例化时，自动构建其内部的 UI 层级结构
            // 创建一个名为 "slotIcon" 的 Image 控件并赋值给 Icon
            Icon = this.CreateChild<Image>("slotIcon");
            // 链式调用：先创建 "slotFrame"，再在其内部创建名为 "stackCount" 的 Label 控件
            StackLabel = this.CreateChild("slotFrame").CreateChild<Label>("stackCount");
            // 注册 PointerDown 事件监听，用于捕获鼠标的按下操作
            RegisterCallback<PointerDownEvent>(OnPointerDown);
        }

        // 处理鼠标按下事件（拖拽交互的起点）
        void OnPointerDown(PointerDownEvent evt)
        {
            // 如果不是鼠标左键（button != 0）或者当前格子是空的（没有物品），则直接返回，不触发拖拽
            if (evt.button != 0 || ItemId.Equals(SerializableGuid.Empty)) return;

            // 触发 OnStartDrag 事件，将当前鼠标的全局坐标（evt.position）和当前格子（this）传递出去
            OnStartDrag.Invoke(evt.position, this);
            evt.StopPropagation(); // 阻止事件继续向下传递，防止触发其他 UI 元素的点击逻辑
        }

        // 设置格子的状态（由 InventoryView 或 InventoryController 调用）
        public void Set(SerializableGuid id, Sprite icon, int qty = 0)
        {
            ItemId = id; // 更新当前格子的物品 ID
            BaseSprite = icon; // 缓存图标

            // 将 Sprite 的 texture 赋值给 Image 控件进行显示
            Icon.image = BaseSprite != null ? icon.texture : null;

            // 如果物品数量大于 1，则显示数量文本；否则隐藏文本（保持界面整洁）
            StackLabel.text = qty > 1 ? qty.ToString() : string.Empty;
            StackLabel.visible = qty > 1;
        }

        // 清空当前格子（通常在物品被移除或拖走后调用）
        public void Remove()
        {
            ItemId = SerializableGuid.Empty; // 重置物品 ID
            Icon.image = null; // 清除图标显示
            // （注：这里建议补充 StackLabel 的隐藏逻辑，防止清空后数量文本残留）
        }
    }
}