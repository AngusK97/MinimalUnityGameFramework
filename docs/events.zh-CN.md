# 事件

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](events.md)

**定义名称 → 订阅 → 广播 → 退订。** 用事件发送通知时，不必知道谁在监听。我们先在 Console 打印一条消息试试，不需要 UI 或配置表。

<a id="试试看"></a>

## 01 试试看

<a id="1-准备场景"></a>

### 1.1 准备场景

打开框架工程。如果正在 Play，先停止运行。

1. 新建一个空场景，将 [GameCore.prefab](../Assets/Framework/Prefabs/GameCore.prefab) 拖到 Hierarchy 最外层，保留已有设置。
2. 新建一个名为 **EventExample** 的空 GameObject，与 GameCore 放在同一层级。
3. 将场景保存为 `Assets/GameContent/MyGame/Scenes/EventExample.unity`，缺少的目录先建好。

<a id="2-定义名称"></a>

### 1.2 定义名称

在 `Assets/GameContent/MyGame/Scripts/` 中选择 **Create → Scripting → MonoBehaviour Script**，命名为 **GameEvents**；缺少的目录先建好。用下面的代码替换 GameEvents.cs 的全部内容并保存。这个文件只保存事件名称，不用挂到 GameObject 上，也不需要枚举、配置表或提前登记。

```csharp
namespace MyGame
{
    public static class GameEvents
    {
        public const string LevelCompleted = "MyGame.LevelCompleted";
    }
}
```

<a id="3-订阅并广播"></a>

### 1.3 订阅并广播

用同一个脚本菜单，在 GameEvents.cs 旁边创建 **EventExample.cs**。用下面的代码替换整个文件并保存：

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

等待编译完成，将 **EventExample.cs** 拖到 EventExample 对象的 Inspector 上。保存场景，按下 Play。

**成功标志：** Console 输出一次 `Level completed.`。我们在 Start 中先订阅、再广播，这样监听方法就能收到第一次通知。GameCore 在 Awake 中初始化，其他对象的 OnEnable 可能比它更早执行，所以这里等到 Start 再使用服务，并先检查 Core 是否就绪。

<a id="4-从游戏中广播"></a>

### 1.4 从游戏中广播

在自己的游戏里发这条通知，只要把下面这一行放进通关方法，或其他需要发通知的方法。调用时 Core 必须已经就绪，脚本顶部要加上 `using Core;` 和 `using MyGame;`：

```csharp
GameCore.Event.DispatchNow(GameEvents.LevelCompleted);
```

在这个例子里，每广播一次，就会调用一次 OnLevelCompleted。发通知时只需要知道事件名称，不需要拿到 EventExample 对象。名称是前面在 GameEvents 里定义的常量。RegisterEvent 告诉框架：收到这个事件时，调用哪个方法。

销毁 EventExample 对象时，OnDestroy 会移除订阅。等游戏能发出这条通知后，可以试试：先广播一次，再在 Play 期间从 Hierarchy 中删除 EventExample，然后再次广播。这时应该不再输出它的消息。仅仅禁用组件不会退订。

<a id="选择通知方式"></a>

## 02 选择通知方式

| 操作 | 调用 |
| --- | --- |
| 订阅无参方法 | `RegisterEvent(GameEvents.LevelCompleted, OnLevelCompleted)` |
| 移除该订阅 | `UnRegisterEvent(GameEvents.LevelCompleted, OnLevelCompleted)` |
| 立即通知 | `DispatchNow(GameEvents.LevelCompleted)` |
| 下次事件更新时通知 | `Dispatch(GameEvents.LevelCompleted)` |

