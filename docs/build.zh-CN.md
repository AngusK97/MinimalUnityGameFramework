# 构建与排错

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](build.md)

**入口场景 → Addressables → Player。** Player 就是可以脱离编辑器运行的游戏。

## 构建 Demo

1. 打开 **File → Build Profiles**，在 **Platforms** 选择已安装的目标平台，需要时点击 **Switch Platform**。
2. 退出 Play，等待编译完成。Demo 已附带可用配置，不需要云端设置。如果修改了它已连接的 Sheets 来源，先执行 **Framework → Config → Update Config**。
3. 选择 **Demo → Build Player**，再选择输出位置。
4. 打开游戏，检查菜单，再试 Start → 键盘通关 → Play again → Menu → Settings → Quit。

辅助工具会构建 Addressables，使用 DemoStart，不改动已保存的场景列表，并随 Demo 删除。

## 构建自己的游戏

1. 打开 **File → Build Profiles**，在 **Platforms** 选择目标平台，需要时点击 **Switch Platform**。
2. 点击左侧 **Scene List**，添加 `MyGame/Scenes/Start.unity`，放在第一项并勾选。不需要 InitScene 时移除它，保留游戏使用的其他原生场景。**Open Scene List** 打开的也是这份共享列表。
3. Google Sheets 或本地资产路径有变化时，执行 **Framework → Config → Update Config**，等待编译完成。构建前先确认报告中保留旧版的表。
4. 打开 **Window → Asset Management → Addressables → Groups**，选择 **Build → New Build → Default Build Script**，等待成功。
5. 回到 **Platforms** 中的目标平台，点击 **Build** 并选择输出位置。
6. 打开生成的游戏，应看到自己的第一个界面，再试试 UI、输入、声音和转场。

![Unity 6.6 Build Profiles 的 Scene List，MyGame Start 已启用并放在第一项](images/build-scene-list.png)

*第 2 步示例：Build Profiles → Scene List，MyGame/Scenes/Start 已勾选，并放在第一项。*

构建检查已提交的本地快照、生成文件归属和当前资产，不访问 Google，也不需要密钥。它无法发现尚未导入的云端修改。缺失的 UI 占位、无效资产或被修改的生成文件会阻止 Player 构建。

资源或平台改变后，重新构建 Addressables。**Use Asset Database (fastest)** 只用于 Editor Play，Player 仍需构建资源。

<details>
<summary>需要独立的场景列表或设置？</summary>

添加自定义 Build Profile。第一次构建使用共享列表即可。

</details>

## 遇到问题时

| 现象 | 优先检查 |
| --- | --- |
| 没有 Demo 菜单 | 打开 DemoStart.unity。InitScene 按设计只初始化服务。 |
| 缺少 GameConfig/UIName/MainResources | 从同一版本恢复生成文件与清单；或暂时注释调用，修复编译后执行 Update Config，再恢复调用。 |
| Update Config 不可用 | 解决 Console 编译错误，退出 Play，在 Framework → Config 选择游戏文件夹并检查来源设置。 |
| 预制体无效 | 引用已保存的预制体，确保根对象上只有一个 UIBase 派生组件。 |
| 没有可加载地址 | 在 Project 选中保存的资产，勾选 Inspector 的 Addressable，并检查组的 Include in Build 和 Include Addresses in Catalog。 |
| 窗口打开失败 | 检查 Core 是否就绪、GameConfig.LoadAndRegister()、UI 行、预制体和 Console 首条错误。 |
| EventSystem 或 AudioListener 重复 | 保留共用 Core 的设施，移除场景或预制体中的额外副本。 |
| 被修改或缺失的输出阻止构建 | 从同一版本恢复生成输出与清单，或修正来源后更新配置。 |
| UI 占位缺失阻止构建 | 补齐预制体或删除其 UI 行，再更新配置。导入允许该警告，Player 构建要求资产可用。 |
| Player 只显示背景 | 将自己的入口放在第一项；默认保存的列表只有 InitScene。 |
| 场景加载失败 | 检查启动时已加载配置、Scene 行和目标平台的 Addressables 内容。原始字符串地址使用 TryTransitionAddress。 |

## 验证范围

**2026-10-24**，使用 Unity **6000.6.5f1**、Addressables **2.11.2**、uGUI **2.6.0**：

- Unity 配置导入检查通过：无变化不写文件、独立表更新、引用回退、删除与输出保护、UI 占位及保留旧表时的资产检查。
- **Demo Play 的 81 项断言通过：** 本地表、生成 ID、资源访问、设置生命周期和两轮菜单 ↔ 关卡往返。这些检查通过程序调用游戏 API。
- Demo 和全新独立游戏均完成 **macOS Mono 开发版 Player 构建**。独立游戏在删除整个 Demo 后，通过配置、事件、资源加载／释放、UI 和场景检查。
- Demo Player 已确认菜单渲染和 Escape 打开设置。**18 组 Google 通信模拟检查**通过。
- Unity 中使用 Demo 三张表完成真实 Google 身份认证与导入：首次连接、无变化不重写、错误表保留旧版及恢复均通过。云端测试值已恢复。

**尚未验证：** 完整原生鼠标／键盘游戏闭环、实际可听性、其他平台及 IL2CPP／正式发布构建。其他平台还需实际构建和设备验证，这不表示已发现它们无法运行。工程不附带云端凭据；按[首次设置](config-setup.zh-CN.md#首次设置)连接自己的来源。

验证辅助内容只存在于临时副本，不随框架交付。以上覆盖 v0.3.0 所包含的代码；旧版本保留各自的教程。

[Editor 升级](upgrading.zh-CN.md) · Unity 参考：[Build Profiles](https://docs.unity3d.com/6000.6/Documentation/Manual/build-profiles.html)、[Scene List](https://docs.unity3d.com/6000.6/Documentation/Manual/build-profile-scene-list.html)。

下一步：[配置接入与恢复](config.zh-CN.md)，或[场景转场](scenes.zh-CN.md)。
