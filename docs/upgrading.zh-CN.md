# 升级到 Unity 6.6

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](upgrading.md)

**先保留能运行的旧版，在另一份副本里试着升级。** 已有游戏可以继续使用原来的框架和 Editor。什么时候换到下面这套版本，由你决定，再比较升级前后的结果。

<a id="当前基线"></a>

## 01 当前基线

| 项目 | 版本／选择 |
| --- | --- |
| Unity Editor | **6.6（6000.6.5f1）**；不是 LTS 版本 |
| Addressables | **2.11.2** |
| uGUI / Timeline | **2.6.0 / 6.6.0**，与 Editor 对应 |
| TextMeshPro | 已包含在 uGUI 中，移除独立包 |
| 旧 VR 模块 | 从 manifest 移除 `com.unity.modules.vr` |
| 渲染／输入 | 保留 Built-in Render Pipeline／旧 Input Manager |

升级 Editor 不要求同时改用 URP 或新输入系统。从 v0.2.0 改用配置表时，配置和场景 ID API 也需要调整，具体见[用法索引与迁移](../Build-in%20Managers%20Usage.zh-CN.md)。从 v0.3.x 升级到 v0.4.0 则只改变编辑器入口、进度与结果窗口，不需要迁移配置数据或修改运行时 API。

<a id="在独立副本中迁移"></a>

## 02 在独立副本中迁移

1. 提交或备份能正常运行的工程，包括 Assets、Packages、ProjectSettings 和 meta 文件。保留一个仍能用原 Editor 打开的分支或副本，不要直接用新版打开唯一的工作副本。
2. 通过 **Unity Hub → Unity 6.6（6000.6.5f1）** 打开迁移副本，等待包解析、资产导入和编译完成。迁移后的 manifest 和 lock 文件要一起保留。如果沿用了旧的 Library 缓存，这次成功导入还不能证明从头导入也能通过。
3. 先看上面的包变更和 Console 中的第一条错误，再判断要不要改游戏代码。迁移这份工程时，不要顺带打开、升级或替换其他游戏仓库中的框架。
4. 使用与你选择的框架版本对应的配置流程。从 v0.2.0 升级时，先完成[配置迁移](../Build-in%20Managers%20Usage.zh-CN.md#01-从-v020-迁移到-google-sheets)。v0.5.0 中，通过 **Framework → Config** 完成设置，日常更新点击顶部工具栏左侧的 **Update Config**。新游戏默认使用本地 Excel；已有的 Google 来源会继续保留，直到你主动切换。本地工作簿、来源设置、生成的快照、清单、C#、JSON 和 meta 文件要一起维护。
5. 打开 **File → Build Profiles**，在 **Platforms** 中选择目标平台，需要切换时点击 **Switch Platform**，并核对左侧共享的 **Scene List**。通过 **Build → New Build → Default Build Script** 重新构建 Addressables，再回到目标平台构建新的 Player。旧资源包或旧 Player 能运行，并不代表新的 Editor 和包版本组合也已通过验证。

在 Unity 6 中，为第一个窗口创建脚本时，使用 **Create → Scripting → MonoBehaviour Script**。构建流程也从原来的 Build Settings 改为 Build Profiles；[开始使用](quick-start.zh-CN.md)和[构建](build.zh-CN.md)教程都已使用当前菜单名称。

<a id="使用迁移副本前检查"></a>

## 03 使用迁移副本前检查

- 运行 DemoStart，按这个顺序试一遍：菜单 → Start → 用键盘通关 → Play again → Menu → Settings。查看 Console 是否报错、渐变是否正常，并多跑几轮。
- 运行自己的入口和 UI，检查登记、生成 ID、事件和借用的资源是否仍然符合预期。
- 另建一份用于测试的副本，删除其中的 `Assets/GameContent/Demo` 和 `Demo.meta`，保留 GameContent 和 `.gitkeep`。重新导入，运行自己的入口或只初始化服务的 InitScene，再构建资源内容和 Player。整个过程都不应依赖 Demo 的类型、资产或构建菜单。
- 打开新构建的 Player，实际检查界面、输入和音频，并记录测试过的目标平台。Editor 中通过检查，不能代替设备上的测试。

[验证范围](build.zh-CN.md#04-验证范围)分别说明了之前的基线检查，以及当前 main 中的新配置流程。自己的游戏还没测完之前，先保留旧分支或副本。

<details>
<summary>Unity 参考资料</summary>

[创建脚本](https://docs.unity3d.com/6000.6/Documentation/Manual/creating-scripts.html) · [Build Profiles](https://docs.unity3d.com/6000.6/Documentation/Manual/build-profiles.html) · [场景列表](https://docs.unity3d.com/6000.6/Documentation/Manual/build-profile-scene-list.html) · [Addressables 版本](https://docs.unity3d.com/6000.6/Documentation/Manual/com.unity.addressables.html) · [uGUI 变更](https://docs.unity3d.com/Packages/com.unity.ugui@2.6/changelog/CHANGELOG.html)

</details>

下一步：[构建并验收迁移后的游戏](build.zh-CN.md)。
