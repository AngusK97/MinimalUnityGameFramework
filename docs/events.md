# Events

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](events.zh-CN.md)

**Define a name → Subscribe → Broadcast → Unsubscribe.** Notify a window without gameplay holding it. Use a direct call when you need a result.

## Try it

Save **GameEvents.cs** in `Assets/GameContent/MyGame/Scripts/`. Names belong to your game: no enum, event table or advance registration is needed.

```csharp
namespace MyGame
{
    public static class GameEvents
    {
        public const string LevelCompleted = "MyGame.LevelCompleted";
    }
}
```

Save **WinNotice.cs** beside it. After Core is ready, the window subscribes when shown and unsubscribes from the **same manager instance** when closed:

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

Attach WinNotice as the only UIBase-derived component on a separate window prefab root, then [register and open it](ui.md). Do not add it beside MyMenuView.

While that window is open, send this from a button handler or level-completion method (`using Core; using MyGame;`), on Unity's main thread:

```csharp
GameCore.Event.DispatchNow(GameEvents.LevelCompleted);
```

**Expected result:** each broadcast prints `Level completed.` once while the window is open; after closing, it produces no output. RegisterEvent subscribes a listener, not an event-name definition.

## Choose the notification

| Action | Call |
| --- | --- |
| Subscribe a parameterless method | `RegisterEvent(GameEvents.LevelCompleted, OnLevelCompleted)` |
| Remove that subscription | `UnRegisterEvent(GameEvents.LevelCompleted, OnLevelCompleted)` |
| Notify immediately | `DispatchNow(GameEvents.LevelCompleted)` |
| Notify at the next event update | `Dispatch(GameEvents.LevelCompleted)` |

- Use game-owned string constants. IDs must be non-empty; handlers cannot be null.
- Use services after Core is ready, on Unity's main thread. Core drives the event update; do not call OnUpdate yourself.
- Subscribe for the receiver's active lifetime and unsubscribe from the saved manager. Keep a lambda delegate if you need to remove it later; a new lambda is a different listener.
- Events have **no history replay**. A window opened later reads current game state first, then listens for future changes.

## Before and after

The original sender/data API remains available; use the shorter signature when you need neither:

| | Sender/data listener | Notification-only listener |
| --- | --- | --- |
| Handler | `void OnLevelCompleted(object sender, string type, BaseEventArgs args)` | `void OnLevelCompleted()` |
| Send | `GameCore.Event.DispatchNow(this, GameEvents.LevelCompleted)` | `GameCore.Event.DispatchNow(GameEvents.LevelCompleted)` |

Registration and removal keep the same method names. Retain the original `HandlerEvent` signature when a receiver needs a source or payload: Demo's [LevelView.cs](../Assets/GameContent/Demo/Scripts/UI/LevelView.cs) filters completion by the active level's sender.

Demo also uses a parameterless `DemoEvents.VolumeChanged` notification: [DataManager.cs](../Assets/GameContent/Demo/Scripts/DataManager.cs) broadcasts after a volume change; [SettingsView.cs](../Assets/GameContent/Demo/Scripts/UI/SettingsView.cs) refreshes current state when shown, listens for changes and unsubscribes when closed.

<details>
<summary>When needed: ordering, data, exceptions and cleanup</summary>

Parameterless Action and three-parameter HandlerEvent listeners share registration order and one dispatch snapshot. Subscription changes affect later dispatches, including nested ones, not the current snapshot. Duplicates are separate registrations; one UnRegisterEvent removes one occurrence. Parameterless dispatch gives HandlerEvent listeners null sender/data; Action listeners ignore sender/data from the original overload.

Each event update takes the pending queue at its start. Events queued during processing wait for the next batch; re-entering OnUpdate does not flush them early.

For data, derive BaseEventArgs, implement Clear(), and send `DispatchNow(sender, eventId, args)` or `Dispatch(sender, eventId, args)`. The deferred queue keeps the original reference without copying or automatically calling Clear(). Do not modify, clear or recycle it before receivers finish.

Exceptions are logged and the next registered listener continues. A multicast delegate supplied in one registration is one entry: if one invocation throws, the rest of that entry is skipped. Destroyed direct Unity targets are skipped; valid invocation targets in the same entry still run unless an earlier invocation throws. Mixed multicast entries retain their original delegate references until removed. Closures still need explicit unsubscription; target checks do not make captured objects safe.

Manager destruction clears subscriptions/queued events and stops remaining callbacks. Registering or dispatching through an old, disposed instance throws ObjectDisposedException; UnRegisterEvent remains safe for cleanup. With the new overloads, invalid-input tests using a bare null handler must cast it to Action or HandlerEvent explicitly. This does not turn events into transactions: use return values or completion callbacks to confirm critical operations.

</details>

Next: [UI display, closing and subscription lifetime](ui.md).
