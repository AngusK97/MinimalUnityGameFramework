# 资源配置表

[框架总览](../README.zh-CN.md) · [配置接入](config-setup.zh-CN.md) · [English](resource-tables.md)

**编辑并保存 Resource 工作表 → Update Config → 加载模块 → 使用 main.Cue。** 一个模块用来管理需要一起加载、一起释放的资产。如果只是组件固定使用的资产，通常在 Inspector 中直接引用就够了。

<a id="添加资源"></a>

## 01 添加资源

1. 将 **cue.wav** 放入 `Assets/GameContent/MyGame/Content/Audio/`。在 **Project** 选中它，勾选 Inspector 中的 **Addressable**。
2. 下面是 **Resource** 表的示例。第一次使用时，按[配置接入](config-setup.zh-CN.md)新建本地工作簿，填入这些内容。如果已经接入，在当前来源中打开已有的 **Resource** 工作表标签，没有就创建一个。新建的工作表从 A1 开始填入全部内容；已有的工作表保留前两行，只添加 **Cue** 这一行数据。

| Id | Name | Type | Module | AssetPath |
| --- | --- | --- | --- | --- |
| string | string | string | string | string |
| Cue | Button sound | AudioClip | Main | Assets/GameContent/MyGame/Content/Audio/cue.wav |

3. 第一次连接时，用上面的表按[配置接入](config-setup.zh-CN.md)完成设置。如果已经连接，先确认 **Framework → Config** 中选的是 **MyGame**。保存工作簿（Google Sheets 则等云端保存完成），再点击顶部工具栏左侧的 **Update Config**，等待更新结果和编译完成。

**成功标志：** 已生成 `MyGame/Scripts/Generated/MainResources.g.cs`，其中包含 `Id = "MyGame.Main"` 和 `AudioClip Cue` 属性。工具还会生成 ResourceTable、ResourceConfig 和本地 JSON。`Name` 只用于说明，不会生成字段。

| Type | 属性类型 |
| --- | --- |
| `AudioClip` | `UnityEngine.AudioClip` |
| `Prefab` | `UnityEngine.GameObject` |
| `Sprite` | `UnityEngine.Sprite` |

表中引用的资产必须存在，并且能通过 Addressables 加载。记得为每个资产勾选 **Addressable**，只放进 GameContent 目录还不够。Sprite 要使用 Single 模式的纹理或独立的 Sprite 主资产；暂不支持切片 Sprite 和其他子资产。

<a id="使用模块"></a>

## 02 使用模块

先按[资源示例](resources.zh-CN.md)创建、登记并加载模块。如果已有 **MyGameEntry.cs**，按其中的合并说明接入即可。每次 Core 启动，配置只加载一次，每个模块只登记一次。

示例中的 `main` 是一个 `MainResources` 字段。加载成功后会调用 `PlayCue()`，把下面这行放在这个回调方法中：

```csharp
GameCore.Sound.PlaySound(SoundLayer.UI, main.Cue);
```

这行代码需要 `using Core; using Sound;`。访问属性不会自动加载资源。给已有模块新增一行资源，不用修改启动代码；新增模块时，才需要另写登记和加载代码。

<a id="修改资源"></a>

## 03 修改资源

- **新增或替换：** 修改对应行，保存并更新配置，再使用生成的属性。只要 Id 不变，原有调用就不用改。
- **移动资产：** 修改 AssetPath，保存并更新配置。
- **重命名或删除 Id/Module：** 更新配置后，同步修改调用代码和模块登记。模块 ID 为 `<namespace>.<Module>`。
- **提交：** 把本地工作簿、来源设置、生成的 CSV、清单、C#、JSON、资产和 meta 一起提交。如果有冲突，先在当前源表中解决，再重新导入，不要手工拼接生成文件。

先用 **Main** 就可以。等到有些资产需要单独加载和释放时，再使用 **Level01** 等其他 Module 值。资产缺失或类型错误都会报错；保留旧版和恢复的方法见[配置表](config.zh-CN.md)。生成的内部资源编号不能用作存档或网络 ID。

下一步：[登记、加载与释放模块](resources.zh-CN.md)。
