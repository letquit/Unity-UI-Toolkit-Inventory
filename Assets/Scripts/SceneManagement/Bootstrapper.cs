using Systems.Persistence; // 引入你之前封装的跨场景持久化单例基类
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement; // 引入编辑器场景管理相关的 API
#endif

// 继承 PersistentSingleton<Bootstrapper>，让引导器自动获得“全局唯一”且“跨场景不销毁”的能力
public class Bootstrapper : PersistentSingleton<Bootstrapper> {
    // 静态只读变量：指定在 Build Settings 中作为启动场景的索引（默认第 0 个场景作为引导场景）
    static readonly int sceneIndex = 0;
    
    // [RuntimeInitializeOnLoadMethod] 是 Unity 的底层特性。
    // 它保证了 Init 方法会在游戏运行时、任何场景加载之前（BeforeSceneLoad）被自动调用。
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init() {
        Debug.Log("Bootstrapper...");
#if UNITY_EDITOR
        // 核心逻辑：在编辑器环境下，强制指定 Play Mode 的启动场景。
        // 这样无论你在编辑器里当前打开的是哪个关卡场景，点击 Play 按钮时，
        // Unity 都会自动先加载这里指定的 Bootstrapper 场景，确保全局单例被正确初始化。
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[sceneIndex].path);
#endif
    }
}