# Resources

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](resources.zh-CN.md)

Load shared assets through a resource module; its owner decides when to unload them. UIManager loads its own windows, so UI-only projects can skip this page.

## Try it

Put **click.wav** in `Assets/GameContent/MyGame/Content/Audio/`. The existing GameContent folder entry provides its full path address.

Create **GameAssets.cs** in `Assets/GameContent/MyGame/Scripts/`:

```csharp
using Resource;
using UnityEngine;

namespace MyGame
{
    public sealed class GameAssets : BaseResourceModule
    {
        public const string Id = "MyGame.Assets";

        public override string GetName()
        {
            return Id;
        }

        protected override void RecordAllResourceInfos()
        {
            AddResourceInfo(ResourceType.AudioClip, 1,
                "Assets/GameContent/MyGame/Content/Audio/click.wav");
        }

        public AudioClip Click => GetResource<AudioClip>(1);
    }
}
```

Register once in your startup method after checking `GameCore.IsReady` (`using Core; using Sound; using UnityEngine;`):

```csharp
var assets = new MyGame.GameAssets();
GameCore.Resource.RegisterResourceModule(assets);

assets.LoadResources(
    loadedCallback: () =>
    {
        GameCore.Sound.PlaySound(SoundLayer.UI, assets.Click);
    },
    failedCallback: () =>
    {
        Debug.LogError("Game resources failed to load or were cancelled.");
    });
```

**Expected result:** after loading succeeds, click.wav plays. Failure or cancellation logs an error.

The Resource service retains the module after startup returns. Other users retrieve it with `GameCore.Resource.GetResourceModule<MyGame.GameAssets>(MyGame.GameAssets.Id)` and borrow `Click`. Expose typed properties; `GetResource<T>` is protected.

After all borrowers finish and dependent instances are destroyed, the resource owner cleans up while Core is ready. `StopSound` stops the whole UI layer:

```csharp
var assets = GameCore.Resource.GetResourceModule<MyGame.GameAssets>(MyGame.GameAssets.Id);
GameCore.Sound.StopSound(SoundLayer.UI);
assets?.UnloadResources();
```

## Conventions

- Module IDs are game-owned, non-empty strings; resource IDs within a module are ints (a local enum is fine).
- Borrow only in or after a successful load callback; handle failure with `failedCallback`.
- Only the owner unloads shared modules, after stopping related audio and destroying dependent instances. Do not unload a playing clip.
- Use the actual Addressables key. Rebuild content for the current target after changing resources or platform.

<details>
<summary>When needed: shared batches, cancellation and unloading</summary>

Concurrent `LoadResources` calls share one batch; assets become available only after the whole group succeeds. Already-ready modules may call success synchronously. `GetResource<T>` returns default when unavailable, missing or of the wrong type.

`UnloadResources` invalidates the batch immediately and cancels the whole group's wait. Late results release themselves without affecting replacement batches. Overrides must call `base.UnloadResources()`.

Duplicate module IDs are rejected without replacing or unloading existing modules. Your game chooses which modules survive scene changes.

</details>

<details>
<summary>When needed: direct loading and handle ownership</summary>

The caller owns the handle returned by `LoadAssetAsync<T>` and must `Release(handle)` after use, including failure. Copying a handle adds no ownership; do not release both asset and handle for one holding.

`InstantiateGameObjectAsync` transfers ownership to a valid completion callback; its receiver eventually calls `ReleaseInstance(handle)`. Without a callback, or if its directly bound Unity target was destroyed, the helper releases the result.

A throwing callback retains ownership. The helper logs without notifying twice or releasing a handle the receiver may have saved. Borrowing a module asset gives no separate holding to release.

</details>

Next: [Play a loaded clip through an audio layer](audio.md).
