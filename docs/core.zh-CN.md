# Core

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](core.md)

**GameCore 负责准备共用服务，你的启动脚本负责开始游戏。**

<a id="配置入口"></a>

## 01 配置入口

1. 将 [GameCore.prefab](../Assets/Framework/Prefabs/GameCore.prefab) 拖入入口场景，放在 Hierarchy 最外层，保留已有设置。
2. 创建空对象，挂上自己的启动脚本。
3. 在 `Start()` 中检查 `GameCore.IsReady`，确认就绪后再使用服务。

把下面的方法放进启动类，在文件顶部添加 `using Core; using UnityEngine;`。想看从启动到打开窗口的完整写法，可以参考[开始使用](quick-start.zh-CN.md)。

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

如果使用生成的配置表，先[生成配置](config.zh-CN.md)，再在这里调用 `MyGame.GameConfig.LoadAndRegister()`。每个 Core 生命周期只调用一次，放在创建资源模块、打开 UI 或切换场景之前。InitScene 只初始化服务，不会加载游戏配置。

<a id="它负责什么"></a>

## 02 它负责什么

| 入口 | 职责 |
| --- | --- |
| `GameCore.UI` | 窗口请求、实例与 UI 层级 |
| `GameCore.Event` | 即时与排队通知 |
| `GameCore.Resource` | 资源访问与已登记的资源模块 |
| `GameCore.Scene` | 场景加载、遮罩与转场流程 |
| `GameCore.Sound` | 分层音频播放 |

Core 在 `Awake()` 中初始化各项服务，在 `Update()` 中处理 UI 请求和排队等待的事件。Unity 通过 `UIBase.OnUpdate()` 更新已经显示的窗口。

<a id="几件需要记住的小事"></a>

## 03 几件需要记住的小事

- 预制体已经包含 Canvas 层级、转场 UI、EventSystem 和 AudioListener，运行时只保留一套启用的组件。
- 切换关卡时保留 Core。需要重新开始游戏时，通过自己的控制器重置游戏，不要重新初始化 Core。
- 游戏规则、存档、UI 标识和导航逻辑放在自己的游戏目录中。由游戏决定从哪个场景开始，Core 不会登记 Demo 菜单。
- 五个管理器都要放在 Core 的层级里。不再需要 Core 时，销毁**整个 Core 根对象**。

**预期结果：** `GameCore.IsReady` 为 true。单独运行共用的 InitScene 时，只会准备服务，不会显示游戏菜单。

下一步：[UI](ui.zh-CN.md)。
