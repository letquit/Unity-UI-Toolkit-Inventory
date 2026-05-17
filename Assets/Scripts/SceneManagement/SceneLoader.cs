using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.SceneManagement
{
    public class SceneLoader : MonoBehaviour
    {
        // 序列化字段：在 Inspector 中拖拽绑定的加载进度条 UI 图片
        [SerializeField] private Image loadingBar;
        // 序列化字段：控制进度条填充的平滑速度系数
        [SerializeField] private float fillSpeed = 0.5f;
        // 序列化字段：加载界面的根 Canvas，用于控制整个加载界面的显示与隐藏
        [SerializeField] private Canvas loadingCanvas;
        // 序列化字段：加载界面专用的摄像机（防止加载时画面黑屏或穿帮）
        [SerializeField] private Camera loadingCamera;
        // 序列化字段：在 Inspector 中配置好的所有场景组数组（如主菜单组、关卡1组等）
        [SerializeField] private SceneGroup[] sceneGroups;

        private float targetProgress; // 目标进度值（由底层加载逻辑驱动，范围 0~1）
        private bool isLoading; // 标记当前是否处于加载状态，用于控制 Update 中的平滑逻辑
        
        // 实例化之前封装好的场景组管理器，负责底层的场景加载与卸载
        public readonly SceneGroupManager manager = new SceneGroupManager();

        private void Awake()
        {
            // 订阅底层管理器的事件，当场景加载/卸载完成时，在控制台打印日志方便调试
            manager.OnSceneLoaded += sceneName => Debug.Log("Loaded: " + sceneName);
            manager.OnSceneUnloaded += sceneName => Debug.Log("Unloaded: " + sceneName);
            manager.OnSceneGroupLoaded += () => Debug.Log("Scene group loaded");
        }

        // 游戏启动时，自动加载场景组数组中的第 0 个场景组（通常是主菜单或初始关卡）
        private async void Start()
        {
            await LoadSceneGroup(0);
        }

        private void Update()
        {
            // 如果不在加载状态，直接跳过，节省性能
            if (!isLoading) return;

            float currentFillAmount = loadingBar.fillAmount; // 获取进度条当前的实际填充量
            float progressDifference = Mathf.Abs(currentFillAmount - targetProgress); // 计算当前值与目标值的差距

            // 核心算法：动态计算平滑速度。差距越大，填充速度越快；差距越小，速度越慢（实现丝滑的缓动效果）
            float dynamicFillSpeed = progressDifference * fillSpeed;

            // 使用 Mathf.Lerp 进行平滑插值，让进度条的视觉表现极其流畅自然
            loadingBar.fillAmount = Mathf.Lerp(currentFillAmount, targetProgress, Time.deltaTime * dynamicFillSpeed);
        }

        // 对外暴露的公共异步方法：根据索引加载指定的场景组
        public async Task LoadSceneGroup(int index)
        {
            loadingBar.fillAmount = 0f; // 每次开始加载前，将进度条重置到 0
            targetProgress = 1f; // 设定视觉上的目标进度为 100%

            // 防御性编程：检查传入的索引是否越界
            if (index < 0 || index >= sceneGroups.Length)
            {
                Debug.LogError("Invalid scene group index: " + index);
                return;
            }

            // 实例化自定义的进度汇报器，用于接收底层的加载进度
            LoadingProgress progress = new LoadingProgress();
            // 当底层汇报进度时，更新 targetProgress。使用 Mathf.Max 是为了防止进度条在视觉上发生“倒退”
            progress.Progressed += target => targetProgress = Mathf.Max(target, targetProgress);
            
            EnableLoadingCanvas(); // 显示加载界面和加载摄像机
            await manager.LoadScenes(sceneGroups[index], progress); // 调用底层管理器，开始异步加载场景组
            EnableLoadingCanvas(false); // 加载完成后，隐藏加载界面
        }

        // 封装加载界面的显示与隐藏逻辑
        private void EnableLoadingCanvas(bool enable = true)
        {
            isLoading = enable; // 更新加载状态标记，控制 Update 中的平滑逻辑是否执行
            loadingCanvas.gameObject.SetActive(enable);
            loadingCamera.gameObject.SetActive(enable);
        }
    }

    // 自定义的进度汇报类，实现了 C# 标准的 IProgress<float> 接口
    public class LoadingProgress : IProgress<float>
    {
        public event Action<float> Progressed; // 当底层调用 Report 时，触发此事件向外传递进度值
        
        private const float ratio = 1f; // 预留的进度比例系数，方便后续调整进度的缩放比例

        // IProgress 接口必须实现的方法，由底层的 SceneGroupManager 在异步加载时不断调用
        public void Report(float value)
        {
            Progressed?.Invoke(value / ratio);
        }
    }
}