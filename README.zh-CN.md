# MinimalUnityGameFramework

[English](README.md) · 简体中文

**框架版本：** [v0.1.0](https://github.com/AngusK97/MinimalUnityGameFramework/releases/tag/v0.1.0) · **Unity：** 6.6（6000.6.5f1，非 LTS）

[开始使用](docs/quick-start.zh-CN.md)

Hello there ~ 我是 Angus，一名游戏开发者。这是一个处理 UI、事件、资源、场景和音频的小 Unity 框架。先玩一下 Demo，再用同样的工具做自己的游戏。

> **简单即美。** 小一点，清楚一点，也容易修改一点。

## 设计哲学

- **让流程明确。** 登记自己的内容，再打开首个界面。
- **知道谁来收尾。** Core 管服务，UI 管窗口，资源模块管自己加载的资产。
- **先用 Unity 的工具。** 组件、Inspector 和普通 C# 足以解决大部分问题。
- **按游戏需要增加。** 规则和数据留在游戏目录，需要共用时再提取。

## 宏观架构

**GameCore** 提供五项公共服务，切场景时保持运行。自己的游戏和可删除的 Demo 使用同一套 API。

```mermaid
flowchart TB
    Game["你的游戏"] --> Core["GameCore · 常驻入口"]
    Demo["Demo · 可删除示例"] --> Core
    Core --> UI["UI"]
    Core --> Events["Event"]
    Core --> Resources["Resource"]
    Core --> Scenes["Scene"]
    Core --> Audio["Sound"]
```

**准备服务 → 登记内容 → 打开首个界面 → 运行游戏。**

UI 通过 Resource 加载窗口；Scene 协调加载、渐变和 UI 收尾。启动与生命周期细节见 [Core](docs/core.zh-CN.md)。

## 按需要选择模块

| 我想…… | 手册 | 入口 |
| --- | --- | --- |
| 准备公共服务 | [Core](docs/core.zh-CN.md) | `GameCore.IsReady` |
| 打开或关闭窗口 | [UI](docs/ui.zh-CN.md) | `GameCore.UI` |
| 在 Inspector 定义界面 | [UI 配置表](docs/ui-tables.zh-CN.md) | `UI.asset` → `GameUI.RegisterAll()` |
| 发送通知 | [事件](docs/events.zh-CN.md) | `GameCore.Event` |
| 加载与释放资产 | [资源](docs/resources.zh-CN.md) | `GameCore.Resource` |
| 切换场景 | [场景](docs/scenes.zh-CN.md) | `GameCore.Scene` |
| 播放声音 | [音频](docs/audio.zh-CN.md) | `GameCore.Sound` |

## 内容放在哪里

以下是 `main` 的目录；**v0.1.0** 保留旧目录，升级见[目录迁移说明](Build-in%20Managers%20Usage.zh-CN.md)。

```text
Assets/
├── Framework/
│   ├── Scripts/       公共框架代码
│   ├── Prefabs/       公共 Core 预制体
│   └── Scenes/        只初始化服务的 InitScene
└── GameContent/
    ├── Demo/          完整、可整体删除的教学游戏
    └── MyGame/        自己的场景、内容、脚本和 Editor/Tables
```

阅读 Demo 可从 [DemoGame.cs](Assets/GameContent/Demo/Scripts/DemoGame.cs)、[UI.asset](Assets/GameContent/Demo/Editor/Tables/UI.asset) 或 [MenuView.cs](Assets/GameContent/Demo/Scripts/UI/MenuView.cs) 开始。自己的游戏跑通后，可以[整体删除 Demo](docs/quick-start.zh-CN.md#删除-demo)。

## 先体验一次

1. 在 Unity Hub 用上面的 Editor 版本打开仓库根目录，等待导入和编译完成。
2. 打开 `Assets/GameContent/Demo/Scenes/DemoStart.unity`，点击 Play。
3. 点击 **Start**，用 **WASD / 方向键**把蓝色方块移到 GOAL。

![Unity 6.6 中的 Demo 关卡，包含蓝色玩家方块和 GOAL](docs/images/demo-level.png)

**Play again** 重开，**Menu** 返回，**Settings / Escape** 打开音量设置。

**下一步：[创建自己的第一个窗口](docs/quick-start.zh-CN.md)** · [构建游戏](docs/build.zh-CN.md)

## 环境与范围

Addressables **2.11.2** · uGUI **2.6.0** · Built-in 渲染管线 · 旧 Input Manager。

需要固定快照时使用上面的发布版；`main` 持续更新，`0.x` 的 API 仍可能变化。旧游戏可以固定旧版。按需查看 [Editor 升级](docs/upgrading.zh-CN.md)、[旧 API 迁移](Build-in%20Managers%20Usage.zh-CN.md)和[已验证范围](docs/build.zh-CN.md#验证范围)。

存档、全局暂停、平台输入和发布验收，按自己的游戏需要补充。

---

如果你试用时卡住了，欢迎告诉我卡在哪一步：**anguskungcn@foxmail.com**。
