# 用法索引与迁移

[框架总览](README.zh-CN.md) · [English](Build-in%20Managers%20Usage.md)

**第一次使用，先跟着[开始使用](docs/quick-start.zh-CN.md)做一个窗口。** 之后需要哪个功能，再查下面的表。这套教程适用于 v0.5.0；[v0.2.0 教程](https://github.com/AngusK97/MinimalUnityGameFramework/blob/v0.2.0/docs/quick-start.zh-CN.md)仍介绍旧的 Inspector/SO 配置方式。

**从 v0.3.x／v0.4.x 升级到 v0.5.0：** 旧设置中已有 Google `FolderId` 的游戏会继续使用 Google，不会在升级时自动改成本地 Excel。表格格式和运行 API 不变，新游戏默认使用 **Local Excel**。已有游戏想改用本地工作簿时，先将相同的表导出为 `.xlsx`，再按[切换已有来源](docs/config-setup.zh-CN.md#03-切换已有来源)操作；目标表必须全部通过检查，表名和结构与原配置一致。

日常更新仍点击顶部工具栏左侧的 **Update Config**，在 **Framework → Config** 中选择游戏和来源。两种来源的进度和结果窗口一样；某张表检查不通过时，都会保留它上次成功导入的版本。

| 想做什么 | 教程 |
| --- | --- |
| 准备公共服务 | [Core](docs/core.zh-CN.md) |
| 编辑 Excel 或 Google Sheets 并更新表格 | [配置](docs/config.zh-CN.md) |
| 登记 UI 界面 | [UI 配置表](docs/ui-tables.zh-CN.md) |
| 打开/关闭窗口 | [UI](docs/ui.zh-CN.md) |
| 订阅/发送通知 | [事件](docs/events.zh-CN.md) |
| 登记资源 | [资源配置表](docs/resource-tables.zh-CN.md) |
| 加载与释放资源 | [资源](docs/resources.zh-CN.md) |
| 登记与切换场景 | [场景](docs/scenes.zh-CN.md) |
| 播放声音 | [音频](docs/audio.zh-CN.md) |
| 打包与排错 | [构建](docs/build.zh-CN.md) |

Unity 和依赖包怎么升级，见[升级到 Unity 6.6](docs/upgrading.zh-CN.md)。已有游戏可以继续使用旧版框架和编辑器；决定升级时，先复制一份工程，在副本里操作。

<a id="从-v020-迁移到-google-sheets"></a>

<a id="01-从-v020-迁移到-google-sheets"></a>

## 01 从 v0.2.0 迁移到配置表

1. 备份当前能运行的工程。删除旧配置前，记下 UI 名称、预制体路径、资源名称／类型和模块 ID。Unity 资产及其 meta 文件都要保留。
2. 按[首次设置](docs/config-setup.zh-CN.md#01-首次设置)创建需要的 **UI**、**Resource**、**Scene** 工作表。把原来 UI 条目的 Name 填到新的 **Id** 列；新的 **Name** 列只写给人看的说明。各个资源模块合并到 Resource 表，用 **Module** 列区分。
3. 确认旧配置已备份，再从工程副本中移除 UI.asset、资源模块 SO 和旧生成代码。旧生成的 C# 及其 meta 要备份到 Assets 外，新导表工具不会覆盖不归它管理的文件。如果是把新版框架复制进旧工程，还要移除旧 SO 编辑工具。删除旧类型后若出现编译错误，先暂时注释相关调用，完成首次更新后再恢复。
4. 在 Config 设置窗口点击 **Update Config**，等待更新结果和编译完成，再按下表修改并恢复调用。本地工作簿、来源设置、生成的 CSV／清单、C#、JSON 和 meta 文件一起提交。以后在选定的来源中改表。

| 旧调用或数据 | v0.5.0 |
| --- | --- |
| `GameUI.RegisterAll()` / `DemoUI.RegisterAll()` | Core 就绪后调用 `GameConfig.LoadAndRegister()`，每个 Core 生命周期一次 |
| `GameUI.Shop` / `DemoUI.Menu` | `UIName.Shop` / `UIName.Menu` |
| Id Prefix + Name 组成 UI ID，例如 `MyGame.Shop` | 直接使用表中 Id，例如 `Shop`；`UIName.Shop = "Shop"` |
| 模块 SO 的 ID 为 `MyGame.Main` | Config 的 Namespace 为 `MyGame`，Resource 的 Module 为 `Main`；`MainResources.Id` 仍为 `MyGame.Main` |
| `TryTransition("Assets/.../Menu.unity")` | 在 Scene 表中登记后调用 `TryTransition(SceneName.Menu)`；直接传地址则用 `TryTransitionAddress("Assets/.../Menu.unity")` |

旧的带前缀 UI ID 不会自动保留。存档或消息里如果也用了这些 ID，需要一起修改。UI、Resource 和 Scene 表的 ID 必须是合法的 ASCII C# 标识符，不能带句点。一个正在运行的游戏中，已登记的 UI ID 不能重复，场景 ID 也一样。

创建资源模块前，先加载 GameConfig，再在代码中创建、登记并加载模块：`new MainResources()`、`GameCore.Resource.RegisterResourceModule(main)`，再调用 `main.LoadResources(...)`。完整写法见[资源](docs/resources.zh-CN.md)；访问 `main.Cue` 不会自动加载。

Demo 现在附带 `Editor/Tables/Source/Demo.xlsx`，默认从这个本地来源更新。在 **Framework → Config** 中选择 Demo，修改并保存工作簿后，再点更新。已有游戏继续使用各自的来源设置。生成的 CSV 保存导表结果，需要修改时回到源表中改。

<a id="较早的目录迁移"></a>

## 02 旧版目录如何迁移

从 **v0.1.0 升级到 v0.2.0** 时，将 `Assets/GameResources` 整体重命名为 `Assets/Framework`，保留目录 `.meta`；把 `Assets/Scripts` 及其 `.meta` 移入其中，再将 `Prefab` 及其 `.meta` 重命名为 `Prefabs`。把 Unity Scene List 中的 InitScene 路径更新为 `Assets/Framework/Scenes/InitScene.unity`。移动目录时保留 GUID，公共 API 不变，`GameContent/Demo` 保持原位；固定使用 **v0.1.0** 时保留旧目录。

<a id="较早的-api-迁移"></a>

## 03 旧版 API 如何迁移

| 旧版模板 | 当前方式 |
| --- | --- |
| 公共 UIName / EventName / ResourceModuleName 枚举 | 游戏自己的字符串 ID，以及生成的 UI／场景常量 |
| 框架 Init 自动登记 Menu/Main | 自己的入口在 Core 就绪后加载配置并登记资源模块 |
| GameCore.Data / GameCore.Level | 自己游戏的数据和关卡控制器 |
| Core 的 Auto Start Game / StartupError / RetryStartup | Core 准备服务；启动流程及重试策略由自己的入口负责 |

旧工程如果自定义过 Core 预制体，需要移除其中的 Data/Level 子对象及引用，保留五项公共服务，并改用自己的启动脚本。初始化和清理的规则见 [Core](docs/core.zh-CN.md)。仍然可以手动登记 UI、编写 `BaseResourceModule` 派生类，但同一项不要重复登记。

Demo 使用 `Demo.MainResources`（`Id = "Demo.Main"`）：将 `MainResourceModule` 查找改为 `MainResources`，将 `main.GetRes<AudioClip>(MainResource.Cue)` 改为 `main.Cue`。重复登记模块会抛异常；获取已登记模块时，若指定的类型不匹配，会抛出 `InvalidOperationException`。

你可以用无参 Action 订阅事件，再通过 `DispatchNow(eventId)` / `Dispatch(eventId)` 发送通知。原来带 sender 和数据的重载仍可使用。事件名在自己的游戏里定义成字符串常量即可，不需要提前登记；订阅后记得在对应的清理阶段退订。具体例子见[事件](docs/events.zh-CN.md)。
