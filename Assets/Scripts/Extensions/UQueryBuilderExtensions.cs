using System;
using System.Collections.Generic;
using System.Linq; // 引入 LINQ 命名空间，为了使用原生的 OrderBy 和 FirstOrDefault
using UnityEngine.UIElements; // 引入 UIElements，UQueryBuilder 定义在这里

namespace Systems.Inventory
{
    public static class UQueryBuilderExtensions
    {
        // 扩展方法 1：通用的自定义排序
        // query 是被扩展的 UQueryBuilder（UI 元素查询构建器）
        // keySelector 是一个委托，用来从 UI 元素中提取用于排序的“关键值”
        // @default 是传入的比较器（比如决定是升序还是降序，或者自定义的比较逻辑）
        public static IEnumerable<T> OrderBy<T, TKey>(this UQueryBuilder<T> query, Func<T, TKey> keySelector,
            Comparer<TKey> @default) where T : VisualElement
        {
            // UQueryBuilder 本身不支持直接排序，所以先用 ToList() 把它转换成普通的 List 集合
            // 然后调用 C# 原生 LINQ 的 OrderBy 方法，并传入自定义的比较器进行排序
            return query.ToList().OrderBy(keySelector, @default);
        }

        // 扩展方法 2：专门针对“距离”的快捷排序
        // keySelector 在这里被限定为提取一个 float 类型的值（比如距离、进度条百分比等）
        public static IEnumerable<T> OrderByDistance<T>(this UQueryBuilder<T> query, Func<T, float> keySelector)
            where T : VisualElement
        {
            // 直接复用上面的通用 OrderBy 方法
            // 传入 Comparer<float>.Default，即使用 C# 默认的浮点数从小到大（升序）比较规则
            return query.OrderBy(keySelector, Comparer<float>.Default);
        }

        // 扩展方法 3：安全地获取查询结果中的第一个元素
        public static T FirstOrDefault<T>(this UQueryBuilder<T> query) where T : VisualElement
        {
            // 同样先转成 List，然后调用 LINQ 的 FirstOrDefault
            // 如果查询结果为空，它会返回 null 而不会报错，非常适合 UI 元素的查找
            return query.ToList().FirstOrDefault();
        }
    }
}