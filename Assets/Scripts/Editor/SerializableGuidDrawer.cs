using System.Text;
using Systems.Inventory; // 引入你背包系统的命名空间（假设 SerializableGuid 定义在这里）
using UnityEditor;
using UnityEngine;

namespace Editor
{
    // 告诉 Unity 编辑器：这个绘制器是专门为 SerializableGuid 类型服务的
    [CustomPropertyDrawer(typeof(SerializableGuid))]
    public class SerializableGuidDrawer : PropertyDrawer
    {
        // OnGUI 是绘制 Inspector 界面的核心方法，每一帧或界面刷新时都会被调用
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 1. 开始绘制属性：处理 Inspector 中的属性前缀（如折叠箭头、复选框等）
            EditorGUI.BeginProperty(position, label, property);

            // 2. 提取 SerializableGuid 内部的 4 个 int 字段（GUID 的标准存储结构）
            var value0 = property.FindPropertyRelative("Part1");
            var value1 = property.FindPropertyRelative("Part2");
            var value2 = property.FindPropertyRelative("Part3");
            var value3 = property.FindPropertyRelative("Part4");

            // 3. 重新计算绘制区域：PrefixLabel 会预留出左侧的标签（Label）空间，
            // 返回剩下的矩形区域用于绘制具体的内容
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // 4. 安全检查：确保 4 个内部字段都成功获取到了
            if (value0 != null && value1 != null && value2 != null && value3 != null)
            {
                // 5. 绘制一个只读的、可高亮复制的文本标签
                EditorGUI.SelectableLabel(position,
                    new StringBuilder()
                        // 将 int 强制转换为 uint（无符号整数），并以 8 位大写十六进制格式（X8）拼接
                        // 例如：int 的 255 会变成 "000000FF"
                        .AppendFormat("{0:X8}", (uint)value0.intValue)
                        .AppendFormat("{0:X8}", (uint)value1.intValue)
                        .AppendFormat("{0:X8}", (uint)value2.intValue)
                        .AppendFormat("{0:X8}", (uint)value3.intValue)
                        .ToString()); // 最终拼接成一个标准的 32 位 GUID 字符串（如 A1B2C3D4...）
            }
            else
            {
                // 如果字段获取失败（比如字段名写错了），显示错误提示
                EditorGUI.SelectableLabel(position, "GUID Not Initialized");
            }

            // 6. 结束属性绘制：与 BeginProperty 配对，保证 Unity 的绘制堆栈正常
            EditorGUI.EndProperty();
        }
    }
}