# Moving to Unity 6.6

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](upgrading.zh-CN.md)

**Keep your working copy and try the upgrade in a separate copy.** Your game can keep using its current framework and Editor. Choose when to move to the versions below, then compare the results.

<a id="current-baseline"></a>

## 01 Current baseline

| Item | Version / choice |
| --- | --- |
| Unity Editor | **6.6 (6000.6.5f1)**; not an LTS release |
| Addressables | **2.11.2** |
| uGUI / Timeline | **2.6.0 / 6.6.0**, matched to the Editor |
| TextMeshPro | Included in uGUI; independent package removed |
| Legacy VR module | `com.unity.modules.vr` removed from the manifest |
| Rendering / input | Built-in Render Pipeline / old Input Manager retained |

The Editor upgrade itself does not require URP or a new input system. Moving from v0.2.0 to the config-table workflow changes configuration and scene-ID APIs; follow [Usage index & migration](../Build-in%20Managers%20Usage.md). Moving from v0.3.x to v0.4.0 only changes the Editor entry and update dialogs; configuration data and runtime APIs stay the same.

<a id="migrate-a-separate-copy"></a>

## 02 Migrate a separate copy

1. Commit or back up the working project, including Assets, Packages, ProjectSettings and their meta files. Keep a branch or copy that still opens in its original Editor. Do not open that only working copy with the newer Editor.
2. Open the migration copy with **Unity Hub → Unity 6.6 (6000.6.5f1)**. Wait for package resolution, asset import and compilation. Keep the migrated manifest and lock file together. If you reused an older Library cache, a successful import does not yet show that a fresh import will work.
3. Check the package changes above and the first Console error before changing gameplay code. Keep this work in the migration copy; do not open, upgrade or replace the framework in another game's repository along the way.
4. Keep the config workflow that belongs to your chosen framework version. When moving from v0.2.0, first follow [config migration](../Build-in%20Managers%20Usage.md#01-from-v020-to-google-sheets). In v0.5.0, use **Framework → Config** for setup and **Update Config** on the left side of Unity's top toolbar for daily updates. Local Excel is the default for new games; existing Google sources stay Google until you explicitly switch. Keep local workbooks, source settings, generated snapshots, manifest, C#, JSON and meta files together.
5. In **File → Build Profiles**, select the target under **Platforms** and use **Switch Platform** if needed. Check the shared **Scene List** on the left. Rebuild Addressables with **Build → New Build → Default Build Script**, then return to the target platform and build a new Player. Old bundles or an old Player may still run, but that does not tell you whether the new Editor and packages work together.

Unity 6 uses **Create → Scripting → MonoBehaviour Script** for the first-window scripts. Build Profiles replaces the old Build Settings workflow; [Quick start](quick-start.md) and [Build](build.md) use the current labels.

<a id="check-before-using-the-migrated-copy"></a>

## 03 Check before using the migrated copy

- Run DemoStart: menu → Start → reach the goal with the keyboard → Play again → Menu → Settings. Watch for Console errors, check the fades and try the loop a few times.
- Run your own entry and UI. Confirm registration, generated IDs, events and borrowed resources still behave as expected.
- In a disposable copy, delete `Assets/GameContent/Demo` and `Demo.meta`. Keep GameContent and its `.gitkeep`; reimport, run your own entry or the services-only InitScene, and rebuild content/Player. No Demo type, asset or build menu should be required.
- Open the new Player and check the real UI, input and audio. Record which target was tested; an Editor check alone is not a device check.

The [verification scope](build.md#04-verification-scope) separates earlier baseline checks from the new main config workflow. Keep the older branch or copy until you have finished testing your own game.

<details>
<summary>Unity references</summary>

[Script creation](https://docs.unity3d.com/6000.6/Documentation/Manual/creating-scripts.html) · [Build Profiles](https://docs.unity3d.com/6000.6/Documentation/Manual/build-profiles.html) · [Scene lists](https://docs.unity3d.com/6000.6/Documentation/Manual/build-profile-scene-list.html) · [Addressables versions](https://docs.unity3d.com/6000.6/Documentation/Manual/com.unity.addressables.html) · [uGUI changes](https://docs.unity3d.com/Packages/com.unity.ugui@2.6/changelog/CHANGELOG.html)

</details>

Next: [Build and test the migrated game](build.md).
