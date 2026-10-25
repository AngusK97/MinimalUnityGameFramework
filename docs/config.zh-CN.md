# 配置表

[总览](../README.zh-CN.md) · [快速开始](quick-start.zh-CN.md) · [English](config.md)

**编辑本地 Excel → Update Config → 使用生成的数据。** 也可以选择 Google Sheets。两种来源使用相同的表格格式、校验和运行 API，每个游戏只启用一个来源。

Demo 已附带 `Editor/Tables/Source/Demo.xlsx` 和生成配置，本地编辑、运行都不需要 Google 账号或凭据。

<a id="日常操作"></a>

## 01 日常操作

1. 编辑并保存配置文件夹中的 `.xlsx`；如果当前来源是 Google Sheets，就编辑对应的云端表格。
2. 在 Unity 顶部工具栏，点击左侧的 **Update Config**。
3. 等待进度窗口关闭，再查看更新结果。有错误时，修正源表或 Unity 中对应的问题，再更新一次。

工具栏会记住这台电脑上当前工程最后选择的游戏。还没选游戏或来源设置不完整时，会先打开 Config 设置窗口。需要切换游戏或修改设置时，打开 **Framework → Config**。

<img src="images/config-toolbar.jpg" alt="Unity 顶部工具栏左侧的 Update Config 按钮，Play 保持居中" width="900">

每次更新都会读取你选中的来源。生成文件的内容有变化时，工具才会重新写入。

不用先把表标为“已完成”，也不用另行发布。只要数据通过检查，即使组员还在编辑，也可能被导入。导入未完成的改动前，先和组员确认一下。

<a id="查看结果"></a>

### 1.1 查看结果

Unity 读取、检查和写入表格时，会显示进度窗口。开始写入前可以点击 **Cancel**，这样表格就会保持原样。

进度窗口关闭后，会弹出结果窗口，说明本次是**更新成功**、**部分更新**还是**失败**。点击 **Open report** 查看详细报告。如果生成 C# 代码触发编译，结果会在编译完成后显示，不用为了查看结果再更新一次。

通过检查的表会更新，出错的表会保留上次成功的版本。按报告修正错误后，再更新一次。

<details>
<summary>查看进度与更新结果示例</summary>

两种来源使用同一个进度窗口，下面是 Google Sheets 更新时的画面：

<img src="images/config-progress.jpg" alt="Unity 原生 Update Config 进度窗口" width="600">

下面用 Demo 表举例。**更新成功：** 所有表都通过检查，更新后的数据已经保存到工程中。

<img src="images/config-success.jpg" alt="Unity Config 结果窗口显示 Demo 表更新成功" width="600">

**部分更新：** 通过检查的表已更新，出错的表保留上次成功的版本。查看对应的错误说明，修正后再更新。

<img src="images/config-partial-update.jpg" alt="Unity Config 结果显示有效 Demo 表已生效，出错的表保留旧版" width="600">

</details>

<a id="首次设置"></a>

## 02 首次设置

