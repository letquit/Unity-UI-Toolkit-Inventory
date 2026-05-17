using System.Collections;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Systems.Inventory
{
    // 继承自 StorageView，负责背包界面的具体渲染与构建
    public class InventoryView : StorageView
    {
        // 序列化字段：允许在 Unity Inspector 面板中自定义背包顶部的标题名称
        [SerializeField] private string panelName = "Inventory";

        // 重写基类的初始化方法，负责动态生成背包的整个 UI 层级结构
        // 核心变化：接收由 Controller 传入的 ViewModel 实例
        public override IEnumerator InitializeView(ViewModel viewModel)
        {
            // 初始化背包格子数组，长度直接取自 ViewModel 提供的容量
            Slots = new Slot[viewModel.Capacity];
            // 获取当前 UI 文档（UIDocument）的根元素并清空旧内容
            root = document.rootVisualElement;
            root.Clear(); 

            root.styleSheets.Add(styleSheet); // 加载并应用背包专属的 USS 样式表

            // 使用扩展方法，在根节点下创建 UI 层级
            container = root.CreateChild("container");

            // 链式调用：创建背包主面板 -> 绑定拖拽交互器 -> 生成框架与标题
            var inventory = container.CreateChild("inventory").WithManipulator(new PanelDragManipulator());
            inventory.CreateChild("inventoryFrame"); // 背包的背景边框
            // 创建标题栏，添加原生 Label 显示背包名称
            inventory.CreateChild("inventoryHeader").Add(new Label(panelName));

            // 创建容纳所有物品格子的容器
            var slotsContainer = inventory.CreateChild("slotsContainer");
            // 循环生成指定数量的物品格子（Slot）
            for (int i = 0; i < viewModel.Capacity; i++)
            {
                var slot = slotsContainer.CreateChild<Slot>("slot");
                Slots[i] = slot; // 存入数组，供 Controller 后续进行数据绑定
            }
            
            // 【MVVM 核心落地】金币 UI 的创建与数据绑定
            var coins = inventory.CreateChild("coins"); // 创建金币的整体容器
            var coinsLabel = new Label(); // 创建用于显示金币数量的文本标签
            coins.CreateChild("coinsIcon"); // 创建金币的图标
            coins.Add(coinsLabel); // 将文本标签添加到金币容器中
            
            // 关键步骤1：将金币容器的数据上下文（dataSource）设置为 ViewModel 中的 Coins 属性
            coins.dataSource = viewModel.Coins;
            
            // 关键步骤2：为 Label 设置数据绑定规则
            coinsLabel.SetBinding(nameof(Label.text), new DataBinding
            {
                // 绑定的数据路径：指向 BindableProperty<string> 的 Value 属性
                dataSourcePath = new PropertyPath(nameof(BindableProperty<string>.Value)),
                // 绑定模式：ToTarget（单向绑定），即数据变化自动推送到 UI 文本上
                bindingMode = BindingMode.ToTarget
            });

            // 创建拖拽物品时跟随鼠标的“幽灵图标”（Ghost Icon）
            ghostIcon = container.CreateChild("ghostIcon");

            yield return null; // 协程返回，表示 UI 初始化完成
        }
    }
}