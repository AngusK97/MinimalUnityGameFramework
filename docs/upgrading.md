# Moving to Unity 6.6

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](upgrading.zh-CN.md)

**Keep a working copy, migrate another, then compare.** An existing game can stay on its pinned framework and Editor; adopting this baseline is a separate decision.

## Current baseline

| Item | Version / choice |
| --- | --- |
| Unity Editor | **6.6 (6000.6.5f1)**; not an LTS release |
| Addressables | **2.11.2** |
| uGUI / Timeline | **2.6.0 / 6.6.0**, matched to the Editor |
| TextMeshPro | Included in uGUI; independent package removed |
| Legacy VR module | `com.unity.modules.vr` removed from the manifest |
| Rendering / input | Built-in Render Pipeline / old Input Manager retained |

This upgrade does not require URP, a new input system or changes to the framework's public calls. Older framework API changes are covered separately in [Usage index & migration](../Build-in%20Managers%20Usage.md).

## Migrate a separate copy

1. Commit or back up the working project, including Assets, Packages, ProjectSettings and their meta files. Keep a branch or copy that still opens in its original Editor. Do not open that only working copy with the newer Editor.
2. Open the migration copy with **Unity Hub → Unity 6.6 (6000.6.5f1)**. Wait for package resolution, asset import and compilation. Keep the migrated manifest and lock file together; do not reuse an older Library cache as proof of a clean import.
3. Check the package changes above and the first Console error before changing gameplay code. This migration does not open, upgrade or replace the framework inside any other game's repository.
4. Select your game's `Editor/Tables/UI.asset`, click **Validate and Generate**, then wait for compilation. Keep the table, generated C# and meta files together; do not rewrite the generated class by hand.
5. In **File → Build Profiles**, select the target under **Platforms** and use **Switch Platform** if needed. Check the shared **Scene List** on the left. Rebuild Addressables with **Build → New Build → Default Build Script**, then return to the target platform and build a new Player. Do not assume old bundles or an old Player certify the new Editor/package combination.

Unity 6 uses **Create → Scripting → MonoBehaviour Script** for the first-window scripts. Build Profiles replaces the old Build Settings workflow; [Quick start](quick-start.md) and [Build](build.md) use the current labels.

## Check before using the migrated copy

- Run DemoStart: menu → Start → keyboard win → Play again → Menu → Settings. Check Console errors, fades and repeated runs.
- Run your own entry and UI. Confirm registration, generated IDs, events and borrowed resources still behave as expected.
- In a disposable copy, delete `Assets/GameContent/Demo` and `Demo.meta`. Keep GameContent and its `.gitkeep`; reimport, run your own entry or the services-only InitScene, and rebuild content/Player. No Demo type, asset or build menu should be required.
- Open the new Player and check the real UI, input and audio. Record which target was tested; an Editor check alone is not a device check.

The [verification scope](build.md#verification-scope) records what has actually passed for this baseline. Keep the older branch/copy until your game's own acceptance is complete.

<details>
<summary>Unity references</summary>

[Script creation](https://docs.unity3d.com/6000.6/Documentation/Manual/creating-scripts.html) · [Build Profiles](https://docs.unity3d.com/6000.6/Documentation/Manual/build-profiles.html) · [Scene lists](https://docs.unity3d.com/6000.6/Documentation/Manual/build-profile-scene-list.html) · [Addressables versions](https://docs.unity3d.com/6000.6/Documentation/Manual/com.unity.addressables.html) · [uGUI changes](https://docs.unity3d.com/Packages/com.unity.ugui@2.6/changelog/CHANGELOG.html)

</details>

Next: [Build and test the migrated game](build.md).
