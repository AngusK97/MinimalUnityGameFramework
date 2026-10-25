# UI

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](ui.md)

**按名称打开或关闭窗口，实例的加载和释放交给 UIManager。**

<a id="打开窗口"></a>

## 01 打开窗口

下面沿用[开始使用](quick-start.zh-CN.md)中的 `Menu` 窗口。如果你只做了 [UI 配置表](ui-tables.zh-CN.md)教程，把下方打开、关闭示例中的 `Menu` 换成 `Shop`。

在 **MyGameEntry.cs** 的 `Start()` 中，用下面的代码替换原来的 OpenUI 调用，放在 `GameConfig.LoadAndRegister()` 后面。Core 启动后加载一次配置就好，不要再加一处调用。也可以在启动完成后的按钮点击方法中打开窗口（需要 `using Core; using UI;`）：

```csharp
GameCore.UI.OpenUI(UILayer.First, MyGame.UIName.Menu, callback: view =>
{
    if (view == null)
    {
        return; // Failed or cancelled.
    }

    // The view is initialized and shown.
});
```

普通界面用 `First`，弹窗用 `Second`；`Transition` 留给转场遮罩。

<a id="关闭窗口"></a>

## 02 关闭窗口

| 调用位置 | 调用方式 |
| --- | --- |
| 游戏代码 | `GameCore.UI.CloseUI(MyGame.UIName.Menu)` |
| 窗口内部 | `CloseSelf()` |
| 关闭当前所有窗口（含加载中） | `GameCore.UI.CloseAll()` |

**预期结果：** 关闭窗口时，窗口消失，实例被释放；再次打开时，会创建一个新实例。

<a id="将逻辑放入合适的钩子"></a>

## 03 在生命周期方法中编写逻辑

| `UIBase` 子类中的方法 | 用途 |
| --- | --- |
| `OnInit()` | 缓存引用，读取从 UIBase 继承的 `Data` |
| `OnShow()` | 绑定按钮，订阅事件 |
| `OnClose()` | 移除对应的绑定与订阅 |
| `OnUpdate()` | 窗口显示期间每帧执行的逻辑，由 Unity 驱动 |

保存预制体时，确保 **RectTransform 根对象上只挂一个继承 UIBase 的组件**。父级 Canvas 和 EventSystem 由 Core 提供。

<details>
<summary>请求行为与手动登记</summary>

同一个窗口还在加载时，重复打开会共用这次加载；已经打开时，会返回现有实例，不会再次调用 OnShow。第一次调用决定窗口的层级和数据。打开失败或被取消时，回调收到 null；如果回调直接绑定到某个 Unity 对象的方法，而这个对象已经销毁，就会跳过该回调。

CloseUI 会取消仍在排队或加载中的请求；如果实例在取消后才加载完成，也会被释放。IsOpen 会把排队和加载中的请求算作已打开，所以返回 true 不代表窗口已经可见。CloseAll 只处理调用那一刻已有的请求；如果在关闭方法或回调中又主动打开了窗口，这些新请求会保留。

Close、Release、CloseSelf 都会结束这次打开，窗口会被释放，不会放进池中复用。编写 OnClose 时，要考虑 OnShow 可能只完成了一部分初始化。重写 OnDestroy 时，完成自己的清理后，再调用 base.OnDestroy()。不要手动调用 Update。

也可以不用生成的配置表，直接在代码中登记：

```csharp
GameCore.UI.RegisterUiData(new UIData("Menu",
    "Assets/GameContent/MyGame/Content/Prefabs/UI/MyMenuView.prefab"));
```

等 Core 就绪后再登记。ID 不能为空，也不能重复；手动登记后，不要再通过 GameConfig 登记相同 ID。地址可以留空作为占位，但此时调用 OpenUI 会报告失败，并向回调传入 null。如果直接调用底层淡入淡出功能，就要自己安排调用顺序。切换场景时，用场景 API 即可。

</details>

下一步：用 [UI 配置表](ui-tables.zh-CN.md) 添加窗口，或用[事件](events.zh-CN.md)连接玩法。