按[配置接入](config-setup.zh-CN.md#01-首次设置)创建本地工作簿，再在 Unity 中选择它所在的文件夹。第一张表可以直接使用快速开始中的 UI 表，或下方的 Item 示例。

<a id="添加一张表"></a>

## 03 添加一张表

下面用 **Item** 表配置普通游戏数据。在源表文件夹中直接创建或打开一个 `.xlsx`；如果已选 Google Sheets，就打开当前使用的云端工作簿。添加 **Item** 工作表，从 A1 开始输入下面三行并保存。第一次接入时，按[配置接入](config-setup.zh-CN.md#01-首次设置)选择这个文件夹。

| Id | Name | Price |
| --- | --- | --- |
| string | string | int |
| Potion | Small potion | 20 |

先确认 **Framework → Config** 中选的是 **MyGame**，再点击 Unity 顶部工具栏左侧的 **Update Config**，等待更新结果和编译完成。如果刚在首次设置的最后一步更新过，就不用重复操作。

**成功标志：** 已生成 `Assets/GameContent/MyGame/Scripts/Generated/ItemTable.g.cs`，结果报告中也列出了 Item 表。接着按下面的示例加载和读取数据。

<a id="读取一张表"></a>

## 04 读取一张表

如果已经做完快速开始，打开 **MyGameEntry.cs**，把下面的代码加到 `Start()` 中，接在已有的 `GameConfig.LoadAndRegister()` 后面。保留那一处加载调用，不要再加一次。

还没有启动脚本的话，先按 [Core 的入口步骤](core.zh-CN.md#01-配置入口)准备。在 `GameCore.IsReady` 检查通过后，调用一次 `MyGame.GameConfig.LoadAndRegister()`，再执行下面的查询：

```csharp
var potion = MyGame.ItemTable.Get("Potion");
UnityEngine.Debug.Log(potion.Price);

foreach (var item in MyGame.ItemTable.All)
{
    UnityEngine.Debug.Log(item.Name);
}
```

保存脚本，等待编译完成，再从自己的入口场景运行。**Console 应先显示 `20`，再显示 `Small potion`。** 将表中的价格改为 30，保存并更新配置，重新运行后应显示 30。

用 **ItemTable** 查表，每一行都是一个 **ItemConfig**。如果 ID 不存在，`Get` 会抛出异常。不确定 ID 是否存在时，用 `TryGet(id, out var row)`。先加载配置，再读取数据，否则会报错。

`GameConfig.LoadAndRegister()` 加载本地表数据，并登记 UI 和场景 ID。资源模块还需要在代码中登记和加载；使用 `main.Cue` 前，先按[资源教程](resources.zh-CN.md)完成这些步骤。

<a id="表格格式"></a>

## 05 表格格式

一个工作表标签对应一张配置表。所选文件夹中的所有工作簿不能使用重复的表名。

| 工作表行号 | 内容 |
| --- | --- |
| 第 1 行 | 字段名，第一列必须是 `Id` |
| 第 2 行 | 字段类型：`string`、`int`、`float`、`bool` 或 `ref:TableName` |
| 第 3 行起 | 数据；每个 `Id` 非空，且在本表内唯一 |

下面是在 WPS 中打开的 Demo.xlsx **UI** 工作表：第 1 行是字段名，第 2 行是类型，第 3 行起是数据。上方“添加一张表”使用的是普通数据 Item 示例。

<img src="images/config-local-sheet.jpg" alt="WPS 中打开的 Demo.xlsx UI 工作表，显示字段名、string 类型和 UI 数据行" width="900">

`Id` 的类型必须是 `string` 或 `int`。表名和字段名用于生成代码，只用英文字母、数字和下划线，且不能以数字开头或使用 C# 关键字。单元格中直接填写数值或文本：布尔值用 `true` / `false`，小数用英文句点。公式会被拒绝，即使它已有缓存结果；导入前请粘贴为值。`ref:Item` 表示这一格必须填写 Item 表中已有的 ID。

<a id="ui资源与场景"></a>

### 5.1 UI、资源与场景

这三张表使用固定表头，按对应教程填写：

| 表 | 生成入口 | 教程 |
| --- | --- | --- |
| UI | `UIName.Shop` | [添加窗口](ui-tables.zh-CN.md) |
| Resource | `main.Cue` | [添加资源](resource-tables.zh-CN.md) |
| Scene | `SceneName.Level01` | [添加场景](scenes.zh-CN.md) |

这三张表的字段类型都是 `string`。其中的 `Name` 只用于说明，不会生成字段；普通表中的 Name 仍然是数据字段。ID 和资源模块名用于生成代码，只用英文字母、数字和下划线，且不能以数字开头或使用 C# 关键字。资产路径从 `Assets/` 开始。

<details>
<summary>校验、部分更新与生成文件</summary>

UI 预制体缺失时只会给出警告，所以可以先准备 ID。不过，预制体准备好并重新更新配置后，这个 UI 才能正常打开。构建 Player 前，必须补齐资产或删除占位行，再更新配置。资产类型错误、UIBase 组件设置不符合要求，或 Addressables 条目不可用，都会报错。当前版本中，Resource 或 Scene 资产缺失也会报错。

更新一张表时，会更新整张表的内容。出错的表保留上次成功生成的代码和数据；如果第一次导入就失败，就没有旧版可用。如果新表通过 `ref:TableName` 引用了本次未能导入的改动，这张表也要等相关问题修好后才能更新。最终留下的表之间，所有引用都必须有效。保留旧表不会找回已经删除的 Unity 资产。工具也会检查旧表引用的本地资产；如果其中的资源或场景不可用，本次更新就会停止。

所以，一次更新中可能有些表已更新，有些表保留了旧版。无法读取源表文件夹时，更新会停止；Google 身份验证或文件夹列表读取失败也一样。提交前先看报告：检查通过只说明结构和引用正确，不代表玩法设计已经完成。

生成的文件放在所选游戏的目录中：

```text
Assets/GameContent/MyGame/
├── Editor/
│   ├── ConfigSource.json         来源、文件夹与命名空间
│   └── Tables/
│       ├── Source/              可编辑的本地 .xlsx 工作簿
│       └── Generated/           CSV 快照与生效清单
├── Scripts/Generated/           C# 表、行类型与登记代码
└── Resources/Config/MyGame/     本地 JSON 数据（命名空间目录）
```

生成的 CSV、C# 和 JSON 都不要手动修改。提交时，把它们和生效清单、来源设置、本地源工作簿、Unity meta 文件一起提交，这样组员没有源表访问权限也能运行同一版本。清单会记录配置来源和实际生效的版本。本机导入报告保存在 `Library/FrameworkConfig/Reports/`，不提交到 Git。

修改内容或解决冲突时，先改当前使用的源表，再点击 **Update Config**，不要手工拼接生成文件。重命名或删除字段、表、ID、模块后，自己写的 C# 调用可能无法编译，需要在生成后一起修改。移动资产后，也要更新源表中的路径并重新导入。

</details>

只有少量玩法参数时，继续用 Inspector 或自己的 ScriptableObject 就好；有大量结构相同的数据时，再考虑配置表。目前还没有专门的文本、本地化或声音播放配置表，不过已经可以登记 AudioClip 资源。

下一步：[UI](ui.zh-CN.md) · [资源](resources.zh-CN.md) · [场景](scenes.zh-CN.md) · [构建](build.zh-CN.md)
