# 事件

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](events.md)

让窗口收到通关消息，玩法代码不用持有这个窗口。需要返回结果时，直接调用方法。

## 试试看

Core 就绪后再订阅。把下面完整的 **WinNotice.cs** 保存到 `Assets/GameContent/MyGame/Scripts/`。它在窗口显示时订阅、关闭时取消：

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

把它放在一个独立窗口 Prefab 的根上，作为唯一的 UIBase 派生组件；经 [UI 配置](ui.zh-CN.md) 登记并打开。不要和已有 MyMenuView 同时挂在同一个根上。

先打开这个窗口，再在现有按钮的处理方法或通关方法中发送通知。需要 `using Core;`：

```csharp
GameCore.Event.DispatchNow(this, MyGame.WinNotice.LevelCompleted);
```

**预期结果：** 窗口打开时发送一次，Console 输出一次 `Level completed.`。关闭窗口再发送，不再输出，因为 OnClose 已取消订阅。

## 使用约定

- 事件 ID 由游戏维护，建议集中定义字符串常量；ID 不可为空，处理方法不可为 null。
- `DispatchNow` 立即发送；`Dispatch` 留到 Core 的下一次事件更新。通常选一种，不必发送两次。
- 谁订阅，谁取消订阅；不要每帧重复登记。Core 已驱动事件更新，不要手调 `OnUpdate`。

<details>
<summary>需要时再看：回调顺序、延迟数据与异常</summary>

立即派发使用监听器快照，按登记顺序调用。回调里增删监听器影响后续派发，包括嵌套派发；不会改变当前快照。重复订阅保留重复项，一次 `UnRegisterEvent` 只移除一项。

延迟队列在一次事件更新开始时取出；处理过程中新增的延迟事件留到下一批。重入 `OnUpdate` 不会提前刷新。

需要携带数据时，派生 `BaseEventArgs`、实现 `Clear()`，通过第三个实参传入。延迟队列持有原对象引用，发送后不要立即修改或回收这份数据；框架不会自动复制它。

单个监听器异常会记录，其他监听器继续运行。不要用“有没有收到事件”判断关键操作是否成功，改用明确的方法返回值或完成回调。

</details>

下一步：[UI 的显示、关闭与订阅生命周期](ui.zh-CN.md)。
