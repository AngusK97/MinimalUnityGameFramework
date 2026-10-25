# Audio

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](audio.zh-CN.md)

Play a sound effect or control your background music through audio layers. The framework takes care of reusing audio sources.

<a id="try-it"></a>

## 01 Try it

Complete the [resource example](resources.md) to register and load `MainResources` after Core is ready. Add this method to your button-handling component (`using Core; using Sound; using UnityEngine;`):

```csharp
public void PlayClick()
{
    var main = GameCore.Resource.GetResourceModule<MyGame.MainResources>(MyGame.MainResources.Id);

    if (main == null || main.Cue == null)
    {
        Debug.LogWarning("Load MainResources before playing its sounds.");
        return;
    }

    GameCore.Sound.PlaySound(SoundLayer.UI, main.Cue);
}
```

In the Button Inspector, expand **On Click ()**, click **+**, drag in the scene object with this component, then select **your component → PlayClick()**.

**Expected result:** once the resources have loaded, clicking the button plays cue.wav. If the module or clip is unavailable, Console reminds you to load MainResources first.

`PlaySound` defaults to volume 1, pitch 1, no loop. For looping music, use `Play`.

<a id="conventions"></a>

## 02 Conventions

Core provides `UI`, `Background`, `Environment` and `Event` layers. Choose the `SoundLayer` that matches what the sound is for.

| Action | Call |
| --- | --- |
| Pause the background layer | `GameCore.Sound.PauseSound(SoundLayer.Background)` |
| Resume it | `GameCore.Sound.UnPauseSound(SoundLayer.Background)` |
| Stop and clear its clips | `GameCore.Sound.StopSound(SoundLayer.Background)` |

- Pause affects only the chosen layer.
- If a layer is paused, or has no available source and cannot create another, it drops new requests. It does not queue them or interrupt another sound to make room.
- Stop ends playback and clears clip references, but leaves the pause state unchanged. If the layer is paused, call UnPauseSound before playing again.
- Audio does not load assets. Stop related playback before unloading its resource module.

<details>
<summary>When needed: source limits and reuse</summary>

For looping music, load the AudioClip, then call `GameCore.Sound.Play` with `SoundLayer.Background`, the clip, volume `0.5f`, pitch `1f` and `isLoop: true`.

Each SourceAgent's `Max Sources` defaults to 16. Once it reaches this number, it stops creating new sources; it does not remove existing ones. Reusing a source resets its clip, loop, pitch and volume.

New sources copy the first valid source's mixer, spatial and listener settings. During `AudioListener.pause`, only `ignoreListenerPause` sources are usable. Missing layers log a warning and skip playback.

Your game decides how global pause works, how to save volume settings and which sounds continue across scenes. Scene changes do not automatically stop all layers.

</details>

<details>
<summary>When needed: direct AudioSource access</summary>

`SourceAgent.GetAudioSource()` may return null. Use borrowed sources immediately; they are not reserved playback handles.

If you call the native Pause, PlayDelayed or PlayScheduled methods directly, the layer can no longer guarantee its pause behavior. `SourceAgent.Play(clip)` tells you whether the request was accepted, not whether the device produced sound. For ordinary gameplay, use the layer APIs.

</details>

Next: [Load and release sound assets through resource modules](resources.md).
