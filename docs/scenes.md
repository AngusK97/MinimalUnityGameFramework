# Scene transitions

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](scenes.zh-CN.md)

Load a level behind an overlay, close the old UI and wait for your scene preparation.

<a id="try-it"></a>

## 01 Try it

Start with the [first-window example](quick-start.md#02-create-your-first-window). This adds a level button to your existing **MyMenuView.prefab** and uses **MyGameEntry.cs** to load your config.

1. Create a scene with a camera and a visible object on the **Default** layer. On the camera, set **Depth = 2** and **Culling Mask = Default**, then remove its **AudioListener**; Core already has one. Core's background camera renders at Depth 1, so a default camera would be covered by its background. Save as `Assets/GameContent/MyGame/Content/Scenes/Level01.unity`, select the saved asset in Project, and check **Addressable** in its Inspector.

<details>
<summary>See the camera settings in the Demo</summary>

<img src="images/scene-camera.jpg" alt="Demo WorldCamera Inspector with Depth 2 and Culling Mask Default" width="360">

This is the Demo's WorldCamera. Match its Depth and Culling Mask for this example; adjust other camera settings for your own scene.

</details>

2. Open `Assets/GameContent/MyGame/Editor/Tables/Source/MyGame.xlsx`, or your connected workbook if Google Sheets is your active source. Add a **Scene** sheet and enter the rows below from A1. If Scene already exists, keep its first two rows and add only the **Level01** data row. Keep the existing UI sheet.

| Id | Name | ScenePath |
| --- | --- | --- |
| string | string | string |
| Level01 | First level | Assets/GameContent/MyGame/Content/Scenes/Level01.unity |

3. Save the local workbook, or wait for Google Sheets to finish saving. Select **MyGame** in **Framework → Config**, click **Update Config**, and wait for the result and compilation. This generates `SceneName.Level01 = "Level01"`. Your existing **MyGameEntry.cs** calls `GameConfig.LoadAndRegister()` after Core is ready, registering this ID with its Addressables address; keep that single call.
4. Open `Assets/GameContent/MyGame/Scripts/MyMenuView.cs`. Add `using Core;` at the top, keep `using UnityEngine;`, and add this method **inside the MyMenuView class**, alongside `OnShow()`:

```csharp
public void GoToLevel()
{
    GameCore.Scene.TryTransition(
        MyGame.SceneName.Level01,
        completed: (succeeded, error) =>
        {
            if (!succeeded)
            {
                Debug.LogWarning(error);
            }
        });
}
```

5. Save the script and wait for compilation. Double-click `Assets/GameContent/MyGame/Content/Prefabs/UI/MyMenuView.prefab` to open Prefab Mode. Select its root and choose **GameObject → UI → Legacy → Button**. Place the button where you can see it and set its text to **Go to level**. In the Button's **On Click ()**, click **+**, drag the **MyMenuView root from the prefab Hierarchy** into the object field, and select **MyMenuView → GoToLevel()**. Save the prefab and leave Prefab Mode.
6. Open `Assets/GameContent/MyGame/Scenes/Start.unity` and press Play. Click **Go to level** in your menu. Start from this entry scene so Core and config are ready before the button runs.

**Expected result:** one click shows the overlay, closes the old window and displays Level01's visible object. Single loading replaces the old scene; Core persists. Build Addressables for the Player target before testing outside the Editor. A load failure preserves old UI and shows Retry; check Console and the panel's error.

<a id="conventions"></a>

## 02 Conventions

- `TryTransition` and `TryTransitionAddress` return true when accepted. Busy or invalid requests return false without queuing and call `completed(false, error)` once immediately. The callback above therefore handles both rejection and later failure.
- Old UI closes only after a successful load. If an accepted transition fails, inspect `LastError`, then use the Retry panel or `RetryTransition()`. A rejected request does not create a retry operation.
- Keep transition controllers persistent or use static methods; Single unloads old-scene callback targets.
- Check `IsTransitioning` in gameplay input. The overlay blocks UGUI raycasts, not keyboard/controller code.

<details>
<summary>When needed: wait for HUD and resources</summary>

Pass `enterScene: scene => PrepareScene(scene)`. Your method returns `IEnumerator` and takes `UnityEngine.SceneManagement.Scene` (`using System.Collections;`).

Preparation runs after activation and old UI cleanup; the overlay stays until it finishes. Wait for resource/HUD callbacks here. Nested enumerators work; exceptions fail the transition.

Preparation failure after Single activation cannot restore the old scene. The error panel still allows retry; `RetryTransition()` retries loading and preparation, not game-progress rollback.

At timeScale=0, use realtime waits or callback polling. Framework fades use unscaled time; `WaitForSeconds` follows Unity's normal behavior. Clean up owned work in finally / Dispose.

</details>

<details>
<summary>When needed: completion, lifetime and low-level APIs</summary>

For an accepted transition, completion runs after the overlay hides and busy clears, so it can submit another transition. Rejection calls completion synchronously and leaves any current transition running. Notification exceptions are logged without changing the result. Direct callbacks on destroyed Unity objects are skipped.

The framework does not reset gameplay pause, unload all modules, stop all audio, cancel loading midway or roll back scenes. Disabling/destroying the manager settles once; late failures release handles and late successes leave the new active scene loaded.

`TryTransition(string)` resolves a registered scene ID. Unknown IDs return false; it does not guess an Addressables address. For an intentional raw key, use `TryTransitionAddress(key, ...)`. Older calls that passed a string address must move to that method.

`LoadSceneAsync` / `UnloadSceneAsync` bypass this flow for coordinated Additive loading, manual activation and handle ownership. Callers release failed loads and unload successful scenes. Do not mix both paths for one operation.

</details>

Next: [Own and release resources across scenes](resources.md).
