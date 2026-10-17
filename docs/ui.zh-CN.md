# UI

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](ui.md)

**给窗口一个名字，再从游戏代码里打开或关闭它。加载和清理由 UIManager 处理。**

## 打开窗口

先完成[开始使用](quick-start.zh-CN.md)，或创建自己的 [UI 配置表](ui-tables.zh-CN.md)。Core 就绪后登记一次。以下示例假定 `MyGame.GameUI.Menu` 已经存在。

需要 `using Core; using UI;`。把下面的代码放在按钮点击方法中，或启动脚本的 `Start()` 里 RegisterAll 之后，在需要时显示菜单：

```csharp
GameCore.UI.OpenUI(UILayer.First, MyGame.GameUI.Menu, callback: view =>
{
    if (view == null)
    {
        return; // Failed or cancelled.
    }

    // The view is initialized and shown.
});
```

普通界面使用 `First`，弹窗使用 `Second`。`Transition` 保留给转场遮罩。

## 关闭窗口

| 调用位置 | 调用方式 |
| --- | --- |
| 游戏代码 | `GameCore.UI.CloseUI(MyGame.GameUI.Menu)` |
| 窗口内部 | `CloseSelf()` |
| 一起关闭当前请求 | `GameCore.UI.CloseAll()` |

可以试着关闭窗口，再打开一次：它应该先消失，再重新出现。UIManager 会帮你释放关闭的实例。

## 将逻辑放入合适的钩子

| `UIBase` 子类中的钩子 | 用途 |
| --- | --- |
| `OnInit()` | 缓存引用，读取继承的 `Data` |
| `OnShow()` | 绑定按钮，订阅事件 |
| `OnClose()` | 移除对应的绑定与订阅 |
| `OnUpdate()` | 窗口显示期间的逐帧逻辑，由 Unity 驱动 |

已保存的预制体需要在**根对象上挂载一个 UIBase 派生组件**。根对象使用 UI RectTransform；共用 Core 提供父级 Canvas 和 EventSystem。

<details>
<summary>请求行为与手动登记</summary>

重复打开会共享尚未完成的加载；窗口已打开时，返回当前实例，不会再次调用 OnShow。第一次请求决定层级和数据。失败或取消时，回调收到 null；直接绑定到已销毁 Unity 对象的方法会跳过。

CloseUI 也会取消排队或加载中的请求，并释放迟到的实例。IsOpen 包含排队和加载状态，因此不代表窗口可见。CloseAll 处理调用开始时的请求快照；关闭钩子或回调中显式打开的新请求会保留。

Close、Release 和 CloseSelf 都会结束窗口所属的请求。框架不使用窗口池。OnClose 必须能处理 OnShow 只完成部分初始化的情况。重写 OnDestroy 时，在自己的清理之后调用 base.OnDestroy()。不要手动调用 Update。

项目也可以手动登记，不使用生成的配置表：

```csharp
GameCore.UI.RegisterUiData(new UIData("MyGame.Menu",
    "Assets/GameContent/MyGame/Content/Prefabs/UI/MyMenuView.prefab"));
```

Core 就绪后再登记。空标识、空地址和重复标识会被拒绝；不要再通过 RegisterAll 重复登记同一批标识。底层淡入淡出用于单独协调的 UI 工作；场景转场请使用场景 API。

</details>

下一步：通过 [UI 配置表](ui-tables.zh-CN.md) 添加另一个窗口，或使用[事件](events.zh-CN.md)连接玩法。
