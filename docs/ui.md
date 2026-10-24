# UI

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](ui.zh-CN.md)

**Open or close a named window. UIManager loads and releases its instance.**

## Open a window

Complete [Quick start](quick-start.md) or create a [UI table](ui-tables.md). Call `MyGame.GameConfig.LoadAndRegister()` once after Core is ready; this example uses `MyGame.UIName.Menu`.

With `using Core; using UI;`, add this to a button handler or startup `Start()` after `GameConfig.LoadAndRegister()`:

```csharp
GameCore.UI.OpenUI(UILayer.First, MyGame.UIName.Menu, callback: view =>
{
    if (view == null)
    {
        return; // Failed or cancelled.
    }

    // The view is initialized and shown.
});
```

Use `First` for a normal screen, `Second` for a popup. Reserve `Transition` for the transition overlay.

## Close a window

| Where | Call |
| --- | --- |
| Game code | `GameCore.UI.CloseUI(MyGame.UIName.Menu)` |
| Inside the view | `CloseSelf()` |
| Close current requests together | `GameCore.UI.CloseAll()` |

**Expected result:** close and reopen the window. It disappears, its instance is released, then a new instance appears.

## Put logic in the right hook

| Hook on your `UIBase` subclass | Use it for |
| --- | --- |
| `OnInit()` | Cache references and read inherited `Data` |
| `OnShow()` | Bind buttons and subscribe to events |
| `OnClose()` | Remove those bindings and subscriptions |
| `OnUpdate()` | Per-frame logic while shown; Unity drives it |

Save the prefab with **one UIBase-derived component on its RectTransform root**. Core provides the parent Canvas and EventSystem.

<details>
<summary>Request behavior and manual registration</summary>

Duplicate opens share a pending load; an open window returns its instance without another OnShow. The first request sets layer and data. Failed/cancelled callbacks receive null; directly bound methods on destroyed Unity targets are skipped.

CloseUI cancels queued/loading requests and releases late instances. IsOpen includes queued/loading states, not just visible windows. CloseAll handles its initial request snapshot; requests explicitly opened by close hooks/callbacks survive.

Close, Release and CloseSelf settle the owning request. There is no window pool. OnClose must tolerate partial OnShow setup. After your OnDestroy cleanup, call base.OnDestroy(). Do not manually call Update.

For manual registration instead of a generated table:

```csharp
GameCore.UI.RegisterUiData(new UIData("Menu",
    "Assets/GameContent/MyGame/Content/Prefabs/UI/MyMenuView.prefab"));
```

Register after Core is ready. Empty IDs and duplicate IDs are rejected; do not register the same IDs again through GameConfig. An empty address is a placeholder: OpenUI reports failure and calls back with null. Low-level fades require separate coordination; use the scene API for scene transitions.

</details>

Next: [UI tables](ui-tables.md) to add a window, or [events](events.md) to connect gameplay.
