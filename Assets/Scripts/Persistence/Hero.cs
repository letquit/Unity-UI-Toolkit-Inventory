using System;
using Systems.Inventory;
using Systems.Persistence;
using UnityEngine;

namespace Systems
{
    // 继承 MonoBehaviour，并实现 IBind<PlayerData> 接口，让英雄物体能够接入自动化存档系统
    public class Hero : MonoBehaviour, IBind<PlayerData>
    {
        // 使用 [field: SerializeField] 特性，让自动实现的属性 Id 也能在 Unity 的 Inspector 面板中显示并持久化
        [field: SerializeField] 
        public SerializableGuid Id { get; set; } = SerializableGuid.NewGuid(); // 默认生成一个全局唯一的 GUID
        
        [SerializeField] private PlayerData data; // 缓存当前英雄绑定的数据对象

        // 实现 IBind 接口的 Bind 方法：当存档系统加载数据时，会自动调用此方法
        public void Bind(PlayerData data)
        {
            this.data = data; // 将外部传入的存档数据对象赋值给本地缓存
            this.data.Id = Id; // 确保数据对象中的 Id 与当前游戏物体的 Id 保持一致
            
            // 读档核心逻辑：将存档中记录的位置和旋转，同步给当前的游戏物体
            transform.position = data.position;
            transform.rotation = data.rotation;
        }
        
        // 实时更新：每一帧都将英雄当前的实际位置和旋转，同步写入到数据对象中
        private void Update()
        {
            data.position = transform.position;
            data.rotation = transform.rotation;
        }
    }

    // 玩家数据类：必须加上 [Serializable] 特性，才能被 JsonUtility 序列化为 JSON 字符串
    [Serializable]
    public class PlayerData : ISaveable
    {
        [field: SerializeField] 
        public SerializableGuid Id { get; set; } // 实现 ISaveable 接口，作为该数据的唯一标识
        
        public Vector3 position; // 记录玩家的世界坐标
        public Quaternion rotation; // 记录玩家的四元数旋转
    }
}