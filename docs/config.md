# Config tables

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](config.zh-CN.md)

**Edit Google Sheets → Update Config → Use the generated data.** The Editor downloads and validates tables; the game reads local generated files. Players do not connect to Google.

The bundled Demo already includes generated configuration. You can run it without a Google account, credentials or an internet connection. Set up your own source below when you want to change tables.

## Daily workflow

1. Edit the Google Sheets tabs in your configuration folder.
2. In Unity, open **Framework → Config**, select your game and click **Update Config**. Wait for compilation.
3. Check the result below. Fix reported errors in Sheets or Unity, then click **Update Config** again.

Each update reads the current table contents from Google Sheets and writes only generated outputs whose contents have changed.

There is no completed-table selection or extra publish step. A valid row can be imported while someone is still editing it, so agree with teammates before importing an unfinished change.

### Read the result

These examples use the Demo tables. **Successful update:** all tables passed validation and their current versions are applied.

<img src="images/config-success.jpg" alt="Unity Config window showing a successful update of the Demo tables" width="600">

**Partial update:** valid tables update; an invalid table keeps its last successful version. Read the table's error, fix it and update again.

<img src="images/config-partial-update.jpg" alt="Unity Config result showing valid Demo tables applied and an invalid table kept at its previous version" width="600">

## First-time setup

You need a Google account and permission to create a Cloud project and a service account key. A service account is the account Unity uses to read your shared folder.

### Prepare Google access

