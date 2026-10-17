# 场景转场

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](scenes.md)

从菜单进入关卡时显示加载遮罩。一次调用完成新场景加载、旧 UI 关闭，并等待你提供的准备工作。

## 试试看

先创建并保存自己的 **Level01.unity** 到 `Assets/GameContent/MyGame/Content/Scenes/`。放入相机和一个明显可见的物体，方便确认加载的是这个场景。移除相机上多余的 AudioListener，Core 已有一个。确认场景有有效 Addressables 地址；Player 需要为当前目标构建内容。

将下面的代码放在现有菜单按钮的点击处理方法中，Core 就绪后再调用，不要放进 Update。需要 `using Core; using UnityEngine;`：

```csharp
bool accepted = GameCore.Scene.TryTransition(
    "Assets/GameContent/MyGame/Content/Scenes/Level01.unity",
    completed: (succeeded, error) =>
    {
        if (!succeeded)
        {
            Debug.LogWarning(error);
        }
    });

if (!accepted)
{
    Debug.LogWarning("Transition was not accepted. Check the current transition and setup.");
}
```

**预期结果：** 点击一次后出现遮罩，旧窗口关闭，目标场景显示出来。加载失败时保留旧 UI，并显示 Retry 面板；先检查 Console 和面板里的错误，再重试。这里使用 Single 加载：目标场景替换旧场景，Core 继续常驻。

## 使用约定

- 返回 true 只表示受理；加载是否成功看完成回调。忙碌或配置无效时返回 false，不排队、不触发完成回调。
- 加载成功后才关闭旧 UI；加载失败保留旧窗口。失败可查看 `LastError`，并使用内置 Retry 面板或调用 `RetryTransition()`。
- 转场控制器放常驻对象，或使用静态方法；Single 会卸载旧场景，旧场景对象不适合接收后续流程回调。
- 玩法输入自行检查 `IsTransitioning`。遮罩阻挡 UGUI 射线，不会自动阻挡你的键盘或手柄代码。

<details>
<summary>需要时再看：等待 HUD 和资源准备好</summary>

在 `TryTransition` 中额外传入 `enterScene: scene => PrepareScene(scene)`。自己的 `PrepareScene` 返回 `IEnumerator`，参数为 `UnityEngine.SceneManagement.Scene`；编写协程需要 `using System.Collections;`。

框架会在新场景激活、旧 UI 清理后执行该准备协程，完成后再揭开遮罩。可在其中等待自己的资源加载回调和 HUD 打开回调；嵌套协程受支持，异常会使转场失败。

Single 激活后准备失败不会回滚旧场景，内置错误面板仍可重试。普通 `RetryTransition()` 会重新尝试该场景与入场准备，不等于游戏进度回滚。

需要在 timeScale=0 下准备时，用 realtime 等待或回调轮询。框架转场动画使用非缩放时间，但自己的 `WaitForSeconds` 仍遵循 Unity 规则。自己的 finally / Dispose 要清理自己拥有的工作。

</details>

<details>
<summary>需要时再看：完成时机、生命周期与低层 API</summary>

完成回调发生在遮罩隐藏、忙碌标志清除后，可以提交下一次转场。完成通知异常只记录，不改变已经结算的结果；直接绑定到已销毁 Unity 对象的方法会跳过。

框架不自动复位游戏暂停、卸载全部资源模块或停止所有音频，也不提供中途取消加载、场景回滚。禁用或销毁管理器只结算请求一次；迟到失败释放句柄，迟到成功不卸载新活动场景。

`LoadSceneAsync` / `UnloadSceneAsync` 是低层包装，供明确协调 Additive、手动激活和句柄归属的代码使用；它们绕过完整转场。低层调用者自行处理失败加载句柄与成功场景卸载，不要和受控转场混用同一流程。

</details>

下一步：[明确跨场景资源的持有和释放](resources.zh-CN.md)。
