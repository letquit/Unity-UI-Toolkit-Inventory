#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace Systems.SceneManagement.Editor {
#if UNITY_EDITOR // 核心防御：确保以下代码仅在编辑器模式下编译，打包游戏时会被完全剔除，不影响包体大小
    // [CustomEditor(typeof(SceneLoader))] 告诉 Unity：请用下面这个自定义编辑器来绘制 SceneLoader 组件的 Inspector 面板
    [CustomEditor(typeof(SceneLoader))]
    public class SceneLoaderEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            // 先绘制 SceneLoader 原本所有的序列化字段（如 loadingBar, sceneGroups 等）
            DrawDefaultInspector();

            // 获取当前在 Inspector 面板中被选中的 SceneLoader 组件实例
            SceneLoader sceneLoader = (SceneLoader) target;

            // 核心限制：EditorApplication.isPlaying 确保按钮只有在游戏运行（Play Mode）时才会显示和生效
            // 防止在编辑模式下误触加载逻辑，导致场景错乱
            if (EditorApplication.isPlaying && GUILayout.Button("Load First Scene Group")) {
                LoadSceneGroup(sceneLoader, 0); // 点击按钮，触发加载第 0 个场景组
            }
            
            if (EditorApplication.isPlaying && GUILayout.Button("Load Second Scene Group")) {
                LoadSceneGroup(sceneLoader, 1); // 点击按钮，触发加载第 1 个场景组
            }
        }

        // 封装一个静态的异步方法来调用 SceneLoader 的加载逻辑
        // 注意：在 Unity 编辑器的自定义 GUI 中，无法直接使用 await，因此需要封装成 async void 的静态方法
        static async void LoadSceneGroup(SceneLoader sceneLoader, int index) {
            await sceneLoader.LoadSceneGroup(index);
        }
    }
#endif
}