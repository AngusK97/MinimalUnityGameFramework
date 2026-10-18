# Core

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](core.md)

**GameCore 准备公共服务，自己的启动脚本负责开始游戏。**

## 配置入口

1. 将 [GameCore.prefab](../Assets/GameResources/Prefab/GameCore.prefab) 拖入入口场景，放在 Hierarchy 最外层，保留已有设置。
2. 创建空对象，挂上自己的启动脚本。
3. 在 `Start()` 中检查 `GameCore.IsReady`，再使用服务。

将此方法放入启动类（需要 `using Core; using UnityEngine;`）。完整的启动类及打开窗口的示例见[开始使用](quick-start.zh-CN.md)。

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

## 它负责什么

| 入口 | 职责 |
| --- | --- |
| `GameCore.UI` | 窗口请求、实例与 UI 层级 |
| `GameCore.Event` | 即时与排队通知 |
| `GameCore.Resource` | 资源访问与已登记的资源模块 |
| `GameCore.Scene` | 受控场景转场 |
| `GameCore.Sound` | 分层音频播放 |

Core 在 `Awake()` 组装服务，在 `Update()` 处理 UI 请求和排队事件。Unity 通过 `UIBase.OnUpdate()` 更新已显示的窗口。

## 几件需要记住的小事

- 预制体提供 Canvas 层级、转场 UI、EventSystem 和 AudioListener，只保留一套启用的设施。
- 跨关卡保留 Core，通过自己的控制器重开玩法，不要重新初始化 Core。
- 游戏规则、存档、UI 标识和导航放在自己的游戏目录；游戏选择首个场景，Core 不登记 Demo 菜单。
- 五个管理器必须属于 Core 自身层级。结束生命周期时，销毁**整个 Core 根对象**。

**预期结果：** `GameCore.IsReady` 为 true。共用 InitScene 单独运行时只准备服务，不显示游戏菜单。

下一步：[UI](ui.zh-CN.md)。
