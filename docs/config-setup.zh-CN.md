# 配置接入

[总览](../README.zh-CN.md) · [配置表](config.zh-CN.md) · [快速开始](quick-start.zh-CN.md) · [English](config-setup.md)

**第一次选好来源，以后改表、保存，再点 Update Config。** 默认使用本地 Excel；需要在线协作时可以选择 Google Sheets。每个游戏只使用一个配置来源。

<a id="首次设置"></a>

## 01 首次设置

### 1.1 准备本地工作簿

1. 在 Unity 工程中创建 `Assets/GameContent/MyGame/Editor/Tables/Source/`。
2. 用表格编辑器新建空白工作簿。跟着[第一个窗口](quick-start.zh-CN.md#23-连接并更新-ui-配置表)做时，把第一张工作表改名为 **UI**；使用[普通数据示例](config.zh-CN.md#03-添加一张表)时，改名为 **Item**。
3. 从 A1 开始填写示例：第 1 行是字段名，第 2 行是字段类型，第 3 行起是数据。删除多余的空白工作表，单元格直接填值，不使用公式。
4. 将文件保存为 `MyGame.xlsx`，直接放在 Source 文件夹内。工作簿名可以自定，工作表名会成为配置表名，整个文件夹内不能重名。工具只读取该文件夹中的 `.xlsx`，忽略 Excel 的 `~$` 临时文件，不扫描子文件夹。

可以打开 `Assets/GameContent/Demo/Editor/Tables/Source/Demo.xlsx`，看看一张填好的表是什么样。做自己的游戏时，只复制需要的工作表，再把 Demo 的 ID 和资产路径换成自己的。源表放在自己游戏的 Editor 目录中，不会作为运行资产打包。

### 1.2 接入 Unity

1. 打开 **Framework → Config**，把 Project 中的 **MyGame 文件夹**拖到 **Game** 栏。填写 **Namespace = MyGame**，选择 **Source = Local Excel**。Namespace 是生成代码的 C# 命名空间，同一个游戏保持不变。
2. 保留 **Excel folder = Editor/Tables/Source**，或点击 **Choose folder…** 选择这个游戏 Editor 目录内的其他文件夹。路径从游戏根目录开始。这里选存放源工作簿的文件夹，不要选存放生成文件的文件夹。
3. 点击 **Update Config**，等待进度窗口、更新结果和编译完成。用 **Open report** 查看详情，再回到对应教程检查生成结果。

<img src="images/config-setup.jpg" alt="Unity Config 设置窗口，已选择 Local Excel 和游戏自己的源表文件夹" width="480">

截图选中的是 Demo；按上面的步骤操作时，请选择 **MyGame**。设置保存在 `MyGame/Editor/ConfigSource.json`，本地文件夹记录为相对游戏根目录的路径，例如 `Editor/Tables/Source`。工作簿、设置、生成文件和 Unity meta 要一起提交。

以后改表，保存工作簿后点击顶部工具栏左侧的 **Update Config** 即可。工具会使用这台电脑上最后选择的游戏；设置不完整时，会先打开 Config。换游戏或改设置时，打开 **Framework → Config**。

## 02 Google Sheets（可选）

小组需要同时在线编辑表格时，可以使用 Google Sheets。你需要一个 Google 账号，并有权限创建 Cloud 项目和服务账号密钥。Unity 通过服务账号读取共享文件夹。

<a id="准备-google-访问权限"></a>
<a id="11-准备-google-访问权限"></a>

### 2.1 准备 Google 访问权限

1. **创建 Cloud 项目。** 打开 [Google Cloud Console](https://console.cloud.google.com/projectcreate)，输入项目名称，例如 `MyGame Config`，然后创建。创建后先选中这个项目，再继续下面的步骤。[Google 项目指南](https://developers.google.com/workspace/guides/create-project)。

2. **启用两个 API。** 在 **APIs & Services → Library** 中分别搜索并启用 **Google Drive API** 和 **Google Sheets API**，确认它们属于刚才的项目。[Google API 指南](https://developers.google.com/workspace/guides/enable-apis)。

3. **创建服务账号。** 打开 **IAM & Admin → Service Accounts → Create service account**，输入名称，例如 `mygame-config-reader`，然后点击 **Done**。项目角色和用户访问权限都是可选项，留空即可；第 5 步再单独设置文件夹权限。[Google 服务账号指南](https://docs.cloud.google.com/iam/docs/service-accounts-create)。

4. **下载 JSON 密钥。** 打开新账号，依次选择 **Keys → Add key → Create new key → JSON → Create**。把下载的文件移到 **Unity 工程和任何 Git 仓库之外**的私人目录，例如专门存放凭据的文件夹。密钥留在自己的电脑上，不要把内容粘贴到代码、Sheets 或聊天中。[Google 密钥指南](https://docs.cloud.google.com/iam/docs/keys-create-delete)。

5. **创建并共享 Drive 文件夹。** 在 [Google Drive](https://drive.google.com/) 新建 `MyGame Config` 文件夹。把文件夹共享给 Cloud Console 中显示的服务账号邮箱，也就是密钥文件中的 `client_email`，权限设为 **Viewer（查看者）**。取消勾选 **Notify people（通知人员）**，完成共享。常规访问保持受限。需要编辑表格的策划或组员，可以另外添加为 **Editor（编辑者）**。直接共享文件夹不需要设置全域委派。[Google 共享指南](https://developers.google.com/workspace/guides/create-credentials#access-google-workspace-files-directly-with-a-service-account)。

<a id="创建并连接配置表"></a>
<a id="创建并连接表格"></a>
<a id="12-创建并连接配置表"></a>
<a id="12-创建并连接表格"></a>

### 2.2 创建并连接配置表

1. 在共享的 Drive 文件夹中选择 **New → Google Sheets → Blank spreadsheet**。把第一个标签改成配置表名，按本地 Excel 示例填写相同的字段名、类型和数据。原生 Google Sheets 工作簿直接放在这个文件夹中，不使用子文件夹或快捷方式。上传的 `.xlsx` 需要先转换为 Google Sheets。
2. 如果还没有游戏目录，先创建 `Assets/GameContent/MyGame/`。打开 **Framework → Config**，选择这个游戏文件夹，设置 **Namespace = MyGame**，再选择 **Source = Google Sheets**。在 **Google Drive Folder ID** 中填写文件夹 ID。例如地址是 `https://drive.google.com/drive/folders/FOLDER_ID`，只复制 `FOLDER_ID`。在 **Service Account JSON** 中选择下载的密钥。
3. 新游戏点击 **Update Config**。如果这个游戏已经导入过配置表，按第 03 节切换来源。等待结果和编译完成，再回到对应教程继续。

工具把来源设置保存在游戏自己的 ConfigSource.json 中。它只在这台电脑的 Unity Editor 偏好设置里记住密钥路径，不会把密钥复制进工程。换电脑后，再选择那台电脑上存放的密钥。

<details>
<summary>Google 接入问题</summary>

- **不允许创建密钥：** 学校或组织可能限制服务账号密钥，请管理员确认项目是否允许使用。本地 Excel 不需要密钥。参见 [Google 密钥策略说明](https://docs.cloud.google.com/iam/docs/keys-create-delete#allow-service-account-key-creation)。
- **找不到文件夹／无权访问：** 检查文件夹 ID、两个 API，以及共享时是否填对服务账号邮箱。你能在浏览器打开文件夹，不代表 Unity 使用的账号也能访问。
- **没有读到表：** 使用直接放在指定文件夹中的原生 Google Sheets 文件。上传的 `.xlsx` 先转换格式。

导入器只请求 Drive 元数据和 Sheets 的只读权限，不会修改云端表格。文件会继承文件夹的共享权限，但学校或公司的策略可能限制服务账号访问。参见 [Google 文件夹共享说明](https://support.google.com/drive/answer/7166529?hl=zh-Hans)。

</details>

## 03 切换已有来源

下面以 **Demo** 为例；自己的游戏把 Demo 换成对应的游戏目录和命名空间。Demo 默认使用本地 Excel，只有改用 Google 时才需要连接 Google。

切换前先停止 Play。把游戏当前使用的全部配置表放到新来源中，**表名、字段名、字段类型和列顺序保持一致**；这次先不增删表或改表头。每张表只保留一份，单元格直接填值，不使用公式。游戏调用代码不用改。

### 3.1 Google Sheets → 本地 Excel

1. **下载表格。** 打开当前使用的 Google 工作簿，选择 **文件 → 下载 → Microsoft Excel（.xlsx）**。有多个工作簿就分别下载，保留其中全部配置工作表。[Google 下载说明](https://support.google.com/docs/answer/9330963?hl=zh-Hans)。
2. **放入 Source。** 将下载的文件直接放到 `Assets/GameContent/Demo/Editor/Tables/Source/`，目录不存在就创建。若已有 Demo.xlsx 或旧工作簿，先移到 Source 外备份，再放入下载版本；不要让新旧文件包含同名工作表，也不要放进子文件夹。
3. **选择本地来源。** 打开 **Framework → Config**，选择 **Game = Demo**，保持 **Namespace = Demo**，设置 **Source = Local Excel**、**Excel folder = Editor/Tables/Source**。
4. **点击 Switch Source。** 等待更新结果和编译完成。结果成功后，重新打开 Config，确认 Source 为 Local Excel。

之后只需：**改本地 Excel → 保存 → Update Config**。不再需要 Google 密钥，也不用把 Excel 手动转成 CSV。

### 3.2 本地 Excel → Google Sheets

1. **准备 Google 连接。** 第一次使用时，先完成 [2.1 准备 Google 访问权限](#21-准备-google-访问权限)，取得共享文件夹和服务账号 JSON；已有可用连接就沿用。
2. **上传并转换表格。** 保存本地工作簿，在共享 Drive 文件夹中选择 **新建 → 上传文件**，上传 Source 内的全部 `.xlsx`。逐个用 Google Sheets 打开，选择 **文件 → 另存为 Google 表格（Save as Google Sheets）**；如果上传时已经自动转换，就不用再转一次。[Google 转换说明](https://support.google.com/docs/answer/9331167?hl=zh-Hans)。
3. **检查目标文件夹。** 确认转换后的原生 Google 表格直接位于共享文件夹中，底部工作表名称和前两行内容保持不变。若文件夹中已有同名配置表的旧工作簿，先移到该文件夹外备份。工具只读取原生 Google 表格，上传的 `.xlsx` 不算配置来源。
4. **选择 Google 来源。** 打开 **Framework → Config**，选择 **Game = Demo**，保持 **Namespace = Demo**，设置 **Source = Google Sheets**。在 **Google Drive Folder ID** 中填写共享文件夹链接最后的 ID，在 **Service Account JSON** 中选择工程外的本机密钥文件。
5. **点击 Switch Source。** 等待更新结果和编译完成。结果成功后，重新打开 Config，确认 Source 为 Google Sheets。

之后只需：**改 Google Sheets → 等待云端保存完成 → Update Config**。不用每次重新上传 Excel。

### 3.3 成功与失败怎么看

- **成功：** 结果窗口显示 `Config is up to date`，Config 中保留新来源。以后顶部的 Update Config 只读取这个来源；切换前后运行同一个 Demo，检查菜单和关卡正常即可。
- **失败：** 原来源和生成结果保持不变。点击 **Open report**，按提示补齐缺少的表、移走重复工作簿，或修正字段和资产错误，再到 Config 重试。所有表都通过检查后，才能切换来源，不能只切换其中几张。

切换不会删除原文件，**本地与 Google 也不会自动同步或互相替补**。切换成功后，只编辑选中的来源。以后更新时，出错的表仍会保留旧版。需要改表头或增删表，等切换成功后再做。
