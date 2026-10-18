# Audio

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](audio.zh-CN.md)

Play effects or control a music layer. The framework reuses audio sources.

## Try it

Complete the [resource example](resources.md) to register and load `GameAssets` after Core is ready. Add this method to your button-handling component (`using Core; using Sound; using UnityEngine;`):

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

In the Button Inspector, expand **On Click ()**, click **+**, drag in the scene object with this component, then select **your component → PlayClick()**.

**Expected result:** after loading, clicking plays click.wav. If the module or clip is unavailable, Console tells you to load GameAssets.

`PlaySound` defaults to volume 1, pitch 1, no loop. For looping music, use `Play`.

## Conventions

Core provides `UI`, `Background`, `Environment` and `Event` layers. Choose `SoundLayer` by purpose.

| Action | Call |
| --- | --- |
| Pause the background layer | `GameCore.Sound.PauseSound(SoundLayer.Background)` |
| Resume it | `GameCore.Sound.UnPauseSound(SoundLayer.Background)` |
| Stop and clear its clips | `GameCore.Sound.StopSound(SoundLayer.Background)` |

- Pause affects only the chosen layer.
- Paused/full layers drop new requests without queuing or stealing voices.
- Stop clears playback and clip references but keeps pause state; call UnPauseSound to accept playback again.
- Audio does not load assets. Stop related playback before unloading its resource module.

<details>
<summary>When needed: source limits and reuse</summary>

For looping music, load the AudioClip, then call `GameCore.Sound.Play` with `SoundLayer.Background`, the clip, volume `0.5f`, pitch `1f` and `isLoop: true`.

Each SourceAgent's `Max Sources` defaults to 16, limiting expansion, not existing sources. Reuse resets clip, loop, pitch and volume.

Expanded sources copy the first valid source's mixer, spatial and listener settings. During `AudioListener.pause`, only `ignoreListenerPause` sources are usable. Missing layers log a warning and skip playback.

Your game owns global pause, saved volume and cross-scene audio policy. Scene changes do not automatically stop all layers.

</details>

<details>
<summary>When needed: direct AudioSource access</summary>

`SourceAgent.GetAudioSource()` may return null. Use borrowed sources immediately; they are not reserved playback handles.

Native Pause, PlayDelayed and PlayScheduled bypass layer pause guarantees. `SourceAgent.Play(clip)` reports acceptance, not device audibility. Use layer APIs for ordinary gameplay.

</details>

Next: [Load and release sound assets through resource modules](resources.md).
