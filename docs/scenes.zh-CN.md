# 场景转场

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](scenes.md)

在遮罩下加载关卡，关闭旧 UI，再等待自己的入场准备完成。

## 试试看

将 **Level01.unity** 保存到 `Assets/GameContent/MyGame/Content/Scenes/`，放入相机和可见物体。移除相机多余的 AudioListener，Core 已有一个。确认 Addressables 地址，并为 Player 目标平台构建内容。

Core 就绪后，从菜单按钮处理方法调用，不要放进 Update（需要 `using Core; using UnityEngine;`）：

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

**预期结果：** 点击一次，出现遮罩、关闭旧窗口、显示目标场景。Single 加载替换旧场景，Core 常驻。加载失败保留旧 UI 并显示 Retry；查看 Console 和面板错误。

## 使用约定

- true 表示受理，是否成功看完成回调。忙碌或请求无效时返回 false，不排队、不调用完成回调。
- 加载成功才关闭旧 UI。失败后查看 `LastError`，使用 Retry 面板或 `RetryTransition()`。
- 转场控制器放常驻对象，或用静态方法；Single 会卸载旧场景中的回调目标。
- 玩法输入自行检查 `IsTransitioning`。遮罩阻挡 UGUI 射线，不阻挡键盘或手柄代码。

<details>
<summary>需要时再看：等待 HUD 和资源准备好</summary>

传入 `enterScene: scene => PrepareScene(scene)`。方法返回 `IEnumerator`，参数为 `UnityEngine.SceneManagement.Scene`（需要 `using System.Collections;`）。

准备协程在场景激活、旧 UI 清理后执行，完成才揭开遮罩。可等待资源和 HUD 回调；支持嵌套协程，异常会使转场失败。

Single 激活后准备失败，不能恢复旧场景，但错误面板仍可重试。`RetryTransition()` 重试加载与准备，不回滚游戏进度。

timeScale=0 时用 realtime 等待或回调轮询。框架动画用非缩放时间，`WaitForSeconds` 仍遵循 Unity 规则。通过 finally / Dispose 清理自己持有的工作。

</details>

<details>
<summary>需要时再看：完成时机、生命周期与低层 API</summary>

遮罩隐藏、忙碌标志清除后调用完成回调，可提交下一次转场。通知异常只记录，不改变结果；直接绑定到已销毁 Unity 对象的方法会跳过。

框架不复位游戏暂停、不卸载全部模块、不停止全部音频，也不支持中途取消加载或回滚场景。禁用或销毁管理器只结算一次；迟到失败释放句柄，迟到成功保留新活动场景。

`LoadSceneAsync` / `UnloadSceneAsync` 绕过受控转场，供自行协调 Additive、手动激活和句柄归属的代码使用。调用者释放失败加载、卸载成功场景；同一操作不要混用两条路径。

</details>

下一步：[明确跨场景资源的持有和释放](resources.zh-CN.md)。