1. **Create a Cloud project.** Open [Google Cloud Console](https://console.cloud.google.com/projectcreate), name it something like `MyGame Config` and create it. Select that project before continuing. [Google's project guide](https://developers.google.com/workspace/guides/create-project).

2. **Enable two APIs.** In **APIs & Services → Library**, find and enable **Google Drive API** and **Google Sheets API** for this project. [Google's API guide](https://developers.google.com/workspace/guides/enable-apis).

3. **Create a service account.** Open **IAM & Admin → Service Accounts → Create service account**. Use a name such as `mygame-config-reader`, then finish with **Done**. Leave the optional project roles and user access fields empty; access to the folder is granted separately in step 5. [Google's service account guide](https://docs.cloud.google.com/iam/docs/service-accounts-create).

4. **Download its JSON key.** Open the new account, then **Keys → Add key → Create new key → JSON → Create**. Move the downloaded file to a private location **outside the Unity project and any Git repository**, such as a private credentials folder. Keep the file on your computer; do not paste its contents into code, Sheets or chat. [Google's key guide](https://docs.cloud.google.com/iam/docs/keys-create-delete).

5. **Create and share a Drive folder.** In [Google Drive](https://drive.google.com/), create `MyGame Config`. Share it with the service account's email shown in Cloud Console—the same value as `client_email` in the key file—as **Viewer**. Uncheck **Notify people** and share. Keep general access restricted; invite designers as **Editor** if needed. This direct sharing needs no domain-wide delegation. [Google's sharing guide](https://developers.google.com/workspace/guides/create-credentials#access-google-workspace-files-directly-with-a-service-account).

### Create and connect a table

6. **Create your first table.** Inside that Drive folder, choose **New → Google Sheets → Blank spreadsheet**. Rename the first tab to **Item** and enter the three rows below, starting at cell A1. The workbook's name is up to you; the tab name becomes the table name. Put workbooks directly in the folder, without subfolders or shortcuts.

| Id | Name | Price |
| --- | --- | --- |
| string | string | int |
| Potion | Small potion | 20 |

7. **Connect Unity.** Create `Assets/GameContent/MyGame/` if needed. Open **Framework → Config**, select that folder in **Game**, enter **Namespace = MyGame**, and paste the folder ID into **Google Drive Folder ID**. In a URL such as `https://drive.google.com/drive/folders/FOLDER_ID`, copy only `FOLDER_ID`. Choose the downloaded key in **Service Account JSON**.

8. **Click Update Config.** Wait for Unity to compile. Confirm the result reports the Item table and `Assets/GameContent/MyGame/Scripts/Generated/ItemTable.g.cs` exists. You can now load and read it using the example below.

The folder ID and namespace are saved in `Assets/GameContent/MyGame/Editor/ConfigSource.json`. The key's file path stays in this computer's Unity Editor preferences; the key is not copied into the project. On another computer, select a locally stored key again.

<img src="images/config-update.jpg" alt="Unity 6 Config window with game selection, first-time setup and Update Config" width="480">

The setup fields are shown here with Demo selected. Select **MyGame** for the steps above.

<details>
<summary>Setup problems and starting from the Demo</summary>

- **Key creation is disabled:** a school or organization policy may block service account keys. Ask its administrator whether this project can use a key. The current importer requires a service account JSON key. See [Google's key policy guidance](https://docs.cloud.google.com/iam/docs/keys-create-delete#allow-service-account-key-creation).
- **Folder not found / access denied:** check the folder ID, both enabled APIs, and that the folder is shared with the exact service account email. Being able to open it in your own browser does not grant Unity access.
- **No tables:** use native Google Sheets files directly inside the selected folder. An uploaded `.xlsx`, shortcut or workbook in a subfolder is not a source in this version.
- **Connect the bundled Demo:** import all three CSV snapshots from `Assets/GameContent/Demo/Editor/Tables/Generated/` into Google Sheets tabs named **UI**, **Resource** and **Scene**, then set your folder ID in the Config window. The first connection requires all three tables to import successfully; otherwise the whole bundled snapshot stays unchanged. Later updates allow per-table retention. These are starting copies, not a public cloud template; change Demo asset paths when adapting them to your own game.

The importer requests read-only Drive metadata and Sheets access. It does not edit your cloud tables. Folder sharing is inherited by its files; school or workplace sharing policies may restrict service-account access. See [Google Drive's folder sharing guide](https://support.google.com/drive/answer/7166529?hl=en).

</details>

## Read a table

In your game's startup, **after Core is ready and before opening UI or changing scenes**, load the generated configuration once per Core lifetime:

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

The tab **Item** generates the query entry **ItemTable** and row type **ItemConfig**. `Get` throws if the ID does not exist; use `TryGet(id, out var row)` when absence is expected. Reading before loading is an error.

`GameConfig.LoadAndRegister()` loads local table data and registers UI and scene IDs. Resource modules still need explicit registration and loading; follow [Resources](resources.md) before using `main.Cue`.

## Table format

Use one tab per table. Table names must be unique across all workbooks in the selected folder.

| Spreadsheet row | Content |
| --- | --- |
| 1 | Field names, with `Id` first |
| 2 | Field types: `string`, `int`, `float`, `bool`, or `ref:TableName` |
| 3 onward | Data; each `Id` is non-empty and unique within the table |

The Demo's **UI** tab shows this layout in Google Sheets: field names in row 1, types in row 2, and data from row 3. It uses the UI fields described below; the first-time setup above uses an Item table.

<img src="images/config-sheet.jpg" alt="Demo UI table in Google Sheets showing field names, string types and UI data rows" width="900">

`Id` must be `string` or `int`. Use ASCII C# identifiers for table and field names. Enter literal values, with `true` / `false` for booleans and a dot for decimals. Formulas are not supported in this version. `ref:Item` means that cell must contain an existing Item ID.

### UI, resources and scenes

These three tab names have fixed headers. Every field in them uses `string` in row 2. Their `Name` column is a human-readable description and is omitted from generated row fields. In ordinary tables such as Item, a `Name` field is normal data.

**UI** — add a row after creating the prefab described in [Quick start](quick-start.md):

| Id | Name | PrefabPath |
| --- | --- | --- |
| string | string | string |
| Shop | Shop window | Assets/GameContent/MyGame/Content/Prefabs/UI/ShopView.prefab |

This generates `UIName.Shop = "Shop"`. Open it with `GameCore.UI.OpenUI(UILayer.First, MyGame.UIName.Shop)` after loading config. The ID stays independent of the prefab's Addressables address.

**Resource** — add assets that exist in your project:

| Id | Name | Type | Module | AssetPath |
| --- | --- | --- | --- | --- |
| string | string | string | string | string |
| Cue | Button sound | AudioClip | Main | Assets/GameContent/MyGame/Content/Audio/cue.wav |

`Type` is `AudioClip`, `Sprite` or `Prefab`. `Module = Main` generates **MainResources** with a typed `Cue` property. Keep assets with the same loading lifetime in one module. [Load and release resources](resources.md).

**Scene** — use a saved scene asset:

| Id | Name | ScenePath |
| --- | --- | --- |
| string | string | string |
| Menu | Main menu | Assets/GameContent/MyGame/Scenes/Menu.unity |

This generates `SceneName.Menu = "Menu"`. Call `GameCore.Scene.TryTransition(MyGame.SceneName.Menu)` after loading config. [Scene transitions](scenes.md).

Use ASCII C# identifiers for these IDs and resource module names. Paths start with `Assets/`. Select each referenced prefab, resource or scene asset in **Project** and check **Addressable** in its Inspector before updating config. Keeping the asset under GameContent alone is not enough.

<details>
<summary>Validation, partial updates and generated files</summary>

A missing UI prefab produces a warning, so its ID can be prepared before the prefab exists. Player builds require you to finish the prefab or remove its placeholder row and update config. Opening that UI cannot succeed until the prefab is ready and you update config again. A wrong asset type, invalid UIBase component setup or unusable Addressables entry is an error. Missing Resource or Scene assets are errors in this version.

Each table updates as a unit. An invalid table keeps its last successful code and data; on the first import it has no older version to keep. If a new table depends on a rejected change through `ref:TableName`, the dependent change also stays back. The final combination must have valid references. This does not restore a Unity asset someone deleted. Retained tables are checked against local assets too; an unusable retained resource or scene stops the update.

The result can therefore contain both updated and retained tables. Authentication or folder-listing failures stop the update. Read the report before committing; successful validation checks structure and references, not whether the game design is finished.

Generated outputs belong to the selected game:

```text
Assets/GameContent/MyGame/
├── Editor/
│   ├── ConfigSource.json         Folder ID and namespace
│   └── Tables/Generated/         CSV snapshots and applied manifest
├── Scripts/Generated/           C# tables, row types and registration
└── Resources/Config/MyGame/     Local JSON data (namespace folder)
```

Treat generated CSV, C# and JSON as read-only. Commit them together with the manifest, source settings and Unity meta files so teammates can run the same version without cloud access. The manifest records the source and applied versions. Local import reports stay under `Library/FrameworkConfig/Reports/`, outside Git.

Edit or resolve conflicts in the Google Sheets source, then run **Update Config** again; do not combine generated files by hand. Renaming or deleting fields, tables, IDs or modules can break handwritten C# callers. Fix those compile errors after generation. Moving an asset also requires updating its path in Sheets and importing again.

</details>

Small gameplay values can stay in the Inspector or your own ScriptableObjects. Use tables when many rows share a structure. Dedicated text/localization and sound-playback configuration are deferred; AudioClip resource registration is available now.

Next: [UI](ui.md) · [Resources](resources.md) · [Scenes](scenes.md) · [Build](build.md)
