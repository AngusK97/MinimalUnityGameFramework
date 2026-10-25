# 配置接入

[总览](../README.zh-CN.md) · [配置表](config.zh-CN.md) · [快速开始](quick-start.zh-CN.md) · [English](config-setup.md)

**选好配置来源，之后改表只需点击 Update Config。** 默认使用本地 Excel；需要在线协作时可以选择 Google Sheets。每个游戏只使用一个配置来源。

<a id="首次设置"></a>

## 01 首次设置

### 1.1 准备本地工作簿

1. 在 Unity 工程中创建 `Assets/GameContent/MyGame/Editor/Tables/Source/`。
2. 用表格编辑器新建空白工作簿。跟着[第一个窗口](quick-start.zh-CN.md#23-连接并更新-ui-配置表)做时，把第一张工作表改名为 **UI**；使用[普通数据示例](config.zh-CN.md#03-添加一张表)时，改名为 **Item**。
3. 从 A1 开始填写示例：第 1 行是字段名，第 2 行是字段类型，第 3 行起是数据。删除多余的空白工作表，单元格直接填值，不使用公式。
4. 将文件保存为 `MyGame.xlsx`，直接放在 Source 文件夹内。工作簿名可以自定，工作表名会成为配置表名，整个文件夹内不能重名。工具只读取该文件夹中的 `.xlsx`，忽略 Excel 的 `~$` 临时文件，不扫描子文件夹。

随包的 `Assets/GameContent/Demo/Editor/Tables/Source/Demo.xlsx` 可以直接打开参考。做自己的游戏时，只复制需要的工作表，再把 Demo 的 ID 和资产路径换成自己的。源表放在自己游戏的 Editor 目录中，不会作为运行资产打包。

### 1.2 接入 Unity

1. 打开 **Framework → Config**，设置 **Game = MyGame**、**Namespace = MyGame**、**Source = Local Excel**。
2. 保留 **Excel folder = Editor/Tables/Source**，或点击 **Choose folder…** 选择这个游戏 Editor 目录内的其他文件夹。路径以游戏根目录为起点，不能把生成文件目录当作来源。
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
3. 新游戏点击 **Update Config**。如果这个游戏已经有生效的来源，按第 03 节切换。等待结果和编译完成，再回到对应教程继续。

来源设置保存在游戏自己的 ConfigSource.json 中。密钥文件路径只保存在本机 Unity Editor 偏好设置里，密钥不会复制进工程。换电脑后，需要重新选择本机存放的密钥。

<details>
<summary>Google 接入问题</summary>

- **不允许创建密钥：** 学校或组织可能限制服务账号密钥，请管理员确认项目是否允许使用。本地 Excel 不需要密钥。参见 [Google 密钥策略说明](https://docs.cloud.google.com/iam/docs/keys-create-delete#allow-service-account-key-creation)。
- **找不到文件夹／无权访问：** 检查文件夹 ID、两个 API，以及共享时是否填对服务账号邮箱。你能在浏览器打开文件夹，不代表 Unity 使用的账号也能访问。
- **没有读到表：** 使用直接放在指定文件夹中的原生 Google Sheets 文件。上传的 `.xlsx` 先转换格式。

导入器只请求 Drive 元数据和 Sheets 的只读权限，不会修改云端表格。文件会继承文件夹的共享权限，但学校或公司的策略可能限制服务账号访问。参见 [Google 文件夹共享说明](https://support.google.com/drive/answer/7166529?hl=zh-Hans)。

</details>

## 03 切换已有来源

先准备目标来源，确保它与当前生效配置的**表名、字段名、字段类型和列顺序一致**。例如，将 Demo.xlsx 导入为原生 Google Sheets 后再把 Demo 切到 Google；从 Google 切回本地时，先将工作簿下载为 `.xlsx`，放到游戏自己的 Source 文件夹。

打开 **Framework → Config**，选择目标来源和文件夹，再点击 **Switch Source**；目标与当前生效来源不同时，主按钮会自动显示这个名称。目标中的所有表都必须通过检查，表名和结构也必须与当前配置一致。检查失败时，原来的有效来源和生成配置保持不变；切换时不会混入旧来源中的部分表。

切换成功后，日常 **Update Config** 只读取新来源，并继续按表保留出错表的旧版。两种来源之间不会自动同步，也不会在读取失败时互相替补。后续需要改表结构时，在当前来源中修改并正常更新即可。
