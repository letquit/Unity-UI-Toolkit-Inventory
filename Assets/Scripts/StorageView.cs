using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Systems.Inventory
{
    // 抽象基类，继承自 MonoBehaviour。作为所有仓库/背包界面的 UI 总基类
    public abstract class StorageView : MonoBehaviour
    {
        public Slot[] Slots; // 存放所有物品槽（Slot）的数组

        [SerializeField] protected UIDocument document; // 绑定 Unity 场景中的 UIDocument 组件
        [SerializeField] protected StyleSheet styleSheet; // 绑定背包专属的 USS 样式表

        protected static VisualElement ghostIcon; // 静态的“幽灵图标”，用于在拖拽时跟随鼠标显示物品

        private static bool isDragging; // 标记当前是否正在拖拽物品
        private static Slot originalSlot; // 记录拖拽开始时，鼠标最初按下的那个物品槽

        protected VisualElement root; // UI 的根节点
        protected VisualElement container; // UI 的总容器节点

        // 核心事件：当物品拖拽结束并释放时触发，将“原槽位”和“目标槽位”传递给上层逻辑（如 InventoryController）
        public event Action<Slot, Slot> OnDrop;

        private IEnumerator Start()
        {
            // 调用子类（如 InventoryView）实现的 InitializeView 方法，完成 UI 的动态构建
            yield return StartCoroutine(InitializeView());

            // 为全局的幽灵图标注册鼠标移动和抬起的事件监听
            ghostIcon.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            ghostIcon.RegisterCallback<PointerUpEvent>(OnPointerUp);

            // 遍历所有物品槽，订阅它们内部发出的 OnStartDrag（开始拖拽）事件
            foreach (var slot in Slots)
            {
                slot.OnStartDrag += OnPointerDown;
            }
        }

        // 抽象方法：强制子类（如 InventoryView）必须实现具体的 UI 初始化与层级构建逻辑
        public abstract IEnumerator InitializeView(int size = 20);

        // 处理拖拽开始（由 Slot 的 OnPointerDown 事件触发）
        private static void OnPointerDown(Vector2 position, Slot slot)
        {
            isDragging = true; // 开启拖拽状态
            originalSlot = slot; // 记录拖拽的源头槽位

            SetGhostIconPosition(position); // 将幽灵图标瞬移到鼠标按下位置

            // 将原槽位的物品图标赋值给幽灵图标进行显示
            ghostIcon.style.backgroundImage = originalSlot.BaseSprite.texture;
            // 隐藏原槽位的图标和数量文本，制造出“物品被拿起来”的视觉效果
            originalSlot.Icon.image = null;
            originalSlot.StackLabel.visible = false;

            ghostIcon.style.visibility = Visibility.Visible; // 显示幽灵图标
            // TODO show stack size on ghost icon (后续可在幽灵图标上显示堆叠数量)
        }

        // 处理拖拽过程中的鼠标移动
        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!isDragging) return; // 如果没有在拖拽，直接返回

            SetGhostIconPosition(evt.position); // 让幽灵图标实时跟随鼠标位置移动
        }

        // 处理拖拽结束时的鼠标抬起
        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!isDragging) return;
            // 核心算法：利用 LINQ 找出鼠标下方距离最近的那个有效物品槽
            Slot closestSlot = Slots
                .Where(slot => slot.worldBound.Overlaps(ghostIcon.worldBound)) // 1. 筛选出与幽灵图标发生碰撞重叠的槽位
                .OrderBy(slot =>
                    Vector2.Distance(slot.worldBound.position, ghostIcon.worldBound.position)) // 2. 按距离远近排序
                .FirstOrDefault(); // 3. 取出距离最近的那个

            if (closestSlot != null)
            {
                // 如果找到了有效的目标槽位，触发 OnDrop 事件，交由 Controller 层处理具体的交换或合并逻辑
                OnDrop?.Invoke(originalSlot, closestSlot);
            }
            else
            {
                // 如果没有找到目标槽位（比如拖到了背包外面的空白区域），则将物品图标还原回原槽位
                originalSlot.Icon.image = originalSlot.BaseSprite.texture;
            }

            isDragging = false; // 结束拖拽状态
            originalSlot = null; // 清空原槽位记录
            ghostIcon.style.visibility = Visibility.Hidden; // 隐藏幽灵图标
        }

        // 设置幽灵图标在屏幕上的绝对位置（通过修改 UIElements 的 top 和 left 样式）
        private static void SetGhostIconPosition(Vector2 position)
        {
            // 减去自身宽高的一半，确保鼠标光标始终位于幽灵图标的正中心
            ghostIcon.style.top = position.y - ghostIcon.layout.height / 2;
            ghostIcon.style.left = position.x - ghostIcon.layout.width / 2;
        }

        // 当该 MonoBehaviour 被销毁时，取消事件订阅，防止内存泄漏
        private void OnDestroy()
        {
            foreach (var slot in Slots)
            {
                slot.OnStartDrag -= OnPointerDown;
            }
        }
    }
}