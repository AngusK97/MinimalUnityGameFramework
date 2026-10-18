# 用法索引与迁移

[框架总览](README.zh-CN.md) · [English](Build-in%20Managers%20Usage.md)

使用说明已拆为独立页面。**第一次先看[开始使用](docs/quick-start.zh-CN.md)**，完成第一个窗口；日常开发按需查模块手册。

| 任务 | 手册 |
| --- | --- |
| 准备公共服务 | [Core](docs/core.zh-CN.md) |
| 打开/关闭窗口 | [UI](docs/ui.zh-CN.md) |
| 编辑和生成 UI 清单 | [UI 配置表](docs/ui-tables.zh-CN.md) |
| 订阅/发送通知 | [事件](docs/events.zh-CN.md) |
| 持有和释放资产 | [资源](docs/resources.zh-CN.md) |
| 编辑和生成共享资源清单 | [资源配置表](docs/resource-tables.zh-CN.md) |
| 切换场景 | [场景](docs/scenes.zh-CN.md) |
| 播放声音 | [音频](docs/audio.zh-CN.md) |
| 打包与排错 | [构建](docs/build.zh-CN.md) |

编辑器与包版本升级见[升级到 Unity 6.6](docs/upgrading.zh-CN.md)。已有游戏可以继续固定旧框架与 Editor；下面的 API 迁移仅适用于决定采用当前框架的项目。

采用 `main` 目录时，将 `Assets/GameResources` 整体重命名为 `Assets/Framework`，保留目录 `.meta`；把 `Assets/Scripts` 及其 `.meta` 移入其中，再将 `Prefab` 及其 `.meta` 重命名为 `Prefabs`。将原生 Scene List 中的 InitScene 路径更新为 `Assets/Framework/Scenes/InitScene.unity`。GUID 与公共 API 不变，`GameContent/Demo` 保持原位；固定使用 **v0.1.0** 时保留旧目录。

## 旧项目迁移

| 旧版模板 | 当前方式 |
| --- | --- |
| 公共 UIName / EventName / ResourceModuleName 枚举 | 游戏自己的字符串标识；UI 常量由 UI.asset 生成 |
| 框架 Init 自动登记 Menu/Main | 自己的入口在 Core 就绪后登记 UI 和资源模块 |
| GameCore.Data / GameCore.Level | 自己游戏的数据和关卡控制器 |
| Core 的 Auto Start Game / StartupError / RetryStartup | Core 准备服务；游戏启动流程及重试策略由自己的入口负责 |

旧项目自定义 Core 预制体需移除 Data/Level 子对象与引用，保留五项公共服务，使用自己的启动入口。当前提供的预制体和 Demo 已迁移；生命周期边界见 [Core](docs/core.zh-CN.md)。

手动 UI 登记仍可用。迁移配置表时，保留 Name/Id Prefix 可保持 ID，移除旧手写常量类与重复登记，再按 [UI 配置表](docs/ui-tables.zh-CN.md) 操作。自己存档/消息中存过的标识需显式迁移。

手写 `BaseResourceModule` 派生类仍可使用。采用资源表时，保留模块 ID，移除旧定义，并只登记一次生成类。Demo 现使用 `Demo.MainResources`（`Id = "Demo.Main"`）：将 `MainResourceModule` 查找改为 `MainResources`，将 `main.GetRes<AudioClip>(MainResource.Cue)` 改为 `main.Cue`；原 Demo 专用类/枚举已移除。重复登记模块现在会抛异常；以错误类型取得已登记模块会抛出 `InvalidOperationException`。见[资源配置表](docs/resource-tables.zh-CN.md)。

事件现支持无参 Action 监听及 `DispatchNow(eventId)` / `Dispatch(eventId)`；原 sender/数据重载仍可使用。游戏维护字符串常量，订阅与收尾退订配对，不需要预登记事件名。见[事件](docs/events.zh-CN.md)。
