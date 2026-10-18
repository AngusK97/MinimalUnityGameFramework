# UI tables

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](ui-tables.zh-CN.md)

**Edit UI.asset → Generate → Use the constant.** Making your first window? Follow [Quick start](quick-start.md).

## Create a table

1. In your game's `Editor/Tables/`, choose **Create → Framework → UI Table**. Keep the name **UI**.
2. Select UI.asset, expand **Entries**, click **+** and fill in:

| Field | Example |
| --- | --- |
| Namespace | `MyGame` |
| Class Name | `GameUI` |
| Id Prefix | `MyGame` |
| Entries → Name | `Menu` |
| Entries → Prefab | MyMenuView.prefab from Project; one UIBase-derived component on its root |

3. Click **Validate and Generate** and wait for compilation.

**Success:** `MyGame/Scripts/Generated/GameUI.g.cs` exists and the status reads **Generated code is up to date.**

![MyGame UI table Inspector with the Menu prefab reference and up-to-date generated code](images/ui-table-inspector.png)

Name is the code label, without spaces; the player-facing title stays in the prefab. Use a saved prefab, not a scene object.

## Use it

After Core is ready, register **once per Core lifetime**, then open a window:

```csharp
// In game startup, after GameCore.IsReady:
MyGame.GameUI.RegisterAll();
GameCore.UI.OpenUI(UILayer.First, MyGame.GameUI.Menu);
```

Add `using Core; using UI;`. Generate before writing code that references GameUI. The example's runtime ID is `MyGame.Menu`.

## Change a window

- **Add:** add Name and Prefab, generate, then use `GameUI.NewName`.
- **Move:** keep the prefab's meta and regenerate. Keep Name/Id Prefix to preserve the UI ID.
- **Delete:** remove the row, generate, then fix references to the removed constant.
- **Commit:** include UI.asset, generated C# and their meta files. Do not edit generated code.

Prefabs under `Assets/GameContent/` inherit its Addressables folder entry. Regenerate after an address change.

<details>
<summary>Folders, validation and recovery</summary>

Tables must be at `<game root>/Editor/Tables/UI.asset`; output goes to `<game root>/Scripts/Generated/<ClassName>.g.cs`. Future authoring tables can share Tables; only UI tables are implemented. The generator records the prefab's actual Addressables address.

Use ASCII C# identifiers for names; Namespace may be empty, and Namespace/Id Prefix may be dot-separated. The tool rejects keywords, RegisterAll, class/member conflicts, duplicate names/prefabs, cross-table class/full-ID collisions and unavailable/non-unique addresses. Prefabs need exactly one UIBase-derived component on the saved root; scene objects and child components are invalid. The Addressables group must enable Include in Build and Include Addresses in Catalog.

**Rename Class Name:** generate the new class, update callers, then delete the old output/meta. **Move the game root:** move the table and existing output/meta to the required locations together, then generate. Old files are not deleted automatically; handwritten type names are not parsed. Name/Id Prefix changes alter runtime IDs; migrate any stored IDs. Namespace changes require caller updates.

Validation failure preserves old output. Handwritten files, another table's output and edited generated bodies are not overwritten; restore or move aside edited output first. Identical content is not rewritten; CRLF/BOM checkouts are supported.

Tables/tools are Editor-only; Player uses generated code. Generation is disabled during Play/compilation; Play uses the last generated version. Player prebuild checks reject missing/stale/edited output. Zero tables is valid.

</details>

Next: [Window lifecycle](ui.md) · [Build](build.md)
