# UI 配置表

[框架总览](../README.zh-CN.md) · [配置接入](config.zh-CN.md) · [English](ui-tables.md)

**编辑 UI 标签 → Update Config → 使用 UIName。** 第一次制作窗口，先看[开始使用](quick-start.zh-CN.md)。按[首次设置](config.zh-CN.md#首次设置)连接一次 Google Sheets。

## 添加窗口

1. 将 UI 预制体保存到自己游戏的 `Content/Prefabs/UI/`。RectTransform 根对象上必须恰有一个 UIBase 派生组件。在 **Project** 选中已保存的预制体，勾选 Inspector 中的 **Addressable**。
2. 打开配置文件夹中已有的 **UI** 标签；没有时再创建。新标签从 A1 输入下面的示例；已有标签保留前两行，在数据末尾添加 **Shop** 行：

| Id | Name | PrefabPath |
| --- | --- | --- |
| string | string | string |
| Shop | Shop window | Assets/GameContent/MyGame/Content/Prefabs/UI/ShopView.prefab |

3. 在 Unity 打开 **Framework → Config**，选择 **MyGame**，点击 **Update Config**，等待编译完成。

**成功标志：** `MyGame/Scripts/Generated/UIName.g.cs` 中有 `Shop = "Shop"`。工具同时生成 UITable、UIConfig 和本地 JSON 数据。`Name` 供人阅读，不生成字段。

## 使用配置

在游戏启动时、Core 就绪后加载一次配置，再按 ID 打开窗口（需要 `using Core; using UI;`）：

```csharp
MyGame.GameConfig.LoadAndRegister();
GameCore.UI.OpenUI(UILayer.First, MyGame.UIName.Shop);
```

ID 是 `Shop`，生成的登记代码负责对应到 Addressables 地址。游戏命名空间区分 C# 类型，但不会给运行时 UI ID 添加前缀。

## 修改窗口

- **新增：** 添加行，更新配置，再使用 `UIName.NewId`。
- **替换或移动：** 修改 PrefabPath，再次导入；保留 Id 即可保持调用。
- **删除或改名 Id：** 更新配置，再修正调用及自己保存过的 ID。
- **提交：** 生成的 CSV、清单、C#、JSON 和 meta 一起提交。修改 Google Sheets，不手改生成文件。

缺失预制体是导入警告：可以提前准备 ID，但资产就绪并再次更新前无法打开。类型不符、UIBase 根组件不合法或 Addressables 条目不可用属于错误。**Player 构建拒绝缺失的 UI 预制体**；构建前先补齐或移除占位行。

部分更新、输出归属与恢复见[配置表](config.zh-CN.md)，窗口行为见 [UI](ui.zh-CN.md)。
