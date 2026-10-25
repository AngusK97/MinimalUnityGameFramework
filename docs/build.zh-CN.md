# 构建与排错

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](build.md)

**设置入口场景 → 构建 Addressables → 构建 Player。** Player 就是构建好的游戏，可以在 Editor 外独立运行。

<a id="构建-demo"></a>

## 01 构建 Demo

1. 打开 **File → Build Profiles**，在 **Platforms** 中选择已安装的目标平台，需要切换时点击 **Switch Platform**。
2. 退出 Play，等待编译完成。Demo 已经附带可用配置，不用先设置云端来源。如果修改过它的源工作簿，先在 **Framework → Config** 中选择 Demo，再点击顶部工具栏左侧的 **Update Config**。
3. 选择 **Demo → Build Player**，再选择输出位置。
4. 打开构建好的游戏，检查菜单，再按这个顺序试一遍：Start → 用键盘通关 → Play again → Menu → Settings → Quit。

这个辅助工具会构建 Addressables，并以 DemoStart 作为入口，不会修改已经保存的场景列表。删除 Demo 时，这个工具也会一起删除。

<a id="构建自己的游戏"></a>

## 02 构建自己的游戏

1. 打开 **File → Build Profiles**，在 **Platforms** 中选择目标平台，需要切换时点击 **Switch Platform**。
2. 点击左侧的 **Scene List**，添加 `MyGame/Scenes/Start.unity`，将它移到第一项并勾选。如果不需要 InitScene，就从列表中移除它，保留游戏使用的其他原生场景。**Open Scene List** 打开的也是这份共享列表。
3. 如果当前源表内容或本地资产路径有变化，先在 **Framework → Config** 中选择自己的游戏，再点击顶部工具栏左侧的 **Update Config**，等待编译完成。构建前看一下导入报告：如果有表保留了旧版，确认这是不是你要使用的内容。
4. 打开 **Window → Asset Management → Addressables → Groups**，选择 **Build → New Build → Default Build Script**，等待成功。
5. 回到 **Platforms**，选中目标平台，点击 **Build** 并选择输出位置。
6. 打开构建好的游戏，应该能看到自己的第一个界面。再试试 UI、输入、声音和场景切换。

![Unity 6.6 Build Profiles 的 Scene List，MyGame Start 已启用并放在第一项](images/build-scene-list.png)

*第 2 步示例：Build Profiles → Scene List，MyGame/Scenes/Start 已勾选，并放在第一项。*

构建时会检查已提交的本地快照、生成文件的归属，以及当前资产是否可用。它不会重新导入 Excel 或访问 Google，也不需要密钥。如果改了源表却还没导入，这一步是检查不出来的。如果 UI 占位仍缺少对应资产、资产无效，或生成文件被修改，Player 构建就会停下来。

修改资源或切换平台后，要重新构建 Addressables。**Use Asset Database (fastest)** 只适用于 Editor 中的 Play 模式，Player 仍然需要构建好的资源内容。

<details>
<summary>需要独立的场景列表或设置？</summary>

可以添加自定义 Build Profile。第一次构建时，使用共享列表就够了。

</details>

<a id="遇到问题时"></a>

## 03 遇到问题时

| 现象 | 优先检查 |
| --- | --- |
| 没有 Demo 菜单 | 打开 DemoStart.unity。InitScene 只负责初始化服务，不会打开菜单。 |
| 缺少 GameConfig/UIName/MainResources | 从同一版本恢复生成文件和清单；也可以先注释相关调用，解决编译错误后执行 Update Config，再恢复调用。 |
| Update Config 不可用 | 退出 Play，等待当前更新、导入或编译结束。Console 中有编译错误时，先修复再继续。 |
| 点击工具栏却打开了 Config 设置 | 选择游戏和源表文件夹；Google 来源还需要本机存在可用的密钥文件。 |
| 预制体无效 | 引用已保存的预制体，确保根对象上只有一个 UIBase 派生组件。 |
| 没有可加载地址 | 在 Project 中选中已保存的资产，勾选 Inspector 中的 Addressable，并检查组的 Include in Build 和 Include Addresses in Catalog。 |
| 窗口打开失败 | 检查 Core 是否就绪、是否调用了 GameConfig.LoadAndRegister()、UI 表对应行和预制体是否正确，再查看 Console 中的第一条错误。 |
| EventSystem 或 AudioListener 重复 | 保留共用 Core 中的组件，移除场景或预制体中多余的副本。 |
| 生成文件被修改或缺失，导致构建失败 | 从同一版本恢复生成文件和清单，或修正来源后重新更新配置。 |
| UI 占位缺失，导致构建失败 | 补齐预制体，或删除对应的 UI 行，然后更新配置。导入时允许保留这条警告，但构建 Player 时必须有可用资产。 |
| Player 只显示背景 | 将自己的入口场景放到第一项；默认保存的列表中只有 InitScene。 |
| 场景加载成功，却看不到关卡物体 | 将世界相机的 Depth 设为 2，并让 Culling Mask 包含物体所在层；见[场景转场](scenes.zh-CN.md)。Core 的背景相机 Depth 为 1。 |
| 场景加载失败 | 确认启动时已加载配置，并检查 Scene 行和目标平台的 Addressables 内容。直接传入字符串地址时，使用 TryTransitionAddress。 |

