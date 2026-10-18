# Resource tables

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](resource-tables.zh-CN.md)

**Edit Main.asset → Generate → Load the module → Use main.Cue.** One table defines one group of shared assets loaded and released together. For a component's own fixed asset, a normal Inspector reference may be enough.

## Create a table

1. Put **cue.wav** in `Assets/GameContent/MyGame/Content/Audio/`.
2. In `Assets/GameContent/MyGame/Editor/Tables/ResourceModules/`, choose **Create → Framework → Resource Module Table**. Name the asset **Main**.
3. Select Main.asset and fill in the native Inspector:

| Field | Example |
| --- | --- |
| Namespace | `MyGame` |
| Class Name | `MainResources` |
| Module Id | `MyGame.Main` |
| Audio Clips → Name | `Cue` |
| Audio Clips → Asset | cue.wav from Project |
| Prefabs | Optional named GameObject prefabs |
| Sprites | Optional named Sprite assets |

4. Click **Validate and Generate**, then **wait for compilation** before writing callers.

**Success:** `MyGame/Scripts/Generated/MainResources.g.cs` exists and the Inspector reports **Generated code is up to date.**

The generated `MyGame.MainResources` inherits `BaseResourceModule`, exposes `public const string Id = "MyGame.Main"` and an `AudioClip Cue` property. Prefabs produce `GameObject` properties; Sprites produce `Sprite` properties. Names are C# code labels, not player-facing text.

Ordinary content under `Assets/GameContent/` inherits its existing Addressables folder entry: do not register every asset again. The generator resolves and validates addresses; it does not change your groups. When adopting the framework elsewhere, set up your content root once.

## Use it

Follow the complete startup, load and cleanup example in [Resources](resources.md). Register each module once per Core lifetime, load it explicitly, then keep the loaded instance for gameplay:

```csharp
GameCore.Sound.PlaySound(SoundLayer.UI, main.Cue);
```

This line assumes Core and the module are ready (`using Core; using Sound;`). Access does not trigger loading. Adding a row needs no new startup registration; adding a new module needs one explicit integration.

## Change resources

- **Add:** add a Name and Asset to the appropriate list, generate, then use the new property.
- **Replace:** change the Asset and generate; keeping Name preserves callers.
- **Move:** keep the asset's meta and regenerate so generated addresses follow the move.
- **Rename/delete a Name:** generate, then update callers of the changed property.
- **Commit:** include the table, generated C#, assets and their meta files. Edit the table, not generated code. After a merge, resolve the table first and regenerate.

Keep separate tables for groups with different loading lifetimes, such as Main and Level01. You can start with Main alone.

<details>
<summary>Folders, validation and recovery</summary>

Tables live at `<game root>/Editor/Tables/ResourceModules/<name>.asset`; output goes to `<game root>/Scripts/Generated/<ClassName>.g.cs`. `ResourceModuleTable` and `ResourceModuleTableGenerator` are Editor-only; Player uses the generated class. Demo's [Main.asset](../Assets/GameContent/Demo/Editor/Tables/ResourceModules/Main.asset) generates [MainResources.g.cs](../Assets/GameContent/Demo/Scripts/Generated/MainResources.g.cs), with type `Demo.MainResources` and ID `Demo.Main`. Both belong to the removable Demo; the framework does not depend on them.

Use ASCII C# identifiers for names, excluding keywords; Namespace may be empty or dot-separated. Names must be unique across all three lists; Id, the generated class name and inherited member names are reserved. Module Id must be non-empty and unique. Validation rejects conflicting names/types/module IDs, invalid references and unavailable or ambiguous Addressables keys. Use saved assets, not scene objects. Sprites support Single-mode textures and standalone Sprite main assets. Multiple-mode slices, atlases and other sub-assets are rejected rather than silently loading a different sprite. Groups need Include in Build and Include Addresses in Catalog.

**Rename Class Name:** generate the new type, update callers, then remove the previous filename and its meta; the build check identifies obsolete output. **Change Namespace:** generate and update callers; the output filename stays the same. **Change Module Id:** update lookups and any stored IDs. **Move a game root:** move its table and existing output/meta together, then generate. Old outputs are not deleted automatically. Internal resource numbers are generated implementation details; never store them in saves or network messages.

Validation failure preserves old output. Generation protects handwritten files and edited output; identical output is not rewritten. Generation is disabled during Play/compilation; Play uses the last generated version. Player prebuild checks reject missing, stale, edited or obsolete output, including generated files whose source table was deleted. A project with no resource tables is valid.

</details>

Next: [Register, load and release the module](resources.md) · [Build](build.md)
