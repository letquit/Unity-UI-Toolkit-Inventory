using UnityEngine;
using UnityEngine.UIElements;

namespace Systems.Inventory
{
    // 继承 PointerManipulator，这是 UIElements 提供的专门用于处理复杂交互（如拖拽、缩放）的基类
    public class PanelDragManipulator : PointerManipulator
    {
        private bool isDragging; // 标记当前是否处于拖拽状态
        private Vector2 offset; // 记录鼠标按下时，点击点距离元素左上角的偏移量

        public PanelDragManipulator()
        {
            // 初始化激活器：指定只有按下鼠标左键（LeftMouse）时，才会触发这个交互器
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        // 当这个交互器被添加到目标元素（target）上时，自动注册底层的指针事件
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown); // 鼠标按下
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove); // 鼠标移动
            target.RegisterCallback<PointerUpEvent>(OnPointerUp); // 鼠标抬起
        }

        // 当交互器从目标元素上移除时，注销事件，防止内存泄漏
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        }

        // 处理鼠标按下事件
        private void OnPointerDown(PointerDownEvent evt)
        {
            // CanStartManipulation 会检查当前按下的按键是否符合 activators 的设定（比如是否是左键）
            if (!CanStartManipulation(evt) || isDragging) return;

            // 核心逻辑：记录鼠标按下时，点击位置在元素内部的局部坐标（即偏移量）
            // 这样拖拽时元素就不会“瞬移”导致鼠标跑到元素中心去
            offset = evt.localPosition;
            isDragging = true;

            // 捕获指针：告诉系统“这个鼠标现在被我包圆了”，即使鼠标移出元素范围，也能继续接收移动事件
            target.CapturePointer(evt.pointerId);
            evt.StopPropagation(); // 阻止事件继续向下传递（防止触发背包格子的点击等）
        }

        // 处理鼠标移动事件
        private void OnPointerMove(PointerMoveEvent evt)
        {
            // 只有处于拖拽状态，且当前元素依然持有该鼠标的捕获权时，才执行移动逻辑
            if (!isDragging || !target.HasPointerCapture(evt.pointerId)) return;

            // 计算鼠标移动的增量（Delta）：当前局部位置 - 按下时的偏移量
            Vector3 delta = evt.localPosition - (Vector3)offset;
            // 将这个增量累加到元素的世界坐标位置上，实现平滑拖拽
            target.transform.position += delta;
            evt.StopPropagation();
        }

        // 处理鼠标抬起事件
        private void OnPointerUp(PointerUpEvent evt)
        {
            // CanStopManipulation 检查按键抬起是否符合交互结束的条件
            if (!CanStopManipulation(evt) || !isDragging) return;

            isDragging = false;
            // 释放指针捕获：告诉系统“这个鼠标我不用了，还给你”
            target.ReleasePointer(evt.pointerId);
            evt.StopPropagation();
        }
    }
}