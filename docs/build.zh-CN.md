# 构建与排错

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](build.md)

**入口场景 → Addressables → Player。** Player 就是可以脱离编辑器运行的游戏。

## 构建 Demo

1. 打开 **File → Build Profiles**，在 **Platforms** 选择已安装的目标平台，需要时点击 **Switch Platform**。
2. 退出 Play，生成有改动的 UI 配置表，等待编译完成。
3. 选择 **Demo → Build Player**，再选择输出位置。
4. 打开游戏，检查菜单，再试 Start → 键盘通关 → Play again → Menu → Settings → Quit。

辅助工具会构建 Addressables，使用 DemoStart，不改动已保存的场景列表，并随 Demo 删除。

## 构建自己的游戏

1. 打开 **File → Build Profiles**，在 **Platforms** 选择目标平台，需要时点击 **Switch Platform**。
2. 点击左侧 **Scene List**，添加 `MyGame/Scenes/Start.unity`，放在第一项并勾选。不需要 InitScene 时移除它，保留游戏使用的其他原生场景。**Open Scene List** 打开的也是这份共享列表。
3. 生成有改动的 UI 配置表，等待编译完成。
4. 打开 **Window → Asset Management → Addressables → Groups**，选择 **Build → New Build → Default Build Script**，等待成功。
5. 回到 **Platforms** 中的目标平台，点击 **Build** 并选择输出位置。
6. 打开生成的游戏，应看到自己的第一个界面，再试试 UI、输入、声音和转场。

![Unity 6.6 Build Profiles 的 Scene List，MyGame Start 已启用并放在第一项](images/build-scene-list.png)

*第 2 步示例：Build Profiles → Scene List，MyGame/Scenes/Start 已勾选，并放在第一项。*

资源或平台改变后，重新构建 Addressables。**Use Asset Database (fastest)** 只用于 Editor Play，Player 仍需构建资源。

<details>
<summary>需要独立的场景列表或设置？</summary>

添加自定义 Build Profile。第一次构建使用共享列表即可。

</details>

## 遇到问题时

| 现象 | 优先检查 |
| --- | --- |
| 没有 Demo 菜单 | 打开 DemoStart.unity。InitScene 按设计只初始化服务。 |
| 缺少 GameUI | 暂时移除或注释对缺失生成类的调用，修复编译后生成配置表，再恢复调用。 |
| 生成器缺失或禁用 | 解决 Console 中的编译错误，选中 UI.asset，退出 Play，等待编译完成。 |
| 预制体无效 | 引用已保存的预制体，确保根对象上只有一个 UIBase 派生组件。 |
| 没有可加载地址 | 将运行时内容放在 GameContent 下；检查组的 Include in Build 和 Include Addresses in Catalog。 |
| 窗口打开失败 | 检查 Core 是否就绪、RegisterAll、实际地址以及 Console 中的首条错误。 |
| EventSystem 或 AudioListener 重复 | 保留共用 Core 的设施，移除场景或预制体中的额外副本。 |
| 过期或被修改的输出阻止构建 | 修复配置表或恢复生成输出，重新生成并等待编译完成。 |
| Player 只显示背景 | 将自己的入口放在第一项；默认保存的列表只有 InitScene。 |
| 场景加载失败 | 检查实际的 Addressables 键，并为所选目标平台重新构建资源内容。 |

## 验证范围

macOS 开发构建已通过；输入、音频及正式发布验收尚未完整完成。

<details>
<summary>基线与详细结果</summary>

当前基线：Unity **6.6（6000.6.5f1）**、Addressables **2.11.2**、uGUI **2.6.0**。导入与 C# 编译已通过；移除临时验证脚本后，默认程序集再次编译通过。在一次性迁移工程中，**13 项针对性检查通过**：

- UI 配置生成与重复生成不重写输出；Demo 公共服务、转场、设置暂停、通关、重开及返回。通关检查通过临时探针把玩家放到 GOAL，没有验证键盘移动。
- 无 Library 缓存的新工程中，仅删除 Demo，再由独立游戏使用剩余框架。
- Demo 与独立游戏的 **macOS Mono 开发版**资源内容/Player 构建，均为 **0 错误、0 警告**。

GUI 检查确认 Demo 菜单正常渲染，Escape 可打开/关闭 Settings，删除 Demo 后独立 Player 显示自己的窗口。完整原生鼠标流程、真实键盘通关、音频可听性、其他平台及 IL2CPP/正式发布构建**尚未验收**。这些结果对应上述基线，不代表所有目标与发布配置。

</details>

[Editor 升级](upgrading.zh-CN.md) · Unity 参考：[Build Profiles](https://docs.unity3d.com/6000.6/Documentation/Manual/build-profiles.html)、[Scene List](https://docs.unity3d.com/6000.6/Documentation/Manual/build-profile-scene-list.html)。

下一步：[UI 配置表恢复](ui-tables.zh-CN.md)，或[场景转场](scenes.zh-CN.md)。
