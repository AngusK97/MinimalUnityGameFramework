# Core

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](core.zh-CN.md)

**GameCore prepares the shared services. Your startup script starts the game.**

<a id="set-it-up"></a>

## 01 Set it up

1. Drag [GameCore.prefab](../Assets/Framework/Prefabs/GameCore.prefab) into your entry scene as a root object in the Hierarchy. Keep the prefab's settings.
2. Create an empty object and attach your startup script.
3. Check `GameCore.IsReady` in `Start()` before using services.

Add this method inside your startup class, with `using Core; using UnityEngine;` at the top of the file. For a complete example that opens a window, see [Quick start](quick-start.md).

```csharp
private void Start()
{
    if (!GameCore.IsReady)
    {
        Debug.LogError("Add the shared GameCore prefab to the entry scene.");
        return;
    }

    // Load config, register content and start your first screen here.
}
```

If you use generated tables, first [generate the config](config.md), then call `MyGame.GameConfig.LoadAndRegister()` here. Call it once per Core lifetime, before creating resource modules, opening UI or requesting a scene. InitScene prepares services only; it does not load your game configuration.

<a id="what-it-owns"></a>

## 02 What it owns

| Entry | Responsibility |
| --- | --- |
| `GameCore.UI` | Window requests, instances and UI layers |
| `GameCore.Event` | Immediate and queued notifications |
| `GameCore.Resource` | Asset access and registered resource modules |
| `GameCore.Scene` | Controlled scene transitions |
| `GameCore.Sound` | Layered audio playback |

Core initializes its services in `Awake()` and processes UI requests and queued events in `Update()`. Unity updates visible windows through `UIBase.OnUpdate()`.

<a id="a-few-things-to-keep-in-mind"></a>

## 03 A few things to keep in mind

- The prefab provides Canvas layers, transition UI, EventSystem and an AudioListener. Keep one active set.
- Keep Core across levels; restart gameplay through your controller, not by reinitializing Core.
- Keep rules, saves, UI IDs and navigation in your game directory. Your game chooses its first scene; Core registers no demo menu.
- Keep all five managers in Core's hierarchy. When you no longer need Core, destroy the **whole Core root**.

**Expected result:** `GameCore.IsReady` is true. The shared InitScene alone prepares services without a game menu.

Next: [UI](ui.md).
