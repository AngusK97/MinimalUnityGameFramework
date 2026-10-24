# 资源配置表

[框架总览](../README.zh-CN.md) · [配置接入](config-setup.zh-CN.md) · [English](resource-tables.md)

**编辑 Resource 标签 → Update Config → 加载模块 → 使用 main.Cue。** 模块将加载生命周期相同的资产放在一起。组件自己的固定资产，用 Inspector 引用也许就够了。

## 添加资源

1. 将 **cue.wav** 放入 `Assets/GameContent/MyGame/Content/Audio/`。在 **Project** 选中它，勾选 Inspector 中的 **Addressable**。
2. 下面是 **Resource** 表的示例。首次连接时，在[配置接入](config-setup.zh-CN.md)第 6 步填写；已经连接时，打开已有的 **Resource** 标签，没有时再创建。新标签从 A1 输入全部内容；已有标签保留前两行，只添加 **Cue** 数据行。

| Id | Name | Type | Module | AssetPath |
| --- | --- | --- | --- | --- |
| string | string | string | string | string |
| Cue | Button sound | AudioClip | Main | Assets/GameContent/MyGame/Content/Audio/cue.wav |

3. 第一次连接时，按[配置接入](config-setup.zh-CN.md)使用上面的表完成设置。已经连接时，在 **Framework → Config** 选择 **MyGame**，点击 **Update Config**，等待编译完成。

**成功标志：** 出现 `MyGame/Scripts/Generated/MainResources.g.cs`，提供 `Id = "MyGame.Main"` 和 `AudioClip Cue` 属性。工具同时生成 ResourceTable、ResourceConfig 和本地 JSON。`Name` 供人阅读，不生成字段。

| Type | 属性类型 |
| --- | --- |
| `AudioClip` | `UnityEngine.AudioClip` |
| `Prefab` | `UnityEngine.GameObject` |
| `Sprite` | `UnityEngine.Sprite` |

资产必须存在，且有可加载的 Addressables 条目。为每个引用资产勾选 **Addressable**；仅放在 GameContent 下还不够。Sprite 使用 Single 模式纹理或独立 Sprite 主资产；暂不支持切片 Sprite 和其他子资产。

## 使用模块

Core 就绪后先调用 `GameConfig.LoadAndRegister()`。创建并登记需要的资源模块，每个只登记一次，再显式加载。启动、失败和清理的完整写法见[资源示例](resources.zh-CN.md)。

MainResources 加载成功后，玩法直接使用：

```csharp
GameCore.Sound.PlaySound(SoundLayer.UI, main.Cue);
```

这一行需要 `using Core; using Sound;`。访问属性不会自动加载。新增一行不需要增加启动登记；新增模块才需要显式接入。

## 修改资源

- **新增或替换：** 编辑行，更新配置，再使用生成属性；保留 Id 即可保持调用。
- **移动资产：** 修改 AssetPath，再次导入。
- **重命名或删除 Id/Module：** 更新配置，再修正调用及模块登记。模块 ID 为 `<namespace>.<Module>`。
- **提交：** 生成的 CSV、清单、C#、JSON、资产和 meta 一起提交。在 Sheets 来源中解决冲突后重新导入，不手工拼接生成正文。

先用 **Main** 即可。资产需要不同生命周期时，再使用 **Level01** 等其他 Module 值。缺失或类型错误的资产会报错；旧版保留与恢复见[配置表](config.zh-CN.md)。生成的内部资源编号不能作为存档或网络标识。

下一步：[登记、加载与释放模块](resources.zh-CN.md)。
