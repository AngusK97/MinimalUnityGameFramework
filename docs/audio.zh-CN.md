# 音频

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](audio.md)

用音频层播放音效、控制音乐，框架会帮你复用音源。

<a id="试试看"></a>

## 01 试试看

先完成[资源示例](resources.zh-CN.md)，在 Core 就绪后登记并加载 `MainResources`。然后把下面的方法加到处理按钮点击的组件中（需要 `using Core; using Sound; using UnityEngine;`）：

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

在 Button 的 Inspector 中展开 **On Click ()**，点击 **+**，拖入挂有该组件的场景对象，再选择 **你的组件 → PlayClick()**。

**预期结果：** 资源加载完成后，点击按钮就能听到 cue.wav。如果模块或音效还不可用，Console 会提示先加载 MainResources。

`PlaySound` 默认以音量 1、音高 1 播放一次。需要循环播放音乐时，使用 `Play`。

<a id="使用约定"></a>

## 02 使用约定

Core 提供 `UI`、`Background`、`Environment`、`Event` 四个音频层，按声音的用途选择 `SoundLayer`。

| 操作 | 调用 |
| --- | --- |
| 暂停背景层 | `GameCore.Sound.PauseSound(SoundLayer.Background)` |
| 恢复背景层 | `GameCore.Sound.UnPauseSound(SoundLayer.Background)` |
| 停止并清空背景层音频 | `GameCore.Sound.StopSound(SoundLayer.Background)` |

- 暂停只影响指定层。
- 如果这一层已暂停，或音源池已满，新的播放请求会被丢弃，不会排队，也不会打断正在播放的声音来腾出音源。
- Stop 会停止播放并清除 clip 引用，但不会解除暂停。暂停的层要调用 UnPauseSound 后，才能再次接受播放请求。
- 音频服务不负责加载资产。卸载资源模块前，先停止使用其中音效的播放。

<details>
<summary>需要时再看：音源数量与复用规则</summary>

要循环播放音乐，先加载 AudioClip，再调用 `GameCore.Sound.Play`，传入 `SoundLayer.Background`、该 clip、音量 `0.5f`、音高 `1f` 和 `isLoop: true`。

每个 SourceAgent 的 `Max Sources` 默认是 16，只限制继续创建音源，不影响已有音源。复用音源时，会重新设置 clip、loop、pitch、volume。

新建音源会复制第一个有效音源的混音、空间和监听器设置。`AudioListener.pause` 生效时，只有启用了 `ignoreListenerPause` 的音源可用。如果缺少对应层的配置，框架会记录警告并跳过播放。

全局暂停、音量保存，以及切换场景时哪些声音继续播放，都由游戏自己处理。切换场景不会自动停止所有音频层。

</details>

<details>
<summary>需要时再看：直接操作 AudioSource</summary>

`SourceAgent.GetAudioSource()` 可能返回 null。获取到音源后应立即使用，这个方法不会为你预留音源供以后播放。

直接调用原生的 Pause、PlayDelayed、PlayScheduled 后，音频层无法保证暂停行为符合预期。`SourceAgent.Play(clip)` 的返回值只表示是否接受了播放请求，不保证设备实际发出了声音。一般的游戏逻辑使用音频层接口即可。

</details>

下一步：[用资源模块管理音效的加载与释放](resources.zh-CN.md)。
