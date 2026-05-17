using System.Collections;
using System.Collections.Generic;

namespace Systems.Persistence
{
    // 数据服务接口：定义了游戏存档管理的标准操作规范（CRUD）
    public interface IDataService
    {
        // 保存数据：将当前的游戏数据（GameData）持久化存储，overwrite 用于控制是否覆盖同名存档
        void Save(GameData data, bool overwrite = true);
        
        // 读取数据：根据存档名称（name）加载并返回对应的游戏数据
        GameData Load(string name);
        
        // 删除单个存档：根据名称删除指定的存档文件
        void Delete(string name);
        
        // 删除所有存档：清空所有本地存档数据（通常用于开发调试或“恢复出厂设置”）
        void DeleteAll();
        
        // 列出所有存档：返回当前所有可用存档的名称集合（用于制作游戏的“读取存档”界面）
        IEnumerable<string> ListSaves();
    }
}