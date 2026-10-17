# Build & troubleshoot

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](build.zh-CN.md)

**Once your game runs in the Editor, let's make an app you can open on its own.** Unity calls that app a Player: choose the entry scene, build its assets, then build the Player.

## Build the Demo

1. In **File → Build Settings**, select a target with its build module installed; Switch Platform if needed.
2. Exit Play, generate any changed UI tables and wait for compilation.
3. Choose **Demo → Build Player**, then select an output location.
4. Open the built app. You should see the Demo menu, just as in the Editor. Try Start → keyboard win → Play again → Menu → Settings → Quit.

The Demo helper builds Addressables and supplies DemoStart for that build. It preserves the saved scene list and disappears when Demo is deleted.

## Build your game

1. In **File → Build Settings**, put your own `MyGame/Scenes/Start.unity` first in **Scenes In Build**. Remove the services-only InitScene entry if you do not need it; keep other native scenes your game uses.
2. Generate changed UI tables and wait for compilation.
3. Open **Window → Asset Management → Addressables → Groups**. Choose **Build → New Build → Default Build Script** and wait for success.
4. Back in Build Settings, choose **Build** and an output location.
5. Open the built app. Your own first screen should appear. Try its UI, input, sound and transitions.

Rebuild content after changing resources or target platform. **Use Asset Database (fastest)** is an Editor play-mode option; it does not supply a Player content build.

## If something fails

| Symptom | First check |
| --- | --- |
| No Demo menu | Open DemoStart.unity. InitScene deliberately initializes services only. |
| GameUI is missing | Temporarily remove/comment callers of the missing generated class, fix compilation, generate the table, then restore callers. |
| Generator is absent/disabled | Resolve Console compile errors, select UI.asset, exit Play and wait for compilation. |
| Invalid prefab | Reference a saved prefab with one UIBase-derived component on its root. |
| No loadable address | Keep runtime content under GameContent; check group Include in Build and Include Addresses in Catalog. |
| Window fails to open | Check Core readiness, RegisterAll, the actual address and the first Console error. |
| Duplicate EventSystem/AudioListener | Keep the shared Core's infrastructure; remove extra copies from scenes/prefabs. |
| Stale/edited output blocks build | Fix the table or restore generated output, generate and wait for compilation. |
| Player shows only a background | Put your own entry first; the default saved list contains only InitScene. |
| Scene load fails | Check its real Addressables key and rebuild content for the selected target. |

## Verification scope

The project declares Unity **2022.3.30f1** and Addressables **1.21.21**. The current code has been validated with Unity **2022.3.59f1**, including Mac development content/Player builds. Full native Player mouse-flow acceptance, other platforms and production release requirements remain separate checks.

Next: [UI table recovery](ui-tables.md) or [scene transitions](scenes.md).
