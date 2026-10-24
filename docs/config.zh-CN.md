# 配置表

[总览](../README.zh-CN.md) · [快速开始](quick-start.zh-CN.md) · [English](config.md)

**编辑 Google Sheets → Update Config → 使用生成的数据。** 编辑器下载并校验表格；游戏读取本地生成文件，玩家不需要连接 Google。

仓库中的 Demo 已附带生成配置。运行它不需要 Google 账号、凭据或网络。需要修改表格时，再按下方步骤连接自己的来源。

## 日常操作

1. 编辑配置文件夹中的 Google Sheets 工作表。
2. 在 Unity 打开 **Framework → Config**，选择自己的游戏，点击 **Update Config**，等待编译完成。
3. 按下方说明查看结果。根据报告修正 Sheets 或 Unity 中的问题，再点击 **Update Config**。

每次更新都会读取 Google Sheets 中当前的表格内容，只写入内容有变化的生成文件。

不需要选择“已完成的表”，也没有额外发布步骤。工具可能导入别人仍在编辑、但结构已合法的内容；导入未完成的改动前，先与组员沟通。

### 查看结果

以下截图使用 Demo 表。**更新成功：** 所有表通过校验，当前版本已生效。

<img src="images/config-success.jpg" alt="Unity Config 窗口显示 Demo 表更新成功" width="600">

**部分更新：** 有效表更新，出错的表保留上次成功版本。查看该表的错误说明，修正后再更新。

<img src="images/config-partial-update.jpg" alt="Unity Config 结果显示有效 Demo 表已生效，出错的表保留旧版" width="600">

## 首次设置

你需要一个 Google 账号，以及创建 Cloud 项目和服务账号密钥的权限。服务账号就是 Unity 读取共享文件夹时使用的账号。

### 准备 Google 访问权限

