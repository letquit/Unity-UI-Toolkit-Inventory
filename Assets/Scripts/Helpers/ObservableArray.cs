using System;
using System.Collections.Generic;
using System.Linq;

namespace Systems.Inventory
{
    // 定义一个泛型可观察数组的接口，规范了外界对数组的操作契约
    public interface IObservableArray<T>
    {
        int Count { get; } // 获取当前数组中有效元素的数量
        T this[int index] { get; } // 提供索引器，允许通过下标直接读取元素
        void Clear(); // 清空数组
        event Action<T[]> AnyValueChanged; // 核心事件：当数组内任何值发生变化时触发，并广播当前的完整数组
        bool TryAdd(T item); // 尝试添加元素，返回是否添加成功（比如数组满了就返回 false）
        bool TryRemove(T item); // 尝试移除指定元素，返回是否移除成功
        void Swap(int index1, int index2); // 交换两个索引位置的元素（背包系统拖拽换位的必备功能）
    }

    [Serializable] // 加上此特性，使得该数组可以在 Unity 的 Inspector 面板中序列化显示
    public class ObservableArray<T> : IObservableArray<T>
    {
        private T[] items; // 内部实际存储数据的原生数组

        // 初始化一个空的委托，防止外界订阅前触发事件导致空引用异常（NullReferenceException）
        public event Action<T[]> AnyValueChanged = delegate { };

        // 动态计算当前数组中不为 null 的元素个数
        public int Count => items.Count(i => i != null);

        // 索引器，直接返回内部数组对应下标的元素
        public T this[int index] => items[index];

        // 构造函数：size 指定背包容量（默认 20），initialList 允许在创建时传入初始物品列表
        public ObservableArray(int size = 20, IList<T> initialList = null)
        {
            items = new T[size];
            if (initialList != null)
            {
                // 将初始列表的元素安全地拷贝到内部数组中（防止初始列表长度超出容量）
                initialList.Take(size).ToArray().CopyTo(items, 0);
                Invoke(); // 数据初始化完毕，立刻通知外界
            }
        }

        // 触发事件的封装方法，保持代码整洁
        void Invoke() => AnyValueChanged.Invoke(items);

        // 交换两个位置的物品。使用了 C# 的元组语法 (a, b) = (b, a)，非常优雅
        public void Swap(int index1, int index2)
        {
            (items[index1], items[index2]) = (items[index2], items[index1]);
            Invoke(); // 交换后立刻通知外界刷新
        }

        // 清空数组：直接 new 一个新数组，效率极高
        public void Clear()
        {
            items = new T[items.Length];
        }

        // 尝试添加物品：遍历数组，找到第一个为 null 的空位并填入
        public bool TryAdd(T item)
        {
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] != null) continue; // 如果该位置已经有物品，跳过
                items[i] = item;
                Invoke(); // 添加成功，立刻通知外界
                return true;
            }

            return false; // 遍历完都没找到空位，说明背包已满，添加失败
        }

        // 尝试移除物品：遍历数组，找到与目标物品相等的元素并置为 default（引用类型为 null，值类型为 0）
        public bool TryRemove(T item)
        {
            for (var i = 0; i < items.Length; i++)
            {
                // 使用 EqualityComparer 进行安全的相等性比较（能完美处理 null 和各种泛型类型）
                if (!EqualityComparer<T>.Default.Equals(items[i], item)) continue;
                items[i] = default;
                Invoke(); // 移除成功，立刻通知外界
                return true;
            }

            return false; // 没找到对应的物品，移除失败
        }
    }
}