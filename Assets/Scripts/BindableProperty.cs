using System;
using Unity.Properties; // 引入 Unity 官方的属性系统库，它是 UI Toolkit 数据绑定的底层引擎

namespace Systems.Inventory
{
    public class BindableProperty<T>
    {
        // 只读的委托字段：用于指向 Model 层（或 ViewModel 层）真实数据的获取方法
        private readonly Func<T> getter;
        // Add setter to make a 2 way binding Action<T> setter; (预留的双向绑定 setter)

        // 私有构造函数：强制外界只能通过静态工厂方法 Bind() 来创建实例
        private BindableProperty(Func<T> getter)
        {
            this.getter = getter;
        }
        
        // 核心特性：[CreateProperty] 告诉 Unity 的属性系统，这个 Value 属性是可以被 UI Toolkit 识别和绑定的！
        [CreateProperty]    // Allows binding to the UI in UI Toolkit
        public T Value => getter(); // 只读属性：每次 UI 读取它时，它都会实时去调用 getter 获取底层最新的真实数据
        
        // 静态工厂方法：外界通过调用 Bind 方法，传入一个获取数据的委托，来创建一个可绑定属性
        public static BindableProperty<T> Bind(Func<T> getter) => new BindableProperty<T>(getter);
    }
}