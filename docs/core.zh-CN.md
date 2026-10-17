# Core

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](core.md)

**在入口场景放一个 GameCore。它准备好公共工具，再由自己的脚本开始游戏。**

## 配置入口

1. 在 Project 找到 [GameCore.prefab](../Assets/GameResources/Prefab/GameCore.prefab)，拖到 Hierarchy 的空白处。让它放在最外层，不在其他对象里面，保留预制体已有设置。
2. 建一个空对象作为游戏入口，挂上自己的启动脚本。
3. 在脚本的 `Start()` 中先检查 `GameCore.IsReady`，再使用服务。需要完整脚本可以先看[开始使用](quick-start.zh-CN.md)。

将此方法放入现有的启动类中（需要 `using Core; using UnityEngine;`）：

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

完整的启动类及打开窗口的示例见[开始使用](quick-start.zh-CN.md)。

## 它负责什么

| 入口 | 职责 |
| --- | --- |
| `GameCore.UI` | 窗口请求、实例与 UI 层级 |
| `GameCore.Event` | 即时与排队通知 |
| `GameCore.Resource` | 资源访问与已登记的资源模块 |
| `GameCore.Scene` | 受控场景转场 |
| `GameCore.Sound` | 分层音频播放 |

Core 在 `Awake()` 中组装服务，在 `Update()` 中处理 UI 请求和排队事件。Unity 通过 `UIBase.OnUpdate()` 更新每个已显示的窗口。

## 几件需要记住的小事

- 共用预制体已经提供 Canvas 层级、转场 UI、EventSystem 和 AudioListener。只保留一套启用的设施。
- 跨关卡保留 Core。通过自己的控制器重开玩法，不要重新初始化 Core。
- 游戏规则、存档、UI 标识和导航放在自己的游戏目录。Core 不登记 Demo 菜单，也不选择首个场景。
- 五个管理器引用必须属于它自己的层级。需要结束生命周期时，销毁**整个 Core 根对象**。

**预期结果：** `GameCore.IsReady` 为 true。单独打开共用 InitScene 时，只准备服务，不显示游戏菜单。

下一步：[UI](ui.zh-CN.md)，或[完整的首个窗口教程](quick-start.zh-CN.md)。
