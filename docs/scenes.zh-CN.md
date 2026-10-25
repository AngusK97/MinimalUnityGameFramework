# 场景转场

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](scenes.md)

用遮罩盖住画面，加载新关卡、关闭旧 UI，等你准备好新场景后再显示出来。

<a id="试试看"></a>

## 01 试试看

1. 创建场景，放入相机和一个可见物体，保存为 `Assets/GameContent/MyGame/Content/Scenes/` 下的 **Level01.unity**。Core 已经有一个 AudioListener，所以要移除场景相机上多余的 AudioListener。然后在 **Project 中选中保存好的 Level01.unity 资产**，勾选 Inspector 中的 **Addressable**。
2. 下面是 **Scene** 表的示例。第一次使用时，按[配置接入](config-setup.zh-CN.md)新建本地工作簿，填入这张表。已经接入的话，在当前来源中打开现有的 **Scene** 工作表；没有这张表时再创建。新建的工作表从 A1 开始输入全部内容；已有工作表保留前两行，只添加 **Level01** 这一行数据。

| Id | Name | ScenePath |
| --- | --- | --- |
| string | string | string |
| Level01 | First level | Assets/GameContent/MyGame/Content/Scenes/Level01.unity |

3. 第一次连接时，按[配置接入](config-setup.zh-CN.md)的步骤，用这张 Scene 表完成设置。已经连接过的话，点击顶部工具栏左侧的 **Update Config**，等待更新结果和编译完成。需要切换游戏时，先在 **Framework → Config** 中选择 **MyGame**。在启动代码中，等 Core 就绪后调用一次 `MyGame.GameConfig.LoadAndRegister()`，将 `SceneName.Level01 = "Level01"` 与实际的 Addressables 地址关联起来。

启动完成后，在菜单按钮的点击处理方法中调用下面的代码，不要放进 Update（需要 `using Core; using UnityEngine;`）：

```csharp
GameCore.Scene.TryTransition(
    MyGame.SceneName.Level01,
    completed: (succeeded, error) =>
    {
        if (!succeeded)
        {
            Debug.LogWarning(error);
        }
    });
```

**预期结果：** 点击一次按钮后，遮罩出现，旧窗口关闭，然后显示目标场景。Single 模式会用新场景替换旧场景，Core 则会保留下来。在 Editor 外测试前，先为 Player 的目标平台构建 Addressables。如果加载失败，旧 UI 会保留，并显示 Retry 面板；错误原因可以在 Console 和面板中查看。

<a id="使用约定"></a>

## 02 使用约定

- `TryTransition` 和 `TryTransitionAddress` 返回 true，表示请求已被接受。如果正在转场，或请求无效，则返回 false，不会排队，并立即调用一次 `completed(false, error)`。所以上例中的回调既会处理请求被拒绝，也会处理转场开始后的失败。
- 只有加载成功后，才会关闭旧 UI。如果已经开始的转场失败，可以查看 `LastError`，然后使用 Retry 面板或 `RetryTransition()` 重试。被拒绝的请求不会创建重试操作。
- 将转场控制器放在常驻对象上，或使用静态方法。Single 模式会卸载旧场景，其中的回调目标也会被销毁。
- 在处理游戏输入时，自行检查 `IsTransitioning`。遮罩会挡住 UGUI 射线，但不会阻止键盘或手柄输入代码执行。

<details>
<summary>需要时再看：等待 HUD 和资源准备好</summary>

传入 `enterScene: scene => PrepareScene(scene)`，就可以让转场等待自己的准备工作。这个方法需要返回 `IEnumerator`，参数类型是 `UnityEngine.SceneManagement.Scene`（需要 `using System.Collections;`）。

准备协程会在场景激活、旧 UI 清理完成后执行，等它结束才会隐藏遮罩。你可以在这里等待资源和 HUD 的回调，也可以使用嵌套协程。如果协程抛出异常，这次转场会被判定为失败。

Single 模式下，新场景激活后，旧场景已经被替换。此时即使准备工作失败，也无法恢复旧场景，但仍可通过错误面板重试。`RetryTransition()` 会重新加载场景并执行准备工作，不会回滚游戏进度。

timeScale=0 时，使用 realtime 等待，或轮询回调是否完成。框架的渐变动画不受时间缩放影响，但 `WaitForSeconds` 仍遵循 Unity 的时间规则。你启动的任务和持有的资源，需要在 finally / Dispose 中自行清理。

</details>

<details>
<summary>需要时再看：完成时机、生命周期与低层 API</summary>

对于已接受的转场，完成回调会在遮罩隐藏、转场状态恢复为空闲后执行，此时可以发起下一次转场。请求被拒绝时，回调会同步执行，不会打断正在进行的转场。通知过程中抛出的异常只会被记录，不会改变转场结果；直接绑定到已销毁 Unity 对象的方法会被跳过。

框架不会自动重置游戏的暂停状态、卸载所有模块或停止所有音频，也不支持中途取消加载或回滚场景。禁用或销毁管理器时，每次转场只会完成一次收尾。之后才返回的加载结果如果失败，会释放句柄；如果成功，则保留新加载的活动场景。

`TryTransition(string)` 会按已登记的场景 ID 查找地址。遇到未知 ID 时，它会返回 false，不会把这个字符串当作 Addressables 地址来尝试加载。如果要直接传入 Addressables 地址，调用 `TryTransitionAddress(key, ...)`。旧版代码中直接传入字符串地址的地方，也要改用这个方法。

`LoadSceneAsync` / `UnloadSceneAsync` 不经过上面的转场流程，适合需要自行管理 Additive 加载、手动激活和句柄释放的代码。加载失败时，由调用者释放句柄；加载成功后，也由调用者负责卸载场景。同一次操作不要混用这两套方式。

</details>

下一步：[管理跨场景资源的加载和释放](resources.zh-CN.md)。
