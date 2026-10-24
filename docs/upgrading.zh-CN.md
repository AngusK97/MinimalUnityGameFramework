# 升级到 Unity 6.6

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](upgrading.md)

**保留能运行的旧版，在另一份副本中迁移，再对比。** 已有游戏可以继续固定原框架与 Editor；是否采用当前基线，单独决定。

## 当前基线

| 项目 | 版本／选择 |
| --- | --- |
| Unity Editor | **6.6（6000.6.5f1）**；不是 LTS 版本 |
| Addressables | **2.11.2** |
| uGUI / Timeline | **2.6.0 / 6.6.0**，与 Editor 对应 |
| TextMeshPro | 已包含在 uGUI 中，移除独立包 |
| 旧 VR 模块 | 从 manifest 移除 `com.unity.modules.vr` |
| 渲染／输入 | 保留 Built-in Render Pipeline／旧 Input Manager |

Editor 升级本身不要求改用 URP 或新输入系统。main 预览版还调整了配置与场景 ID API；采用这个框架版本时，按[用法索引与迁移](../Build-in%20Managers%20Usage.zh-CN.md)处理。

## 在独立副本中迁移

1. 提交或备份能运行的工程，包括 Assets、Packages、ProjectSettings 和 meta 文件。保留可由原 Editor 打开的分支或副本，不要直接用新版打开唯一的工作副本。
2. 通过 **Unity Hub → Unity 6.6（6000.6.5f1）** 打开迁移副本，等待包解析、资产导入和编译。迁移后的 manifest 与 lock 文件一起保留；不能把复用旧 Library 缓存当作干净导入已通过。
3. 先核对上面的包变更和 Console 第一条错误，再决定是否需要改玩法代码。本次迁移不打开、升级或替换其他游戏仓库中的框架。
4. 使用所选框架版本对应的配置流程。main 预览版先按[配置迁移](../Build-in%20Managers%20Usage.zh-CN.md#从-v020-迁移到-google-sheets)处理，再执行 **Framework → Config → Update Config**。生成快照、清单、C#、JSON 和 meta 一起维护。
5. 在 **File → Build Profiles** 的 **Platforms** 中选择目标，需要时点击 **Switch Platform**，核对左侧共享的 **Scene List**。通过 **Build → New Build → Default Build Script** 重新构建 Addressables，再回目标平台构建新的 Player；不能用旧资源包或旧 Player 证明新组合已通过。

Unity 6 中，首个窗口的脚本通过 **Create → Scripting → MonoBehaviour Script** 创建。Build Profiles 替代旧 Build Settings 流程；[开始使用](quick-start.zh-CN.md)与[构建](build.zh-CN.md)已按当前标签说明。

## 使用迁移副本前检查

- 运行 DemoStart：菜单 → Start → 键盘通关 → Play again → Menu → Settings，检查 Console、渐变与多轮运行。
- 运行自己的入口和 UI，核对登记、生成 ID、事件和资源借用是否符合预期。
- 在一次性副本中删除 `Assets/GameContent/Demo` 与 `Demo.meta`。保留 GameContent 和 `.gitkeep`，重新导入、运行自己的入口或只初始化服务的 InitScene，再构建资源内容与 Player，不应依赖任何 Demo 类型、资产或构建菜单。
- 打开新的 Player，检查真实界面、输入和音频，记录实际验收的目标平台；编辑器检查不能代替设备检查。

[验证范围](build.zh-CN.md#验证范围)区分此前基线检查与当前 main 的新配置流程。自己的游戏完成验收之前，继续保留旧分支或副本。

<details>
<summary>Unity 参考资料</summary>

[创建脚本](https://docs.unity3d.com/6000.6/Documentation/Manual/creating-scripts.html) · [Build Profiles](https://docs.unity3d.com/6000.6/Documentation/Manual/build-profiles.html) · [场景列表](https://docs.unity3d.com/6000.6/Documentation/Manual/build-profile-scene-list.html) · [Addressables 版本](https://docs.unity3d.com/6000.6/Documentation/Manual/com.unity.addressables.html) · [uGUI 变更](https://docs.unity3d.com/Packages/com.unity.ugui@2.6/changelog/CHANGELOG.html)

</details>

下一步：[构建并验收迁移后的游戏](build.zh-CN.md)。
