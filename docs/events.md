# Events

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](events.zh-CN.md)

Let a window hear that a level has finished, without making the gameplay code hold that window. Use a direct method call when you need a result back.

## Try it

Subscribe after Core is ready. Save this complete **WinNotice.cs** in `Assets/GameContent/MyGame/Scripts/`. It subscribes when its window opens and unsubscribes when it closes:

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

Use it as the only UIBase-derived component on a separate window prefab root, then [register and open that UI](ui.md). Do not add it beside MyMenuView on the same root.

Open that window first. Then send this notification from an existing button handler or your level-completion method, with `using Core;`:

```csharp
GameCore.Event.DispatchNow(this, MyGame.WinNotice.LevelCompleted);
```

**Expected result:** sending once while the window is open prints `Level completed.` once in Console. Close the window and send again; it no longer prints, because OnClose removed the subscription.

## Conventions

- Your game owns event IDs; prefer shared string constants. IDs cannot be empty and handlers cannot be null.
- `DispatchNow` sends immediately; `Dispatch` waits for Core's next event update. Usually choose one rather than sending both.
- The subscriber removes its own subscription. Do not register every frame or call `OnUpdate` manually; Core drives it.

<details>
<summary>When needed: ordering, deferred data and exceptions</summary>

Immediate dispatch uses a listener snapshot in registration order. Changes inside a callback affect later dispatches, including nested ones, but not the current snapshot. Duplicate subscriptions remain separate; one `UnRegisterEvent` removes one occurrence.

An event update takes the pending queue at its start. Deferred events added during processing wait for the next batch. Re-entering `OnUpdate` does not flush them early.

To carry data, derive `BaseEventArgs`, implement `Clear()`, and pass it as the third argument. The deferred queue holds the original reference: do not immediately modify or recycle it after sending. The framework does not copy it.

A listener exception is logged and other listeners continue. Do not infer a critical operation's success from an event; use an explicit return value or completion callback.

</details>

Next: [UI display, closing and subscription lifetime](ui.md).
