# Resources

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](resources.zh-CN.md)

Load a shared sound once, then let your game use it. A resource module keeps the loaded assets until its owner unloads them.

## Try it

Skip this page if you only need UI; UIManager loads its own windows. For a shared sound, first put your **click.wav** in `Assets/GameContent/MyGame/Content/Audio/`. The existing GameContent folder entry supplies its full path address.

Create **GameAssets.cs** in `Assets/GameContent/MyGame/Scripts/` and replace the whole file with:

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

Register once in your game startup method after checking `GameCore.IsReady`. This snippet needs `using Core; using Sound; using UnityEngine;`:

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

The Resource service keeps the registered module, even after this startup method returns. Other users retrieve it with `GameCore.Resource.GetResourceModule<MyGame.GameAssets>(MyGame.GameAssets.Id)` and borrow through `Click`. `GetResource<T>` is protected; expose typed properties rather than calling it from outside the module.

Later, the game's resource owner can use the following in a cleanup method while Core is still ready. Do this only after all borrowers have finished and dependent instances have been destroyed. This example owns one UI sound group; `StopSound` stops the whole UI layer. Individual borrowers should not unload the shared module:

```csharp
var assets = GameCore.Resource.GetResourceModule<MyGame.GameAssets>(MyGame.GameAssets.Id);
GameCore.Sound.StopSound(SoundLayer.UI);
assets?.UnloadResources();
```

## Conventions

- A module ID is a game-owned, non-empty string. Resource IDs inside a module are ints; a game-local enum is fine.
- Borrow only after successful loading. Handle failure through `failedCallback`; do not read `Click` immediately after requesting a load.
- After borrowers finish, stop related audio and destroy dependent instances, then let the owner call `assets.UnloadResources()`. Do not unload immediately after starting playback.
- Use the actual Addressables key. Rebuild content for the current target after changing resources or platform.

<details>
<summary>When needed: shared batches, cancellation and unloading</summary>

Concurrent `LoadResources` calls share one batch. Assets become available only when the whole group succeeds. If already ready, the success callback may run synchronously. `GetResource<T>` returns the default value when unavailable, missing or of the wrong type.

`UnloadResources` immediately invalidates the batch and cancels the whole group's wait, not one borrower's subscription. Late results release themselves without affecting a replacement batch. Overrides must call `base.UnloadResources()`.

Duplicate module IDs are rejected without replacing or unloading the registered module. Your game decides which shared modules survive scene changes; do not unload everything on every transition.

</details>

<details>
<summary>When needed: direct loading and handle ownership</summary>

`LoadAssetAsync<T>` transfers its returned handle to the caller. Call `Release(handle)` after use, including failure. Copying a handle adds no ownership; do not release both the asset and its handle for the same holding.

`InstantiateGameObjectAsync` transfers the operation to a valid completion callback; the receiver eventually calls `ReleaseInstance(handle)`. With no callback, or a directly bound Unity target that was destroyed, the helper releases the result.

A throwing callback still owns the operation. The helper logs the exception without notifying twice or releasing a handle the receiver may have retained. Borrowing a module's asset does not give you a separate holding to release.

</details>

Next: [Play a loaded clip through an audio layer](audio.md).
