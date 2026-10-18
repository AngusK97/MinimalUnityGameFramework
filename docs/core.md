# Core

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](core.zh-CN.md)

**GameCore prepares the shared services. Your startup script starts the game.**

## Set it up

1. Drag [GameCore.prefab](../Assets/Framework/Prefabs/GameCore.prefab) into the entry scene at Hierarchy's top level. Keep its settings.
2. Create an empty object and attach your startup script.
3. Check `GameCore.IsReady` in `Start()` before using services.

Add this method to your startup class (`using Core; using UnityEngine;`). For a complete class that opens a window, see [Quick start](quick-start.md).

```csharp
private void Start()
{
    if (!GameCore.IsReady)
    {
        Debug.LogError("Add the shared GameCore prefab to the entry scene.");
        return;
    }

    // Register your game's content and start its first screen here.
}
```

## What it owns

| Entry | Responsibility |
| --- | --- |
| `GameCore.UI` | Window requests, instances and UI layers |
| `GameCore.Event` | Immediate and queued notifications |
| `GameCore.Resource` | Asset access and registered resource modules |
| `GameCore.Scene` | Controlled scene transitions |
| `GameCore.Sound` | Layered audio playback |

Core assembles services in `Awake()` and pumps UI requests and queued events in `Update()`. Unity drives shown views through `UIBase.OnUpdate()`.

## A few things to keep in mind

- The prefab provides Canvas layers, transition UI, EventSystem and an AudioListener. Keep one active set.
- Keep Core across levels; restart gameplay through your controller, not by reinitializing Core.
- Keep rules, saves, UI IDs and navigation in your game directory. Your game chooses its first scene; Core registers no demo menu.
- All five managers must belong to Core's hierarchy. End its lifetime by destroying the **whole Core root**.

**Expected result:** `GameCore.IsReady` is true. The shared InitScene alone prepares services without a game menu.

Next: [UI](ui.md).
