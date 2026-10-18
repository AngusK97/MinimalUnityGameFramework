# UI

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](ui.md)

**按名称打开或关闭窗口，UIManager 负责加载和释放实例。**

## 打开窗口

完成[开始使用](quick-start.zh-CN.md)，或创建 [UI 配置表](ui-tables.zh-CN.md)，在 Core 就绪后登记一次。示例使用 `MyGame.GameUI.Menu`。

需要 `using Core; using UI;`。将代码放在按钮点击方法，或启动脚本 `Start()` 的 RegisterAll 之后：

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

普通界面用 `First`，弹窗用 `Second`；`Transition` 留给转场遮罩。

## 关闭窗口

| 调用位置 | 调用方式 |
| --- | --- |
| 游戏代码 | `GameCore.UI.CloseUI(MyGame.GameUI.Menu)` |
| 窗口内部 | `CloseSelf()` |
| 一起关闭当前请求 | `GameCore.UI.CloseAll()` |

**预期结果：** 关闭再打开窗口。窗口消失、实例释放，随后出现新实例。

## 将逻辑放入合适的钩子

| `UIBase` 子类中的钩子 | 用途 |
| --- | --- |
| `OnInit()` | 缓存引用，读取继承的 `Data` |
| `OnShow()` | 绑定按钮，订阅事件 |
| `OnClose()` | 移除对应的绑定与订阅 |
| `OnUpdate()` | 窗口显示期间的逐帧逻辑，由 Unity 驱动 |

保存预制体时，**RectTransform 根对象上只挂一个 UIBase 派生组件**。Core 提供父级 Canvas 和 EventSystem。

<details>
<summary>请求行为与手动登记</summary>

重复打开共享未完成的加载；窗口已打开时返回现有实例，不再调用 OnShow。首次请求决定层级和数据。失败或取消时回调收到 null；直接绑定到已销毁 Unity 对象的方法会跳过。

CloseUI 取消排队或加载中的请求，并释放迟到实例。IsOpen 包含排队和加载状态，不只表示可见。CloseAll 处理调用时的请求快照；关闭钩子或回调中显式打开的新请求会保留。

Close、Release、CloseSelf 都会结束所属请求；框架没有窗口池。OnClose 要能处理 OnShow 只完成部分初始化的情况。重写 OnDestroy 时，清理后调用 base.OnDestroy()。不要手动调用 Update。

也可手动登记，替代生成的配置表：

```csharp
GameCore.UI.RegisterUiData(new UIData("MyGame.Menu",
    "Assets/GameContent/MyGame/Content/Prefabs/UI/MyMenuView.prefab"));
```

Core 就绪后再登记。空标识、空地址及重复标识会被拒绝，不要再用 RegisterAll 登记相同标识。底层淡入淡出需单独协调；场景转场使用场景 API。

</details>

下一步：用 [UI 配置表](ui-tables.zh-CN.md) 添加窗口，或用[事件](events.zh-CN.md)连接玩法。
