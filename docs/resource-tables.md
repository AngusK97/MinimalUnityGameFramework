# Resource tables

[Overview](../README.md) · [Config setup](config.md) · [简体中文](resource-tables.zh-CN.md)

**Edit the Resource tab → Update Config → Load a module → Use main.Cue.** A module groups assets with the same loading lifetime. For a component's own fixed asset, an Inspector reference may be enough.

## Add resources

1. Complete [Config setup](config.md#first-time-setup). Put **cue.wav** in `Assets/GameContent/MyGame/Content/Audio/`. Select it in **Project** and check **Addressable** in its Inspector.
2. Open the **Resource** tab in your configuration folder; create it only if none exists. For a new tab, enter the example below from A1. For an existing tab, keep its first two rows and append the **Cue** data row:

| Id | Name | Type | Module | AssetPath |
| --- | --- | --- | --- | --- |
| string | string | string | string | string |
| Cue | Button sound | AudioClip | Main | Assets/GameContent/MyGame/Content/Audio/cue.wav |

3. In Unity, open **Framework → Config**, select **MyGame**, click **Update Config** and wait for compilation.

**Success:** `MyGame/Scripts/Generated/MainResources.g.cs` exists. It exposes `Id = "MyGame.Main"` and an `AudioClip Cue` property. The tool also generates ResourceTable, ResourceConfig and local JSON. `Name` is a description for people; it is not a generated field.

| Type | Property type |
| --- | --- |
| `AudioClip` | `UnityEngine.AudioClip` |
| `Prefab` | `UnityEngine.GameObject` |
| `Sprite` | `UnityEngine.Sprite` |

Assets must exist and have loadable Addressables entries. Check **Addressable** for each referenced asset; its location under GameContent alone is not enough. For sprites, use a Single-mode texture or a standalone Sprite main asset; sliced sprites and other sub-assets are unsupported.

## Use a module

After Core is ready, call `GameConfig.LoadAndRegister()`. Create and register each required resource module once, then load it explicitly. Follow the complete [Resources example](resources.md) for startup, failure and cleanup.

Once MainResources is loaded, gameplay can use:

```csharp
GameCore.Sound.PlaySound(SoundLayer.UI, main.Cue);
```

This line needs `using Core; using Sound;`. Property access does not trigger loading. Adding a row requires no new startup registration; a new module needs explicit integration.

## Change resources

- **Add or replace:** edit the row, update config, then use the generated property. Keeping Id preserves callers.
- **Move an asset:** update AssetPath and import again.
- **Rename or delete Id/Module:** update config, then fix callers and module registration. The module ID is `<namespace>.<Module>`.
- **Commit:** include generated CSV, manifest, C#, JSON, assets and meta files together. Resolve the Sheets source and import again instead of merging generated bodies by hand.

Start with **Main**. Use other Module values, such as **Level01**, when assets need a different lifetime. Missing or wrong-type assets are errors; [Config tables](config.md) explains retained versions and recovery. Generated internal resource numbers are not save or network IDs.

Next: [Register, load and release the module](resources.md).
