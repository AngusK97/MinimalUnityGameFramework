# Events

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](events.zh-CN.md)

**Define a name → Subscribe → Broadcast → Unsubscribe.** Send a notification without knowing who listens. This example prints a message in Console; it needs no UI or configuration table.

<a id="try-it"></a>

## 01 Try it

<a id="1-prepare-the-scene"></a>

### 1.1 Prepare the scene

Use this repository's Unity project. Create an empty scene, drag [GameCore.prefab](../Assets/Framework/Prefabs/GameCore.prefab) into Hierarchy at the top level, and keep its settings. Create an empty GameObject named **EventExample**.

<a id="2-define-the-name"></a>

### 1.2 Define the name

Save **GameEvents.cs** in `Assets/GameContent/MyGame/Scripts/` (create the folders if needed). The string constant is the event's name; no enum, table or advance registration is needed.

```csharp
namespace MyGame
{
    public static class GameEvents
    {
        public const string LevelCompleted = "MyGame.LevelCompleted";
    }
}
```

<a id="3-subscribe-and-broadcast"></a>

### 1.3 Subscribe and broadcast

Save **EventExample.cs** beside it, then attach it to the EventExample object:

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

Press Play. **Console should print `Level completed.` once.** Start subscribes before broadcasting, so the first notification cannot be missed. GameCore initializes in Awake; a different object's OnEnable may run before that, so this example waits for Start and checks readiness.

<a id="4-broadcast-from-your-game"></a>

### 1.4 Broadcast from your game

In a level-completion method or another game script, use this line after Core is ready (`using Core; using MyGame;`):

```csharp
GameCore.Event.DispatchNow(GameEvents.LevelCompleted);
```

Each broadcast invokes the listener once. The sender only needs the event name, not the EventExample object. RegisterEvent subscribes a listener; it does not define the name.

Destroying the EventExample object calls OnDestroy and removes its subscription. To check cleanup, broadcast from your game, delete EventExample in Hierarchy during Play, then broadcast again: its message should stop. Disabling the component alone does not unsubscribe it.

<a id="choose-the-notification"></a>

## 02 Choose the notification

| Action | Call |
| --- | --- |
| Subscribe a parameterless method | `RegisterEvent(GameEvents.LevelCompleted, OnLevelCompleted)` |
| Remove that subscription | `UnRegisterEvent(GameEvents.LevelCompleted, OnLevelCompleted)` |
| Notify immediately | `DispatchNow(GameEvents.LevelCompleted)` |
| Notify at the next event update | `Dispatch(GameEvents.LevelCompleted)` |

- Use game-owned string constants. IDs cannot be blank; handlers cannot be null.
- Use services after Core is ready, on Unity's main thread. Core drives the event update; do not call OnUpdate yourself.
- Subscribe when the receiver needs notifications; unsubscribe from the saved manager when it stops listening. Disabling a component does not unsubscribe it.
- Keep a lambda delegate if you need to remove it later; a new lambda is a different listener.
- Events have **no history replay**. A receiver created later must read current game state and subscribe to future changes.

<a id="before-and-after"></a>

## 03 Before and after

The original sender/data API remains available; use the shorter signature when you need neither:

| | Sender/data listener | Notification-only listener |
| --- | --- | --- |
| Handler | `void OnLevelCompleted(object sender, string type, BaseEventArgs args)` | `void OnLevelCompleted()` |
| Send | `GameCore.Event.DispatchNow(this, GameEvents.LevelCompleted)` | `GameCore.Event.DispatchNow(GameEvents.LevelCompleted)` |

Registration and removal keep the same method names. Retain the original `HandlerEvent` signature when a receiver needs a source or payload: Demo's [LevelView.cs](../Assets/GameContent/Demo/Scripts/UI/LevelView.cs) filters completion by the active level's sender.

<details>
<summary>Optional: connect events to UI</summary>

For a working UI example, see Demo's `DemoEvents.VolumeChanged`: [DataManager.cs](../Assets/GameContent/Demo/Scripts/DataManager.cs) broadcasts after a volume change; [SettingsView.cs](../Assets/GameContent/Demo/Scripts/UI/SettingsView.cs) subscribes in OnShow, reads current state with RefreshVolume(), and unsubscribes in OnClose. Subscribe before refreshing so later changes are covered. See [UI](ui.md) when you are ready to create a window.

</details>

<details>
<summary>When needed: ordering, data, exceptions and cleanup</summary>

Parameterless Action and three-parameter HandlerEvent listeners share registration order and one dispatch snapshot. Subscription changes affect later dispatches, including nested ones, not the current snapshot. Duplicates are separate registrations; one UnRegisterEvent removes one occurrence. Parameterless dispatch gives HandlerEvent listeners null sender/data; Action listeners ignore sender/data from the original overload.

Each event update takes the pending queue at its start. Events queued during processing wait for the next batch; re-entering OnUpdate does not flush them early.

For data, derive BaseEventArgs, implement Clear(), and send `DispatchNow(sender, eventId, args)` or `Dispatch(sender, eventId, args)`. The deferred queue keeps the original reference without copying or automatically calling Clear(). Do not modify, clear or recycle it before receivers finish.

Exceptions are logged and the next registered listener continues. A multicast delegate supplied in one registration is one entry: if one invocation throws, the rest of that entry is skipped. Destroyed direct Unity targets are skipped; valid invocation targets in the same entry still run unless an earlier invocation throws. Mixed multicast entries retain their original delegate references until removed. Closures still need explicit unsubscription; target checks do not make captured objects safe.

Manager destruction clears subscriptions/queued events and stops remaining callbacks. Registering or dispatching through an old, disposed instance throws ObjectDisposedException; UnRegisterEvent remains safe for cleanup. Use return values or completion callbacks to confirm critical operations.

</details>

Next: [UI display, closing and subscription lifetime](ui.md).
