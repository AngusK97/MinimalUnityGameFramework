# UI

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](ui.zh-CN.md)

**Open or close a named window. UIManager loads and releases its instance.**

<a id="open-a-window"></a>

## 01 Open a window

This example uses the `Menu` window from [Quick start](quick-start.md). If you followed [UI tables](ui-tables.md) instead, replace `Menu` with `Shop` in the open and close examples below.

In **MyGameEntry.cs**, replace the existing OpenUI call in `Start()` with this code, after `GameConfig.LoadAndRegister()`. Keep that config call only once per Core lifetime. You can also open the window from a button handler after startup (`using Core; using UI;`):

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

Duplicate opens share a pending load; an open window returns its instance without another OnShow. The first request sets layer and data. Failed/cancelled callbacks receive null; directly bound methods on destroyed Unity targets are skipped.

CloseUI cancels queued/loading requests and releases late instances. IsOpen includes queued/loading states, not just visible windows. CloseAll handles its initial request snapshot; requests explicitly opened by close hooks/callbacks survive.

Close, Release and CloseSelf all end the window’s open request. Windows are released, not pooled. OnClose must also work if OnShow only finished some of its setup. If you override OnDestroy, call base.OnDestroy() after your cleanup. Do not manually call Update.

For manual registration instead of a generated table:

```csharp
GameCore.UI.RegisterUiData(new UIData("Menu",
    "Assets/GameContent/MyGame/Content/Prefabs/UI/MyMenuView.prefab"));
```

Register after Core is ready. Empty IDs and duplicate IDs are rejected; do not register the same IDs again through GameConfig. An empty address is a placeholder: OpenUI reports failure and calls back with null. Low-level fades require separate coordination; use the scene API for scene transitions.

</details>

Next: [UI tables](ui-tables.md) to add a window, or [events](events.md) to connect gameplay.
