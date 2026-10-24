# 事件

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](events.md)

**定义名称 → 订阅 → 广播 → 退订。** 通知窗口时，玩法无需持有窗口。需要返回结果时，直接调用方法。

## 试试看

先完成[开始使用](quick-start.zh-CN.md)，然后退出 Play。下面沿用其中的 MyGame 场景、UI 配置表和游戏入口。

### 1. 定义名称

将 **GameEvents.cs** 保存到 `Assets/GameContent/MyGame/Scripts/`。事件名称由游戏维护，不需要枚举、配置表或预先登记。

```csharp
namespace MyGame
{
    public static class GameEvents
    {
        public const string LevelCompleted = "MyGame.LevelCompleted";
    }
}
```

### 2. 添加监听

在同目录保存 **WinNotice.cs**。窗口显示时订阅，关闭时向保存的管理器退订：

```csharp
using Core;
using Event;
using UI;
using UnityEngine;

namespace MyGame
{
    public sealed class WinNotice : UIBase
    {
        private EventManager _events;

        protected override void OnShow()
        {
            _events = GameCore.Event;
            _events.RegisterEvent(GameEvents.LevelCompleted, OnLevelCompleted);
        }

        protected override void OnClose()
        {
            _events?.UnRegisterEvent(GameEvents.LevelCompleted, OnLevelCompleted);
            _events = null;
        }

        private void OnLevelCompleted()
        {
            Debug.Log("Level completed.", this);
        }
    }
}
```

### 3. 打开窗口并发送

1. 新建独立的 UI Prefab，保存到 `MyGame/Content/Prefabs/UI/WinNotice.prefab`。在它的 RectTransform 根上挂载 WinNotice，根上只保留这一个 UIBase 派生组件。在 **Project** 选中保存后的预制体，勾选 Inspector 中的 **Addressable**。
2. 在 Google Sheets 的 **UI** 标签中新增一行：**Id = WinNotice**、**Name = Win notice**、**PrefabPath = Assets/GameContent/MyGame/Content/Prefabs/UI/WinNotice.prefab**。点击 **Framework → Config → Update Config**，等待编译完成。参见 [UI 配置表](ui-tables.zh-CN.md)。
3. 在 **MyGameEntry.cs** 中保留一次 `GameConfig.LoadAndRegister()`，将它后面的原 OpenUI 调用替换为：

```csharp
GameCore.UI.OpenUI(UILayer.Second, UIName.WinNotice, callback: view =>
{
    if (view == null)
    {
        return;
    }

    GameCore.Event.DispatchNow(GameEvents.LevelCompleted);
});
```

按下 Play，**Console 应输出一次 `Level completed.`。** 打开窗口是异步操作；在完成回调里发送，就能等到 OnShow 完成订阅。

接下来，在按钮处理方法或通关方法中用这一行广播（需要 `using Core; using MyGame;`）。Core 就绪后，在 Unity 主线程调用：

```csharp
GameCore.Event.DispatchNow(GameEvents.LevelCompleted);
```

窗口打开时，每次广播输出一次。用 `GameCore.UI.CloseUI(UIName.WinNotice)` 关闭窗口后，再广播就不输出了。RegisterEvent 登记的是监听器，不是事件名定义。

## 选择通知方式

| 操作 | 调用 |
| --- | --- |
| 订阅无参方法 | `RegisterEvent(GameEvents.LevelCompleted, OnLevelCompleted)` |
| 移除该订阅 | `UnRegisterEvent(GameEvents.LevelCompleted, OnLevelCompleted)` |
| 立即通知 | `DispatchNow(GameEvents.LevelCompleted)` |
| 下次事件更新时通知 | `Dispatch(GameEvents.LevelCompleted)` |

- 使用游戏自己的字符串常量，ID 不能是空白，处理方法不能为 null。
- Core 就绪后，在 Unity 主线程使用服务。Core 已负责事件更新，不要手调 OnUpdate。
- 需要接收通知时订阅，不再需要时向保存的管理器退订。禁用组件不会自动退订。
- 需要移除 lambda 时保留原委托；新写一个 lambda 是另一个监听器。
- 事件**不重放历史**。稍后打开的窗口先读取当前游戏状态，再监听后续变化。

## 前后用法

原 sender/数据接口仍可使用；不需要来源或数据时，选择简短签名：

| | 带来源/数据的监听器 | 无参通知监听器 |
| --- | --- | --- |
| 处理方法 | `void OnLevelCompleted(object sender, string type, BaseEventArgs args)` | `void OnLevelCompleted()` |
| 发送 | `GameCore.Event.DispatchNow(this, GameEvents.LevelCompleted)` | `GameCore.Event.DispatchNow(GameEvents.LevelCompleted)` |

登记与退订仍使用相同方法名。接收者需要来源或数据时保留原有 `HandlerEvent` 签名；Demo 的 [LevelView.cs](../Assets/GameContent/Demo/Scripts/UI/LevelView.cs) 仍按当前关卡的 sender 过滤通关通知。

Demo 也使用无参的 `DemoEvents.VolumeChanged` 通知：[DataManager.cs](../Assets/GameContent/Demo/Scripts/DataManager.cs) 在音量变化后广播，[SettingsView.cs](../Assets/GameContent/Demo/Scripts/UI/SettingsView.cs) 显示时刷新当前状态，监听后续变化，关闭时退订。

<details>
<summary>需要时再看：顺序、数据、异常与清理</summary>

无参 Action 与三参 HandlerEvent 监听器共用登记顺序和一次派发快照。回调中的订阅变化影响后续派发（包括嵌套派发），不改变当前快照。重复订阅分别保留，一次 UnRegisterEvent 只移除一份。无参派发给 HandlerEvent 传入 null sender/数据；Action 监听器忽略原重载传入的 sender/数据。

每次事件更新取出开始时的待处理队列；处理中新增的延迟事件留到下一批，重入 OnUpdate 不会提前刷新。

携带数据时，派生 BaseEventArgs、实现 Clear()，调用 `DispatchNow(sender, eventId, args)` 或 `Dispatch(sender, eventId, args)`。延迟队列保留原引用，不复制，也不自动调用 Clear()；接收者用完前不要修改、清空或回收。

异常会记录，下一条登记监听继续。一次登记传入的多播委托视为一个条目：其中某项抛出，跳过该条目剩余项。直接指向已销毁 Unity 对象的调用项会被跳过；同条目内其他有效调用项仍执行，除非前项抛异常。混合多播条目在退订前仍保留原委托引用。闭包仍需显式退订；目标检查不能保证捕获对象安全。

管理器销毁时清空订阅/队列，并停止后续回调。向已清理的旧实例登记或派发会抛 ObjectDisposedException；UnRegisterEvent 仍可安全用于收尾。关键操作是否成功应通过返回值或完成回调确认。

</details>

下一步：[UI 的显示、关闭与订阅生命周期](ui.zh-CN.md)。
