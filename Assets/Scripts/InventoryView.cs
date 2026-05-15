using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Systems.Inventory
{
    // 继承自 StorageView（推测是你的 UI 基类），负责背包界面的具体渲染与构建
    public class InventoryView : StorageView
    {
        // 序列化字段：允许在 Unity Inspector 面板中自定义背包顶部的标题名称
        [SerializeField] private string panelName = "Inventory";

        // 重写基类的初始化方法，负责动态生成背包的整个 UI 层级结构
        public override IEnumerator InitializeView(int size = 20)
        {
            // 初始化背包格子数组，长度为指定的容量大小
            Slots = new Slot[size];
            // 获取当前 UI 文档（UIDocument）的根元素
            root = document.rootVisualElement;
            root.Clear(); // 清空根元素下的所有旧内容，防止重复生成

            root.styleSheets.Add(styleSheet); // 加载并应用背包专属的 USS 样式表

            // 使用你写的扩展方法，在根节点下创建一个名为 "container" 的容器
            container = root.CreateChild("container");

            // 链式调用：创建背包主面板 -> 绑定拖拽交互器(PanelDragManipulator) -> 生成框架与标题
            var inventory = container.CreateChild("inventory").WithManipulator(new PanelDragManipulator());
            inventory.CreateChild("inventoryFrame"); // 背包的背景边框
            // 创建标题栏，并直接添加一个 Unity 原生的 Label 控件来显示背包名称
            inventory.CreateChild("inventoryHeader").Add(new Label(panelName));

            // 创建专门用来容纳所有物品格子的容器
            var slotsContainer = inventory.CreateChild("slotsContainer");
            // 循环生成指定数量的物品格子（Slot）
            for (int i = 0; i < size; i++)
            {
                // 使用泛型扩展方法 CreateChild<T> 实例化自定义的 Slot 控件，并赋予 "slot" 样式类
                var slot = slotsContainer.CreateChild<Slot>("slot");
                Slots[i] = slot; // 将生成的格子存入数组，方便后续 Controller 层进行数据绑定
            }

            // 创建拖拽物品时跟随鼠标的“幽灵图标”（Ghost Icon）
            ghostIcon = container.CreateChild("ghostIcon");

            yield return null; // 协程返回，表示 UI 初始化完成
        }
    }
}