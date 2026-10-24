# 用法索引与迁移

[框架总览](README.zh-CN.md) · [English](Build-in%20Managers%20Usage.md)

**第一次先看[开始使用](docs/quick-start.zh-CN.md)**，完成第一个窗口；日常开发按需查下表。本套教程跟随 v0.3.0，固定的 [v0.2.0 教程](https://github.com/AngusK97/MinimalUnityGameFramework/blob/v0.2.0/docs/quick-start.zh-CN.md)保留原 Inspector/SO 流程。

| 任务 | 手册 |
| --- | --- |
| 准备公共服务 | [Core](docs/core.zh-CN.md) |
| 连接 Google Sheets 并更新表格 | [配置](docs/config.zh-CN.md) |
| 定义窗口 | [UI 配置表](docs/ui-tables.zh-CN.md) |
| 打开/关闭窗口 | [UI](docs/ui.zh-CN.md) |
| 订阅/发送通知 | [事件](docs/events.zh-CN.md) |
| 定义共享资源 | [资源配置表](docs/resource-tables.zh-CN.md) |
| 持有和释放资产 | [资源](docs/resources.zh-CN.md) |
| 定义与切换场景 | [场景](docs/scenes.zh-CN.md) |
| 播放声音 | [音频](docs/audio.zh-CN.md) |
| 打包与排错 | [构建](docs/build.zh-CN.md) |

编辑器与包版本升级见[升级到 Unity 6.6](docs/upgrading.zh-CN.md)。已有游戏可以继续固定旧框架与 Editor；选择采用当前框架时，在独立副本中迁移。

## 从 v0.2.0 迁移到 Google Sheets

1. 备份可运行的游戏。移除旧方案前，记录 UI 名称、预制体路径、资源名称／类型及模块 ID，保留 Unity 资产和 meta 文件。
2. 按[配置首次设置](docs/config-setup.zh-CN.md#首次设置)创建需要的 **UI**、**Resource**、**Scene** 标签。原 UI 条目的 Name 对应新的 **Id**；新的 **Name** 列只作说明。资源模块合并到 Resource 标签，通过 **Module** 列区分。
3. 保存原有值后，从迁移副本中移除旧 UI.asset、资源模块 SO 资产及其旧生成定义。首次导入前，将旧生成 C# 和 meta 备份到 Assets 之外；新导入器不会覆盖不属于自己的文件。如果把框架文件覆盖复制到旧安装中，也要移除遗留 SO 编辑器工具。若缺少生成类型导致编译失败，先暂时注释调用，首次更新后恢复。
4. 点击 **Framework → Config → Update Config**，等待编译，按下表恢复并修改调用。新来源设置、生成 CSV／清单、C#、JSON 和 meta 一起提交。此后只在 Google Sheets 编辑表格。

| 旧调用或数据 | v0.3.0 |
| --- | --- |
| `GameUI.RegisterAll()` / `DemoUI.RegisterAll()` | Core 就绪后调用 `GameConfig.LoadAndRegister()`，每个 Core 生命周期一次 |
| `GameUI.Shop` / `DemoUI.Menu` | `UIName.Shop` / `UIName.Menu` |
| Id Prefix + Name 组成 UI ID，例如 `MyGame.Shop` | 直接使用表中 Id，例如 `Shop`；`UIName.Shop = "Shop"` |
| 模块 SO 的 ID 为 `MyGame.Main` | Config 的 Namespace 为 `MyGame`，Resource 的 Module 为 `Main`；`MainResources.Id` 仍为 `MyGame.Main` |
| `TryTransition("Assets/.../Menu.unity")` | Scene 行 + `TryTransition(SceneName.Menu)`，或显式 `TryTransitionAddress("Assets/.../Menu.unity")` |

旧的带前缀 UI ID 不会自动保留，存档／消息中的 ID 需显式迁移。新专用表 ID 使用 ASCII C# 标识符，不能包含句点。运行中的游戏登记范围内，UI ID 和场景 ID 各自保持唯一。

创建资源模块之前先加载 GameConfig。资源仍显式登记和加载：`new MainResources()`、`GameCore.Resource.RegisterResourceModule(main)`，再调用 `main.LoadResources(...)`。完整写法见[资源](docs/resources.zh-CN.md)；访问 `main.Cue` 不会自动加载。

Demo 附带本地生成数据，ConfigSource.json 初始未绑定云端，因此无需 Google 凭据即可运行。需要编辑时，将它的三份 CSV 快照导入为名为 **UI**、**Resource**、**Scene** 的 Sheets 标签，再通过 Config 窗口绑定自己的文件夹。首次连接要求三张表全部导入成功，否则保留整份随包快照；后续更新才按表保留旧版。CSV 是快照，不是另一份编辑源。

## 较早的目录迁移

从 **v0.1.0 升级到 v0.2.0** 时，将 `Assets/GameResources` 整体重命名为 `Assets/Framework`，保留目录 `.meta`；把 `Assets/Scripts` 及其 `.meta` 移入其中，再将 `Prefab` 及其 `.meta` 重命名为 `Prefabs`。将原生 Scene List 中的 InitScene 路径更新为 `Assets/Framework/Scenes/InitScene.unity`。目录移动保留 GUID 与公共 API，`GameContent/Demo` 保持原位；固定使用 **v0.1.0** 时保留旧目录。

## 较早的 API 迁移

| 旧版模板 | 当前方式 |
| --- | --- |
| 公共 UIName / EventName / ResourceModuleName 枚举 | 游戏自己的字符串 ID，以及生成的 UI／场景常量 |
| 框架 Init 自动登记 Menu/Main | 自己的入口在 Core 就绪后加载配置并登记资源模块 |
| GameCore.Data / GameCore.Level | 自己游戏的数据和关卡控制器 |
| Core 的 Auto Start Game / StartupError / RetryStartup | Core 准备服务；启动流程及重试策略由自己的入口负责 |

旧项目自定义 Core 预制体需移除 Data/Level 子对象与引用，保留五项公共服务，使用自己的启动入口。生命周期边界见 [Core](docs/core.zh-CN.md)。手动 UI 登记和手写 `BaseResourceModule` 派生类仍可使用，避免重复登记。

Demo 使用 `Demo.MainResources`（`Id = "Demo.Main"`）：将 `MainResourceModule` 查找改为 `MainResources`，将 `main.GetRes<AudioClip>(MainResource.Cue)` 改为 `main.Cue`。重复登记模块会抛异常；以错误类型取得已登记模块会抛出 `InvalidOperationException`。

事件支持无参 Action 监听及 `DispatchNow(eventId)` / `Dispatch(eventId)`；原 sender/数据重载仍可使用。游戏维护字符串常量，订阅与收尾退订配对，不需要预登记事件名。见[事件](docs/events.zh-CN.md)。
