# 音频

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](audio.md)

播放音效或控制音乐层，框架负责复用音源。

## 试试看

完成[资源示例](resources.zh-CN.md)，在 Core 就绪后登记并加载 `GameAssets`。将此方法加入按钮处理组件（需要 `using Core; using Sound; using UnityEngine;`）：

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

在 Button Inspector 展开 **On Click ()**，点击 **+**，拖入挂有该组件的场景对象，再选择 **你的组件 → PlayClick()**。

**预期结果：** 加载后点击按钮，听到 click.wav；模块或音效不可用时，Console 提示先加载 GameAssets。

`PlaySound` 默认音量 1、音高 1、不循环；循环音乐用 `Play`。

## 使用约定

Core 提供 `UI`、`Background`、`Environment`、`Event` 四层，按用途选择 `SoundLayer`。

| 操作 | 调用 |
| --- | --- |
| 暂停背景层 | `GameCore.Sound.PauseSound(SoundLayer.Background)` |
| 恢复背景层 | `GameCore.Sound.UnPauseSound(SoundLayer.Background)` |
| 停止并清空背景层音频 | `GameCore.Sound.StopSound(SoundLayer.Background)` |

- 暂停只影响指定层。
- 已暂停或满池时，新请求被丢弃，不排队、不抢占声音。
- Stop 清除播放和 clip 引用，保留暂停状态；调用 UnPauseSound 后才能重新接收播放。
- 音频不加载资产，卸载资源模块前先停止相关播放。

<details>
<summary>需要时再看：音源数量与复用规则</summary>

循环音乐先加载 AudioClip，再调用 `GameCore.Sound.Play`，传入 `SoundLayer.Background`、该 clip、音量 `0.5f`、音高 `1f` 和 `isLoop: true`。

每个 SourceAgent 的 `Max Sources` 默认 16，限制扩容，不影响已有音源。复用时重设 clip、loop、pitch、volume。

扩容复制首个有效音源的混音、空间和监听器设置。`AudioListener.pause` 时，仅 `ignoreListenerPause` 音源可用。缺少层配置会警告并跳过播放。

游戏负责全局暂停、保存音量和跨场景音频策略；切场景不自动停止全部层。

</details>

<details>
<summary>需要时再看：直接操作 AudioSource</summary>

`SourceAgent.GetAudioSource()` 可能返回 null。借到后立即使用，它不是预留的播放句柄。

原生 Pause、PlayDelayed、PlayScheduled 绕过层暂停保证。`SourceAgent.Play(clip)` 表示是否受理，不保证设备可听；普通玩法使用层接口。

</details>

下一步：[用资源模块管理音效的加载与释放](resources.zh-CN.md)。
