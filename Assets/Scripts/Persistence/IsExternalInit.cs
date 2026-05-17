// 条件编译指令：判断当前编译的目标框架是否“不是” .NET 5.0 或更高版本
// 如果当前是旧版本框架（如 .NET Framework 或旧版 Unity 运行时），则执行下面的代码
#if !NET5_0_OR_GREATER

// 必须使用这个特定的命名空间，因为 C# 编译器在底层只会去这里寻找 IsExternalInit 类型
namespace System.Runtime.CompilerServices
{
    // 定义一个空的内部静态类
    // 它不需要任何具体的代码逻辑，只需要存在这个“壳”就能骗过编译器，让它认为环境支持 init 属性。
    internal static class IsExternalInit { }
}
#endif