# UI 配置表

[框架总览](../README.zh-CN.md) · [配置接入](config-setup.zh-CN.md) · [English](ui-tables.md)

**编辑 UI 工作表 → Update Config → 使用 UIName。** 第一次制作窗口，可以先跟着[开始使用](quick-start.zh-CN.md)做。Google Sheets 只需按[首次设置](config-setup.zh-CN.md#01-首次设置)连接一次。

<a id="添加窗口"></a>

## 01 添加窗口

1. 将 UI 预制体保存到自己游戏的 `Content/Prefabs/UI/`。根对象必须使用 RectTransform，并且只挂一个继承 UIBase 的组件。在 **Project** 中选中已保存的预制体，勾选 Inspector 中的 **Addressable**。
2. 下面是 **UI** 表的示例。第一次连接时，在[配置接入](config-setup.zh-CN.md)的第 6 步填入这些内容。如果已经连接，打开已有的 **UI** 工作表标签，没有就创建一个。新建的工作表从 A1 开始填入全部内容；已有的工作表保留前两行，只添加 **Shop** 这一行数据。

| Id | Name | PrefabPath |
| --- | --- | --- |
| string | string | string |
| Shop | Shop window | Assets/GameContent/MyGame/Content/Prefabs/UI/ShopView.prefab |

3. 第一次连接时，用上面的表按[配置接入](config-setup.zh-CN.md)完成设置。如果已经连接，点击 Play 左侧的 **Update Config**，等待更新结果和编译完成。需要切换游戏时，先在 **Framework → Config** 中选择 **MyGame**。

**成功标志：** `MyGame/Scripts/Generated/UIName.g.cs` 中有 `Shop = "Shop"`。工具还会生成 UITable、UIConfig 和本地 JSON 数据。`Name` 只用于说明，不会生成字段。

<a id="使用配置"></a>

## 02 使用配置

游戏启动时，等 Core 就绪后加载一次配置，再按 ID 打开窗口（需要 `using Core; using UI;`）：

```csharp
MyGame.GameConfig.LoadAndRegister();
GameCore.UI.OpenUI(UILayer.First, MyGame.UIName.Shop);
```

这里的 ID 是 `Shop`，生成的登记代码会把它对应到 Addressables 地址。游戏命名空间用于区分 C# 类型，不会给运行时的 UI ID 添加前缀。

<a id="修改窗口"></a>

## 03 修改窗口

- **新增：** 添加行，更新配置，再使用 `UIName.NewId`。
- **替换或移动：** 修改 PrefabPath，再次导入。只要 Id 不变，原有调用就不用改。
- **删除或重命名 Id：** 更新配置后，同步修改调用代码和自己保存过的 ID。
- **提交：** 把生成的 CSV、清单、C#、JSON 和 meta 一起提交。需要修改时，改 Google Sheets，不要手动改生成文件。

预制体缺失时，导入只会给出警告，所以可以提前准备 ID。但要等资产准备好并重新更新配置后，窗口才能打开。类型不符、根对象上的 UIBase 组件不符合要求，或 Addressables 条目不可用，都会报错。**缺少 UI 预制体时无法构建 Player**；构建前先补齐资产或删除占位行。

部分更新、生成文件的位置和恢复方法见[配置表](config.zh-CN.md)，窗口的具体行为见 [UI](ui.zh-CN.md)。
