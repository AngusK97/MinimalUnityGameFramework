# Events

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](events.zh-CN.md)

Notify a window when a level finishes, without gameplay holding the window. Use a direct call when you need a result.

## Try it

Save **WinNotice.cs** in `Assets/GameContent/MyGame/Scripts/`. After Core is ready, it subscribes when shown and unsubscribes when closed:

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

Attach it as the only UIBase-derived component on a separate window prefab root, then [register and open it](ui.md). Do not add it beside MyMenuView.

With that window open, send this from a button handler or level-completion method (`using Core;`):

```csharp
GameCore.Event.DispatchNow(this, MyGame.WinNotice.LevelCompleted);
```

**Expected result:** each send prints `Level completed.` once in Console while the window is open. After closing, sends produce no output.

## Conventions

- Your game owns event IDs; use shared string constants. IDs cannot be empty; handlers cannot be null.
- `DispatchNow` sends immediately; `Dispatch` waits for Core's next event update. Choose one per notification.
- Subscribers remove their subscriptions. Do not register every frame or manually call `OnUpdate`; Core drives it.

<details>
<summary>When needed: ordering, deferred data and exceptions</summary>

Immediate dispatch snapshots listeners in registration order. Callback changes affect later dispatches, including nested ones, not the current snapshot. Duplicate subscriptions remain separate; one `UnRegisterEvent` removes one occurrence.

Each event update takes the pending queue at its start. Events queued during processing wait for the next batch; re-entering `OnUpdate` does not flush them early.

For data, derive `BaseEventArgs`, implement `Clear()`, and pass it as the third argument. The deferred queue retains the original reference without copying: do not modify or recycle it immediately after sending.

Listener exceptions are logged; other listeners continue. Use a return value or completion callback to confirm critical operations.

</details>

Next: [UI display, closing and subscription lifetime](ui.md).
