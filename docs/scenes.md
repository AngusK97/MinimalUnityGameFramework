# Scene transitions

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](scenes.zh-CN.md)

Move from a menu to a level with a loading overlay. One call loads the new scene, closes the old UI and waits for any preparation you provide.

## Try it

First save your **Level01.unity** in `Assets/GameContent/MyGame/Content/Scenes/`. Put a camera and an obvious visible object in it so you can recognize the loaded scene. Remove any extra AudioListener from that camera; Core already has one. Confirm its Addressables address; a Player needs content built for the current target.

Put this code in an existing menu button's click handler, after Core is ready. Do not put it in Update. It needs `using Core; using UnityEngine;`:

```csharp
bool accepted = GameCore.Scene.TryTransition(
    "Assets/GameContent/MyGame/Content/Scenes/Level01.unity",
    completed: (succeeded, error) =>
    {
        if (!succeeded)
        {
            Debug.LogWarning(error);
        }
    });

if (!accepted)
{
    Debug.LogWarning("Transition was not accepted. Check the current transition and setup.");
}
```

**Expected result:** click once to see the overlay, then the target scene after the old window closes. A load failure keeps the old UI and shows a Retry panel; check Console and the panel's error before retrying. This uses Single loading: the target replaces the old scene, while Core stays alive.

## Conventions

- True means accepted, not loaded; inspect the completion callback for success. Busy or invalid requests return false without queuing or a completion callback.
- Old UI closes only after a successful load; a load failure preserves it. Inspect `LastError` and use the built-in Retry panel or `RetryTransition()` after failure.
- Keep transition controllers on persistent objects or use static methods. Single unloads the old scene, so its objects should not own the remaining flow's callbacks.
- Gameplay input should check `IsTransitioning`. The overlay blocks UGUI raycasts, not your keyboard or controller code.

<details>
<summary>When needed: wait for HUD and resources</summary>

Pass `enterScene: scene => PrepareScene(scene)` to `TryTransition`. Your `PrepareScene` returns `IEnumerator` and takes `UnityEngine.SceneManagement.Scene`; writing the coroutine requires `using System.Collections;`.

The framework runs it after scene activation and old UI cleanup, then removes the overlay when it finishes. Wait for your resource and HUD completion callbacks here. Nested enumerators are supported; an exception fails the transition.

Preparation failure after Single activation does not restore the old scene; the error panel still allows retry. Ordinary `RetryTransition()` retries the scene and its preparation, not a rollback of game progress.

At timeScale=0, use realtime waits or callback polling. Framework fades use unscaled time; your `WaitForSeconds` retains Unity's normal behavior. Your finally / Dispose must clean up work you own.

</details>

<details>
<summary>When needed: completion, lifetime and low-level APIs</summary>

Completion runs after the overlay is hidden and the busy flag clears, so it can submit another transition. Completion notification exceptions are logged without changing the settled result. Direct callbacks on destroyed Unity objects are skipped.

The framework does not reset gameplay pause, unload all modules, stop all audio, cancel a load midway or roll back scenes. Disabling/destroying the manager settles its request once; late failures release their handles and late successes do not unload the new active scene.

`LoadSceneAsync` / `UnloadSceneAsync` are low-level wrappers for explicitly coordinated Additive loading, manual activation and handle ownership. They bypass the controlled flow. Low-level callers handle failed-load release and successful-scene unloading themselves; do not mix both paths for one operation.

</details>

Next: [Own and release resources across scenes](resources.md).
