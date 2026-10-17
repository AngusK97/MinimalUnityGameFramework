# Quick start

[Overview](../README.md) · [Build](build.md) · [简体中文](quick-start.zh-CN.md)

Let's start with something you can see: **run the Demo, then make a menu of your own**.

Have Unity Hub, a Unity Editor and a C# editor ready. These steps assume basic Unity use; they introduce the framework as we go. In Unity, **Project** holds files, **Hierarchy** shows objects in the current scene, and **Inspector** shows the selected object's settings.

## Run the Demo

1. Clone or download this repository. In **Unity Hub → Add / Open**, select the folder containing Assets, Packages and ProjectSettings. Choose **Unity 6.6 (6000.6.5f1)** for this baseline. Wait for package resolution, import and compilation to finish. Keep older projects on their own pinned version unless you are deliberately [upgrading a copy](upgrading.md).
2. Before pressing Play, open **Window → Asset Management → Addressables → Groups**. Set **Play Mode Script → Use Asset Database (fastest)** so the Editor reads assets directly.
3. In Project, open `Assets/GameContent/Demo/Scenes/DemoStart.unity`. Press Play; you should see the MINIMAL LOOP menu.
4. Choose Start and move with WASD/arrows to GOAL. Play again restarts; Menu returns; Settings/Escape opens volume settings; Done/Escape saves and closes.
5. Press Unity's **Play button again to stop** before editing the project below.

**Success:** the MINIMAL LOOP menu appears and the example level runs. Demo settings pause its movement/timer, rather than providing a full-game pause system.

## Create your first window

I'll use `MyGame` for the example. Keep these names on your first pass so you can copy the code as it is: **view → prefab → table → generate → startup**.

### 1. Create the view

In Project, right-click `Assets/GameContent` → **Create → Folder**, and name it **MyGame**. Create the folders below inside it; slashes mean nested folders:

```text
Scenes/
Content/Prefabs/UI/
Editor/Tables/
Scripts/
```

In your new Scripts folder, right-click **Create → Scripting → MonoBehaviour Script**, name it **MyMenuView**, then double-click it. Replace the **whole file** with this code, save, and return to Unity. Wait for compilation:

```csharp
using UI;
using UnityEngine;

namespace MyGame
{
    public sealed class MyMenuView : UIBase
    {
        protected override void OnShow()
        {
            Debug.Log("MyGame menu opened.", this);
        }
    }
}
```

### 2. Save a UI prefab

1. Use **File → New Scene** to make a temporary scene for building the UI. Choose an empty template, then **GameObject → UI → Panel**. Unity creates a Canvas with a Panel under it.
2. Rename the **Panel** to MyMenuView. Select it in Hierarchy, then drag **MyMenuView.cs from Project into its Inspector** to attach the script. Keep only this one UIBase-derived component on the Panel.
3. With that Panel selected, choose **GameObject → UI → Legacy → Text**. Set the Text to `My first menu`; adjust its position and color so you can see it inside the Panel.
4. Drag **MyMenuView from Hierarchy** into `MyGame/Content/Prefabs/UI/` in Project. You should now see **MyMenuView.prefab** there. Drag the Panel, not the outer Canvas; keep cameras and EventSystem out of the prefab.

The Panel is the prefab's **root**: its top-level object. It has a RectTransform because it is UI. The Canvas you used here is just for authoring; the real entry scene below gets its Canvas from Core.

### 3. Generate the UI table

In Project, right-click `MyGame/Editor/Tables/` → **Create → Framework → UI Table**. Keep the name **UI** (Unity saves it as UI.asset). Select it, expand **Entries** and click **+** to add one row. Fill the Inspector:

| Field | Value |
| --- | --- |
| Namespace | `MyGame` |
| Class Name | `GameUI` |
| Id Prefix | `MyGame` |
| Entries | One row: Name=`Menu`, Prefab=your saved MyMenuView root component |

Drag **MyMenuView.prefab from Project** into that row's **Prefab** field, not the object still in Hierarchy. Click **Validate and Generate** and wait for compilation. You should see `MyGame/Scripts/Generated/GameUI.g.cs` and the status **Generated code is up to date.**

**Generate first.** The next script refers to GameUI and cannot compile before that class exists. UI.asset stays in Editor/Tables; it is not a runtime scene component.

### 4. Add your game entry

Once GameUI.g.cs exists, use **Create → Scripting → MonoBehaviour Script** in your Scripts folder again, name it **MyGameEntry**, and replace its whole contents with this:

```csharp
using Core;
using UI;
using UnityEngine;

namespace MyGame
{
    public sealed class MyGameEntry : MonoBehaviour
    {
        private void Start()
        {
            if (!GameCore.IsReady)
            {
                Debug.LogError("Add the shared GameCore prefab to the entry scene.", this);
                return;
            }

            GameUI.RegisterAll();
            GameCore.UI.OpenUI(UILayer.First, GameUI.Menu, callback: view =>
            {
                if (view == null)
                {
                    Debug.LogError("Menu failed to open. Check the earlier Console errors.");
                }
            });
        }
    }
}
```

1. Use **File → New Scene** again and choose an empty template. If it contains a default Camera/Light, delete those default objects so this new Hierarchy is empty. You can discard the temporary authoring scene once its prefab has been saved.
2. Save this scene as `Assets/GameContent/MyGame/Scenes/Start.unity`.
3. Find [GameCore.prefab](../Assets/GameResources/Prefab/GameCore.prefab) in Project. Drag it into an empty area of Hierarchy, with no parent object. It already has the needed Canvas layers, EventSystem and AudioListener.
4. In Hierarchy, right-click **Create Empty**, name it **GameEntry**, and drag **MyGameEntry.cs from Project into its Inspector**. GameEntry and GameCore should be beside each other, not one inside the other.
5. Save and press Play.

**Success:** you see `My first menu`, and Console prints `MyGame menu opened.` without red errors. Noooooice 🎉 That's your own window running through the framework.

If it does not appear, stop Play and check these three things: GameCore is in the scene, GameEntry has MyGameEntry attached, and UI.asset says the generated code is up to date. [Build & troubleshoot](build.md) covers other common errors.

## Remove the Demo

Delete `Assets/GameContent/Demo` in Unity's Project window. For disk deletion, close Unity first and remove the folder **and Demo.meta**. Keep GameContent, its `.gitkeep`, the shared framework and your own game directory.

Run your own Start.unity again. Your game should use its own IDs/content instead of DemoUI, DemoGame or Demo assets. The shared `Assets/GameResources/Scenes/InitScene.unity` remains a services-only entry and intentionally opens no menu.

## Next steps

[Add another UI](ui-tables.md) · [Learn window hooks](ui.md) · [Build your game](build.md)
