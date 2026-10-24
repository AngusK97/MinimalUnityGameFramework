# 事件

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](events.md)

**定义名称 → 订阅 → 广播 → 退订。** 发送通知时不必知道谁在监听。本例只在 Console 打印消息，不需要 UI 或配置表。

## 试试看

### 1. 准备场景

使用本仓库的 Unity 工程。新建空场景，将 [GameCore.prefab](../Assets/Framework/Prefabs/GameCore.prefab) 拖到 Hierarchy 最外层，保留已有设置。新建一个名为 **EventExample** 的空 GameObject。

### 2. 定义名称

在 `Assets/GameContent/MyGame/Scripts/` 保存 **GameEvents.cs**，没有目录就先创建。字符串常量就是事件名称，不需要枚举、配置表或预先登记。

```csharp
namespace MyGame
{
    public static class GameEvents
    {
        public const string LevelCompleted = "MyGame.LevelCompleted";
    }
}
```

### 3. 订阅并广播

在同目录保存 **EventExample.cs**，挂到 EventExample 对象上：

```csharp
using Core;
using Event;
using UnityEngine;

namespace MyGame
{
    public sealed class EventExample : MonoBehaviour
    {
        private EventManager _events;

        private void Start()
        {
            if (!GameCore.IsReady)
            {
                Debug.LogError("Add the shared GameCore prefab to the scene.", this);
                return;
            }

            _events = GameCore.Event;
            _events.RegisterEvent(GameEvents.LevelCompleted, OnLevelCompleted);

            _events.DispatchNow(GameEvents.LevelCompleted);
        }

        private void OnLevelCompleted()
        {
            Debug.Log("Level completed.", this);
        }

        private void OnDestroy()
        {
            if (_events != null)
            {
                _events.UnRegisterEvent(GameEvents.LevelCompleted, OnLevelCompleted);
            }

            _events = null;
        }
    }
}
```

按下 Play，**Console 应输出一次 `Level completed.`。** Start 中先订阅再广播，不会漏掉首次通知。GameCore 在 Awake 初始化，其他对象的 OnEnable 可能比它更早，因此本例等到 Start 并检查是否就绪。

### 4. 从游戏中广播

在通关方法或其他游戏脚本中，Core 就绪后用这一行发送（需要 `using Core; using MyGame;`）：

```csharp
GameCore.Event.DispatchNow(GameEvents.LevelCompleted);
```

每次广播都会调用一次监听方法。发送方只需知道事件名称，不需要持有 EventExample 对象。RegisterEvent 订阅的是监听器，不是定义事件名。

销毁 EventExample 对象时，OnDestroy 移除订阅。要检查清理效果，先从游戏中广播，再在 Play 期间从 Hierarchy 删除 EventExample，重新广播应不再输出它的消息。仅禁用组件不会退订。

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
- 事件**不重放历史**。稍后创建的接收者需读取当前游戏状态，并订阅后续变化。

## 前后用法

原 sender/数据接口仍可使用；不需要来源或数据时，选择简短签名：

| | 带来源/数据的监听器 | 无参通知监听器 |
| --- | --- | --- |
| 处理方法 | `void OnLevelCompleted(object sender, string type, BaseEventArgs args)` | `void OnLevelCompleted()` |
| 发送 | `GameCore.Event.DispatchNow(this, GameEvents.LevelCompleted)` | `GameCore.Event.DispatchNow(GameEvents.LevelCompleted)` |

登记与退订仍使用相同方法名。接收者需要来源或数据时保留原有 `HandlerEvent` 签名；Demo 的 [LevelView.cs](../Assets/GameContent/Demo/Scripts/UI/LevelView.cs) 仍按当前关卡的 sender 过滤通关通知。

<details>
<summary>可选：将事件连接到 UI</summary>

完整 UI 用法可看 Demo 的 `DemoEvents.VolumeChanged`：[DataManager.cs](../Assets/GameContent/Demo/Scripts/DataManager.cs) 在音量变化后广播；[SettingsView.cs](../Assets/GameContent/Demo/Scripts/UI/SettingsView.cs) 在 OnShow 订阅并通过 RefreshVolume() 读取当前状态，在 OnClose 退订。先订阅再刷新，后续变化就有人接收。准备创建窗口时再看 [UI](ui.zh-CN.md)。

</details>

<details>
<summary>需要时再看：顺序、数据、异常与清理</summary>

无参 Action 与三参 HandlerEvent 监听器共用登记顺序和一次派发快照。回调中的订阅变化影响后续派发（包括嵌套派发），不改变当前快照。重复订阅分别保留，一次 UnRegisterEvent 只移除一份。无参派发给 HandlerEvent 传入 null sender/数据；Action 监听器忽略原重载传入的 sender/数据。

每次事件更新取出开始时的待处理队列；处理中新增的延迟事件留到下一批，重入 OnUpdate 不会提前刷新。

携带数据时，派生 BaseEventArgs、实现 Clear()，调用 `DispatchNow(sender, eventId, args)` 或 `Dispatch(sender, eventId, args)`。延迟队列保留原引用，不复制，也不自动调用 Clear()；接收者用完前不要修改、清空或回收。

异常会记录，下一条登记监听继续。一次登记传入的多播委托视为一个条目：其中某项抛出，跳过该条目剩余项。直接指向已销毁 Unity 对象的调用项会被跳过；同条目内其他有效调用项仍执行，除非前项抛异常。混合多播条目在退订前仍保留原委托引用。闭包仍需显式退订；目标检查不能保证捕获对象安全。

管理器销毁时清空订阅/队列，并停止后续回调。向已清理的旧实例登记或派发会抛 ObjectDisposedException；UnRegisterEvent 仍可安全用于收尾。关键操作是否成功应通过返回值或完成回调确认。

</details>

下一步：[UI 的显示、关闭与订阅生命周期](ui.zh-CN.md)。
