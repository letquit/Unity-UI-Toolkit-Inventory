using System;
using System.Collections.Generic;
using System.Linq;
using Eflatun.SceneReference; // 引入强大的场景引用库，彻底告别用字符串管理场景的噩梦
using UnityEngine;

namespace Systems.SceneManagement
{
    // 场景组类：用于将多个相关的场景打包在一起（比如“主菜单组”包含菜单背景、UI、特效等场景）
    [Serializable]
    public class SceneGroup
    {
        public string GroupName = "New Scene Group"; // 场景组的名称，方便在 Inspector 中识别
        public List<SceneData> Scenes; // 该组内包含的所有场景数据列表

        // 核心工具方法：根据指定的场景类型（如 MainMenu），在列表中查找并返回对应的场景名称
        public string FindSceneNameByType(SceneType sceneType)
        {
            // 使用 LINQ 的 FirstOrDefault 优雅地筛选出符合条件的场景，并安全地提取其名称
            // 如果没找到，FirstOrDefault 会返回 null，此时通过 ?. 运算符安全返回 null，不会报错
            return Scenes.FirstOrDefault(scene => scene.SceneType == sceneType)?.Reference.Name;
        }
    }
    
    // 场景数据类：单个场景的元数据封装
    [Serializable]
    public class SceneData
    {
        // SceneReference 是 Eflatun 库的核心类型。它在 Inspector 中提供一个下拉菜单让你选场景，
        // 即使场景文件被移动或重命名，它也能自动更新路径，极其稳健！
        public SceneReference Reference;
        
        // 只读属性：对外提供该场景的纯字符串名称（供 Unity 原生的 SceneManager 加载使用）
        public string Name => Reference.Name;
        
        public SceneType SceneType; // 标记该场景的用途（如 HUD、环境、主菜单等）
    }
    
    // 场景类型枚举：用强类型代替魔法字符串，明确界定场景中各个部分的职责
    public enum SceneType
    {
        ActiveScene,    // 当前主活跃场景（比如具体的某个关卡）
        MainMenu,       // 主菜单场景
        UserInterface,  // 全局 UI 界面
        HUD,            // 游戏内的抬头显示（血条、小地图等）
        Cinematic,      // 过场动画场景
        Environment,    // 纯环境场景（天空盒、地形等）
        Tooling         // 开发工具或调试场景
    }
}