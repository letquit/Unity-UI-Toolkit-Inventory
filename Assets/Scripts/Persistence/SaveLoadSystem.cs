using System;
using System.Collections.Generic;
using System.Linq;
using Systems.Inventory;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Systems.Persistence
{
    // 游戏总数据容器：必须加上 [Serializable] 特性，JsonUtility 才能识别并序列化它
    [Serializable]
    public class GameData
    {
        public string Name; // 存档名称（如 "Save1"）
        public string CurrentLevelName; // 当前所在的场景名称（用于读档后跳转）
        public PlayerData playerData; // 玩家属性数据（后续需定义）
        public InventoryData inventoryData; // 背包数据（后续需定义）
    }

    // 可保存物体的标识接口：所有需要存档的游戏物体（如 Hero, Inventory）都要实现它
    public interface ISaveable
    {
        SerializableGuid Id { get; set; } // 拥有唯一的 GUID 标识
    }

    // 数据绑定接口：定义了如何将数据对象（TData）绑定到具体的游戏物体上
    public interface IBind<TData> where TData : ISaveable
    {
        SerializableGuid Id { get; set; }
        void Bind(TData data); // 接收数据对象，并更新自身的状态
    }
    
    // 继承自持久化单例基类，确保场景切换时存档系统不被销毁
    public class SaveLoadSystem : PersistentSingleton<SaveLoadSystem>
    {
        [SerializeField] public GameData gameData; // 当前在内存中运行的总游戏数据
        
        private IDataService dataService; // 依赖注入：上一轮实现的本地文件存取服务

        protected override void Awake()
        {
            base.Awake();
            // 组合注入：将本地文件服务（FileDataService）和 JSON 翻译官（JsonSerializer）组装起来
            dataService = new FileDataService(new JsonSerializer());
        }
        
        // 注册场景加载完成的事件监听
        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // 每当新场景加载完毕时，自动触发数据与物体的绑定逻辑
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Menu") return; // 菜单场景不需要绑定游戏数据
    
            // 自动寻找场景中的 Hero 物体，并将其与 playerData 绑定
            gameData.playerData = Bind<Hero, PlayerData>(gameData.playerData);
            // 自动寻找场景中的 Inventory 物体，并将其与 inventoryData 绑定
            gameData.inventoryData = Bind<Systems.Inventory.Inventory, InventoryData>(gameData.inventoryData);
        }

        // 核心泛型绑定方法（单体绑定）：将数据对象绑定到场景中唯一的物体上
        private TData Bind<T, TData>(TData data)
            where T : MonoBehaviour, IBind<TData> // T 必须是挂载在物体上的脚本，且实现了 IBind 接口
            where TData : ISaveable, new() // TData 必须是可保存的数据类，且有默认构造函数
        {
            // 在场景中自动寻找第一个类型为 T 的组件（比如找到场景里的 Hero）
            var entity = FindObjectsByType<T>(FindObjectsSortMode.None).FirstOrDefault();
            if (entity == null) return data; // 如果场景里没找到这个物体，直接返回原数据

            // 如果是新游戏（data 为 null），则根据物体的 ID 创建一个新的数据对象
            if (data == null)
            {
                data = new TData { Id = entity.Id };
            }

            // 核心步骤：调用物体上的 Bind 方法，将数据对象“注入”给物体，物体据此更新自己的 UI 和状态
            entity.Bind(data);
            return data;
        }

        // 核心泛型绑定方法（集合绑定）：用于处理场景中可能有多个的同类型物体（如多个宝箱、多个敌人）
        private void Bind<T, TData>(List<TData> datas) where T : MonoBehaviour, IBind<TData>
            where TData : ISaveable, new()
        {
            var entities = FindObjectsByType<T>(FindObjectsSortMode.None);

            foreach (var entity in entities)
            {
                // 根据 ID 在数据列表中查找对应的数据
                var data = datas.FirstOrDefault(d => d.Id == entity.Id);
                if (data == null)
                {
                    // 如果没找到（说明是新物体），则创建新数据并加入列表
                    data = new TData { Id = entity.Id };
                    datas.Add(data);
                }
                entity.Bind(data); // 将数据绑定给物体
            }
        }

        // 开启新游戏：初始化空的总数据容器，并加载第一个游戏场景
        public void NewGame()
        {
            gameData = new GameData
            {
                Name = "Game", 
                CurrentLevelName = "Demo"
            };
            SceneManager.LoadScene(gameData.CurrentLevelName);
        }

        // 保存游戏：调用数据服务，将当前的总数据容器序列化并写入本地硬盘
        public void SaveGame() => dataService.Save(gameData);

        // 读取游戏：从硬盘读取数据覆盖当前的内存数据，并跳转到存档记录的场景
        public void LoadGame(string gameName)
        {
            gameData = dataService.Load(gameName);

            if (String.IsNullOrWhiteSpace(gameData.CurrentLevelName))
            {
                gameData.CurrentLevelName = "Demo";
            }
            
            SceneManager.LoadScene(gameData.CurrentLevelName);
        }

        // 重载当前游戏：重新读取当前存档（常用于角色死亡后重置状态）
        public void ReloadGame() => LoadGame(gameData.Name);
        
        // 删除游戏：调用数据服务删除本地指定的存档文件
        public void DeleteGame(string gameName) => dataService.Delete(gameName);
    }
}