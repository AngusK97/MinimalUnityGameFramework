# 资源

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](resources.md)

**登记 → 加载 → 使用 → 卸载。** 资源模块统一管理共用的资源，什么时候加载和卸载由游戏决定。UIManager 会自行加载窗口，如果只用 UI，可以跳过本页。

<a id="试试看"></a>

## 01 试试看

先在 Google Sheets 的[资源表](resource-tables.zh-CN.md)中添加 Cue 行，填写 **Type = AudioClip**、**Module = Main**，再点击顶部工具栏左侧的 **Update Config**。工具会生成 `MyGame.MainResources`，ID 为 `MyGame.Main`。等编译完成后再继续。

在 `Assets/GameContent/MyGame/Scripts/` 创建 **MyResourceStartup.cs**，挂到一个空对象上，作为启动入口和资源的所有者。同一场景中还需要放入共用的 GameCore 预制体。如果 MyGameEntry 已经加载了配置，把本例合并到原来的入口中，确保 `GameConfig.LoadAndRegister()` 只执行一次：

```csharp
using Core;
using Sound;
using UnityEngine;

namespace MyGame
{
    public sealed class MyResourceStartup : MonoBehaviour
    {
        private MainResources main;

        private void Start()
        {
            if (!GameCore.IsReady)
            {
                Debug.LogError("Add the shared GameCore prefab to the entry scene.");
                return;
            }

            GameConfig.LoadAndRegister();

            main = new MainResources();
            GameCore.Resource.RegisterResourceModule(main);
            main.LoadResources(loadedCallback: PlayCue, failedCallback: OnLoadFailed);
        }

        private void PlayCue()
        {
            GameCore.Sound.PlaySound(SoundLayer.UI, main.Cue);
        }

        private void OnLoadFailed()
        {
            Debug.LogError("Main resources failed to load or were cancelled.");
        }

        private void OnDestroy()
        {
            if (main == null || !GameCore.IsReady)
            {
                return;
            }

            GameCore.Sound.StopSound(SoundLayer.UI);
            main.UnloadResources();
        }
    }
}
```

**预期结果：** 整个模块加载成功后，会播放 cue.wav；加载失败或被取消时，会输出错误。销毁这个资源所有者时，会停止整个 UI 音频层并卸载模块。在自己的游戏中，要等所有借用者用完资源、依赖这些资源的实例也销毁后，再卸载模块。因此，资源所有者至少要存活到这些使用者不再需要资源为止。

Resource 服务会保留已登记的模块，Start 结束后仍然可以访问。其他组件可以查找并保存同一个模块实例；查找本身不会加载资源（需要 `using Core;`）：

```csharp
var main = GameCore.Resource.GetResourceModule<MyGame.MainResources>(MyGame.MainResources.Id);
```

加载成功后，就可以在游戏逻辑中直接使用 `main.Cue`。加载前或卸载后，这个属性返回 null。卸载只释放资产，模块仍然保持登记，再次加载时继续使用同一个实例即可。每个 Core 生命周期只需登记一次，不用在切换场景或新增资源行时重复登记。

<a id="使用约定"></a>

## 02 使用约定

- 模块 ID 由游戏定义，确定后应保持稳定。生成的内部资源编号不能用作存档或网络通信中的标识。
- 生成的快照、清单、代码和 JSON 要一起提交，详见[资源配置表](resource-tables.zh-CN.md)。你也可以继续手写 BaseResourceModule 派生类。
- 等加载成功的回调执行时或执行后，再借用资产；加载失败由 `failedCallback` 处理。
- 共用模块只能由所有者卸载。卸载前，先停止相关音频，并销毁依赖这些资源的实例，不要卸载正在播放的 clip。
- 使用实际的 Addressables Key。修改资源或目标平台后，重新构建对应平台的资源内容。

<details>
<summary>需要时再看：共享批次、取消与卸载</summary>

同时调用多次 `LoadResources` 时，这些调用会共用同一批加载操作。只有整组资源都加载成功后，资产才可用。如果模块已经就绪，成功回调可能会同步执行。`GetResource<T>` 在资源未就绪、编号不存在或类型不符时返回默认值。

`UnloadResources` 会立即让当前批次失效，取消对整组资源的等待。之后才返回的加载结果会自行释放，不影响新的加载批次。重写这个方法时，必须调用 `base.UnloadResources()`。

重复登记会抛出异常，不会替换或卸载已有模块。查找未登记的 ID 会返回 null；如果 ID 已登记，但指定的模块类型不对，则会抛出 InvalidOperationException。切换场景时保留哪些模块，由游戏自己决定。

</details>

<details>
<summary>需要时再看：直接加载与句柄归属</summary>

调用 `LoadAssetAsync<T>` 后，你需要负责释放它返回的句柄：用完后调用 `Release(handle)`，加载失败也要释放。复制句柄不会多出一份需要释放的引用。同一次加载不要既释放资产，又释放句柄。

`InstantiateGameObjectAsync` 会通过有效的完成回调把结果交给接收方，由接收方在用完后调用 `ReleaseInstance(handle)`。如果没有回调，或回调直接绑定的 Unity 对象已销毁，工具会自行释放结果。

即使回调抛出异常，接收方仍然负责释放这次操作。工具只记录异常，不会再次通知，也不会释放接收方可能已经保存的句柄。从模块中借用资产不会增加一份独立引用，因此借用者不应单独释放它。

</details>

下一步：[把已加载的音效交给音频层播放](audio.zh-CN.md)。
