# Usage index & migration

[Overview](README.md) · [简体中文](Build-in%20Managers%20Usage.zh-CN.md)

**Start with [Quick start](docs/quick-start.md)** to build your first window; use these pages for daily work. These guides match v0.5.0. The fixed [v0.2.0 guides](https://github.com/AngusK97/MinimalUnityGameFramework/blob/v0.2.0/docs/quick-start.md) retain the earlier Inspector/SO workflow.

**From v0.3.x / v0.4.x to v0.5.0:** existing settings with a Google `FolderId` remain Google sources; upgrading does not silently change them to local Excel. Table format and runtime APIs stay the same. New games default to **Local Excel**. To migrate an existing game to a local workbook, export the same tables to `.xlsx`, then use [Switch Source](docs/config-setup.md#03-switch-an-existing-source). The destination must pass all checks with the same table names and schema.

Daily updates still use **Update Config** on the left side of Unity's top toolbar. **Framework → Config** selects the game and source; progress, results and per-table retention work for either source.

| Task | Guide |
| --- | --- |
| Prepare services | [Core](docs/core.md) |
| Edit Excel or Google Sheets and update tables | [Config](docs/config.md) |
| Define windows | [UI tables](docs/ui-tables.md) |
| Open/close windows | [UI](docs/ui.md) |
| Subscribe/send notifications | [Events](docs/events.md) |
| Define shared resources | [Resource tables](docs/resource-tables.md) |
| Own and release assets | [Resources](docs/resources.md) |
| Define and change scenes | [Scenes](docs/scenes.md) |
| Play sound | [Audio](docs/audio.md) |
| Package or troubleshoot | [Build](docs/build.md) |

For the Editor/package upgrade, see [Moving to Unity 6.6](docs/upgrading.md). Existing games can stay pinned to their older framework and Editor. Migrate a separate copy when choosing the current framework.

<a id="from-v020-to-google-sheets"></a>

<a id="01-from-v020-to-google-sheets"></a>

## 01 From v0.2.0 to config tables

1. Back up the working game. Before removing the old setup, copy its UI names, prefab paths, resource names/types and module IDs. Keep the Unity assets and their meta files.
2. Follow [Config setup](docs/config-setup.md#01-first-time-setup). Create the **UI**, **Resource** and **Scene** tabs you need. The old UI entry Name becomes the new **Id**; the new **Name** column is only a description. Combine resource modules into the Resource tab using its **Module** column.
3. Remove the old UI.asset, resource module SO assets and their obsolete generated definitions from the migration copy after preserving their values. Back up old generated C# and meta files outside Assets before importing: the new importer does not overwrite files it does not own. Remove obsolete SO editor tools if you copied framework files over an older installation. If missing generated types prevent compilation, temporarily comment their callers until the first update completes.
4. In the Config dialog, click **Update Config**, then wait for the result and compilation. Restore and update callers as below. Commit the local workbook, source settings, generated CSV/manifest, C#, JSON and meta files together. Make later edits in the selected source.

| Earlier call or data | v0.5.0 |
| --- | --- |
| `GameUI.RegisterAll()` / `DemoUI.RegisterAll()` | `GameConfig.LoadAndRegister()` after Core is ready, once per Core lifetime |
| `GameUI.Shop` / `DemoUI.Menu` | `UIName.Shop` / `UIName.Menu` |
| UI ID formed from Id Prefix + Name, such as `MyGame.Shop` | Exact table Id, such as `Shop`; `UIName.Shop = "Shop"` |
| Module SO with ID `MyGame.Main` | Config Namespace `MyGame` + Resource Module `Main`; `MainResources.Id` remains `MyGame.Main` |
| `TryTransition("Assets/.../Menu.unity")` | Scene row + `TryTransition(SceneName.Menu)`, or explicit `TryTransitionAddress("Assets/.../Menu.unity")` |

Old prefixed UI IDs are not preserved automatically. Migrate IDs in saves/messages explicitly. New special-table IDs are ASCII C# identifiers, without dots. Keep UI IDs and scene IDs unique within the running game's registrations.

Load GameConfig before creating resource modules. Resource registration and loading remain explicit: `new MainResources()`, `GameCore.Resource.RegisterResourceModule(main)`, then `main.LoadResources(...)`. Follow [Resources](docs/resources.md); accessing `main.Cue` does not load it.

The Demo now includes `Editor/Tables/Source/Demo.xlsx` and uses that local source by default. Select Demo in **Framework → Config**, edit its workbook, and update. Existing games keep their own source settings. Generated CSV files remain snapshots and should not be edited.

<a id="earlier-directory-migration"></a>

## 02 Earlier directory migration

To move from **v0.1.0 to v0.2.0**, rename `Assets/GameResources` to `Assets/Framework`, preserving its directory `.meta`; move `Assets/Scripts` and its `.meta` inside it, then rename `Prefab` and its `.meta` to `Prefabs`. Update InitScene's path in the native Scene List to `Assets/Framework/Scenes/InitScene.unity`. The directory move preserves GUIDs and public APIs; `GameContent/Demo` stays in place. Pinned **v0.1.0** keeps the old layout.

<a id="earlier-api-migration"></a>

## 03 Earlier API migration

| Earlier template | Current approach |
| --- | --- |
| Shared UIName / EventName / ResourceModuleName enums | Game-owned string IDs and generated UI/scene constants |
| Framework Init registers Menu/Main automatically | Your startup loads config and registers resource modules after Core is ready |
| GameCore.Data / GameCore.Level | Your game's own data and level controllers |
| Core Auto Start Game / StartupError / RetryStartup | Core prepares services; your startup owns its flow and retry policy |

An older custom Core prefab must remove its Data/Level children and references, keep the five shared services, and use your own startup. See [Core](docs/core.md) for lifetime boundaries. Manual UI registration and handwritten `BaseResourceModule` subclasses still work; avoid duplicate registrations.

Demo uses `Demo.MainResources` (`Id = "Demo.Main"`): replace `MainResourceModule` lookups with `MainResources` and `main.GetRes<AudioClip>(MainResource.Cue)` with `main.Cue`. Duplicate module registration throws; requesting a registered module as the wrong type throws `InvalidOperationException`.

Events accept parameterless Action listeners and `DispatchNow(eventId)` / `Dispatch(eventId)`; sender/data overloads remain supported. Keep string constants in your game and pair subscriptions with cleanup; no event-name registration is needed. See [Events](docs/events.md).
