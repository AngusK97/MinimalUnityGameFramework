# UI tables

[Overview](../README.md) · [Config setup](config-setup.md) · [简体中文](ui-tables.zh-CN.md)

**Edit and save the UI tab → Update Config → Use UIName.** Making your first window? Follow [Quick start](quick-start.md). Choose your source once with [Config setup](config-setup.md#01-first-time-setup).

<a id="add-a-window"></a>

## 01 Add a window

1. Save a UI prefab under your game's `Content/Prefabs/UI/`. Its RectTransform root must have exactly one UIBase-derived component. Select the saved prefab in **Project** and check **Addressable** in its Inspector.
2. Here is an example **UI** table. If this is your first table, create a local workbook with these rows using [Config setup](config-setup.md). If you already have a source, open its **UI** tab, or add one if it is missing. Enter all rows from A1 in a new tab; in an existing tab, keep its first two rows and add only the **Shop** data row.

| Id | Name | PrefabPath |
| --- | --- | --- |
| string | string | string |
| Shop | Shop window | Assets/GameContent/MyGame/Content/Prefabs/UI/ShopView.prefab |

3. For your first connection, follow [Config setup](config-setup.md) using the table above. If already connected, check that **MyGame** is selected in **Framework → Config**. Save the workbook (or wait for Google Sheets to finish saving), then click **Update Config** on the left side of Unity's top toolbar. Wait for the result and compilation.

**Success:** `MyGame/Scripts/Generated/UIName.g.cs` contains `Shop = "Shop"`. The tool also generates UITable, UIConfig and local JSON data. Use `Name` to describe the window for yourself and your teammates. It does not become a field in the generated code.

<a id="use-it"></a>

## 02 Use it

In **MyGameEntry.cs** from Quick start, put this in `Start()` after the existing `GameConfig.LoadAndRegister()` call. Replace the Menu OpenUI call if you want Shop to be the first window. Do not load config again; it runs once per Core lifetime (`using Core; using UI;`):

```csharp
GameCore.UI.OpenUI(UILayer.First, MyGame.UIName.Shop);
```

The generated config connects `Shop` to its Addressables address. The game namespace keeps C# types separate, but does not prefix UI IDs at runtime.

<a id="change-a-window"></a>

## 03 Change a window

- **Add:** add a row, save, update config, then use `UIName.NewId`.
- **Replace or move:** update PrefabPath, save and update config. If you keep the same Id, the code that opens it can stay as it is.
- **Delete or rename Id:** update config, then fix callers and any saved IDs.
- **Commit:** include the local workbook, source settings, generated CSV, manifest, C#, JSON and meta files together. Edit the active source, not generated files.

If the prefab is missing, the import gives a warning. You can prepare its ID first, but you need to create the prefab and update config again before opening the window. A wrong type, invalid UIBase root or unusable Addressables entry is an error. **Player builds reject missing UI prefabs**; finish or remove placeholders before building.

For partial updates, generated files and recovery, see [Config tables](config.md). For window behavior, see [UI](ui.md).
