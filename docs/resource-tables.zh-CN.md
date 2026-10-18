# 资源配置表

[框架总览](../README.zh-CN.md) · [开始使用](quick-start.zh-CN.md) · [English](resource-tables.md)

**编辑 Main.asset → 生成 → 加载模块 → 使用 main.Cue。** 一份表定义一组共同加载和释放的共享资产。组件自己固定使用的资产，普通 Inspector 引用就可能够用。

## 创建配置表

1. 把 **cue.wav** 放到 `Assets/GameContent/MyGame/Content/Audio/`。
2. 在 `Assets/GameContent/MyGame/Editor/Tables/ResourceModules/` 中选择 **Create → Framework → Resource Module Table**，将资产命名为 **Main**。
3. 选中 Main.asset，在原生 Inspector 中填写：

| 字段 | 示例 |
| --- | --- |
| Namespace | `MyGame` |
| Class Name | `MainResources` |
| Module Id | `MyGame.Main` |
| Audio Clips → Name | `Cue` |
| Audio Clips → Asset | Project 里的 cue.wav |
| Prefabs | 可选，填写名称并拖入 GameObject 预制体 |
| Sprites | 可选，填写名称并选择 Sprite 资产 |

4. 点击 **Validate and Generate**，**等待编译完成**，再编写调用代码。

**成功标志：** 出现 `MyGame/Scripts/Generated/MainResources.g.cs`，Inspector 显示 **Generated code is up to date.**。

生成的 `MyGame.MainResources` 继承 `BaseResourceModule`，提供 `public const string Id = "MyGame.Main"` 和 `AudioClip Cue` 属性。Prefabs 生成 `GameObject` 属性，Sprites 生成 `Sprite` 属性。Name 是代码名称，玩家看到的文字放在游戏内容中。

`Assets/GameContent/` 下的普通内容继承现有 Addressables 父目录条目，**不用逐个重复登记资产**。生成器解析并校验地址，不修改打包分组。接入其他工程时，由程序一次性设置内容根目录。

## 使用配置表

完整的启动、加载与清理示例见[资源](resources.zh-CN.md)。每个 Core 生命周期登记一次模块，显式加载，再为玩法保留已加载的模块实例：

```csharp
GameCore.Sound.PlaySound(SoundLayer.UI, main.Cue);
```

这一行的前提是 Core 和模块已就绪（需要 `using Core; using Sound;`）。读取属性不会触发加载。新增条目不用再改启动登记；新增模块才需要程序接入一次。

## 修改资源

- **新增：** 在对应列表添加 Name 和 Asset，生成后使用新属性。
- **替换：** 修改 Asset 并生成；保持 Name 不变，调用代码就不变。
- **移动：** 保留资产 meta，重新生成，让输出地址跟随移动。
- **名称改名或删除：** 生成，再更新使用该属性的调用方。
- **提交：** 配置表、生成的 C#、资产及其 meta 一起提交。修改配置表，不改生成代码；合并冲突时先解决配置表，再生成。

按加载生命周期拆分 Main、Level01 等配置表；只有一组资源时，先建 Main 即可。

<details>
<summary>目录、校验与恢复</summary>

配置表位于 `<game root>/Editor/Tables/ResourceModules/<name>.asset`，输出为 `<game root>/Scripts/Generated/<ClassName>.g.cs`。`ResourceModuleTable` 和 `ResourceModuleTableGenerator` 仅用于 Editor，Player 使用生成类。Demo 的 [Main.asset](../Assets/GameContent/Demo/Editor/Tables/ResourceModules/Main.asset) 生成 [MainResources.g.cs](../Assets/GameContent/Demo/Scripts/Generated/MainResources.g.cs)，类型为 `Demo.MainResources`，ID 为 `Demo.Main`。两者均属于可整体删除的 Demo，框架不依赖它们。

名称使用英文 C# 标识符且不能是关键字；Namespace 可为空或以点分隔。三个列表中的名称统一唯一，不能使用 Id、生成类名或继承成员名。Module Id 必须非空且唯一。校验拒绝名称/类型/模块 ID 冲突、无效引用及不可用或歧义 Addressables 键。使用保存的资产，不使用场景对象。Sprite 支持 Single 模式贴图和独立 Sprite 主资产；Multiple 模式切片、图集及其他子资源会被拒绝，不会静默加载其他切片。组需启用 Include in Build 和 Include Addresses in Catalog。

**修改 Class Name：** 生成新类型，更新调用方，再删除旧文件名对应的输出及 meta；构建检查会指出遗留输出。**修改 Namespace：** 生成并更新调用方，输出文件名不变。**修改 Module Id：** 更新查找代码与已保存的标识。**移动游戏根目录：** 配置表、现有输出及 meta 一起移动，再生成。旧输出不会自动删除。资源内部编号是生成细节，不要写入存档或网络消息。

校验失败保留旧输出。生成器保护手写文件和被修改的输出，内容相同不重写。Play/编译时禁止生成，Play 使用上次生成结果。Player 构建前检查拒绝缺失、过期、被修改或遗留的输出，包括源配置表已删除的生成文件。零资源表工程也有效。

</details>

下一步：[登记、加载和释放模块](resources.zh-CN.md) · [构建](build.zh-CN.md)
