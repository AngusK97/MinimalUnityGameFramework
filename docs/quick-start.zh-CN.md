# 开始使用

[框架总览](../README.zh-CN.md) · [构建](build.zh-CN.md) · [English](quick-start.md)

先做一件看得见的事：**运行 Demo，再做一个自己的菜单**。

准备好 Unity Hub、Unity Editor 和 C# 编辑器。本页假设你会一点 Unity 基本操作，框架的用法会边做边介绍。下面说的 **Project** 是文件窗口，**Hierarchy** 是当前场景中的对象，**Inspector** 是选中对象的设置。

## 运行 Demo

1. 克隆或下载仓库。在 **Unity Hub → Add / Open** 中选择同时包含 Assets、Packages、ProjectSettings 的目录。当前基线选择 **Unity 6.6（6000.6.5f1）**，等待包解析、导入和编译完成。旧项目继续使用自己固定的版本；决定[迁移副本](upgrading.zh-CN.md)时再升级。
2. **先不要 Play**。打开 **Window → Asset Management → Addressables → Groups**，把 **Play Mode Script** 设为 **Use Asset Database (fastest)**，让编辑器直接读取资产。
3. 在 Project 打开 `Assets/GameContent/Demo/Scenes/DemoStart.unity`，点击 Play，应看到 MINIMAL LOOP 菜单。
4. 点击 Start，用 WASD/方向键移动到 GOAL。Play again 重开，Menu 返回；Settings/Escape 打开音量设置，Done/Escape 保存并关闭。
5. 继续下面的制作前，**再点一次 Unity 的 Play 按钮，退出运行**。

**成功标志：** 看到 MINIMAL LOOP 菜单，示例关卡可运行。Demo 设置只暂停示例的移动/计时，并非全局暂停系统。

## 创建第一个窗口

我用 `MyGame` 作为示例名字。第一次可以先保留这些名字，方便直接复制代码，顺着做完：**视图 → Prefab → 配置表 → 生成 → 启动入口**。

### 1. 创建视图

在 Project 右键 `Assets/GameContent` → **Create → Folder**，命名 **MyGame**。在里面继续创建下列目录；斜线表示一层层的子目录：

```text
Scenes/
Content/Prefabs/UI/
Editor/Tables/
Scripts/
```

在刚建的 Scripts 目录右键 **Create → Scripting → MonoBehaviour Script**，命名 **MyMenuView**，双击打开。把文件的**全部内容替换**为下面的代码，保存后回到 Unity，等待编译：

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

### 2. 保存 UI 预制体

1. 用 **File → New Scene** 新建一个临时场景来制作 UI，选择空模板，再点 **GameObject → UI → Panel**。Unity 会创建 Canvas，下面带一个 Panel。
2. 把 **Panel** 改名 MyMenuView。在 Hierarchy 选中它，把 **Project 里的 MyMenuView.cs 拖进它的 Inspector**，这样就挂上脚本了。Panel 上只保留这一个 UIBase 派生组件。
3. 选中该 Panel，点 **GameObject → UI → Legacy → Text**，文字设为 `My first menu`，调整位置和颜色，确保能在 Panel 里看到。
4. 把 **Hierarchy 里的 MyMenuView** 拖到 Project 的 `MyGame/Content/Prefabs/UI/` 目录，应看到 **MyMenuView.prefab**。拖的是 Panel，不是外层 Canvas；相机和 EventSystem 也不放进 Prefab。

这里的 Panel 就是 Prefab 的**根对象**，也就是它最上面那一层。因为它是 UI，所以带 RectTransform。刚才的 Canvas 只用来制作；真正运行的入口场景会使用 Core 自带的 Canvas。

### 3. 生成 UI 配置表

在 Project 右键 `MyGame/Editor/Tables/` → **Create → Framework → UI Table**。保留名字 **UI**（Unity 会保存为 UI.asset），选中它，展开 **Entries**，点击 **+** 加一行，再填写 Inspector：

| 字段 | 值 |
| --- | --- |
| Namespace | `MyGame` |
| Class Name | `GameUI` |
| Id Prefix | `MyGame` |
| Entries | 一行：Name=`Menu`，Prefab=保存的 MyMenuView 根组件 |

把 **Project 里的 MyMenuView.prefab** 拖进这一行的 **Prefab** 字段，不拖 Hierarchy 里还留着的场景对象。点击 **Validate and Generate**，等待编译，应出现 `MyGame/Scripts/Generated/GameUI.g.cs`，状态为 **Generated code is up to date.**。

**先生成。** 下一步的脚本引用 GameUI，类存在后才能编译。UI.asset 留在 Editor/Tables 中，不是要挂进场景的运行时组件。

### 4. 添加游戏入口

确认 GameUI.g.cs 已出现后，在自己的 Scripts 目录再次选择 **Create → Scripting → MonoBehaviour Script**，命名 **MyGameEntry**，把全部内容替换为：

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

            GameUI.RegisterAll();
            GameCore.UI.OpenUI(UILayer.First, GameUI.Menu, callback: view =>
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

1. 再用 **File → New Scene** 选空模板；若这个新场景有默认 Camera/Light，就删除这些默认对象，让 Hierarchy 为空。刚才的 Prefab 保存好后，临时制作场景可以不保存。
2. 把新场景保存为 `Assets/GameContent/MyGame/Scenes/Start.unity`。
3. 在 Project 找到 [GameCore.prefab](../Assets/GameResources/Prefab/GameCore.prefab)，拖到 Hierarchy 的空白处，保持没有父对象。它已经带了需要的 Canvas 层、EventSystem 和 AudioListener。
4. 在 Hierarchy 右键 **Create Empty**，命名 **GameEntry**，再把 **Project 里的 MyGameEntry.cs 拖进它的 Inspector**。GameEntry 和 GameCore 应并排放着，互相不包含。
5. 保存并 Play。

**成功标志：** 看到 `My first menu`，Console 输出 `MyGame menu opened.`，没有红色错误。Noooooice 🎉 你自己的窗口已经通过框架跑起来了。

若没出现，先退出 Play，检查三件事：场景里有 GameCore、GameEntry 挂了 MyGameEntry、UI.asset 显示生成代码是最新的。其他常见问题见[构建与排错](build.zh-CN.md)。

## 删除 Demo

在 Unity Project 窗口删除 `Assets/GameContent/Demo`。从磁盘删除时先关 Unity，再删除目录**及 Demo.meta**。保留 GameContent、`.gitkeep`、公共框架和自己的游戏目录。

再次运行自己的 Start.unity。自己的游戏使用自己的标识与内容，避免引用 DemoUI、DemoGame 或 Demo 资产。公共 `Assets/GameResources/Scenes/InitScene.unity` 保持为只初始化服务的入口，按设计不会打开菜单。

## 下一步

[新增 UI](ui-tables.zh-CN.md) · [窗口生命周期](ui.zh-CN.md) · [构建游戏](build.zh-CN.md)
