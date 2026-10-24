# 资源

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](resources.md)

**登记 → 加载 → 使用 → 卸载。** 资源模块持有共享资产，游戏决定其生命周期。UIManager 自行加载窗口，只用 UI 可跳过本页。

## 试试看

先在 Google Sheets 的[资源表](resource-tables.zh-CN.md)添加 Cue 行，填写 **Type = AudioClip**、**Module = Main**，点击 **Update Config**。工具会生成 `MyGame.MainResources`，ID 为 `MyGame.Main`；等待编译完成。

在 `Assets/GameContent/MyGame/Scripts/` 创建 **MyResourceStartup.cs**，将它作为启动入口挂到一个空对象上，同一场景还需放置共用 GameCore 预制体。如果 MyGameEntry 已加载配置，将本例合并进原入口，确保 `GameConfig.LoadAndRegister()` 只执行一次：

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

**预期结果：** 整个模块加载成功后播放 cue.wav，失败或取消时输出错误。移除所有者时会停止整个 UI 音频层并卸载模块。自己的游戏应等所有借用者用完、依赖实例销毁后再卸载；所有者需存活到这些使用者结束。

Resource 服务保留已登记的模块，Start 返回后仍可用。其他组件可以查找一次并保留同一个实例；查找不会加载资源（需要 `using Core;`）：

```csharp
var main = GameCore.Resource.GetResourceModule<MyGame.MainResources>(MyGame.MainResources.Id);
```

加载成功后，玩法直接使用 `main.Cue`。未加载或卸载后，属性返回 null。卸载释放资产，但保留模块登记；再次加载复用同一实例。每个 Core 生命周期登记一次，不随切场景或新增条目重复登记。

## 使用约定

- 模块 ID 由游戏定义并保持稳定；生成的内部资源编号不能作为存档/网络标识。
- 生成快照、清单、代码和 JSON 一起提交，见[资源配置表](resource-tables.zh-CN.md)；手写 BaseResourceModule 派生类仍可使用。
- 只在加载成功回调中或之后借用资产；失败用 `failedCallback` 处理。
- 共用模块仅由所有者卸载，先停相关音频、销毁依赖实例，不要卸载正在播放的 clip。
- 使用实际 Addressables Key；修改资源或目标平台后，重新构建对应内容。

<details>
<summary>需要时再看：共享批次、取消与卸载</summary>

并发 `LoadResources` 共享同一批，整组成功后资产才可用。已就绪时，成功回调可能同步执行。`GetResource<T>` 在未就绪、编号不存在或类型不符时返回默认值。

`UnloadResources` 立即使该批失效，取消整组等待。迟到结果自行释放，不影响新批次。重写时必须调用 `base.UnloadResources()`。

重复登记会抛异常，不替换或卸载已有模块。查找未登记 ID 返回 null；以错误模块类型查找已登记 ID 会抛出 InvalidOperationException。哪些模块跨场景保留，由游戏决定。

</details>

<details>
<summary>需要时再看：直接加载与句柄归属</summary>

调用者持有 `LoadAssetAsync<T>` 返回的句柄，用完必须 `Release(handle)`，失败也要释放。复制句柄不增加持有次数；同一份持有不要同时释放资产和句柄。

`InstantiateGameObjectAsync` 将持有权交给有效完成回调，接收方最终调用 `ReleaseInstance(handle)`。没有回调，或直接绑定的 Unity 目标已销毁时，工具释放结果。

回调抛异常后仍持有该操作。工具记录异常，不重复通知，也不释放接收方可能已保存的句柄。借用模块资产不产生可独立释放的持有。

</details>

下一步：[把已加载的音效交给音频层播放](audio.zh-CN.md)。
