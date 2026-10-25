# Events

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](events.zh-CN.md)

**Define a name → Subscribe → Broadcast → Unsubscribe.** Events let you send a notification without knowing who listens. Let's try it with a Console message; you won't need a UI or configuration table.

<a id="try-it"></a>

## 01 Try it

<a id="1-prepare-the-scene"></a>

### 1.1 Prepare the scene

Open the framework project and stop Play mode if it is running.

1. Create an empty scene and drag [GameCore.prefab](../Assets/Framework/Prefabs/GameCore.prefab) into Hierarchy at the top level. Keep its settings.
2. Create an empty GameObject named **EventExample**, beside GameCore.
3. Save the scene as `Assets/GameContent/MyGame/Scenes/EventExample.unity`. Create the folders if needed.

<a id="2-define-the-name"></a>

### 1.2 Define the name

In `Assets/GameContent/MyGame/Scripts/` (create it if needed), choose **Create → Scripting → MonoBehaviour Script** and name it **GameEvents**. Replace all of GameEvents.cs with the code below and save. This file just holds the event name, so it doesn't go on a GameObject. No enum, table or advance registration is needed.

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

Use the same script menu to create **EventExample.cs** beside GameEvents.cs. Replace the whole file with this code and save:

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

Wait for compilation, then drag **EventExample.cs** onto the EventExample object's Inspector. Save the scene and press Play.

**Success:** Console prints `Level completed.` once. In Start, we subscribe before broadcasting, so the listener is ready for the first notification. GameCore initializes in Awake, and another object's OnEnable may run before that. That's why this example waits for Start and checks that Core is ready.

<a id="4-broadcast-from-your-game"></a>

### 1.4 Broadcast from your game

To send the same notification from your game, put this line inside a level-completion method or another method that needs to send it. Core must be ready before you call it. Add `using Core;` and `using MyGame;` at the top of that script:

```csharp
GameCore.Event.DispatchNow(GameEvents.LevelCompleted);
```

In this example, each broadcast calls OnLevelCompleted once. You only need the event name to send it; you don't need a reference to the EventExample object. The name comes from the constant in GameEvents. RegisterEvent tells the framework which method to call when that event is sent.

When you destroy the EventExample object, OnDestroy removes its subscription. Once your game sends this notification, try broadcasting once, deleting EventExample in Hierarchy during Play, then broadcasting again. Its message should stop. Disabling the component alone does not unsubscribe it.

<a id="choose-the-notification"></a>

## 02 Choose the notification

| Action | Call |
| --- | --- |
| Subscribe a parameterless method | `RegisterEvent(GameEvents.LevelCompleted, OnLevelCompleted)` |
| Remove that subscription | `UnRegisterEvent(GameEvents.LevelCompleted, OnLevelCompleted)` |
| Notify immediately | `DispatchNow(GameEvents.LevelCompleted)` |
| Notify at the next event update | `Dispatch(GameEvents.LevelCompleted)` |

- Define event IDs as string constants in your game. IDs cannot be blank; handlers cannot be null.
- Use services after Core is ready, on Unity's main thread. Core drives the event update; do not call OnUpdate yourself.
- Subscribe when the receiver needs notifications; unsubscribe from the saved manager when it stops listening. Disabling a component does not unsubscribe it.
- If you'll need to unsubscribe a lambda later, save the delegate. Writing a new lambda creates a different listener.
- Events **don't replay past notifications**. A receiver created later needs to read the current game state and subscribe to future changes.

<a id="before-and-after"></a>

## 03 Before and after

You can still send a source and data with the original API. If the receiver doesn't need either, use the shorter, parameterless form:

| | Sender/data listener | Notification-only listener |
| --- | --- | --- |
| Handler | `void OnLevelCompleted(object sender, string type, BaseEventArgs args)` | `void OnLevelCompleted()` |
| Send | `GameCore.Event.DispatchNow(this, GameEvents.LevelCompleted)` | `GameCore.Event.DispatchNow(GameEvents.LevelCompleted)` |

Both forms use the same methods to subscribe and unsubscribe. Keep the original `HandlerEvent` signature when a receiver needs a source or data. For example, the Demo's [LevelView.cs](../Assets/GameContent/Demo/Scripts/UI/LevelView.cs) uses sender to check that a completion notification came from the current level.

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
