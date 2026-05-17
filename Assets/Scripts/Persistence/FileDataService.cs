using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Systems.Persistence
{
    // 继承 IDataService 接口，提供基于本地文件系统的具体存档管理实现
    public class FileDataService : IDataService
    {
        private ISerializer serializer; // 依赖注入：上一轮实现的“翻译官”
        private string dataPath;        // 本地存档的根目录路径
        private string fileExtension;   // 存档文件的后缀名（如 .json）

        public FileDataService(ISerializer serializer)
        {
            // 使用 Unity 提供的跨平台持久化数据路径，确保在 PC、手机等不同设备上都能正确存取
            this.dataPath = Application.persistentDataPath;
            this.fileExtension = "json";
            this.serializer = serializer;
        }

        // 私有辅助方法：根据文件名拼接出完整的本地文件绝对路径
        private string GetPathToFile(string fileName)
        {
            return Path.Combine(dataPath, string.Concat(fileName, ".", fileExtension));
        }
        
        // 保存数据：将 GameData 对象序列化并写入本地文件
        public void Save(GameData data, bool overwrite = true)
        {
            string fileLocation = GetPathToFile(data.Name);

            // 防御性编程：如果不允许覆盖且文件已存在，则主动抛出异常，防止误删玩家旧存档
            if (!overwrite && File.Exists(fileLocation))
            {
                throw new IOException(
                    $"The file '{data.Name}.{fileExtension}' already exists and cannot be overwritten.");
            }
            
            // 1. 调用 serializer 将 GameData 对象翻译成 JSON 字符串
            // 2. 使用 File.WriteAllText 将字符串一次性写入本地文件
            File.WriteAllText(fileLocation, serializer.Serialize(data));
        }

        // 读取数据：从本地文件读取 JSON 字符串并反序列化为 GameData 对象
        public GameData Load(string name)
        {
            string fileLocation = GetPathToFile(name);
            
            // 如果存档文件不存在，抛出异常提示调用者（比如提示玩家“未找到存档”）
            if (!File.Exists(fileLocation))
            {
                throw new ArgumentException($"No persisted GameData with name '{name}'");
            }

            // 1. 使用 File.ReadAllText 读取本地文件的完整内容
            // 2. 调用 serializer 将 JSON 字符串还原为 GameData 对象
            return serializer.Deserialize<GameData>(File.ReadAllText(fileLocation));
        }

        // 删除单个存档：根据文件名删除指定的本地存档文件
        public void Delete(string name)
        {
            string fileLocation = GetPathToFile(name);

            if (File.Exists(fileLocation))
            {
                File.Delete(fileLocation);
            }
        }

        // 删除所有存档：遍历并删除存档目录下的所有文件
        public void DeleteAll()
        {
            foreach (string filePath in Directory.GetFiles(dataPath))
            {
                File.Delete(filePath);
            }
        }

        // 列出所有存档：扫描目录，筛选出指定后缀的文件名并返回
        public IEnumerable<string> ListSaves()
        {
            // 使用 yield return 实现延迟执行，遍历大目录时性能更好
            foreach (string path in Directory.EnumerateFiles(dataPath))
            {
                // 只筛选出后缀名为 .json 的存档文件
                if (Path.GetExtension(path) == fileExtension)
                {
                    yield return Path.GetFileNameWithoutExtension(path);
                }
            }
        }
    }
}