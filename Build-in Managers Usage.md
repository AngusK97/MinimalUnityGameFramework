# 01 UIManager

To add a new ui:

Each active, enabled, shown view receives its update from Unity through `UIBase.Update()` and its `OnUpdate()` hook. Do not call a view's `Update()` manually. `UIManager.OnUpdate()` starts pending opens and cleans records whose instances were externally destroyed.

Duplicate opens share a pending load; once open, they synchronously return the existing view without calling `OnInit` or `OnShow` again. The first request determines the layer and data. An accepted callback receives the view on success or `null` on failure/cancellation; callbacks targeting destroyed Unity objects are skipped. Always handle `null`.

`CloseUI` cancels queued/loading requests as well as closing visible views; late results are released. `IsOpen` includes queued/loading requests and does not mean the view is visible. `CloseAll` closes requests present at the start of the call; requests explicitly opened by close hooks/callbacks are new requests and survive.

Use `OnInit` for references, `OnShow` to subscribe, and `OnClose` to unsubscribe. `OnClose` also cleans a partially completed `OnShow`, so cleanup must tolerate partial setup. Managed `Close()`, `Release()`, and `CloseSelf()` all close the owning request and release its instance. If overriding `OnDestroy`, call `base.OnDestroy()` after your own cleanup. Closing releases the instance; this is not a window pool.

1. create a ui script derived from `UIBase` class which provides basic lifecycle methods.
	<img src="Attachments/UIScript.png" alt="FrameworkUML" width="430"/>
2. create a ui prefab and attach the ui script to it.
	<img src="Attachments/UIPrefab.png" alt="FrameworkUML" width="870"/>
3. make sure the Addressable button on the ui prefab is toggled
	<img src="Attachments/UISetting.png" alt="FrameworkUML" width="670"/>
4. add a name for the new ui in `UIName.cs`
	<img src="Attachments/UIName.png" alt="FrameworkUML" width="230"/>
5. register the new ui in `UIManager.Init()` using `RegisterUiData`.
	<img src="Attachments/RegisterUI.png" alt="FrameworkUML" width="870"/>

Then, in logic code, we can use these APIs to control the ui:
```
// open Menu panel at first layer
GameCore.UI.OpenUI(UILayer.First, UIName.Menu, callback: view =>
{
    if (view == null) return; // failed or cancelled
    // The view is initialized and shown.
});

// close Menu panel
GameCore.UI.CloseUI(UIName.Menu);

// close all panels
GameCore.UI.CloseAll();

// fade out and fade in
GameCore.UI.TransitionFadeOut(() =>
{
	// load new scene and resources and balabala... 
	GameCore.UI.TransitionFadeIn();
});
```

# 02 EventManager
Every event should has a name in `EventName.cs`

`DispatchNow` notifies a snapshot of listeners in registration order. Subscribing or unsubscribing during a callback affects subsequent dispatches, including nested dispatches, while the current snapshot remains unchanged. Repeated registrations are retained; one `UnRegisterEvent` removes one registration.

`Dispatch` queues a notification for the next `GameCore` event update. Notifications added during that update wait for the following update. Re-entering `OnUpdate` from a callback does not flush them early. Each listener exception is logged and the remaining notifications continue; use ordinary methods for operations that must return success or failure.

<img src="Attachments/EventName.png" alt="FrameworkUML" width="270"/>

Then, in logic code, we can use events through these APIs:
```
// register event
GameCore.Event.RegisterEvent(EventName.OnPhaseChanged, OnPhaseChanged);

// unregister event
GameCore.Event.UnRegisterEvent(EventName.OnPhaseChanged, OnPhaseChanged);

// broadcast event
GameCore.Event.DispatchNow(this, EventName.OnPhaseChanged);
```

# 03 ResourceManager

<img src="Attachments/ResourceManager.png" alt="FrameworkUML" width="700"/>

ResourceManager registers all the resouce modules at `GameCore_Resource.cs`

<img src="Attachments/RegisterEventModule.png" alt="FrameworkUML" width="550"/>

Every resource module registers its own resources at its method `RecordAllResourceInfos()`

<img src="Attachments/RegisterResource.png" alt="FrameworkUML" width="1000"/>

Then, we can get access to resources like this:
```
// get a resource module from the ResourceManager
var resourceModule = GameCore.Resource.GetResourceModule<MainResourceModule>(ResourceModuleName.Main);

// get a resource from resource the module
var playerPrefab = resourceModule.GetRes<GameObject>(MainResource.PlayerObj);

// use the resource
var playerObj = Instantiate(playerPrefab);
...
```

# 04 SceneManager
```
GameCore.Scene.LoadSceneAsync("Assets/GameResources/Scenes/MenuScene.scene").Completed += handle =>  
{  
    if (handle.Status != AsyncOperationStatus.Succeeded)  
    {
	    return;  
    }
    
	// init the scene...
};
```

# 05 SoundManager
```
// play a AudioClip at Background layer, with volume = bgmVolume, pitch = 1f and isLoop = true
GameCore.Sound.Play(SoundLayer.Background, bgmClip, bgmVolume, 1f, true);

// stop all sounds in a layer
GameCore.Sound.StopSound(SoundLayer.Background);
```
