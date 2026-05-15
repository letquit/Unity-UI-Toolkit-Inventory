using UnityEngine;
using Sirenix.OdinInspector; // 引入 Odin 插件的命名空间，解锁强大的 Inspector 定制功能

namespace Systems.Inventory
{
    // CreateAssetMenu 特性：允许你在 Unity 菜单栏中右键快速创建该类型的资源文件
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class ItemDetails : ScriptableObject
    {
        // Odin 布局特性：创建左右分栏的水平组（ItemSplit）。
        // 左侧占据 80% 的宽度，并在此区域内创建名为 "Left" 的垂直组
        [HorizontalGroup("ItemSplit", 0.8f), VerticalGroup("ItemSplit/Left")]
        public string Name; // 物品名称

        [HorizontalGroup("ItemSplit", 0.8f), VerticalGroup("ItemSplit/Left")]
        public int maxStack = 1; // 该物品的最大堆叠数量（比如药水可以堆 99，武器只能堆 1）

        [HorizontalGroup("ItemSplit", 0.8f), VerticalGroup("ItemSplit/Left")]
        public SerializableGuid Id = SerializableGuid.NewGuid(); // 物品配置的唯一标识（注意：这里是该物品“种类”的 ID）

        // Odin 按钮与颜色特性：在 Inspector 中生成一个大型按钮，并赋予清新的蓝色。
        // 点击按钮即可为当前物品重新生成一个新的 Guid
        [HorizontalGroup("ItemSplit", 0.8f), VerticalGroup("ItemSplit/Left"), Button(ButtonSizes.Large),
         GUIColor(0.4f, 0.8f, 1)]
        private void AssignNewGuid()
        {
            Id = SerializableGuid.NewGuid();
        }

        // Odin 预览图特性：右侧占据 20% 的宽度，隐藏字段名标签，并生成一个 100 像素高的 Sprite 预览图
        [HorizontalGroup("ItemSplit", 0.2f), VerticalGroup("ItemSplit/Right"), HideLabel, PreviewField(100)]
        public Sprite Icon; // 物品在背包格子中显示的图标

        // Unity 原生特性：将字符串转换为带滚动条的多行文本域，并隐藏字段名标签，适合写长段落的物品介绍
        [TextArea, HideLabel] public string Description; // 物品的详细描述

        // 工厂方法：根据这份静态配置（蓝图），动态创建出一个带有数量的运行时物品实例（Item）
        public Item Create(int quantity)
        {
            return new Item(this, quantity);
        }
    }
}