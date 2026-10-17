# 资源

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](resources.md)

加载一次共用音效，再交给游戏使用。资源模块会保留已加载的资产，由所有者决定何时卸载。

## 试试看

只使用 UI 时可跳过本页，UIManager 已负责窗口加载。需要共用音效时，先把自己的 **click.wav** 放到 `Assets/GameContent/MyGame/Content/Audio/`；现有 GameContent 父目录登记会提供完整路径地址。

在 `Assets/GameContent/MyGame/Scripts/` 中创建 **GameAssets.cs**，用下面代码替换整个文件：

```csharp
using Resource;
using UnityEngine;

namespace MyGame
{
    public sealed class GameAssets : BaseResourceModule
    {
        public const string Id = "MyGame.Assets";

        public override string GetName()
        {
            return Id;
        }

        protected override void RecordAllResourceInfos()
        {
            AddResourceInfo(ResourceType.AudioClip, 1,
                "Assets/GameContent/MyGame/Content/Audio/click.wav");
        }

        public AudioClip Click => GetResource<AudioClip>(1);
    }
}
```

在自己的游戏启动方法中，确认 `GameCore.IsReady` 后登记一次。以下片段需要 `using Core; using Sound; using UnityEngine;`：

```csharp
var assets = new MyGame.GameAssets();
GameCore.Resource.RegisterResourceModule(assets);

assets.LoadResources(
    loadedCallback: () =>
    {
        GameCore.Sound.PlaySound(SoundLayer.UI, assets.Click);
    },
    failedCallback: () =>
    {
        Debug.LogError("Game resources failed to load or were cancelled.");
    });
```

Resource 服务会保存已登记的模块，启动方法返回后模块仍在。其他使用者通过 `GameCore.Resource.GetResourceModule<MyGame.GameAssets>(MyGame.GameAssets.Id)` 取得它，再从 `Click` 属性借用。`GetResource<T>` 是模块的受保护方法，不直接向外部暴露。

之后，游戏中的资源所有者可以在清理方法里使用以下代码，此时 Core 仍需就绪。等所有借用者用完、依赖实例销毁后再执行。这个例子只管理一组 UI 音效，`StopSound` 会停止整个 UI 层；单个借用者不要自行卸载共用模块：

```csharp
var assets = GameCore.Resource.GetResourceModule<MyGame.GameAssets>(MyGame.GameAssets.Id);
GameCore.Sound.StopSound(SoundLayer.UI);
assets?.UnloadResources();
```

## 使用约定

- 模块 ID 是游戏定义的非空字符串；模块内资源编号是 int，可用自己的枚举管理。
- 只在加载成功后借用资产，失败走 `failedCallback`；不要刚发起加载就读取 `Click`。
- 等借用者用完、相关音频停播、依赖实例销毁后，由所有者调用 `assets.UnloadResources()`。不要播放后立即卸载。
- 资源地址使用实际 Addressables Key；修改资源或目标平台后，重新构建对应内容。

<details>
<summary>需要时再看：共享批次、取消与卸载</summary>

并发 `LoadResources` 共享同一批加载，整组成功后资源才可用；已经就绪时，成功回调可能同步执行。`GetResource<T>` 在未就绪、编号不存在或类型不匹配时返回默认值。

`UnloadResources` 立即使该批失效，并取消整组等待，不是取消某个借用者自己的订阅。迟到结果自行释放，不影响后续新批次。覆盖卸载方法时调用 `base.UnloadResources()`。

重复模块 ID 会拒绝覆盖，不替换或卸载已登记模块。自己的游戏决定共享模块跨场景存活多久，不要每次切场景无条件卸载。

</details>

<details>
<summary>需要时再看：直接加载与句柄归属</summary>

`LoadAssetAsync<T>` 返回的句柄交给调用者持有；使用完调用 `Release(handle)`，失败结果也要释放。复制句柄不会增加持有次数，不要同时释放同一持有的资产和句柄。

`InstantiateGameObjectAsync` 把操作交给有效完成回调，接收方最终调用 `ReleaseInstance(handle)`。没有回调或直接绑定的 Unity 目标已销毁时，工具释放结果。

回调抛异常后，操作仍由接收方持有；工具记录异常，不重复通知或释放接收方可能已经保存的句柄。借用资产不等于获得一份独立持有，不能代替模块卸载它。

</details>

下一步：[把已加载的音效交给音频层播放](audio.zh-CN.md)。
