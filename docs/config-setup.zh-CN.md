# 配置接入

[总览](../README.zh-CN.md) · [配置表](config.zh-CN.md) · [快速开始](quick-start.zh-CN.md) · [English](config-setup.md)

**首次连接 Google Sheets 与 Unity，之后修改只需 Update Config。** 随包 Demo 无需这套设置即可运行；需要编辑表格时再连接自己的来源。

## 首次设置

你需要一个 Google 账号，以及创建 Cloud 项目和服务账号密钥的权限。服务账号就是 Unity 读取共享文件夹时使用的账号。

### 准备 Google 访问权限

1. **创建 Cloud 项目。** 打开 [Google Cloud Console](https://console.cloud.google.com/projectcreate)，输入名称，例如 `MyGame Config`，创建项目。继续前先选中该项目。[Google 项目指南](https://developers.google.com/workspace/guides/create-project)。

2. **启用两个 API。** 在 **APIs & Services → Library** 中分别搜索并启用 **Google Drive API** 和 **Google Sheets API**，确认它们属于刚才的项目。[Google API 指南](https://developers.google.com/workspace/guides/enable-apis)。

3. **创建服务账号。** 打开 **IAM & Admin → Service Accounts → Create service account**，输入名称，例如 `mygame-config-reader`，最后点击 **Done**。可选的项目角色和用户访问权限留空；文件夹权限在第 5 步单独授予。[Google 服务账号指南](https://docs.cloud.google.com/iam/docs/service-accounts-create)。

4. **下载 JSON 密钥。** 打开新账号，依次选择 **Keys → Add key → Create new key → JSON → Create**。将下载的文件移到 **Unity 工程和任何 Git 仓库之外**的私人位置，例如专用凭据文件夹。文件留在自己的电脑上，不要把内容粘贴到代码、Sheets 或聊天中。[Google 密钥指南](https://docs.cloud.google.com/iam/docs/keys-create-delete)。

5. **创建并共享 Drive 文件夹。** 在 [Google Drive](https://drive.google.com/) 新建 `MyGame Config` 文件夹。将它共享给 Cloud Console 中显示的服务账号邮箱，也就是密钥文件的 `client_email`，权限设为 **Viewer（查看者）**。取消勾选 **Notify people（通知人员）**，完成共享。常规访问保持受限；需要编辑的策划或组员可另加为 **Editor（编辑者）**。这种直接共享不需要全域委派。[Google 共享指南](https://developers.google.com/workspace/guides/create-credentials#access-google-workspace-files-directly-with-a-service-account)。

### 创建并连接表格

6. **创建第一张表。** 在共享 Drive 文件夹内选择 **New → Google Sheets → Blank spreadsheet**。使用当前教程中的表，例如[快速开始中的 UI 示例](quick-start.zh-CN.md#3-连接并更新-ui-配置表)、普通数据的 [Item 示例](config.zh-CN.md#添加一张表)，或资源／场景教程中的 Resource／Scene 表。把第一个标签改为对应的表名，从 A1 输入该示例。工作簿名可自定；标签名才是表名。工作簿直接放在该文件夹中，不使用子文件夹或快捷方式。

7. **连接 Unity。** 如果还没有，创建 `Assets/GameContent/MyGame/`。打开 **Framework → Config**，在 **Game** 选择此文件夹，填写 **Namespace = MyGame**，把文件夹 ID 填入 **Google Drive Folder ID**。例如网址为 `https://drive.google.com/drive/folders/FOLDER_ID` 时，只复制 `FOLDER_ID`。在 **Service Account JSON** 选择下载的密钥文件。

8. **点击 Update Config。** 等待 Unity 编译完成，确认所选表已更新。返回刚才的教程，核对生成文件并继续。只需创建当前教程使用的那张表。

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