- 用游戏自己的字符串常量定义事件 ID。ID 不能为空或只有空白字符，处理方法不能为 null。
- 等 Core 就绪后，在 Unity 主线程使用事件服务。Core 已经负责更新事件，不要手动调用 OnUpdate。
- 需要接收通知时订阅，不再需要时，通过之前保存的管理器引用退订。禁用组件不会自动退订。
- 用 lambda 订阅时，如果之后还要退订，就先保存这个委托。重新写一个 lambda 会得到另一个监听器，无法用它移除原来的订阅。
- 事件**不会重放过去的通知**。后来创建的接收者需要读取当前游戏状态，并订阅后续变化。

<a id="前后用法"></a>

## 03 前后用法

原来带 sender 和数据的接口还可以继续用。如果接收者不需要知道通知来自哪里，也不需要读取数据，就用更简短的无参写法：

| | 带来源/数据的监听器 | 无参通知监听器 |
| --- | --- | --- |
| 处理方法 | `void OnLevelCompleted(object sender, string type, BaseEventArgs args)` | `void OnLevelCompleted()` |
| 发送 | `GameCore.Event.DispatchNow(this, GameEvents.LevelCompleted)` | `GameCore.Event.DispatchNow(GameEvents.LevelCompleted)` |

两种写法用的是相同的订阅和退订方法。需要知道来源或读取数据时，继续用原来的 `HandlerEvent` 签名就好。例如，Demo 的 [LevelView.cs](../Assets/GameContent/Demo/Scripts/UI/LevelView.cs) 就是通过 sender 判断通关通知是否来自当前关卡。

<details>
<summary>可选：将事件连接到 UI</summary>

想看事件如何配合 UI，可以参考 Demo 的 `DemoEvents.VolumeChanged`：[DataManager.cs](../Assets/GameContent/Demo/Scripts/DataManager.cs) 在音量变化后广播；[SettingsView.cs](../Assets/GameContent/Demo/Scripts/UI/SettingsView.cs) 在 OnShow 中订阅，再调用 RefreshVolume() 读取当前状态，并在 OnClose 中退订。先订阅再刷新，就能接住后续的变化。准备创建窗口时，可以接着看 [UI](ui.zh-CN.md)。

</details>

<details>
<summary>需要时再看：顺序、数据、异常与清理</summary>

无参 Action 和三参 HandlerEvent 监听器都会按订阅顺序执行。每次派发开始时，会保存一份监听器快照；回调中新增或移除订阅，只影响之后的派发（包括嵌套派发），不会改变当前这份快照。重复订阅会分别保留，一次 UnRegisterEvent 只移除其中一份。使用无参接口派发时，HandlerEvent 收到的 sender 和数据都为 null；使用原来的重载派发时，Action 监听器会忽略 sender 和数据。

每次更新只处理更新开始时已经排队的事件。处理期间新加入队列的延迟事件会留到下一批，即使再次进入 OnUpdate，也不会提前处理它们。

如果通知需要携带数据，创建 BaseEventArgs 的派生类并实现 Clear()，再调用 `DispatchNow(sender, eventId, args)` 或 `Dispatch(sender, eventId, args)`。延迟队列只保留原来的数据引用，不会复制数据，也不会自动调用 Clear()。在接收者用完之前，不要修改、清空或回收这份数据。

监听器抛出异常时，框架会记录错误，并继续执行下一条订阅。如果一次订阅传入了多播委托，它会被视为一个条目；其中某个方法抛出异常后，该条目剩下的方法都会被跳过。直接绑定到已销毁 Unity 对象的方法也会被跳过，但同一条目中其他有效方法仍会执行，除非前面的方法已经抛出异常。混合了多种调用目标的多播条目会一直保留原委托引用，直到退订。使用闭包时仍需显式退订：检查委托目标并不能保证闭包捕获的对象仍然可用。

管理器销毁时，会清空订阅和事件队列，并停止后续回调。继续通过已经清理的旧实例订阅或派发，会抛出 ObjectDisposedException；不过，仍然可以安全调用 UnRegisterEvent 完成清理。要确认关键操作是否成功，应检查返回值或完成回调。

</details>

下一步：[UI 的显示、关闭与订阅生命周期](ui.zh-CN.md)。
