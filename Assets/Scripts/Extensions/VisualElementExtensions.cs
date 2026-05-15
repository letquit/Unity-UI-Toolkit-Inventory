using UnityEngine.UIElements;

namespace Systems.Inventory
{
    public static class VisualElementExtensions
    {
        // 扩展方法 1：为父元素创建一个基础的 VisualElement 子元素
        // parent 是被扩展的父元素
        // params string[] classes 是一个可变参数，允许你直接传入多个 USS 样式类名（比如 "slot", "item-border"）
        public static VisualElement CreateChild(this VisualElement parent, params string[] classes)
        {
            var child = new VisualElement(); // 实例化一个新的 UI 元素
            // 链式调用：先给子元素添加样式类，再把它挂载到父元素上
            child.AddClass(classes).AddTo(parent);
            return child; // 返回创建好的子元素，方便后续继续操作
        }

        // 扩展方法 2：泛型版的创建子元素（支持创建 Button, Label, Image 等任意 VisualElement 子类）
        // where T : VisualElement, new() 约束了 T 必须是 VisualElement 的子类，且必须有无参构造函数
        public static T CreateChild<T>(this VisualElement parent, params string[] classes)
            where T : VisualElement, new()
        {
            var child = new T(); // 实例化一个泛型指定的 UI 控件（比如 new Button()）
            child.AddClass(classes).AddTo(parent);
            return child;
        }

        // 扩展方法 3：将子元素挂载到指定的父元素上
        // 这是一个非常核心的“粘合剂”方法，把原本 parent.Add(child) 的调用逻辑反转成了 child.AddTo(parent)
        public static T AddTo<T>(this T child, VisualElement parent) where T : VisualElement
        {
            parent.Add(child); // 执行原生的添加子元素操作
            return child; // 返回子元素本身，以便继续链式调用其他方法
        }

        // 扩展方法 4：批量为 UI 元素添加 USS 样式类
        public static T AddClass<T>(this T visualElement, params string[] classes) where T : VisualElement
        {
            foreach (string cls in classes)
            {
                // 增加了一个空值检查，防止传入空字符串导致 USS 样式匹配出错
                if (!string.IsNullOrEmpty(cls))
                {
                    visualElement.AddToClassList(cls); // 调用 Unity 原生的添加样式类方法
                }
            }

            return visualElement; // 返回元素本身，支持链式调用
        }

        // 扩展方法 5：为 UI 元素绑定交互器（Manipulator，比如拖拽、点击、缩放等交互逻辑）
        public static T WithManipulator<T>(this T visualElement, IManipulator manipulator) where T : VisualElement
        {
            visualElement.AddManipulator(manipulator); // 调用 Unity 原生的添加交互器方法
            return visualElement; // 返回元素本身，支持链式调用
        }
    }
}