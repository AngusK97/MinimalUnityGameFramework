# 开始使用

[框架总览](../README.zh-CN.md) · [构建](build.zh-CN.md) · [English](quick-start.md)

**先运行 Demo，再做一个自己的窗口。** 你需要 Unity Hub、Unity 6.6（6000.6.5f1）和 C# 编辑器，会 Unity 的基本操作就可以开始。

直接用下载的框架工程开始制作游戏。按下面的步骤操作，会保留工程原有的依赖和设置，把自己的内容放进 `GameContent/MyGame/`。

本页对应 **v0.5.0**。使用 SO 配置的旧版本请看 [v0.2.0 教程](https://github.com/AngusK97/MinimalUnityGameFramework/blob/v0.2.0/docs/quick-start.zh-CN.md)。

<a id="运行-demo"></a>

## 01 运行 Demo

1. [下载 v0.5.0（ZIP）](https://github.com/AngusK97/MinimalUnityGameFramework/archive/refs/tags/v0.5.0.zip)并解压。用 Unity Hub 打开包含 Assets、Packages、ProjectSettings 的目录，等待导入和编译完成。
2. 打开 **Window → Asset Management → Addressables → Groups**，把 **Play Mode Script** 设为 **Use Asset Database (fastest)**。
3. 打开 `Assets/GameContent/Demo/Scenes/DemoStart.unity`，点击 Play，再点 **Start**，用 **WASD / 方向键**移动到 GOAL。
4. 点击 **Play again** 重新开始，点击 **Menu** 返回菜单。用 **Settings / Escape** 打开音量设置，再用 **Done / Escape** 保存并关闭。
5. 再点一次 Play 停止运行，然后继续制作自己的内容。

**成功标志：** MINIMAL LOOP 菜单和关卡正常运行。打开设置时，只会暂停 Demo 的移动和计时。Demo 已包含生成配置，运行不需要 Google 账号、密钥或网络。

<a id="创建第一个窗口"></a>

## 02 创建第一个窗口

第一次跟着做时，先保留示例中的名称，按这个顺序完成：**视图 → Prefab → 本地 Excel → Update Config → 启动入口**。

准备一个能保存 `.xlsx` 的表格编辑器即可，不需要 Google 账号。[第 2.3 节](#23-连接并更新-ui-配置表)会创建自己的 UI 工作簿并接入 Unity。

<a id="1-创建视图"></a>

### 2.1 创建视图

在 `Assets/GameContent/MyGame/` 下创建这些目录：

```text
Scenes/
Content/Prefabs/UI/
Editor/Tables/Source/
Scripts/
```

在 Scripts 目录中选择 **Create → Scripting → MonoBehaviour Script**，命名为 **MyMenuView**。用下面的代码替换整个文件，保存后等待编译完成：

```csharp
using UI;
using UnityEngine;

namespace MyGame
{
    public sealed class MyMenuView : UIBase
    {
        protected override void OnShow()
        {
            Debug.Log("MyGame menu opened.", this);
        }
    }
}
```

<a id="2-保存-ui-预制体"></a>

### 2.2 保存 UI 预制体

1. 用 **File → New Scene** 创建临时空场景，再选择 **GameObject → UI → Panel**。
2. 把 Panel 改名为 **MyMenuView**，将 MyMenuView.cs 拖到它的 Inspector 中。Panel 上只能有一个继承 UIBase 的组件。
3. 选中 Panel，选择 **GameObject → UI → Legacy → Text**，将文字设为 `My first menu`，调整位置，确保能在 Panel 内看到它。
4. 将 Hierarchy 中的 Panel 拖到 `MyGame/Content/Prefabs/UI/`，保存为 **MyMenuView.prefab**。只保存 Panel，不要把外层 Canvas、相机或 EventSystem 放进预制体；运行时 Core 会提供它们。
5. 在 Project 中选中已保存的 **MyMenuView.prefab**，勾选 Inspector 顶部的 **Addressable**，保留默认地址。

<a id="3-连接并更新-ui-配置表"></a>

### 2.3 连接并更新 UI 配置表

在表格编辑器中新建空白工作簿，把第一张工作表改名为 **UI**。从 A1 开始填写下面三行：第 1 行是字段名，第 2 行是类型，第 3 行是你的第一个窗口。

| Id | Name | PrefabPath |
| --- | --- | --- |
| string | string | string |
| Menu | My first menu | Assets/GameContent/MyGame/Content/Prefabs/UI/MyMenuView.prefab |

1. 将工作簿保存为 `Assets/GameContent/MyGame/Editor/Tables/Source/MyGame.xlsx`。单元格直接填写值，不使用公式，删除多余的空白工作表。
2. 打开 **Framework → Config**，设置 **Game = MyGame**、**Namespace = MyGame**、**Source = Local Excel**。保留 **Excel folder = Editor/Tables/Source**；需要手动选择时，点击 **Choose folder…**。
3. 点击 **Update Config**，等待更新结果和编译完成。以后改表，保存工作簿后点击顶部工具栏左侧的 **Update Config** 即可。

也可以把 `Demo/Editor/Tables/Source/Demo.xlsx` 中的 UI 工作表复制到自己的工作簿，参考它的格式，再把数据替换成上面的 Menu 行。自己的游戏使用自己的 Prefab 和生成代码。Google Sheets 接入及来源切换见[配置接入](config-setup.zh-CN.md)。

**成功标志：** UI 表更新成功，`Assets/GameContent/MyGame/Scripts/Generated/UIName.g.cs` 中出现 `Menu = "Menu"`。

**先更新再继续：** 下一步的脚本会用到生成的 GameConfig 和 UIName。后续修改方法见 [UI 配置表](ui-tables.zh-CN.md)。

<a id="4-添加游戏入口"></a>

### 2.4 添加游戏入口

在 Scripts 目录中，用刚才的脚本菜单创建 **MyGameEntry.cs**，用下面的代码替换整个文件：

```csharp
using Core;
using UI;
using UnityEngine;

namespace MyGame
{
    public sealed class MyGameEntry : MonoBehaviour
    {
        private void Start()
        {
            if (!GameCore.IsReady)
            {
                Debug.LogError("Add the shared GameCore prefab to the entry scene.", this);
                return;
            }

            GameConfig.LoadAndRegister();

            GameCore.UI.OpenUI(UILayer.First, UIName.Menu, callback: view =>
            {
                if (view == null)
                {
                    Debug.LogError("Menu failed to open. Check the earlier Console errors.");
                }
            });
        }
    }
}
```

1. 新建空场景，移除默认的 Camera/Light。刚才用来制作 UI 的临时场景，在保存好 Prefab 后可以直接关闭，不用保存。
2. 保存为 `Assets/GameContent/MyGame/Scenes/Start.unity`。
3. 将 [GameCore.prefab](../Assets/Framework/Prefabs/GameCore.prefab) 拖入 Hierarchy，放在最外层，作为根对象。它已经包含 Canvas 层、EventSystem 和 AudioListener。
4. 创建一个空的根对象 **GameEntry**，挂上 **MyGameEntry.cs**。让它和 GameCore 处于同一层级，不要互相嵌套。
5. 保存并 Play。

**成功标志：** 看到 `My first menu`，Console 输出 `MyGame menu opened.`，没有红色错误。第一个窗口就做好了 🎉

![MyGame Start 场景显示 My first menu、GameEntry 和公共 GameCore](images/first-window.png)

如果没有成功，先检查 GameCore、MyGameEntry 是否放对、挂好，再检查 UI 表是否生成成功。更多排查方法见[构建与排错](build.zh-CN.md)。

<a id="删除-demo"></a>

## 03 删除 Demo

在 Unity 的 Project 窗口中删除 `Assets/GameContent/Demo`。如果从磁盘删除，先关闭 Unity，再同时删除**这个目录和 Demo.meta**。保留 GameContent、`.gitkeep`、公共框架和自己的游戏内容。

再次运行自己的 Start.unity，确认使用的都是自己游戏的 ID 和资产，没有引用 Demo 命名空间、DemoGame 或 Demo 资产。公共场景 `Assets/Framework/Scenes/InitScene.unity` 只负责准备服务，不会打开菜单。

<a id="下一步"></a>

## 04 下一步

[新增 UI](ui-tables.zh-CN.md) · [窗口生命周期](ui.zh-CN.md) · [构建游戏](build.zh-CN.md)
