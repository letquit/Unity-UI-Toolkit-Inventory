using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Systems.SceneManagement
{
    public class SceneGroupManager
    {
        // 使用 delegate { } 初始化事件，确保外界在订阅时永远不需要判断事件是否为 null，极其稳健的防御性编程
        public event Action<string> OnSceneLoaded = delegate { };
        public event Action<string> OnSceneUnloaded = delegate { };
        public event Action OnSceneGroupLoaded = delegate { };
        
        
        // 专门用于统一管理 Addressables 场景加载句柄的容器
        private readonly AsyncOperationHandleGroup handleGroup = new AsyncOperationHandleGroup(10);
        
        private SceneGroup ActiveSceneGroup; // 记录当前正在运行的场景组

        // 核心加载方法：异步加载整个场景组，并实时向外汇报加载进度
        public async Task LoadScenes(SceneGroup group, IProgress<float> progress, bool reloadDupScenes = false)
        {
            ActiveSceneGroup = group;
            var loadedScenes = new List<string>();

            // 在加载新场景组之前，先彻底清理掉旧的场景，防止内存堆积
            await UnloadScenes();

            int sceneCount = SceneManager.sceneCount;
            // 记录当前内存中已经存在的场景名称（比如全局常驻的 Bootstrapper 或 Persistent 场景）
            for (var i = 0; i < sceneCount; i++)
            {
                loadedScenes.Add(SceneManager.GetSceneAt(i).name);
            }

            var totalScenesToLoad = ActiveSceneGroup.Scenes.Count;
            // 创建一个异步操作组，用于统一管理这一批次所有场景的加载进度
            var operationGroup = new AsyncOperationGroup(totalScenesToLoad);

            for (var i = 0; i < totalScenesToLoad; i++)
            {
                var sceneData = group.Scenes[i];
                // 如果不需要强制重载，且该场景已经在内存中，则直接跳过
                if (reloadDupScenes == false && loadedScenes.Contains(sceneData.Name)) continue;

                // 核心分支：根据 Eflatun.SceneReference 的状态，自动选择最优的加载方式
                if (sceneData.Reference.State == SceneReferenceState.Regular)
                {
                    // 【传统方式】使用 Unity 原生 SceneManager 异步加载场景（Additive 叠加模式）
                    var operation = SceneManager.LoadSceneAsync(sceneData.Reference.Path, LoadSceneMode.Additive);
                    operationGroup.Operations.Add(operation);
                }
                else if (sceneData.Reference.State == SceneReferenceState.Addressable)
                {
                    // 【现代方式】使用 Addressables 异步加载场景（支持本地打包或远程 CDN 热更新）
                    var sceneHandle = Addressables.LoadSceneAsync(sceneData.Reference.Path, LoadSceneMode.Additive);
                    handleGroup.Handles.Add(sceneHandle); // 将加载句柄存入专属容器，方便后续统一释放
                }
                
                OnSceneLoaded.Invoke(sceneData.Name); // 通知外界：某个具体的场景已经开始加载了
            }
            
            // 核心循环：等待【原生场景】和【Addressables 场景】这两批任务全部加载完毕
            while (!operationGroup.IsDone || !handleGroup.IsDone)
            {
                // 综合计算整体进度：将两种加载方式的进度取平均值汇报给 UI
                progress?.Report((operationGroup.Progress + handleGroup.Progress) / 2);
                await Task.Delay(100); // 每 100 毫秒汇报一次，避免死循环卡死主线程
            }

            // 加载完成后，自动将该场景组中的“ActiveScene”类型场景设为 Unity 的主活跃场景
            Scene activeScene =
                SceneManager.GetSceneByName(ActiveSceneGroup.FindSceneNameByType(SceneType.ActiveScene));

            if (activeScene.IsValid())
            {
                SceneManager.SetActiveScene(activeScene);
            }
            
            OnSceneGroupLoaded.Invoke(); // 通知外界：整个场景组已经全部加载就绪，可以开始游戏了！
        }

        // 核心卸载方法：智能地清理掉除了“常驻场景”以外的所有旧场景
        public async Task UnloadScenes()
        {
            var scenes = new List<string>();
            var activeScene = SceneManager.GetActiveScene().name;

            int sceneCount = SceneManager.sceneCount;
            // 倒序遍历当前内存中的所有场景
            for (var i = sceneCount - 1; i > 0; i--)
            {
                var sceneAt = SceneManager.GetSceneAt(i);
                if (!sceneAt.isLoaded) continue;

                var sceneName = sceneAt.name;
                // 核心防御逻辑：绝对不卸载当前的主活跃场景，以及全局常驻的 Bootstrapper 场景
                if (sceneName.Equals(activeScene) || sceneName == "Bootstrapper") continue;
                // 额外防御：如果该场景是由 Addressables 加载的，则跳过原生卸载逻辑，交给 handleGroup 处理
                if (handleGroup.Handles.Any(h => h.IsValid() && h.Result.Scene.name == sceneName)) continue;
                scenes.Add(sceneName);
            }
            
            // 创建一个异步操作组，统一管理这一批次的场景卸载
            var operationGroup = new AsyncOperationGroup(scenes.Count);

            foreach (var scene in scenes)
            {
                var operation = SceneManager.UnloadSceneAsync(scene);
                if (operation == null) continue;

                operationGroup.Operations.Add(operation);
                OnSceneUnloaded.Invoke(scene); // 通知外界：某个具体的场景已经开始卸载了
            }

            // 遍历并释放所有由 Addressables 加载的场景句柄（这是防止内存泄漏的关键！）
            foreach (var handle in handleGroup.Handles)
            {
                if (handle.IsValid())
                {
                    Addressables.UnloadSceneAsync(handle);
                }
            }
            handleGroup.Handles.Clear(); // 清空句柄列表，为下一次加载做准备
            
            // 等待所有场景彻底从内存中移除
            while (!operationGroup.IsDone)
            {
                await Task.Delay(100);  // 避免死循环卡死主线程
            }
            
            // 场景卸载后，强制调用 Unity 的垃圾回收，释放被旧场景占用的贴图、模型等内存
            await Resources.UnloadUnusedAssets();
        }
    }

    // 异步操作组结构体：用于将多个 Unity 的 AsyncOperation 打包在一起，统一计算整体进度
    public readonly struct AsyncOperationGroup
    {
        public readonly List<AsyncOperation> Operations;

        // 整体进度 = 组内所有异步操作进度的平均值
        public float Progress => Operations.Count == 0 ? 0 : Operations.Average(o => o.progress);
        // 整体是否完成 = 组内所有异步操作都必须完成
        public bool IsDone => Operations.All(o => o.isDone);

        public AsyncOperationGroup(int initialCapacity)
        {
            Operations = new List<AsyncOperation>(initialCapacity);
        }
    }

    // Addressables 异步句柄组结构体，专门用于管理 Addressables 的加载与释放
    public readonly struct AsyncOperationHandleGroup
    {
        public readonly List<AsyncOperationHandle<SceneInstance>> Handles;

        // Addressables 使用 PercentComplete 来获取进度（范围 0~1）
        public float Progress => Handles.Count == 0 ? 0 : Handles.Average(h => h.PercentComplete);
        // 只要所有句柄都加载完成，即视为该组任务完成
        public bool IsDone => Handles.Count == 0 || Handles.All(o => o.IsDone);
        
        public AsyncOperationHandleGroup(int initialCapacity)
        {
            Handles = new List<AsyncOperationHandle<SceneInstance>>(initialCapacity);
        }
    }
}