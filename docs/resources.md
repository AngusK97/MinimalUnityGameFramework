# Resources

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](resources.zh-CN.md)

**Register → Load → Use → Unload.** Resource modules own shared assets; your game decides their lifetime. UIManager loads its own windows, so UI-only projects can skip this page.

## Try it

First create and generate [Main.asset](resource-tables.md), with `MyGame.MainResources`, ID `MyGame.Main` and the Audio Clip `Cue`. Wait for compilation.

Create **MyResourceStartup.cs** in `Assets/GameContent/MyGame/Scripts/`. Attach it to one empty object in your entry scene alongside the shared GameCore prefab:

```csharp
using Core;
using Sound;
using UnityEngine;

namespace MyGame
{
    public sealed class MyResourceStartup : MonoBehaviour
    {
        private MainResources main;

        private void Start()
        {
            if (!GameCore.IsReady)
            {
                Debug.LogError("Add the shared GameCore prefab to the entry scene.");
                return;
            }

            main = new MainResources();
            GameCore.Resource.RegisterResourceModule(main);
            main.LoadResources(loadedCallback: PlayCue, failedCallback: OnLoadFailed);
        }

        private void PlayCue()
        {
            GameCore.Sound.PlaySound(SoundLayer.UI, main.Cue);
        }

        private void OnLoadFailed()
        {
            Debug.LogError("Main resources failed to load or were cancelled.");
        }

        private void OnDestroy()
        {
            if (main == null || !GameCore.IsReady)
            {
                return;
            }

            GameCore.Sound.StopSound(SoundLayer.UI);
            main.UnloadResources();
        }
    }
}
```

**Expected result:** cue.wav plays after the entire module loads. Failure or cancellation logs an error. Removing this owner stops the UI audio layer and unloads its module. In your own game, wait until all borrowers finish and dependent instances are destroyed before unloading; choose an owner that lives as long as those users.

The Resource service retains the registered module after Start returns. Another component can retrieve the same instance once and keep it; lookup does not load it (`using Core;`):

```csharp
var main = GameCore.Resource.GetResourceModule<MyGame.MainResources>(MyGame.MainResources.Id);
```

After successful loading, gameplay uses `main.Cue` directly. Before loading or after unloading, the property returns null. Unload releases assets but leaves the module registered; reuse the same instance to load again. Register once per Core lifetime, not for each scene or new resource row.

## Conventions

- Keep game-owned Module Ids stable. Generated internal resource numbers are not save/network IDs.
- Keep source tables and generated code together; see [Resource tables](resource-tables.md). Handwritten BaseResourceModule subclasses remain supported.
- Borrow only in or after a successful load callback; handle failure with `failedCallback`.
- Only the owner unloads shared modules, after stopping related audio and destroying dependent instances. Do not unload a playing clip.
- Use the actual Addressables key. Rebuild content for the current target after changing resources or platform.

<details>
<summary>When needed: shared batches, cancellation and unloading</summary>

Concurrent `LoadResources` calls share one batch; assets become available only after the whole group succeeds. Already-ready modules may call success synchronously. `GetResource<T>` returns default when unavailable, missing or of the wrong type.

`UnloadResources` invalidates the batch immediately and cancels the whole group's wait. Late results release themselves without affecting replacement batches. Overrides must call `base.UnloadResources()`.

Duplicate registration throws without replacing or unloading the existing module. Looking up an unknown ID returns null; looking up a registered ID as the wrong module type throws InvalidOperationException. Your game chooses which modules survive scene changes.

</details>

<details>
<summary>When needed: direct loading and handle ownership</summary>

The caller owns the handle returned by `LoadAssetAsync<T>` and must `Release(handle)` after use, including failure. Copying a handle adds no ownership; do not release both asset and handle for one holding.

`InstantiateGameObjectAsync` transfers ownership to a valid completion callback; its receiver eventually calls `ReleaseInstance(handle)`. Without a callback, or if its directly bound Unity target was destroyed, the helper releases the result.

A throwing callback retains ownership. The helper logs without notifying twice or releasing a handle the receiver may have saved. Borrowing a module asset gives no separate holding to release.

</details>

Next: [Play a loaded clip through an audio layer](audio.md).
