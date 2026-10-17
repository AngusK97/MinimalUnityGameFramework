# Core

[Overview](../README.md) · [Quick start](quick-start.md) · [简体中文](core.zh-CN.md)

**Add one GameCore to your entry scene. It prepares the shared tools, then your own script starts the game.**

## Set it up

1. Find [GameCore.prefab](../Assets/GameResources/Prefab/GameCore.prefab) in Project and drag it into a blank area of Hierarchy. It should be at the top level, not inside another object. Keep the prefab's settings.
2. Create an empty object for your game entry and attach your startup script to it.
3. In that script's `Start()`, check `GameCore.IsReady`, then use the services. If you need a complete script to copy, start with [Quick start](quick-start.md).

Place this method in your existing startup class (`using Core; using UnityEngine;`):

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

For a complete startup class that opens a window, follow [Quick start](quick-start.md).

## What it owns

| Entry | Responsibility |
| --- | --- |
| `GameCore.UI` | Window requests, instances and UI layers |
| `GameCore.Event` | Immediate and queued notifications |
| `GameCore.Resource` | Asset access and registered resource modules |
| `GameCore.Scene` | Controlled scene transitions |
| `GameCore.Sound` | Layered audio playback |

Core assembles services in `Awake()`. Its `Update()` pumps UI requests and queued events. Unity updates each shown view through `UIBase.OnUpdate()`.

## A few things to keep in mind

- The shared prefab already provides Canvas layers, transition UI, EventSystem and an AudioListener. Keep one active set.
- Keep Core alive across levels. Restart gameplay through your own controller, rather than initializing Core again.
- Keep game rules, saves, UI IDs and navigation in your game directory. Core does not register a demo menu or choose a first scene.
- Its five manager references must belong to its own hierarchy. To end its lifetime, destroy the **whole Core root**.

**Expected result:** `GameCore.IsReady` is true. Opening the shared InitScene alone prepares services and shows no game menu.

Next: [UI](ui.md) or [the complete first-window tutorial](quick-start.md).
