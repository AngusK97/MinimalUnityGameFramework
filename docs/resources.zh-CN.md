# 资源

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](resources.md)

通过资源模块加载共用资产，由所有者决定何时卸载。UIManager 自行加载窗口，只用 UI 可跳过本页。

## 试试看

把 **click.wav** 放到 `Assets/GameContent/MyGame/Content/Audio/`，现有 GameContent 父目录登记提供完整路径地址。

在 `Assets/GameContent/MyGame/Scripts/` 创建 **GameAssets.cs**：

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

在启动方法中检查 `GameCore.IsReady` 后登记一次（需要 `using Core; using Sound; using UnityEngine;`）：

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

**预期结果：** 加载成功后播放 click.wav；失败或取消时输出错误。

Resource 服务保留模块，启动方法返回后仍可通过 `GameCore.Resource.GetResourceModule<MyGame.GameAssets>(MyGame.GameAssets.Id)` 获取，再借用 `Click`。`GetResource<T>` 是受保护方法，对外提供具体类型的属性即可。

等所有借用者用完、依赖实例销毁后，由资源所有者在 Core 就绪时清理。`StopSound` 会停止整个 UI 层：

```csharp
var assets = GameCore.Resource.GetResourceModule<MyGame.GameAssets>(MyGame.GameAssets.Id);
GameCore.Sound.StopSound(SoundLayer.UI);
assets?.UnloadResources();
```

## 使用约定

- 模块 ID 是游戏定义的非空字符串；模块内资源编号是 int，可用本地枚举管理。
- 只在加载成功回调中或之后借用资产；失败用 `failedCallback` 处理。
- 共用模块仅由所有者卸载，先停相关音频、销毁依赖实例，不要卸载正在播放的 clip。
- 使用实际 Addressables Key；修改资源或目标平台后，重新构建对应内容。

<details>
<summary>需要时再看：共享批次、取消与卸载</summary>

并发 `LoadResources` 共享同一批，整组成功后资产才可用。已就绪时，成功回调可能同步执行。`GetResource<T>` 在未就绪、编号不存在或类型不符时返回默认值。

`UnloadResources` 立即使该批失效，取消整组等待。迟到结果自行释放，不影响新批次。重写时必须调用 `base.UnloadResources()`。

重复模块 ID 会被拒绝，不替换或卸载已有模块。哪些模块跨场景保留，由游戏决定。

</details>

<details>
<summary>需要时再看：直接加载与句柄归属</summary>

调用者持有 `LoadAssetAsync<T>` 返回的句柄，用完必须 `Release(handle)`，失败也要释放。复制句柄不增加持有次数；同一份持有不要同时释放资产和句柄。

`InstantiateGameObjectAsync` 将持有权交给有效完成回调，接收方最终调用 `ReleaseInstance(handle)`。没有回调，或直接绑定的 Unity 目标已销毁时，工具释放结果。

回调抛异常后仍持有该操作。工具记录异常，不重复通知，也不释放接收方可能已保存的句柄。借用模块资产不产生可独立释放的持有。

</details>

下一步：[把已加载的音效交给音频层播放](audio.zh-CN.md)。
