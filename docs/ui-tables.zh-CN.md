# UI 配置表

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](ui-tables.md)

**编辑 UI.asset → 生成 → 使用常量。** 第一次制作窗口，先看[开始使用](quick-start.zh-CN.md)。

## 创建配置表

1. 在自己游戏的 `Editor/Tables/` 选择 **Create → Framework → UI Table**，保留名字 **UI**。
2. 选中 UI.asset，展开 **Entries**，点 **+**，填写：

| 字段 | 示例 |
| --- | --- |
| Namespace | `MyGame` |
| Class Name | `GameUI` |
| Id Prefix | `MyGame` |
| Entries → Name | `Menu` |
| Entries → Prefab | Project 里的 MyMenuView.prefab；根对象上只有一个 UIBase 派生组件 |

3. 点击 **Validate and Generate**，等待编译。

**成功标志：** 出现 `MyGame/Scripts/Generated/GameUI.g.cs`，状态为 **Generated code is up to date.**。

![MyGame UI 配置表 Inspector，显示 Menu 预制体引用和代码已是最新状态](images/ui-table-inspector.png)

Name 是代码用的名字，不带空格；玩家看到的标题留在预制体里。使用保存的 Prefab，不要使用场景对象。

## 使用配置表

Core 就绪后，**每个 Core 生命周期登记一次**，再打开窗口：

```csharp
// In game startup, after GameCore.IsReady:
MyGame.GameUI.RegisterAll();
GameCore.UI.OpenUI(UILayer.First, MyGame.GameUI.Menu);
```

添加 `using Core; using UI;`。先生成，再编写引用 GameUI 的代码。示例的运行时标识为 `MyGame.Menu`。

## 修改窗口

- **新增：** 添加 Name 和 Prefab，生成后使用 `GameUI.NewName`。
- **移动：** 保留预制体的 meta，重新生成。保持 Name/Id Prefix 不变，UI 标识就不变。
- **删除：** 删除条目，生成，再修复对已移除常量的引用。
- **提交：** UI.asset、生成的 C# 及其 meta 一起提交，不要修改生成代码。

`Assets/GameContent/` 下的预制体继承其 Addressables 文件夹条目。地址改变后重新生成。

<details>
<summary>目录、校验与恢复</summary>

配置表必须位于 `<game root>/Editor/Tables/UI.asset`，输出为 `<game root>/Scripts/Generated/<ClassName>.g.cs`。共享资产使用 Tables/ResourceModules 下独立的[资源模块表](resource-tables.zh-CN.md)。生成器记录预制体实际的 Addressables 地址。

名称使用英文 C# 标识符（字母、数字和下划线）；Namespace 可为空，Namespace/Id Prefix 可用点分隔。工具拒绝关键字、RegisterAll、类名与成员冲突、重复名称/预制体、跨表类名/完整标识冲突，以及不可用或不唯一的地址。已保存的预制体根对象上必须恰有一个 UIBase 派生组件；场景对象和子组件无效。Addressables 组需启用 Include in Build 和 Include Addresses in Catalog。

**修改 Class Name：** 先生成新类，更新调用方，再删除旧输出及 meta。**移动游戏根目录：** 将配置表、现有输出及 meta 一起移到规定位置，再生成。工具不会自动删除旧文件或解析手写类型名。Name/Id Prefix 改变会更换运行时标识，已保存的标识需迁移；Namespace 改变需更新调用方。

校验失败保留旧输出。手写文件、其他表的输出和被修改的生成正文不会被覆盖；先恢复或移走被修改的输出。内容相同不重写，支持 CRLF/BOM 检出。

配置表和工具仅用于 Editor，Player 使用生成代码。Play/编译时禁止生成，Play 使用上次生成结果。Player 构建前检查拒绝缺失、过期或被修改的输出。零配置表也有效。

</details>

下一步：[窗口生命周期](ui.zh-CN.md) · [构建](build.zh-CN.md)