1. **创建 Cloud 项目。** 打开 [Google Cloud Console](https://console.cloud.google.com/projectcreate)，输入名称，例如 `MyGame Config`，创建项目。继续前先选中该项目。[Google 项目指南](https://developers.google.com/workspace/guides/create-project)。

2. **启用两个 API。** 在 **APIs & Services → Library** 中分别搜索并启用 **Google Drive API** 和 **Google Sheets API**，确认它们属于刚才的项目。[Google API 指南](https://developers.google.com/workspace/guides/enable-apis)。

3. **创建服务账号。** 打开 **IAM & Admin → Service Accounts → Create service account**，输入名称，例如 `mygame-config-reader`，最后点击 **Done**。可选的项目角色和用户访问权限留空；文件夹权限在第 5 步单独授予。[Google 服务账号指南](https://docs.cloud.google.com/iam/docs/service-accounts-create)。

4. **下载 JSON 密钥。** 打开新账号，依次选择 **Keys → Add key → Create new key → JSON → Create**。将下载的文件移到 **Unity 工程和任何 Git 仓库之外**的私人位置，例如专用凭据文件夹。文件留在自己的电脑上，不要把内容粘贴到代码、Sheets 或聊天中。[Google 密钥指南](https://docs.cloud.google.com/iam/docs/keys-create-delete)。

5. **创建并共享 Drive 文件夹。** 在 [Google Drive](https://drive.google.com/) 新建 `MyGame Config` 文件夹。将它共享给 Cloud Console 中显示的服务账号邮箱，也就是密钥文件的 `client_email`，权限设为 **Viewer（查看者）**。取消勾选 **Notify people（通知人员）**，完成共享。常规访问保持受限；需要编辑的策划或组员可另加为 **Editor（编辑者）**。这种直接共享不需要全域委派。[Google 共享指南](https://developers.google.com/workspace/guides/create-credentials#access-google-workspace-files-directly-with-a-service-account)。

### 创建并连接表格

6. **创建第一张表。** 在此 Drive 文件夹内选择 **New → Google Sheets → Blank spreadsheet**。把第一个工作表标签改名为 **Item**，从 A1 开始输入下方三行。工作簿名可自定；标签名才是表名。工作簿直接放在该文件夹中，不使用子文件夹或快捷方式。

| Id | Name | Price |
| --- | --- | --- |
| string | string | int |
| Potion | Small potion | 20 |

7. **连接 Unity。** 如果还没有，创建 `Assets/GameContent/MyGame/`。打开 **Framework → Config**，在 **Game** 选择此文件夹，填写 **Namespace = MyGame**，把文件夹 ID 填入 **Google Drive Folder ID**。例如网址为 `https://drive.google.com/drive/folders/FOLDER_ID` 时，只复制 `FOLDER_ID`。在 **Service Account JSON** 选择下载的密钥文件。

8. **点击 Update Config。** 等待 Unity 编译完成。确认结果包含 Item 表，且已生成 `Assets/GameContent/MyGame/Scripts/Generated/ItemTable.g.cs`。接着可以用下方示例加载和读取。

文件夹 ID 和命名空间保存在 `Assets/GameContent/MyGame/Editor/ConfigSource.json`。密钥文件路径只保存在这台电脑的 Unity Editor 偏好设置中，密钥不会复制进工程。换电脑后重新选择本机保存的密钥即可。

<img src="images/config-update.jpg" alt="Unity 6 Config 窗口显示游戏选择、首次设置与 Update Config 按钮" width="480">

图中以 Demo 展示设置字段的位置；按上方步骤操作时选择 **MyGame**。

<details>
<summary>首次设置遇到问题，以及如何从 Demo 开始</summary>

- **无法创建密钥：** 学校或组织的策略可能禁止服务账号密钥。请管理员确认该项目是否允许使用密钥；当前导入器需要服务账号 JSON 密钥。参见 [Google 密钥策略说明](https://docs.cloud.google.com/iam/docs/keys-create-delete#allow-service-account-key-creation)。
- **找不到文件夹／无权访问：** 检查文件夹 ID、两个 API 是否都已启用，以及共享对象是否为准确的服务账号邮箱。你自己的浏览器能打开文件夹，不代表 Unity 的服务账号也能访问。
- **没有读到表：** 使用直接位于指定文件夹中的原生 Google Sheets 文件。当前版本不读取上传的 `.xlsx`、快捷方式或子文件夹中的工作簿。
- **连接随包 Demo：** 将 `Assets/GameContent/Demo/Editor/Tables/Generated/` 中全部三份 CSV 快照导入 Google Sheets，标签分别命名为 **UI**、**Resource**、**Scene**，再在 Config 窗口填写自己的文件夹 ID。首次连接要求三张表全部成功导入，否则保留整份随包快照；后续更新才按表保留旧版。这是初始内容副本，不是公开云端模板；改成自己的游戏时要替换 Demo 资产路径。

导入器只请求 Drive 元数据和 Sheets 的只读权限，不会修改云端表格。文件会继承父文件夹的共享权限；学校或公司的共享策略可能限制服务账号访问。参见 [Google Drive 文件夹共享说明](https://support.google.com/drive/answer/7166529?hl=zh-Hans)。

</details>

## 读取一张表

在游戏启动代码中，**Core 就绪后、打开 UI 或切换场景前**加载生成配置，每个 Core 生命周期执行一次：

```csharp
// In startup, after GameCore.IsReady:
MyGame.GameConfig.LoadAndRegister();

var potion = MyGame.ItemTable.Get("Potion");
UnityEngine.Debug.Log(potion.Price);

foreach (var item in MyGame.ItemTable.All)
{
    UnityEngine.Debug.Log(item.Name);
}
```

标签 **Item** 生成查询入口 **ItemTable** 和行类型 **ItemConfig**。ID 不存在时 `Get` 会抛出异常；允许查询不到的场合使用 `TryGet(id, out var row)`。未加载就读取也会报错。

`GameConfig.LoadAndRegister()` 加载本地表数据，并登记 UI 和场景 ID。资源模块仍需显式登记和加载；使用 `main.Cue` 前，按[资源教程](resources.zh-CN.md)完成这些步骤。

## 表格格式

一个标签对应一张表。所选文件夹内所有工作簿的表名必须唯一。

| 工作表行号 | 内容 |
| --- | --- |
| 第 1 行 | 字段名，第一列必须是 `Id` |
| 第 2 行 | 字段类型：`string`、`int`、`float`、`bool` 或 `ref:TableName` |
| 第 3 行起 | 数据；每个 `Id` 非空，且在本表内唯一 |

Demo 的 **UI** 标签展示了这一布局：第 1 行是字段名，第 2 行是类型，第 3 行起是数据。图中使用下方介绍的 UI 字段；上方首次设置使用的是 Item 表。

<img src="images/config-sheet.jpg" alt="Google Sheets 中的 Demo UI 表，显示字段名、string 类型和 UI 数据行" width="900">

`Id` 类型必须为 `string` 或 `int`。表名和字段名使用 ASCII 字符组成的合法 C# 标识符。直接输入值，布尔值用 `true` / `false`，小数用英文句点；当前版本不支持公式。`ref:Item` 表示该格必须填写一个存在的 Item ID。

### UI、资源与场景

这三个标签名使用固定表头，第 2 行的所有类型都填 `string`。它们的 `Name` 列只供人阅读，不生成行字段；普通表（例如 Item）中的 `Name` 则是正常数据字段。

**UI** — 按[快速开始](quick-start.zh-CN.md)创建预制体后添加一行：

| Id | Name | PrefabPath |
| --- | --- | --- |
| string | string | string |
| Shop | Shop window | Assets/GameContent/MyGame/Content/Prefabs/UI/ShopView.prefab |

生成 `UIName.Shop = "Shop"`。加载配置后，使用 `GameCore.UI.OpenUI(UILayer.First, MyGame.UIName.Shop)` 打开。ID 与预制体的 Addressables 地址相互独立。

**Resource** — 填写工程中实际存在的资产：

| Id | Name | Type | Module | AssetPath |
| --- | --- | --- | --- | --- |
| string | string | string | string | string |
| Cue | Button sound | AudioClip | Main | Assets/GameContent/MyGame/Content/Audio/cue.wav |

`Type` 使用 `AudioClip`、`Sprite` 或 `Prefab`。`Module = Main` 会生成 **MainResources**，其中有类型明确的 `Cue` 属性。将加载生命周期相同的资产放在同一模块。参见[加载与释放资源](resources.zh-CN.md)。

**Scene** — 填写已保存的场景资产：

| Id | Name | ScenePath |
| --- | --- | --- |
| string | string | string |
| Menu | Main menu | Assets/GameContent/MyGame/Scenes/Menu.unity |

生成 `SceneName.Menu = "Menu"`。加载配置后调用 `GameCore.Scene.TryTransition(MyGame.SceneName.Menu)`。参见[场景切换](scenes.zh-CN.md)。

这些表的 ID 和资源模块名使用 ASCII 字符组成的合法 C# 标识符。路径从 `Assets/` 开始。更新配置前，在 **Project** 逐一选中引用的预制体、资源或场景资产，勾选 Inspector 中的 **Addressable**。仅放在 GameContent 下还不够。

<details>
<summary>校验、部分更新与生成文件</summary>

UI 预制体缺失时给出警告，因此可以先准备 ID；Player 构建前必须补齐资产或删除占位行，再更新配置。预制体就绪并再次更新配置前，该 UI 无法正常打开。资产类型错误、UIBase 组件设置不合法或 Addressables 条目不可用属于错误。当前版本中，Resource 或 Scene 资产缺失也属于错误。

每张表整体更新。错误表保留上次成功生成的代码和数据；首次导入失败时，没有旧版可保留。如果新表通过 `ref:TableName` 依赖被拒绝的改动，其变更也会保留旧版，直到最终组合中的引用全部有效。这不能恢复被删除的 Unity 资产。保留的旧表也会检查本地资产；旧资源或场景已经不可用时，本次更新停止。

因此，一次结果可能同时包含已更新和已保留的表。身份验证失败或无法列出文件夹内容时会停止更新。提交前先看报告；校验检查结构与引用，不判断玩法设计是否已经完成。

生成内容归所选游戏所有：

```text
Assets/GameContent/MyGame/
├── Editor/
│   ├── ConfigSource.json         文件夹 ID 与命名空间
│   └── Tables/Generated/         CSV 快照与生效清单
├── Scripts/Generated/           C# 表、行类型与登记代码
└── Resources/Config/MyGame/     本地 JSON 数据（命名空间目录）
```

把生成的 CSV、C# 和 JSON 当作只读文件。将它们与生效清单、来源设置和 Unity meta 文件一起提交，组员无需云端权限也能运行同一版本。清单记录来源与实际生效版本。本机导入报告保存在 `Library/FrameworkConfig/Reports/`，不进入 Git。

在 Google Sheets 来源中编辑或解决冲突，再点击 **Update Config**，不要手工拼接生成文件。重命名或删除字段、表、ID、模块可能让手写 C# 调用无法编译，生成后需要修正调用。移动资产后，也要更新 Sheets 中的路径并重新导入。

</details>

少量玩法参数可以继续用 Inspector 或自己的 ScriptableObject；大量同结构数据再考虑表格。专门的文本／本地化和声音播放配置暂缓，当前已支持 AudioClip 资源登记。

下一步：[UI](ui.zh-CN.md) · [资源](resources.zh-CN.md) · [场景](scenes.zh-CN.md) · [构建](build.zh-CN.md)
