# Build & troubleshoot

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](build.zh-CN.md)

**Entry scene → Addressables → Player.** A Player is the app you can run outside the Editor.

<a id="build-the-demo"></a>

## 01 Build the Demo

1. Open **File → Build Profiles**. Under **Platforms**, select an installed target and **Switch Platform** if needed.
2. Exit Play and wait for compilation. The bundled Demo config is ready; no cloud setup is needed. If you changed its source workbook, select Demo in **Framework → Config**, then click **Update Config** on the left side of Unity's top toolbar first.
3. Choose **Demo → Build Player**, then select an output location.
4. Open the app. Check the menu, then Start → keyboard win → Play again → Menu → Settings → Quit.

The helper builds Addressables and uses DemoStart without changing the saved scene list. It is deleted with Demo.

<a id="build-your-game"></a>

## 02 Build your game

1. Open **File → Build Profiles**. Select your target under **Platforms** and **Switch Platform** if needed.
2. Select **Scene List** on the left. Add `MyGame/Scenes/Start.unity`, put it first and enable it. Remove InitScene if unneeded; keep other native scenes your game uses. **Open Scene List** opens this same shared list.
3. If you changed the active source tables or local asset paths, select your game in **Framework → Config**, then click **Update Config** on the left side of Unity's top toolbar and wait for compilation. Review any retained-table warnings before building.
4. Open **Window → Asset Management → Addressables → Groups**. Choose **Build → New Build → Default Build Script** and wait for success.
5. Return to your target under **Platforms**, choose **Build** and an output location.
6. Open the built app. Your own first screen should appear. Try its UI, input, sound and transitions.

![Unity 6.6 Build Profiles Scene List with MyGame Start enabled as the first scene](images/build-scene-list.png)

*Step 2 example — Build Profiles → Scene List: MyGame/Scenes/Start is enabled as the first scene.*

The build checks committed local snapshots, generated file ownership and current assets. It does not reimport Excel or contact Google, and needs no key. It cannot detect source edits you have not imported. Missing UI placeholders, invalid assets or altered generated files block the Player build.

Rebuild Addressables after resource or platform changes. **Use Asset Database (fastest)** is for Editor Play only; Player still needs built content.

<details>
<summary>Need a separate scene list or settings?</summary>

Add a custom Build Profile. The shared list is enough for your first build.

</details>

<a id="if-something-fails"></a>

## 03 If something fails

| Symptom | First check |
| --- | --- |
| No Demo menu | Open DemoStart.unity. InitScene deliberately initializes services only. |
| GameConfig/UIName/MainResources is missing | Restore generated files and manifest from the same revision, or temporarily comment callers, fix compilation and run Update Config; then restore callers. |
| Update Config is disabled | Exit Play and wait for any update, import or compilation to finish. Resolve Console compile errors before continuing. |
| The toolbar opens Config instead of updating | Select a game and its source folder. A Google source also needs an existing local key file. |
| Invalid prefab | Reference a saved prefab with one UIBase-derived component on its root. |
| No loadable address | Select the saved asset in Project, check Addressable in its Inspector, and check group Include in Build and Include Addresses in Catalog. |
| Window fails to open | Check Core readiness, GameConfig.LoadAndRegister(), the UI row, its prefab and the first Console error. |
| Duplicate EventSystem/AudioListener | Keep the shared Core's infrastructure; remove extra copies from scenes/prefabs. |
| Edited/missing output blocks build | Restore generated outputs and manifest from the same revision, or fix the source and update config. |
| Missing UI placeholder blocks build | Create the prefab or remove its UI row, then update config. Import allows this warning; Player builds require usable assets. |
| Player shows only a background | Put your own entry first; the default saved list contains only InitScene. |
| Scene loads but the world stays hidden | Set the world camera to Depth 2 and include the objects’ layer in its Culling Mask; see [Scenes](scenes.md). Core’s background camera renders at Depth 1. |
| Scene load fails | Check startup config loading, the Scene row and Addressables content for the selected target. Raw string keys use TryTransitionAddress. |

<a id="verification-scope"></a>

## 04 Verification scope

Checked on **2026-10-25**, using Unity **6000.6.5f1** on macOS:

- **29 config integration checks passed**, including local Excel import, partial retention, corruption, source switching and a real read-only Google roundtrip. Numeric cells also matched Google's reader under Unity's Mono.
- **81 Demo Play assertions passed** after importing the local workbook: generated tables, UI, resources and two Menu ↔ Example cycles.
- After deleting Demo, an independent game generated UI, Resource, Scene and Item from its own local workbook. Its **macOS Mono development Player** built and passed config queries, events, resource loading, UI and scene transitions in batch mode.
- Final default compilation passed with Demo and after deleting it, with all validation helpers removed.
- Real Unity dialogs passed successful import, partial retention and recovery; retained Resource files kept their contents and modification times. The Demo workbook was restored after testing.

These are targeted checks, not a claim of every platform or shipping configuration. **Other platforms, IL2CPP/release builds, audible output and the full native mouse/keyboard loop remain unverified.** Validation helpers stay outside the framework.

<details>
<summary>Earlier verification</summary>

Checked on **2026-10-24** with Unity **6000.6.5f1**, Addressables **2.11.2** and uGUI **2.6.0**:

- Config import checks passed in Unity: unchanged files, independent updates, reference rollback, deletion and output protection, UI placeholders and retained assets.
- **81 Demo Play assertions passed:** local tables, generated IDs, resource access, Settings lifecycle and two Menu ↔ Example cycles. These checks invoked game APIs.
- Demo and a fresh independent game built as **macOS Mono development Players**. The independent Player passed config, events, resource load/release, UI and scene checks after deleting the entire Demo folder.
- The Demo Player rendered its menu and opened Settings with Escape. **18 simulated Google transport groups** passed.
- Real Google authentication and import passed in Unity with the three Demo tables: first connection, unchanged output, invalid-table retention and recovery. The cloud test values were restored.

**Still unverified:** the full native mouse/keyboard game loop, audible output, other platforms and IL2CPP/release builds. Other platforms need their own build and device checks; this does not indicate a known incompatibility. No cloud credentials are bundled. Follow [first-time setup](config-setup.md#01-first-time-setup) to connect your own source.

Validation helpers ran only in temporary copies and are not part of the framework. These results cover the code included in v0.3.1; older releases keep their own guides.

**v0.4.0 Editor verification (2026-10-24):** 31 targeted checks and result recovery after a domain reload passed. Default compilation passed with Demo and in an independent MyGame copy after removing Demo. Real toolbar updates passed first connection (**3 updated**), partial update (**1 updated, 1 kept previous**) and **Cancel**. Retained Resource files, and all generated files after cancellation, kept the same contents and modification times. Cloud test values were restored.

**v0.4.1 Editor verification (2026-10-25):** the button moves from the saved center layout to the left toolbar, with a visible background and centered Play controls. Opening Config, script reload and Play-mode disabling passed in Unity 6000.6.5f1 on macOS. Default compilation passed with and without Demo. The update pipeline and runtime code are unchanged.


</details>

[Editor upgrades](upgrading.md) · Unity references: [Build Profiles](https://docs.unity3d.com/6000.6/Documentation/Manual/build-profiles.html) and [Scene List](https://docs.unity3d.com/6000.6/Documentation/Manual/build-profile-scene-list.html).

Next: [Config setup and recovery](config.md) or [scene transitions](scenes.md).
