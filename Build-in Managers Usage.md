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
| Change scenes | [Scenes](docs/scenes.md) |
| Play sound | [Audio](docs/audio.md) |
| Package or troubleshoot | [Build](docs/build.md) |

## Migrating an older project

| Earlier template | Current approach |
| --- | --- |
| Shared UIName / EventName / ResourceModuleName enums | Game-owned string IDs; generate UI constants from UI.asset |
| Framework Init registers Menu/Main automatically | Your startup registers its UI and resource modules after Core is ready |
| GameCore.Data / GameCore.Level | Your game's own data and level controllers |
| Core Auto Start Game / StartupError / RetryStartup | Core prepares services; your game's startup owns its flow and retry policy |

An older custom Core prefab must remove its Data/Level children and references, keep the five shared services, and use your own startup. The supplied prefab and Demo are already migrated. See [Core](docs/core.md) for lifetime boundaries.

Manual UI registration still works. When adopting tables, preserve Name/Id Prefix to preserve IDs, remove the old handwritten constant class and duplicate registrations, then follow [UI tables](docs/ui-tables.md). Migrate any IDs stored in your own saves/messages explicitly.
