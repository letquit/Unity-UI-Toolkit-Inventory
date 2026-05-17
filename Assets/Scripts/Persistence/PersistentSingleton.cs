using UnityEngine;

namespace Systems.Persistence
{
    // 定义为抽象类（abstract），意味着它不能直接使用，必须被其他类继承。
    // 泛型约束 `where T : Component` 保证了 T 必须是 Unity 的组件类型（比如继承自 MonoBehaviour 的脚本）。
    public abstract class PersistentSingleton<T> : MonoBehaviour where T : Component
    {
        // 静态私有变量，用于在内存中保存该类的唯一实例。
        // 因为是 static，所以它在整个程序运行期间只有一份，不会随着场景切换而丢失。
        private static T _instance;
        
        // 对外暴露的全局唯一访问入口（单例模式的核心）。
        public static T Instance
        {
            get
            {
                // 如果当前内存中还没有实例（比如第一次访问），就开始寻找或创建。
                if (_instance == null)
                {
                    // 先在当前所有已加载的场景中找找看，有没有已经挂载好的 T 类型组件。
                    var objs = FindObjectsOfType<T>();
                    if (objs.Length > 0)
                    {
                        _instance = objs[0]; // 找到了，就把它指定为全局唯一实例
                        // 如果发现场景里不小心放了多个同样的管理器，发出警告提示开发者排查
                        if (objs.Length > 1)
                        {
                            Debug.LogWarning($"More than one {_instance.GetType().Name} in scene, using first one");
                        }
                    }
                    else
                    {
                        // 如果场景里完全没找到（比如还没加载包含该组件的场景），就动态创建一个！
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name; // 把这个新物体命名为类名（如 "SaveLoadSystem"）
                        _instance = obj.AddComponent<T>(); // 给它挂上 T 脚本，并赋值给实例
                    }
                }
                return _instance; // 返回这个唯一的实例
            }
        }

        // Awake 是 Unity 脚本生命周期中最早执行的方法之一。
        // 加上 virtual 关键字，允许子类（如 SaveLoadSystem）重写 Awake，但别忘了调用 base.Awake()。
        protected virtual void Awake()
        {
            // 核心防御逻辑：如果全局已经有实例了，且那个实例不是“我”，说明出现了重复的“大管家”。
            if (_instance != null && _instance != this)
            {
                DestroyImmediate(gameObject); // 立刻销毁当前这个多余的物体，防止重复！
                return;
            }
            _instance = GetComponent<T>(); // 把自己注册为全局唯一的实例
            DontDestroyOnLoad(gameObject); // 告诉 Unity：切换场景时，千万不要销毁这个物体！
        }
    }
}