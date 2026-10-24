# Scene transitions

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](scenes.zh-CN.md)

Load a level behind an overlay, close the old UI and wait for your scene preparation.

## Try it

1. Save **Level01.unity** in `Assets/GameContent/MyGame/Content/Scenes/` with a camera and visible object. Remove the camera's extra AudioListener; Core has one. Select the saved **Level01.unity asset in Project** and check **Addressable** in its Inspector.
2. Here is the **Scene** table. For your first connection, enter it at step 6 of [Config setup](config-setup.md). If already connected, open the existing **Scene** tab or create one if missing. Enter all rows from A1 in a new tab; in an existing tab, keep its first two rows and add only the **Level01** data row.

| Id | Name | ScenePath |
| --- | --- | --- |
| string | string | string |
| Level01 | First level | Assets/GameContent/MyGame/Content/Scenes/Level01.unity |

3. For your first connection, follow [Config setup](config-setup.md) using this Scene table. If already connected, click **Framework → Config → Update Config** and wait for compilation. In startup, after Core is ready, call `MyGame.GameConfig.LoadAndRegister()` once. This registers `SceneName.Level01 = "Level01"` to its Addressables address.

Add this to a menu button handler after startup, not Update (`using Core; using UnityEngine;`):

```csharp
GameCore.Scene.TryTransition(
    MyGame.SceneName.Level01,
    completed: (succeeded, error) =>
    {
        if (!succeeded)
        {
            Debug.LogWarning(error);
        }
    });
```

**Expected result:** one click shows the overlay, closes the old window and displays the target scene. Single loading replaces the old scene; Core persists. Build Addressables for the Player target before testing outside the Editor. A load failure preserves old UI and shows Retry; check Console and the panel's error.

## Conventions

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
