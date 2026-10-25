# MinimalUnityGameFramework

[English](README.md) · 简体中文

**框架版本：** v0.5.0（本地 Excel 配置） · **Unity：** 6.6（6000.6.5f1，非 LTS）

[下载 v0.5.0（ZIP）](https://github.com/AngusK97/MinimalUnityGameFramework/archive/refs/tags/v0.5.0.zip) · [开始使用](docs/quick-start.zh-CN.md)

这套教程适用于 **v0.5.0**。更新了什么，可以看[发布说明](https://github.com/AngusK97/MinimalUnityGameFramework/releases/tag/v0.5.0)；旧工程升级请看[迁移步骤](Build-in%20Managers%20Usage.zh-CN.md)。下载后，使用包内的文档即可。

Hello there ~ 我是 Angus，一名游戏开发者。

我做这个小框架，是想把 UI、事件、资源、场景和音频这些基础工作整理好，方便自己做游戏，也分享给你。希望它能帮你少做一些重复工作，多花点时间在自己的玩法上。

> **简单即美。** 容易上手，容易理解，也容易修改。

<a id="设计哲学"></a>

## 01 设计哲学

写这个框架时，我主要在意这几件事：

- **常用的操作，直接一点。** 配置方便改，功能方便调用。
- **把流程和分工写清楚。** 什么时候初始化、加载和释放，由哪段代码负责，都能看明白。
- **同一份配置，只在一个地方改。** 修改源数据，让工具生成代码和运行数据。
- **用合适的现成工具。** 组件参数用 Inspector，批量配置用表格，游戏逻辑用普通 C#。
- **用到了，再扩展。** 玩法先写在自己的游戏里，重复用到时再整理成公共功能。

<a id="宏观架构"></a>

## 02 宏观架构

**GameCore** 提供五个公共模块，切换场景时也会保留。你自己的游戏和 Demo 都通过同一套 API 使用它们。

配置的流程是：**改表 → Update Config → 生成代码和数据 → 游戏读取**。默认用本地 Excel，需要多人在线编辑时可以换成 Google Sheets。运行游戏只读取已生成的数据，不需要联网。

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

**准备公共服务 → 加载配置、登记内容 → 打开第一个界面 → 开始游戏。**

UI 通过 Resource 加载界面；Scene 负责场景加载、淡入淡出和旧界面的清理。准备接入自己的游戏时，可以跟着 [Core](docs/core.zh-CN.md) 设置启动入口和退出清理。

<a id="按需要选择模块"></a>

## 03 按需要选择模块

第一次使用，先跟着[开始使用](docs/quick-start.zh-CN.md)跑 Demo、做一个自己的窗口。有具体需求时，再查下面的表。配置来源的设置与切换放在[配置接入](docs/config-setup.zh-CN.md)。

| 我想…… | 教程 | 从哪里调用 |
| --- | --- | --- |
| 准备公共服务 | [Core](docs/core.zh-CN.md) | `GameCore.IsReady` |
| 打开或关闭窗口 | [UI](docs/ui.zh-CN.md) | `GameCore.UI` |
| 编辑和读取配置 | [配置表](docs/config.zh-CN.md) | **Update Config** → `GameConfig.LoadAndRegister()` |
| 登记 UI 界面 | [UI 配置表](docs/ui-tables.zh-CN.md) | `UI` 工作表 → `UIName.Shop` |
| 发送通知 | [事件](docs/events.zh-CN.md) | `GameCore.Event` |
| 加载与释放资源 | [资源](docs/resources.zh-CN.md) | `GameCore.Resource` |
| 登记资源 | [资源配置表](docs/resource-tables.zh-CN.md) | `Resource` 工作表 → `MainResources` |
| 登记与切换场景 | [场景](docs/scenes.zh-CN.md) | `Scene` 工作表 → `SceneName.Menu` |
| 播放声音 | [音频](docs/audio.zh-CN.md) | `GameCore.Sound` |

<a id="内容放在哪里"></a>

## 04 内容放在哪里

Framework 放公共框架。自己的场景、脚本、配置来源设置和生成数据，都放进自己的游戏目录，之后找起来也方便。旧工程调整目录时，可参考[迁移说明](Build-in%20Managers%20Usage.zh-CN.md)。

```text
Assets/
├── Framework/
│   ├── Scripts/       公共框架代码
│   ├── Prefabs/       公共 Core 预制体
│   └── Scenes/        只初始化服务的 InitScene
└── GameContent/
    ├── Demo/          完整、可整体删除的教学游戏
    └── MyGame/        自己的场景、内容、来源设置和生成数据
```

想看 Demo 怎么写，可以从 [DemoGame.cs](Assets/GameContent/Demo/Scripts/DemoGame.cs)、[Demo.xlsx](Assets/GameContent/Demo/Editor/Tables/Source/Demo.xlsx) 或 [MenuView.cs](Assets/GameContent/Demo/Scripts/UI/MenuView.cs) 开始。第一次改表时，在 **Framework → Config** 中选择 Demo；以后保存 Demo.xlsx，再点 **Update Config**。自己的游戏跑通后，可以[整体删除 Demo](docs/quick-start.zh-CN.md#03-删除-demo)。

<a id="先体验一次"></a>

## 05 先体验一次

Demo 自带 Excel 源表和生成配置，本地编辑和运行都不需要 Google 账号。

1. 在 Unity Hub 中，用上面的 Unity 版本打开工程根目录，等待导入和编译完成。
2. 打开 `Assets/GameContent/Demo/Scenes/DemoStart.unity`，点击 Play。
3. 点击 **Start**，用 **WASD / 方向键**把蓝色方块移到 GOAL。

![Unity 6.6 中的 Demo 关卡，包含蓝色玩家方块和 GOAL](docs/images/demo-level.png)

**Play again** 重开，**Menu** 返回，**Settings / Escape** 打开音量设置。

**下一步：[创建自己的第一个窗口](docs/quick-start.zh-CN.md)** · [构建游戏](docs/build.zh-CN.md)

<a id="环境与范围"></a>

## 06 环境与范围

Addressables **2.11.2** · uGUI **2.6.0** · Built-in 渲染管线 · 旧 Input Manager。

上面的下载链接和教程适用于 **v0.5.0**，默认使用本地 Excel 配置，也可以选择 Google Sheets。`0.x` 阶段的 API 仍可能调整，已有游戏可以继续使用原来的版本和文档。需要升级时，再查看 [Unity 升级](docs/upgrading.zh-CN.md)、[旧 API 迁移](Build-in%20Managers%20Usage.zh-CN.md)和[已验证范围](docs/build.zh-CN.md#04-验证范围)。

玩法参数用 Inspector 或游戏自己的 ScriptableObject 就好。文本／本地化表和声音播放配置表还没加入，AudioClip 资源已经可以在资源表中登记。存档、全局暂停和不同平台的输入，按自己游戏的需要补充；准备发布时，再到目标平台上测试。

---

如果你试用时卡住了，欢迎告诉我卡在哪一步：**anguskungcn@foxmail.com**。
