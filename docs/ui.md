# UI

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](ui.zh-CN.md)

**Open or close a named window. UIManager loads and releases its instance.**

<a id="open-a-window"></a>

## 01 Open a window

This example uses the `Menu` window from [Quick start](quick-start.md). If you followed [UI tables](ui-tables.md) instead, replace `Menu` with `Shop` in the open and close examples below.

In **MyGameEntry.cs**, replace the existing OpenUI call in `Start()` with this code, after `GameConfig.LoadAndRegister()`. Load config once after Core starts; do not add a second config call. You can also open the window from a button handler after startup (`using Core; using UI;`):

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

<a id="close-a-window"></a>

## 02 Close a window

| Where | Call |
| --- | --- |
| Game code | `GameCore.UI.CloseUI(MyGame.UIName.Menu)` |
| Inside the view | `CloseSelf()` |
| Close all, including loading windows | `GameCore.UI.CloseAll()` |

**Expected result:** close and reopen the window. It disappears, its instance is released, then a new instance appears.

<a id="put-logic-in-the-right-hook"></a>

## 03 Put logic in the right hook

| Method on your `UIBase` subclass | Use it for |
| --- | --- |
| `OnInit()` | Cache references and read inherited `Data` |
| `OnShow()` | Bind buttons and subscribe to events |
| `OnClose()` | Remove those bindings and subscriptions |
| `OnUpdate()` | Per-frame logic while shown; Unity drives it |

Save the prefab with **one UIBase-derived component on its RectTransform root**. Core provides the parent Canvas and EventSystem.

<details>
<summary>Request behavior and manual registration</summary>

If you open the same window while it is loading, the calls share that load. If it is already open, you get the existing instance without another OnShow. The first call sets its layer and data. A failed or cancelled open calls back with null. If a callback is bound directly to a method on a destroyed Unity object, it is skipped.

CloseUI also cancels queued or loading windows. If an instance finishes loading after cancellation, it is released. IsOpen returns true for queued and loading windows too, so it does not tell you whether a window is visible. CloseAll handles the requests that exist when you call it. Windows opened from a close method or callback stay open.

Close, Release and CloseSelf all end the window’s open request. Windows are released, not pooled. OnClose must also work if OnShow only finished some of its setup. If you override OnDestroy, call base.OnDestroy() after your cleanup. Do not manually call Update.

For manual registration instead of a generated table:

```csharp
GameCore.UI.RegisterUiData(new UIData("Menu",
    "Assets/GameContent/MyGame/Content/Prefabs/UI/MyMenuView.prefab"));
```

Register after Core is ready. Empty IDs and duplicate IDs are rejected; do not register the same IDs again through GameConfig. An empty address is a placeholder: OpenUI reports failure and calls back with null. If you call the low-level fades yourself, you need to coordinate them. Use the scene API for scene transitions.

</details>

Next: [UI tables](ui-tables.md) to add a window, or [events](events.md) to connect gameplay.
