# 事件

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](events.md)

通知窗口关卡已完成，玩法代码无需持有窗口。需要返回结果时，直接调用方法。

## 试试看

将 **WinNotice.cs** 保存到 `Assets/GameContent/MyGame/Scripts/`。Core 就绪后，它在窗口显示时订阅、关闭时取消：

```csharp
using Core;
using Event;
using UI;
using UnityEngine;

namespace MyGame
{
    public sealed class WinNotice : UIBase
    {
        public const string LevelCompleted = "MyGame.LevelCompleted";

        protected override void OnShow()
        {
            GameCore.Event.RegisterEvent(LevelCompleted, OnLevelCompleted);
        }

        protected override void OnClose()
        {
            if (GameCore.Event != null)
            {
                GameCore.Event.UnRegisterEvent(LevelCompleted, OnLevelCompleted);
            }
        }

        private void OnLevelCompleted(object sender, string type, BaseEventArgs args)
        {
            Debug.Log("Level completed.", this);
        }
    }
}
```

将它作为唯一的 UIBase 派生组件挂在独立窗口 Prefab 根上，再[登记并打开](ui.zh-CN.md)。不要和 MyMenuView 挂在同一个根上。

打开窗口后，从按钮处理方法或通关方法发送通知（需要 `using Core;`）：

```csharp
GameCore.Event.DispatchNow(this, MyGame.WinNotice.LevelCompleted);
```

**预期结果：** 窗口打开时，每次发送在 Console 输出一次 `Level completed.`；关闭后发送不再输出。

## 使用约定

- 游戏维护事件 ID，建议使用共用字符串常量。ID 不可为空，处理方法不可为 null。
- `DispatchNow` 立即发送；`Dispatch` 等待 Core 下一次事件更新。每条通知选一种。
- 谁订阅，谁取消；不要每帧登记，也不要手调 `OnUpdate`，Core 已负责更新。

<details>
<summary>需要时再看：回调顺序、延迟数据与异常</summary>

立即派发按登记顺序调用监听器快照。回调里增删监听器影响后续派发（包括嵌套派发），不改变当前快照。重复订阅分别保留，一次 `UnRegisterEvent` 只移除一项。

每次事件更新取出开始时的待处理队列。处理中新增的延迟事件留到下一批，重入 `OnUpdate` 不会提前刷新。

携带数据时，派生 `BaseEventArgs`、实现 `Clear()`，作为第三个实参传入。延迟队列保留原引用，不复制数据；发送后不要立即修改或回收。

监听器异常会记录，其他监听器继续。关键操作是否成功，用返回值或完成回调确认。

</details>

下一步：[UI 的显示、关闭与订阅生命周期](ui.zh-CN.md)。
