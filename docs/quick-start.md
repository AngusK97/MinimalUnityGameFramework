# Quick start

[Overview](../README.md) · [Build](build.md) · [简体中文](quick-start.zh-CN.md)

**Run the Demo, then make your first window.** You'll need Unity Hub, Unity 6.6 (6000.6.5f1), a C# editor and basic Unity knowledge.

Use the downloaded framework project as the starting point for your game. The steps below keep its packages and settings, and add your content under `GameContent/MyGame/`.

This page follows **v0.5.0**. For the fixed SO-based release, use the [v0.2.0 guide](https://github.com/AngusK97/MinimalUnityGameFramework/blob/v0.2.0/docs/quick-start.md).

<a id="run-the-demo"></a>

## 01 Run the Demo

1. [Download v0.5.0 (ZIP)](https://github.com/AngusK97/MinimalUnityGameFramework/archive/refs/tags/v0.5.0.zip) and extract it. In Unity Hub, open the folder containing Assets, Packages and ProjectSettings. Wait for import and compilation.
2. Open **Window → Asset Management → Addressables → Groups**. Set **Play Mode Script → Use Asset Database (fastest)**.
3. Open `Assets/GameContent/Demo/Scenes/DemoStart.unity` and press Play. Choose **Start** and move to GOAL with **WASD / arrows**.
4. **Play again** restarts; **Menu** returns; **Settings / Escape** opens volume settings; **Done / Escape** saves and closes them.
5. Press Play again to stop before editing.

**Success:** the MINIMAL LOOP menu and level run. Settings pause the Demo's movement/timer only. Its generated config is included; playing needs no Google account, key or network.

<a id="create-your-first-window"></a>

## 02 Create your first window

Use the names below on your first pass: **view → prefab → local Excel → Update Config → startup**.

You need a spreadsheet editor that can save `.xlsx` files. No Google account is needed. [Section 2.3](#23-connect-and-update-the-ui-table) creates your own UI workbook and connects it to Unity.

<a id="1-create-the-view"></a>

### 2.1 Create the view

Create these folders under `Assets/GameContent/MyGame/`:

```text
Scenes/
Content/Prefabs/UI/
Editor/Tables/Source/
Scripts/
```

In Scripts, choose **Create → Scripting → MonoBehaviour Script** and name it **MyMenuView**. Replace the whole file with this code, save and wait for compilation:

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

<a id="2-save-a-ui-prefab"></a>

### 2.2 Save a UI prefab

1. Use **File → New Scene** to create a temporary empty scene. Choose **GameObject → UI → Panel**.
2. Rename the Panel **MyMenuView**. Drag MyMenuView.cs into its Inspector; keep only one UIBase-derived component on it.
3. With the Panel selected, choose **GameObject → UI → Legacy → Text**. Set its text to `My first menu` and make it visible inside the Panel.
4. Drag the Panel from Hierarchy into `MyGame/Content/Prefabs/UI/` to save **MyMenuView.prefab**. Save the Panel, not the outer Canvas; keep cameras and EventSystem out of it. Core supplies these at runtime.
5. Select the saved **MyMenuView.prefab in Project** and check **Addressable** at the top of its Inspector. Keep the default address.

<a id="3-connect-and-update-the-ui-table"></a>

### 2.3 Connect and update the UI table

Create a blank workbook in your spreadsheet editor and rename its first sheet **UI**. Enter these three rows from A1: row 1 contains field names, row 2 contains types, and row 3 contains your first window.

| Id | Name | PrefabPath |
| --- | --- | --- |
| string | string | string |
| Menu | My first menu | Assets/GameContent/MyGame/Content/Prefabs/UI/MyMenuView.prefab |

1. Save the workbook as `Assets/GameContent/MyGame/Editor/Tables/Source/MyGame.xlsx`. Use literal values, without formulas, and remove unused blank sheets.
2. Open **Framework → Config**. Set **Game = MyGame**, **Namespace = MyGame**, and **Source = Local Excel**. Keep **Excel folder = Editor/Tables/Source**; **Choose folder…** lets you select the folder on disk if needed.
3. Click **Update Config**, then wait for the result and compilation. For later edits, save the workbook and use **Update Config** on the left side of Unity's top toolbar.

You can also copy the UI sheet from `Demo/Editor/Tables/Source/Demo.xlsx` into your workbook as a layout example. Replace its data rows with the Menu row above; your game uses its own prefab and generated code. For Google Sheets or source switching, see [Config setup](config-setup.md).

**Success:** the UI table updated and `Assets/GameContent/MyGame/Scripts/Generated/UIName.g.cs` contains `Menu = "Menu"`.

**Update before continuing:** the next script needs GameConfig and UIName. For later edits, use [UI tables](ui-tables.md).

<a id="4-add-your-game-entry"></a>

### 2.4 Add your game entry

Create **MyGameEntry.cs** in Scripts with the same script menu. Replace its whole contents with:

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

            GameConfig.LoadAndRegister();

            GameCore.UI.OpenUI(UILayer.First, UIName.Menu, callback: view =>
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

1. Create a new empty scene. Remove any default Camera/Light; discard the temporary authoring scene after saving the prefab.
2. Save as `Assets/GameContent/MyGame/Scenes/Start.unity`.
3. Drag [GameCore.prefab](../Assets/Framework/Prefabs/GameCore.prefab) into Hierarchy as a root object. It includes the Canvas layers, EventSystem and AudioListener.
4. Create an empty root object named **GameEntry** and attach **MyGameEntry.cs**. Keep it beside GameCore, not inside it.
5. Save and press Play.

**Success:** `My first menu` appears and Console prints `MyGame menu opened.` without red errors. Noooooice 🎉

![MyGame Start scene showing My first menu, GameEntry and the shared GameCore](images/first-window.png)

If it fails, check GameCore, the MyGameEntry attachment and the UI table's generation status. More checks: [Build & troubleshoot](build.md).

<a id="remove-the-demo"></a>

## 03 Remove the Demo

Delete `Assets/GameContent/Demo` in Unity's Project window. If deleting on disk, close Unity and remove **both the folder and Demo.meta**. Keep GameContent, `.gitkeep`, the shared framework and your game.

Run your own Start.unity again. It must use its own IDs and assets, with no Demo namespace, DemoGame or Demo asset references. The shared `Assets/Framework/Scenes/InitScene.unity` prepares services only; it does not open a menu.

<a id="next"></a>

## 04 Next

[Add a UI](ui-tables.md) · [Window lifecycle](ui.md) · [Build your game](build.md)
