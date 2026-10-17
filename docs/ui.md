# UI

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](ui.zh-CN.md)

**Give a window a name, then open or close it from your game code. UIManager handles loading and cleanup.**

## Open a window

First complete [Quick start](quick-start.md), or create your own [UI table](ui-tables.md). Register it once after Core is ready. The example assumes `MyGame.GameUI.Menu` already exists.

Use `using Core; using UI;`. Put this in a button's click handler, or in your startup `Start()` after RegisterAll, when you want to show the menu:

```csharp
GameCore.UI.OpenUI(UILayer.First, MyGame.GameUI.Menu, callback: view =>
{
    if (view == null)
    {
        return; // Failed or cancelled.
    }

    // The view is initialized and shown.
});
```

Use `First` for a normal screen and `Second` for a popup. `Transition` is reserved for the transition overlay.

## Close a window

| Where | Call |
| --- | --- |
| Game code | `GameCore.UI.CloseUI(MyGame.GameUI.Menu)` |
| Inside the view | `CloseSelf()` |
| Close current requests together | `GameCore.UI.CloseAll()` |

You can try closing the window, then opening it again. It should disappear and reappear. UIManager releases the closed instance for you.

## Put logic in the right hook

| Hook on your `UIBase` subclass | Use it for |
| --- | --- |
| `OnInit()` | Cache references and read inherited `Data` |
| `OnShow()` | Bind buttons and subscribe to events |
| `OnClose()` | Remove those bindings and subscriptions |
| `OnUpdate()` | Per-frame logic while shown; Unity drives it |

A saved prefab needs **one UIBase-derived component on its root**. Its root is a UI RectTransform; the shared Core provides the parent Canvas and EventSystem.

<details>
<summary>Request behavior and manual registration</summary>

Duplicate opens share a pending load; an already-open window returns its current instance without another OnShow. The first request determines layer and data. Failed/cancelled callbacks receive null; directly bound methods on destroyed Unity targets are skipped.

CloseUI also cancels queued/loading requests and releases late instances. IsOpen includes queued/loading states, so it does not mean visible. CloseAll operates on the initial request snapshot; explicitly opened requests from close hooks/callbacks survive.

Close, Release and CloseSelf settle the owning request. There is no window pool. OnClose must tolerate partial OnShow setup. If overriding OnDestroy, call base.OnDestroy() after your cleanup. Do not manually call Update.

Projects may use manual registration instead of a generated table:

```csharp
GameCore.UI.RegisterUiData(new UIData("MyGame.Menu",
    "Assets/GameContent/MyGame/Content/Prefabs/UI/MyMenuView.prefab"));
```

Register after Core is ready. Empty IDs/addresses and duplicate IDs are rejected; do not also register the same IDs with RegisterAll. Low-level fades are for separately coordinated UI work; use the scene API for a scene transition.

</details>

Next: [UI tables](ui-tables.md) to add another window, or [events](events.md) to connect it to gameplay.
