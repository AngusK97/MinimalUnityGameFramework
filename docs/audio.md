# Audio

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](audio.zh-CN.md)

Play a button sound, pause the background music or stop a layer. The framework reuses the audio sources for you.

## Try it

First complete the [resource example](resources.md), so your startup registers and loads `GameAssets` after Core is ready. Add this method to the component handling an existing button, then connect the button's On Click event to `PlayClick()`. Add `using Core; using Sound; using UnityEngine;` at the top of that script:

```csharp
public void PlayClick()
{
    var assets = GameCore.Resource.GetResourceModule<MyGame.GameAssets>(MyGame.GameAssets.Id);

    if (assets == null || assets.Click == null)
    {
        Debug.LogWarning("Load GameAssets before playing its sounds.");
        return;
    }

    GameCore.Sound.PlaySound(SoundLayer.UI, assets.Click);
}
```

In the Button Inspector, expand **On Click ()**, click **+**, drag the scene object with this component into the object slot, then select **your component → PlayClick()**.

**Try it:** after loading finishes, click the button. You should hear your click.wav; if the module or clip is not ready, Console tells you to load GameAssets first.

`PlaySound` defaults to volume 1, pitch 1 and no loop. Use it for short effects; request looping music explicitly with `Play`.

## Conventions

The shared Core configures four layers: `UI`, `Background`, `Environment` and `Event`. Pick a `SoundLayer` by purpose instead of creating each AudioSource yourself.

| Action | Call |
| --- | --- |
| Pause the background layer | `GameCore.Sound.PauseSound(SoundLayer.Background)` |
| Resume it | `GameCore.Sound.UnPauseSound(SoundLayer.Background)` |
| Stop and clear its clips | `GameCore.Sound.StopSound(SoundLayer.Background)` |

- Pause affects one layer; pausing background music does not pause UI effects.
- Paused or full layers drop new requests without queuing or stealing voices.
- Stop clears playback and clip references but preserves pause state. Call UnPauseSound to accept playback again.
- Audio does not load assets. Stop related playback before unloading the owning resource module; do not release a playing clip.

<details>
<summary>When needed: source limits and reuse</summary>

For looping music, first load its AudioClip, then call `GameCore.Sound.Play` with `SoundLayer.Background`, that clip, volume `0.5f`, pitch `1f` and `isLoop: true`. Unlike `PlaySound`, this lets you choose the playback parameters explicitly.

Each SourceAgent defaults to `Max Sources` 16. This limits dynamic expansion, not existing sources. Reused sources reset clip, loop, pitch and volume rather than inheriting the previous play's parameters.

Expanded sources copy the first valid source's mixer, spatial and listener configuration. During `AudioListener.pause`, only `ignoreListenerPause` sources are usable. Missing layer configuration logs a warning and skips playback.

Scene changes do not automatically stop every layer, and the framework does not save user volume. Your game owns global pause, volume settings and cross-scene audio policy.

</details>

<details>
<summary>When needed: direct AudioSource access</summary>

Low-level `SourceAgent.GetAudioSource()` may return null. Use a borrowed source immediately; it is not a reserved, long-lived playback handle.

Native Pause, PlayDelayed and PlayScheduled bypass the layer's pause guarantees. `SourceAgent.Play(clip)` returns whether the request was accepted, not whether it was audible on the device. Prefer the layer APIs for ordinary gameplay.

</details>

Next: [Load and release sound assets through resource modules](resources.md).
