# Resources

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](resources.zh-CN.md)

**Register → Load → Use → Unload.** Put shared assets in a module, then choose when to load and unload them. UIManager loads its own windows, so UI-only projects can skip this page.

<a id="try-it"></a>

## 01 Try it

First add the Cue row to your [Resource table](resource-tables.md), with **Type = AudioClip** and **Module = Main**, save the workbook (or wait for Google Sheets to finish saving), then click **Update Config** on the left side of Unity's top toolbar. This generates `MyGame.MainResources` with ID `MyGame.Main`. Wait for compilation.

To try this on its own, create **MyResourceStartup.cs** in `Assets/GameContent/MyGame/Scripts/` and attach it to an empty root object beside the shared GameCore prefab.

If you already have **MyGameEntry.cs**, use that script instead. Add `using Sound;`, copy the `main` field and the `PlayCue`, `OnLoadFailed`, `OnDestroy` methods into the class. Add the three lines from `main = new MainResources()` to `Start()`, after the existing `GameConfig.LoadAndRegister()`. Keep that config call only once; do not add a second startup component:

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

            GameConfig.LoadAndRegister();

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

**Expected result:** cue.wav plays after the entire module loads. Failure or cancellation logs an error. Destroying the object with this script stops the entire UI audio layer and unloads the module. In your own game, keep the owner alive until everything using its assets has finished. Stop using those assets and destroy instances that depend on them before unloading.

Resource keeps the registered module after Start finishes. Once registered, another component can look it up and keep the same instance. Looking it up does not load its assets (`using Core;`):

```csharp
var main = GameCore.Resource.GetResourceModule<MyGame.MainResources>(MyGame.MainResources.Id);
```

After successful loading, gameplay uses `main.Cue` directly. Before loading or after unloading, the property returns null. Unload releases assets but leaves the module registered; reuse the same instance to load again. Register once per Core lifetime, not for each scene or new resource row.

<a id="conventions"></a>

## 02 Conventions

- Choose stable IDs for your modules. Do not use the generated internal resource numbers in saves or network messages.
- Commit generated snapshots, manifest, code and JSON together; see [Resource tables](resource-tables.md). Handwritten BaseResourceModule subclasses remain supported.
- Use the assets only in or after a successful load callback; handle failure with `failedCallback`.
- Only the owner unloads shared modules, after stopping related audio and destroying dependent instances. Do not unload a playing clip.
- Use the actual Addressables key. Rebuild content for the current target after changing resources or platform.

<details>
<summary>When needed: shared batches, cancellation and unloading</summary>

Calling `LoadResources` several times while loading shares the same batch. You can use the assets only after all of them load successfully. If the module is already ready, the success callback may run immediately, before `LoadResources` returns. `GetResource<T>` returns default if the module is not ready, the ID is missing or the type does not match.

`UnloadResources` invalidates the batch immediately and cancels the whole group's wait. Late results release themselves without affecting replacement batches. Overrides must call `base.UnloadResources()`.

Duplicate registration throws without replacing or unloading the existing module. Looking up an unknown ID returns null; looking up a registered ID as the wrong module type throws InvalidOperationException. Your game chooses which modules survive scene changes.

</details>

<details>
<summary>When needed: direct loading and handle ownership</summary>

When you call `LoadAssetAsync<T>`, you are responsible for the handle it returns. Call `Release(handle)` when you are done, even if loading failed. Copying the handle does not add another reference to release. For one load, release either the asset or the handle, not both.

`InstantiateGameObjectAsync` transfers ownership to a valid completion callback; its receiver eventually calls `ReleaseInstance(handle)`. Without a callback, or if its directly bound Unity target was destroyed, the helper releases the result.

If your callback throws, you are still responsible for releasing the operation. The helper logs the exception; it does not call back again or release a handle you may have saved. A module asset is borrowed without adding a separate reference, so do not release it yourself.

</details>

Next: [Play a loaded clip through an audio layer](audio.md).
