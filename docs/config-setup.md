# Config setup

[Overview](../README.md) · [Config tables](config.md) · [Quick start](quick-start.md) · [简体中文](config-setup.zh-CN.md)

**Connect Google Sheets to Unity once, then use Update Config for later edits.** The bundled Demo runs without this setup; connect your own source when you want to edit tables.

<a id="first-time-setup"></a>

## 01 First-time setup

You need a Google account and permission to create a Cloud project and a service account key. A service account is the account Unity uses to read your shared folder.

<a id="prepare-google-access"></a>

### 1.1 Prepare Google access

1. **Create a Cloud project.** Open [Google Cloud Console](https://console.cloud.google.com/projectcreate), name it something like `MyGame Config` and create it. Select that project before continuing. [Google's project guide](https://developers.google.com/workspace/guides/create-project).

2. **Enable two APIs.** In **APIs & Services → Library**, find and enable **Google Drive API** and **Google Sheets API** for this project. [Google's API guide](https://developers.google.com/workspace/guides/enable-apis).

3. **Create a service account.** Open **IAM & Admin → Service Accounts → Create service account**. Use a name such as `mygame-config-reader`, then finish with **Done**. Leave the optional project roles and user access fields empty; access to the folder is granted separately in step 5. [Google's service account guide](https://docs.cloud.google.com/iam/docs/service-accounts-create).

4. **Download its JSON key.** Open the new account, then **Keys → Add key → Create new key → JSON → Create**. Move the downloaded file to a private location **outside the Unity project and any Git repository**, such as a private credentials folder. Keep the file on your computer; do not paste its contents into code, Sheets or chat. [Google's key guide](https://docs.cloud.google.com/iam/docs/keys-create-delete).

5. **Create and share a Drive folder.** In [Google Drive](https://drive.google.com/), create `MyGame Config`. Share it with the service account's email shown in Cloud Console—the same value as `client_email` in the key file—as **Viewer**. Uncheck **Notify people** and share. Keep general access restricted; invite designers as **Editor** if needed. This direct sharing needs no domain-wide delegation. [Google's sharing guide](https://developers.google.com/workspace/guides/create-credentials#access-google-workspace-files-directly-with-a-service-account).

<a id="create-and-connect-a-table"></a>

### 1.2 Create and connect a table

6. **Create your first table.** Inside the shared Drive folder, choose **New → Google Sheets → Blank spreadsheet**. Use the table from the tutorial you are following, such as **UI** with the [Quick start rows](quick-start.md#23-connect-and-update-the-ui-table), **Item** with the [ordinary-data example](config.md#03-add-a-table), or the Resource / Scene table from its guide. Rename the first tab accordingly and enter that example from A1. The workbook's name is up to you; the tab name becomes the table name. Put workbooks directly in the folder, without subfolders or shortcuts.

7. **Connect Unity.** Create `Assets/GameContent/MyGame/` if needed. Open **Framework → Config**. The dialog shows all setup fields without a foldout. Select that folder in **Game**, enter **Namespace = MyGame**, and paste the folder ID into **Google Drive Folder ID**. In a URL such as `https://drive.google.com/drive/folders/FOLDER_ID`, copy only `FOLDER_ID`. Choose the downloaded key in **Service Account JSON**.

8. **Click Update Config.** A native progress window shows the update, followed by a result dialog. Wait for any compilation and confirm your table updated; use **Open report** for details. Return to the tutorial you came from to check its generated file and continue. You only need the table used by that tutorial.

The folder ID and namespace are saved in `Assets/GameContent/MyGame/Editor/ConfigSource.json`. The last selected game and the key's file path stay in this computer's Unity Editor preferences; the key is not copied into the project. On another computer, select a locally stored key again.

For later edits, click **Update Config** immediately left of **Play** in the top toolbar. It uses the selected game and opens this dialog if setup is incomplete or the key file is missing. Return to **Framework → Config** whenever you need to change games or settings.

<img src="images/config-update.jpg" alt="Unity Config dialog with Game, Namespace, Google Drive Folder ID, Service Account JSON and Update Config" width="480">

The setup fields are shown here with Demo selected. Select **MyGame** for the steps above.

<details>
<summary>Setup problems and starting from the Demo</summary>

- **Key creation is disabled:** a school or organization policy may block service account keys. Ask its administrator whether this project can use a key. The current importer requires a service account JSON key. See [Google's key policy guidance](https://docs.cloud.google.com/iam/docs/keys-create-delete#allow-service-account-key-creation).
- **Folder not found / access denied:** check the folder ID, both enabled APIs, and that the folder is shared with the exact service account email. Being able to open it in your own browser does not grant Unity access.
- **No tables:** use native Google Sheets files directly inside the selected folder. An uploaded `.xlsx`, shortcut or workbook in a subfolder is not a source in this version.
- **Connect the bundled Demo:** import all three CSV snapshots from `Assets/GameContent/Demo/Editor/Tables/Generated/` into Google Sheets tabs named **UI**, **Resource** and **Scene**, then set your folder ID in the Config window. The first connection requires all three tables to import successfully; otherwise the whole bundled snapshot stays unchanged. Later updates allow per-table retention. These are starting copies, not a public cloud template; change Demo asset paths when adapting them to your own game.

The importer requests read-only Drive metadata and Sheets access. It does not edit your cloud tables. Folder sharing is inherited by its files; school or workplace sharing policies may restrict service-account access. See [Google Drive's folder sharing guide](https://support.google.com/drive/answer/7166529?hl=en).

</details>