<a id="验证范围"></a>

## 04 验证范围

本轮于 **2026-10-25** 在 macOS、Unity **6000.6.5f1** 上验证：

- **29 项配置集成检查通过**，涵盖本地 Excel、部分保留、文件损坏、来源切换和真实 Google 只读往返。数字单元格也在 Unity 的 Mono 环境下与 Google 读取结果一致。
- 本地表格导入后，**81 项 Demo Play 检查通过**，覆盖生成表、UI、资源及两轮菜单／关卡往返。
- 整体删除 Demo 后，独立游戏从自己的本地表格生成 UI、Resource、Scene 和 Item。其 **macOS Mono 开发版 Player** 构建成功，并在批处理模式通过配置查询、事件、资源加载、UI 和场景转场检查。
- 移除所有验证辅助代码后，含 Demo／整体删除 Demo 两种情况下的最终默认编译均通过。
- 真实 Unity 弹窗验证了成功导入、部分保留和恢复；保留的 Resource 文件内容及修改时间均未变化。测试后 Demo 表格已恢复。

这些是针对性检查，不代表所有平台和发布模式都已验证。**其他平台、IL2CPP／正式发布构建、实际听音和完整原生鼠标／键盘闭环仍待验收。** 验证辅助代码只留在工程外。

<details>
<summary>此前的验证记录</summary>

**2026-10-24**，使用 Unity **6000.6.5f1**、Addressables **2.11.2**、uGUI **2.6.0**：

- Unity 中的配置导入检查通过，覆盖了内容不变时不写文件、单张表独立更新、引用回退、删除与生成文件保护、UI 占位，以及保留旧表时的资产检查。
- **Demo Play 的 81 项断言通过：** 覆盖本地表、生成 ID、资源访问、设置界面的生命周期，以及两轮菜单 ↔ 关卡往返。这些检查通过程序调用游戏 API 完成。
- Demo 和一个全新的独立游戏都完成了 **macOS Mono 开发版 Player 构建**。独立游戏删除整个 Demo 后，仍通过了配置、事件、资源加载／释放、UI 和场景检查。
- Demo Player 已确认能显示菜单，并通过 Escape 打开设置。**18 组 Google 通信模拟检查**通过。
- 在 Unity 中，使用 Demo 的三张表完成了真实的 Google 身份认证与导入。首次连接、内容不变时不重写、表有错误时保留旧版，以及修复后恢复导入，均已通过。云端测试值已恢复。

**尚未验证：** 用真实鼠标／键盘完整玩一轮游戏、设备是否实际发出声音、其他平台，以及 IL2CPP／正式发布构建。其他平台还需要实际构建并在设备上测试；尚未测试不代表不能运行。工程不附带云端凭据；需要连接自己的来源时，按[首次设置](config-setup.zh-CN.md#01-首次设置)操作。

验证用的辅助内容只放在临时副本中，不会随框架一起交付。以上结果对应 v0.3.1 包含的代码，旧版本仍保留各自的教程。

**v0.4.0 Editor 验证（2026-10-24）：** 31 项针对性检查通过，程序集重载后仍能显示更新结果。包含 Demo 的工程，以及删除 Demo、保留独立 MyGame 的副本，默认编译均通过。实际点击工具栏完成了首次连接（**3 updated**）、部分更新（**1 updated, 1 kept previous**）和 **Cancel** 检查。部分更新保留的 Resource 文件，以及取消后的全部生成文件，内容与修改时间均未变化。云端测试值已恢复。

**v0.4.1 Editor 验证（2026-10-25）：** 按钮从已有的居中布局迁移到左侧工具栏，带有底板，Play 控件保持居中。macOS 上的 Unity 6000.6.5f1 已实测打开设置、脚本重载和播放时禁用；含 Demo、删除 Demo 后的默认编译均通过。更新流程和运行代码未变。


</details>

[Editor 升级](upgrading.zh-CN.md) · Unity 参考：[Build Profiles](https://docs.unity3d.com/6000.6/Documentation/Manual/build-profiles.html)、[Scene List](https://docs.unity3d.com/6000.6/Documentation/Manual/build-profile-scene-list.html)。

下一步：[配置接入与恢复](config.zh-CN.md)，或[场景转场](scenes.zh-CN.md)。
