# Events

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](events.zh-CN.md)

**Define a name → Subscribe → Broadcast → Unsubscribe.** Notify a window without gameplay holding it. Use a direct call when you need a result.

## Try it

Complete [Quick start](quick-start.md) first, then exit Play mode. Use the same MyGame scene, UI table and game entry below.

### 1. Define the name

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

### 2. Add the listener

Save **WinNotice.cs** beside it. The window subscribes when shown and unsubscribes from the saved manager when closed:

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

### 3. Open the window and send

1. Create a separate UI prefab at `MyGame/Content/Prefabs/UI/WinNotice.prefab`. Attach WinNotice as the only UIBase-derived component on its RectTransform root. Select the saved prefab in **Project** and check **Addressable** in its Inspector.
2. In your Google Sheets **UI** tab, add **Id = WinNotice**, **Name = Win notice**, **PrefabPath = Assets/GameContent/MyGame/Content/Prefabs/UI/WinNotice.prefab**. Click **Framework → Config → Update Config** and wait for compilation. See [UI tables](ui-tables.md).
3. In **MyGameEntry.cs**, keep `GameConfig.LoadAndRegister()` once. Replace the existing OpenUI call after it with:

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

Press Play. **Console should print `Level completed.` once.** Opening is asynchronous; the callback sends after OnShow has subscribed.

From here, use this one line in your button handler or level-completion method (`using Core; using MyGame;`), after Core is ready and on Unity's main thread:

```csharp
GameCore.Event.DispatchNow(GameEvents.LevelCompleted);
```

Each broadcast prints once while the window is open. Close it with `GameCore.UI.CloseUI(UIName.WinNotice)`; later broadcasts produce no output. RegisterEvent subscribes a listener, not an event-name definition.

## Choose the notification

| Action | Call |
| --- | --- |
| Subscribe a parameterless method | `RegisterEvent(GameEvents.LevelCompleted, OnLevelCompleted)` |
| Remove that subscription | `UnRegisterEvent(GameEvents.LevelCompleted, OnLevelCompleted)` |
| Notify immediately | `DispatchNow(GameEvents.LevelCompleted)` |
| Notify at the next event update | `Dispatch(GameEvents.LevelCompleted)` |

- Use game-owned string constants. IDs must be non-empty; handlers cannot be null.
- Use services after Core is ready, on Unity's main thread. Core drives the event update; do not call OnUpdate yourself.
- Subscribe when the receiver needs notifications; unsubscribe from the saved manager when it stops listening. Disabling a component does not unsubscribe it.
- Keep a lambda delegate if you need to remove it later; a new lambda is a different listener.
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

Manager destruction clears subscriptions/queued events and stops remaining callbacks. Registering or dispatching through an old, disposed instance throws ObjectDisposedException; UnRegisterEvent remains safe for cleanup. Use return values or completion callbacks to confirm critical operations.

</details>

Next: [UI display, closing and subscription lifetime](ui.md).
