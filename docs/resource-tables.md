# Resource tables

[Overview](../README.md) · [Config setup](config-setup.md) · [简体中文](resource-tables.zh-CN.md)

**Edit the Resource tab → Update Config → Load a module → Use main.Cue.** A module groups assets with the same loading lifetime. For a component's own fixed asset, an Inspector reference may be enough.

<a id="add-resources"></a>

## 01 Add resources

1. Put **cue.wav** in `Assets/GameContent/MyGame/Content/Audio/`. Select it in **Project** and check **Addressable** in its Inspector.
2. Here is the **Resource** table. For your first connection, enter it at step 6 of [Config setup](config-setup.md). If already connected, open the existing **Resource** tab or create one if missing. Enter all rows from A1 in a new tab; in an existing tab, keep its first two rows and add only the **Cue** data row.

| Id | Name | Type | Module | AssetPath |
| --- | --- | --- | --- | --- |
| string | string | string | string | string |
| Cue | Button sound | AudioClip | Main | Assets/GameContent/MyGame/Content/Audio/cue.wav |

3. For your first connection, follow [Config setup](config-setup.md) using the table above. If already connected, click **Update Config** on the left side of Unity's top toolbar and wait for the result and compilation. To switch games first, choose **MyGame** in **Framework → Config**.

**Success:** `MyGame/Scripts/Generated/MainResources.g.cs` exists. It exposes `Id = "MyGame.Main"` and an `AudioClip Cue` property. The tool also generates ResourceTable, ResourceConfig and local JSON. `Name` is a description for people; it is not a generated field.

| Type | Property type |
| --- | --- |
| `AudioClip` | `UnityEngine.AudioClip` |
| `Prefab` | `UnityEngine.GameObject` |
| `Sprite` | `UnityEngine.Sprite` |

Assets must exist and have loadable Addressables entries. Check **Addressable** for each referenced asset; its location under GameContent alone is not enough. For sprites, use a Single-mode texture or a standalone Sprite main asset; sliced sprites and other sub-assets are unsupported.

<a id="use-a-module"></a>

## 02 Use a module

After Core is ready, call `GameConfig.LoadAndRegister()`. Create and register each required resource module once, then load it explicitly. Follow the complete [Resources example](resources.md) for startup, failure and cleanup.

Once MainResources is loaded, gameplay can use:

```csharp
GameCore.Sound.PlaySound(SoundLayer.UI, main.Cue);
```

This line needs `using Core; using Sound;`. Property access does not trigger loading. Adding a row requires no new startup registration; a new module needs explicit integration.

<a id="change-resources"></a>

## 03 Change resources

- **Add or replace:** edit the row, update config, then use the generated property. Keeping Id preserves callers.
- **Move an asset:** update AssetPath and import again.
- **Rename or delete Id/Module:** update config, then fix callers and module registration. The module ID is `<namespace>.<Module>`.
- **Commit:** include generated CSV, manifest, C#, JSON, assets and meta files together. Resolve the Sheets source and import again instead of merging generated bodies by hand.

Start with **Main**. Use other Module values, such as **Level01**, when assets need a different lifetime. Missing or wrong-type assets are errors; [Config tables](config.md) explains retained versions and recovery. Generated internal resource numbers are not save or network IDs.

Next: [Register, load and release the module](resources.md).
