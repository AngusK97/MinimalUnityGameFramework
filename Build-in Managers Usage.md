# Usage index & migration

[Overview](README.md) · [简体中文](Build-in%20Managers%20Usage.zh-CN.md)

The usage guide now has focused pages. **Start with [Quick start](docs/quick-start.md)** to build your first window; use the module pages for daily work.

| Task | Guide |
| --- | --- |
| Prepare services | [Core](docs/core.md) |
| Open/close windows | [UI](docs/ui.md) |
| Edit and generate UI definitions | [UI tables](docs/ui-tables.md) |
| Subscribe/send notifications | [Events](docs/events.md) |
| Own and release assets | [Resources](docs/resources.md) |
| Edit and generate shared resource definitions | [Resource tables](docs/resource-tables.md) |
| Change scenes | [Scenes](docs/scenes.md) |
| Play sound | [Audio](docs/audio.md) |
| Package or troubleshoot | [Build](docs/build.md) |

For the Editor/package upgrade, see [Moving to Unity 6.6](docs/upgrading.md). Existing games can stay pinned to their older framework and Editor. The API migration below is for projects that choose to adopt the current framework.

To move from **v0.1.0 to v0.2.0**, rename `Assets/GameResources` to `Assets/Framework`, preserving its directory `.meta`; move `Assets/Scripts` and its `.meta` inside it, then rename `Prefab` and its `.meta` to `Prefabs`. Update InitScene’s path in the native Scene List to `Assets/Framework/Scenes/InitScene.unity`. The directory move preserves GUIDs and public APIs; `GameContent/Demo` stays in place. Pinned **v0.1.0** keeps the old layout.

## Migrating an older project

| Earlier template | Current approach |
| --- | --- |
| Shared UIName / EventName / ResourceModuleName enums | Game-owned string IDs; generate UI constants from UI.asset |
| Framework Init registers Menu/Main automatically | Your startup registers its UI and resource modules after Core is ready |
| GameCore.Data / GameCore.Level | Your game's own data and level controllers |
| Core Auto Start Game / StartupError / RetryStartup | Core prepares services; your game's startup owns its flow and retry policy |

An older custom Core prefab must remove its Data/Level children and references, keep the five shared services, and use your own startup. The supplied prefab and Demo are already migrated. See [Core](docs/core.md) for lifetime boundaries.

Manual UI registration still works. When adopting tables, preserve Name/Id Prefix to preserve IDs, remove the old handwritten constant class and duplicate registrations, then follow [UI tables](docs/ui-tables.md). Migrate any IDs stored in your own saves/messages explicitly.

Handwritten `BaseResourceModule` subclasses still work. To adopt resource tables, preserve the module ID, remove the old definition and register the generated class once. Demo now uses `Demo.MainResources` (`Id = "Demo.Main"`): replace `MainResourceModule` lookups with `MainResources` and `main.GetRes<AudioClip>(MainResource.Cue)` with `main.Cue`. Its old Demo-only class/enum are removed. Duplicate module registration now throws; requesting a registered module as the wrong type throws `InvalidOperationException`. See [Resource tables](docs/resource-tables.md).

Events now accept parameterless Action listeners and `DispatchNow(eventId)` / `Dispatch(eventId)`; the sender/data overloads remain supported. Keep string constants in your game and pair subscriptions with cleanup; no event-name registration is needed. See [Events](docs/events.md).
