# Scene transitions

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](scenes.zh-CN.md)

Load a level behind an overlay, close the old UI and wait for your scene preparation.

## Try it

Save **Level01.unity** in `Assets/GameContent/MyGame/Content/Scenes/` with a camera and visible object. Remove the camera's extra AudioListener; Core has one. Confirm the Addressables address and build content for the Player's target.

Add this to a menu button handler after Core is ready, not Update (`using Core; using UnityEngine;`):

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

**Expected result:** one click shows the overlay, closes the old window and displays the target scene. Single loading replaces the old scene; Core persists. A load failure preserves old UI and shows Retry; check Console and the panel's error.

## Conventions

- True means accepted; success comes through the completion callback. Busy/invalid requests return false without queuing or calling completion.
- Old UI closes only after a successful load. After failure, inspect `LastError`, then use the Retry panel or `RetryTransition()`.
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

Completion runs after the overlay hides and busy clears, so it can submit another transition. Notification exceptions are logged without changing the result. Direct callbacks on destroyed Unity objects are skipped.

The framework does not reset gameplay pause, unload all modules, stop all audio, cancel loading midway or roll back scenes. Disabling/destroying the manager settles once; late failures release handles and late successes leave the new active scene loaded.

`LoadSceneAsync` / `UnloadSceneAsync` bypass this flow for coordinated Additive loading, manual activation and handle ownership. Callers release failed loads and unload successful scenes. Do not mix both paths for one operation.

</details>

Next: [Own and release resources across scenes](resources.md).
