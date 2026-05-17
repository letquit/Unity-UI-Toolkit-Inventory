using UnityEditor;
using UnityEngine;

namespace Systems.Persistence.Editor {
    // [CustomEditor(typeof(SaveLoadSystem))] 是编辑器扩展的核心特性。
    // 它告诉 Unity：“请不要再使用默认的 Inspector 面板来绘制 SaveLoadSystem 组件了，改用下面这个自定义的编辑器类来绘制。”
    [CustomEditor(typeof(SaveLoadSystem))]
    public class SaveManagerEditor : UnityEditor.Editor {
        
        // OnInspectorGUI 方法会在 Inspector 面板每一帧重绘时被调用（类似于运行时的 OnGUI）
        public override void OnInspectorGUI() {
            // 获取当前在 Inspector 面板中被选中的 SaveLoadSystem 组件实例
            SaveLoadSystem saveLoadSystem = (SaveLoadSystem) target;
            // 提前获取当前存档的名称，方便后续读取和删除操作时作为参数传入
            string gameName = saveLoadSystem.gameData.Name;
            
            // DrawDefaultInspector() 会先绘制出 SaveLoadSystem 原本所有的序列化字段（比如 gameData）。
            // 如果没有这行代码，Inspector 面板上原本的数据就看不到了。
            DrawDefaultInspector();
            
            // 在 Inspector 面板上绘制一个“New Game”按钮。当按钮被点击时，执行大括号内的逻辑。
            if (GUILayout.Button("New Game")) {
                saveLoadSystem.NewGame();
            }

            if (GUILayout.Button("Save Game")) {
                saveLoadSystem.SaveGame();
            }

            if (GUILayout.Button("Load Game")) {
                saveLoadSystem.LoadGame(gameName);
            }

            if (GUILayout.Button("Delete Game")) {
                saveLoadSystem.DeleteGame(gameName);
            }
        }
    }
}