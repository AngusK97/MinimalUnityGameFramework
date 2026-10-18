# Build & troubleshoot

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](build.zh-CN.md)

**Entry scene → Addressables → Player.** A Player is the app you can run outside the Editor.

## Build the Demo

1. Open **File → Build Profiles**. Under **Platforms**, select an installed target and **Switch Platform** if needed.
2. Exit Play, generate any changed UI/resource tables and wait for compilation.
3. Choose **Demo → Build Player**, then select an output location.
4. Open the app. Check the menu, then Start → keyboard win → Play again → Menu → Settings → Quit.

The helper builds Addressables and uses DemoStart without changing the saved scene list. It is deleted with Demo.

## Build your game

1. Open **File → Build Profiles**. Select your target under **Platforms** and **Switch Platform** if needed.
2. Select **Scene List** on the left. Add `MyGame/Scenes/Start.unity`, put it first and enable it. Remove InitScene if unneeded; keep other native scenes your game uses. **Open Scene List** opens this same shared list.
3. Generate changed UI/resource tables and wait for compilation.
4. Open **Window → Asset Management → Addressables → Groups**. Choose **Build → New Build → Default Build Script** and wait for success.
5. Return to your target under **Platforms**, choose **Build** and an output location.
6. Open the built app. Your own first screen should appear. Try its UI, input, sound and transitions.

![Unity 6.6 Build Profiles Scene List with MyGame Start enabled as the first scene](images/build-scene-list.png)

*Step 2 example — Build Profiles → Scene List: MyGame/Scenes/Start is enabled as the first scene.*

Rebuild Addressables after resource or platform changes. **Use Asset Database (fastest)** is for Editor Play only; Player still needs built content.

<details>
<summary>Need a separate scene list or settings?</summary>

Add a custom Build Profile. The shared list is enough for your first build.

</details>

## If something fails

| Symptom | First check |
| --- | --- |
| No Demo menu | Open DemoStart.unity. InitScene deliberately initializes services only. |
| GameUI/MainResources is missing | Temporarily remove/comment callers of the missing generated class, fix compilation, generate the table, then restore callers. |
| Generator is absent/disabled | Resolve Console compile errors, select UI.asset or a resource module table, exit Play and wait for compilation. |
| Invalid prefab | Reference a saved prefab with one UIBase-derived component on its root. |
| No loadable address | Keep runtime content under GameContent; check group Include in Build and Include Addresses in Catalog. |
| Window fails to open | Check Core readiness, RegisterAll, the actual address and the first Console error. |
| Duplicate EventSystem/AudioListener | Keep the shared Core's infrastructure; remove extra copies from scenes/prefabs. |
| Stale/edited output blocks build | Fix the table or restore generated output, generate and wait for compilation. |
| Player shows only a background | Put your own entry first; the default saved list contains only InitScene. |
| Scene load fails | Check its real Addressables key and rebuild content for the selected target. |

## Verification scope

macOS development builds passed; input, audio and release acceptance remain incomplete.

<details>
<summary>Baseline and exact results</summary>

Baseline: Unity **6.6 (6000.6.5f1)**, Addressables **2.11.2**, uGUI **2.6.0**. Current resource-table verification passed:

- **31 Editor checks:** generation, typed properties, invalid references/names, address ambiguity, moving/replacing assets, output protection and build guards.
- **7 Demo checks:** startup, events, transitions, settings pause, win, restart and return. The win check used a temporary probe, not keyboard movement.
- **3 independent-game checks:** from a fresh no-Library copy deleting only Demo, generated UI/resources and lifecycle checks passed. AudioClip, prefab and Single Sprite access, cancellation/reload and late-result isolation also passed in Play and the independent Player.
- Demo and independent-game **macOS Mono development** content/Player builds: **0 errors and 0 warnings**. Default compilation passed with/without Demo after removing all temporary validation assets.

Earlier GUI checks confirmed the Demo menu rendered, Escape opened/closed Settings, and the independent Player displayed its own window after Demo removal. The full native mouse flow, real keyboard completion, audible output, other platforms and IL2CPP/release builds have **not** been accepted. These results apply to the baseline above, not every target or release configuration.

</details>

[Editor upgrades](upgrading.md) · Unity references: [Build Profiles](https://docs.unity3d.com/6000.6/Documentation/Manual/build-profiles.html) and [Scene List](https://docs.unity3d.com/6000.6/Documentation/Manual/build-profile-scene-list.html).

Next: [UI table recovery](ui-tables.md), [resource table recovery](resource-tables.md) or [scene transitions](scenes.md).
