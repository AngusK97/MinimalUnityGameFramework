# UI 配置表

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](ui-tables.md)

**在 Inspector 的列表里加上窗口，再点一个按钮，代码里就有了可以调用的名字。**

## 创建配置表

1. 在自己游戏的 `Editor/Tables/` 目录中，右键选择 **Create → Framework → UI Table**。保留默认名字 **UI**（保存后是 UI.asset）。
2. 选中 UI.asset，展开 **Entries**，点 **+** 加一行。填写 Inspector，再把保存的预制体**从 Project 拖进 Prefab 字段**，不要拖 Hierarchy 里的场景对象：

| 字段 | 示例 |
| --- | --- |
| Namespace | `MyGame` |
| Class Name | `GameUI` |
| Id Prefix | `MyGame` |
| Entries → Name | `Menu` |
| Entries → Prefab | 已保存预制体根对象上的 UIBase 派生组件 |

3. 点击 **Validate and Generate**，等待编译完成。
4. 状态应显示 **Generated code is up to date.** 输出为 `MyGame/Scripts/Generated/GameUI.g.cs`。

Name 是给代码用的名字，比如 Menu，不带空格；玩家看到的标题仍写在 Prefab 里。以[第一个窗口教程](quick-start.zh-CN.md)中的 MyGame 为例，目录遵循这条规则：

```text
<game root>/Editor/Tables/UI.asset
<game root>/Scripts/Generated/<ClassName>.g.cs
```

未来其他编辑配置表也放在同一个 Tables 目录；目前只实现了 UI 配置表工具。

## 使用配置表

Core 就绪后，**每个 Core 生命周期调用一次** `MyGame.GameUI.RegisterAll()`。使用生成的常量打开窗口：

```csharp
// In game startup, after GameCore.IsReady:
MyGame.GameUI.RegisterAll();
GameCore.UI.OpenUI(UILayer.First, MyGame.GameUI.Menu);
```

需要 `using Core; using UI;`。编写引用 GameUI 的代码之前，应先生成配置表。运行时标识为 `MyGame.Menu`；生成器也会记录预制体实际的 Addressables 地址。

## 修改窗口

**编辑 UI.asset → 生成 → 等待编译 → 使用常量。**

- 添加窗口：添加 Name 和 Prefab，生成后使用 `GameUI.NewName`。
- 移动预制体或修改地址：保留其 meta，重新生成。保持 Name 和 Id Prefix 不变，就能保持 UI 标识不变。
- 删除条目：生成后，修复对已移除成员的引用。
- 配置表、生成的 C# 及其 meta 文件一起提交。修改配置表，不要修改生成代码。

`Assets/GameContent/` 下的运行时预制体继承现有的 Addressables 文件夹条目，不需要单独标记其中的每个资产。

<details>
<summary>校验、改名与恢复</summary>

源文件必须位于规定位置。Namespace 可以为空；名称使用英文 C# 标识符。Namespace 和 Id Prefix 可以用点分隔。关键字、RegisterAll、类名与成员冲突、重复名称或预制体、跨表类名或完整标识冲突、不可用或不唯一的地址都会被拒绝。预制体必须已保存，且根对象上只有一个 UIBase 派生组件；场景对象和子对象组件无效。所属组必须将其包含在构建中，并在目录中包含地址。

修改 Class Name：先生成新类，更新调用方，再删除旧生成文件及 meta。修改游戏根目录：将配置表、现有输出及 meta 一起移动到新规则推导的位置，再生成。工具不会自动删除旧源码，也不会解析所有手写类型名。

修改 Name 或 Id Prefix 会改变运行时标识；自己的存档或消息若保存了这些标识，需要自行迁移。修改 Namespace 需要更新调用方。

校验失败时保留旧输出；生成器拒绝覆盖手写文件、其他配置表的输出或正文被修改的生成文件。重新生成前，先恢复被修改的输出，或将其移到别处。内容相同不会重写；支持 CRLF/BOM 检出的文件。

配置表和工具仅用于 Editor。Player 使用生成代码，不加载配置表。Play 或编译期间禁用生成；Play 使用上次生成的版本。只读的 Player 构建前检查会拒绝缺失、过期或被修改的输出。项目中没有配置表也是有效状态。

</details>

下一步：通过 [UI](ui.zh-CN.md) 了解窗口行为，或查看[构建](build.zh-CN.md)打包游戏。
