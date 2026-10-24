# UI tables

[Overview](../README.md) · [Config setup](config.md) · [简体中文](ui-tables.zh-CN.md)

**Edit the UI tab → Update Config → Use UIName.** Making your first window? Follow [Quick start](quick-start.md). Connect Google Sheets once with [Config setup](config.md#first-time-setup).

## Add a window

1. Save a UI prefab under your game's `Content/Prefabs/UI/`. Its RectTransform root must have exactly one UIBase-derived component. Select the saved prefab in **Project** and check **Addressable** in its Inspector.
2. Open the **UI** tab in your configuration folder; create it only if none exists. For a new tab, enter the example below from A1. For an existing tab, keep its first two rows and append the **Shop** data row:

| Id | Name | PrefabPath |
| --- | --- | --- |
| string | string | string |
| Shop | Shop window | Assets/GameContent/MyGame/Content/Prefabs/UI/ShopView.prefab |

3. In Unity, open **Framework → Config**, select **MyGame**, click **Update Config** and wait for compilation.

**Success:** `MyGame/Scripts/Generated/UIName.g.cs` contains `Shop = "Shop"`. The tool also generates UITable, UIConfig and local JSON data. `Name` is a description for people; it is not a generated field.

## Use it

In game startup, load config once after Core is ready. Then open windows by ID (`using Core; using UI;`):

```csharp
MyGame.GameConfig.LoadAndRegister();
GameCore.UI.OpenUI(UILayer.First, MyGame.UIName.Shop);
```

The ID is `Shop`; the generated registration resolves its Addressables address. The game namespace keeps C# types separate, but does not prefix UI IDs at runtime.

## Change a window

- **Add:** add a row, update config, then use `UIName.NewId`.
- **Replace or move:** update PrefabPath and import again. Keeping Id preserves callers.
- **Delete or rename Id:** update config, then fix callers and any saved IDs.
- **Commit:** include generated CSV, manifest, C#, JSON and meta files together. Edit Google Sheets, not generated files.

A missing prefab is an import warning: you can prepare its ID early, but opening it fails until the asset exists and you update config again. A wrong type, invalid UIBase root or unusable Addressables entry is an error. **Player builds reject missing UI prefabs**; finish or remove placeholders before building.

For partial updates, output ownership and recovery, see [Config tables](config.md). For window behavior, see [UI](ui.md).
