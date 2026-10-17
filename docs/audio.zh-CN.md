# 音频

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](audio.md)

播放按钮音效、暂停背景音乐，或停止某一层的声音。框架会帮你复用音源。

## 试试看

先完成[资源示例](resources.zh-CN.md)，让启动入口在 Core 就绪后登记并加载 `GameAssets`。将下面的方法放入处理现有按钮的组件中，把按钮的 On Click 事件连接到 `PlayClick()`。在该脚本顶部加入 `using Core; using Sound; using UnityEngine;`：

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

在 Button 的 Inspector 中展开 **On Click ()**，点击 **+**，把挂有该组件的场景对象拖入对象槽，再选择 **你的组件 → PlayClick()**。

**试试看：** 加载完成后点击按钮，应听到自己的 click.wav；模块或音效还没就绪时，Console 会提示先加载 GameAssets。

`PlaySound` 默认音量 1、音高 1、不循环。短音效用它即可，循环音乐显式调用 `Play`。

## 使用约定

公共 Core 配有 `UI`、`Background`、`Environment`、`Event` 四层。按声音用途选择 `SoundLayer`，不用自己逐个创建 AudioSource。

| 操作 | 调用 |
| --- | --- |
| 暂停背景层 | `GameCore.Sound.PauseSound(SoundLayer.Background)` |
| 恢复背景层 | `GameCore.Sound.UnPauseSound(SoundLayer.Background)` |
| 停止并清空背景层音频 | `GameCore.Sound.StopSound(SoundLayer.Background)` |

- 暂停只影响指定层；暂停背景层不会暂停 UI 音效。
- 已暂停或满池时，新播放请求会被丢弃，不排队、不抢占已有声音。
- Stop 会清除播放与 clip 引用，但保留该层暂停状态；要重新接收播放，调用 UnPauseSound。
- 音频系统不负责加载资源。停止相关播放后再卸载资源模块，不要边播放边释放 clip。

<details>
<summary>需要时再看：音源数量与复用规则</summary>

循环音乐先加载对应 AudioClip，再调用 `GameCore.Sound.Play`，依次传入 `SoundLayer.Background`、该音效、音量 `0.5f`、音高 `1f` 和 `isLoop: true`。与 `PlaySound` 相比，这样可以明确指定播放参数。

每个 SourceAgent 的 `Max Sources` 默认 16，限制动态扩容，不会删除已有音源。复用音源会重设 clip、loop、pitch、volume，避免沿用上次播放的参数。

扩容复制首个有效音源的混音、空间和监听器配置。`AudioListener.pause` 时，只有 `ignoreListenerPause` 的来源可用；缺少层配置会警告并跳过播放。

切场景不会自动停止全部层，框架也不自动保存用户音量。全局暂停、音量设置和声音跨场景策略由自己的游戏决定。

</details>

<details>
<summary>需要时再看：直接操作 AudioSource</summary>

底层 `SourceAgent.GetAudioSource()` 可能返回 null。借到后应立即使用，它不是预留的长期播放句柄。

原生 Pause、PlayDelayed、PlayScheduled 会绕过层暂停保证。`SourceAgent.Play(clip)` 返回请求是否受理，不保证设备实际可听；普通业务优先使用上面的层接口。

</details>

下一步：[用资源模块管理音效的加载与释放](resources.zh-CN.md)。
