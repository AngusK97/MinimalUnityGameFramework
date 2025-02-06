# 01 UIManager
Firstly, every ui should has a name in `UIName.cs`

<img src="Attachments/UIName.png" alt="FrameworkUML" width="270"/>

Secondly, every ui should be registered at `GameCore_View.RegisterAllViewModules()`

<img src="Attachments/RegisterUI.png" alt="FrameworkUML" width="870"/>

Then, in logic code, we can use these APIs to control ui
```
// open Menu panel at first layer, 
GameCore.UI.OpenUI(UILayer.First, UIName.Menu);

// close Menu panel
GameCore.UI.CloseUI(UIName.Menu);

// close all panels
GameCore.UI.CloseAll();

// fade out and fade in
GameCore.UI.TransitionFadeOut(() =>
{
	// load new scene and resources, then
	GameCore.UI.TransitionFadeIn();
});
```

# 02 EventManager
Every event should has a name in `EventName.cs`

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