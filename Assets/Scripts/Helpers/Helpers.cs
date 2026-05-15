using UnityEngine;
using UnityEngine.UIElements;

namespace Systems.Inventory
{
    public static class Helpers
    {
        // 静态工具方法：计算并返回一个被限制在屏幕范围内的坐标
        // element 是需要被限制范围的 UI 元素
        // targetPosition 是你原本想要设置的目标位置（比如鼠标拖拽到的位置）
        public static Vector2 ClampToScreen(VisualElement element, Vector2 targetPosition)
        {
            // 核心逻辑：使用 Mathf.Clamp 对 X 和 Y 坐标分别进行“夹紧”处理
            // X 轴限制：最小值为 0（屏幕左边缘），最大值为 屏幕总宽度 - 元素自身的宽度（防止右边缘超出）
            float x = Mathf.Clamp(targetPosition.x, 0, Screen.width - element.layout.width);
            // Y 轴限制：最小值为 0（屏幕下边缘），最大值为 屏幕总高度 - 元素自身的高度（防止上边缘超出）
            float y = Mathf.Clamp(targetPosition.y, 0, Screen.height - element.layout.height);

            // 返回经过修正后的安全坐标
            return new Vector2(x, y);
        }
    }
}