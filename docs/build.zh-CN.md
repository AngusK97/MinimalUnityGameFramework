# 构建与排错

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](build.md)

**在编辑器里跑通后，我们再做一个可以单独打开的游戏。** Unity 把它叫作 Player：选好入口场景，构建需要的资产，再构建 Player。

## 构建 Demo

1. 在 **File → Build Settings** 中选择已安装构建模块的目标平台；需要时执行 Switch Platform。
2. 退出 Play，生成有改动的 UI 配置表，等待编译完成。
3. 选择 **Demo → Build Player**，再选择输出位置。
4. 打开生成的游戏，应该能看到和编辑器里一样的 Demo 菜单，再试一次 Start → 键盘通关 → Play again → Menu → Settings → Quit。

Demo 辅助工具会构建 Addressables，并为本次构建提供 DemoStart。它保留已保存的场景列表，并随 Demo 一起删除。

## 构建自己的游戏

1. 在 **File → Build Settings** 的 **Scenes In Build** 中，将自己的 `MyGame/Scenes/Start.unity` 放在第一项。不需要只提供服务的 InitScene 时，移除其条目；保留游戏使用的其他原生场景。
2. 生成有改动的 UI 配置表，等待编译完成。
3. 打开 **Window → Asset Management → Addressables → Groups**，选择 **Build → New Build → Default Build Script**，等待成功。
4. 返回 Build Settings，选择 **Build** 和输出位置。
5. 打开生成的游戏，应看到自己的第一个界面，再试试 UI、输入、声音和转场。

资源或目标平台改变后，重新构建资源内容。**Use Asset Database (fastest)** 是 Editor 的 Play Mode 选项，不能代替 Player 所需的资源构建。

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

项目声明的版本为 Unity **2022.3.30f1** 和 Addressables **1.21.21**。当前代码已使用 Unity **2022.3.59f1** 验证，包括 Mac 开发版资源内容与 Player 构建。原生 Player 的完整鼠标流程验收、其他平台和正式发布要求仍需单独检查。

下一步：[UI 配置表恢复](ui-tables.zh-CN.md)，或[场景转场](scenes.zh-CN.md)。
