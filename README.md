# jamBass

基于 Unity 和 C# 开发的 2D 游戏工程，包含建筑放置、资源管理、敌人生成与战斗等功能。

## 引擎与版本

- **游戏引擎：Unity**
- **编辑器版本：2022.3.62f3c1**（完整版本记录在 `ProjectSettings/ProjectVersion.txt` 中）
- **开发语言：C#**
- 主要依赖：Unity Input System 1.14.0、TextMesh Pro 3.0.7、Unity UI 1.0.0；DOTween 已包含在 `Assets/DOTween` 中。

请使用与工程一致的编辑器版本打开。Unity 包依赖记录在 `Packages/manifest.json` 和 `Packages/packages-lock.json` 中，首次打开时需要联网下载依赖。

## 打开与运行工程

1. 安装 Unity Hub 和 **Unity 2022.3.62f3c1** 编辑器，并安装目标平台所需的构建支持模块。
2. 下载或克隆完整工程，保留 `Assets`、`Packages`、`ProjectSettings` 目录以及资源对应的 `.meta` 文件。
3. 在 Unity Hub 的项目页面选择添加已有工程，选中本 README 所在的工程根目录（不要只选择 `Assets` 目录）。
4. 使用上述版本打开工程，等待依赖解析、资源导入和 C# 脚本编译完成。首次导入耗时可能较长。
5. 在 Project 窗口打开 `Assets/Scenes/MainMenu.unity`，点击编辑器顶部的 **Play** 按钮，从主菜单进入游戏。

## 编译与导出

Unity 在打开工程或修改脚本后会自动编译 C# 代码，无需单独编译 Visual Studio 工程。导出前请等待编译结束，并确认 **Window > General > Console** 中没有红色错误。

### 导出 Windows 可执行程序

1. 打开 **File > Build Settings**。
2. 确认 **Scenes In Build** 中以下场景均已勾选，并保持此顺序（工程已配置）：

   | 索引 | 场景路径 | 用途 |
   | --- | --- | --- |
   | 0 | `Assets/Scenes/MainMenu.unity` | 主菜单，程序启动入口 |
   | 1 | `Assets/Scenes/Game.unity` | 游戏主场景 |
   | 2 | `Assets/Scenes/Explain.unity` | 说明场景 |

3. 选择 **PC, Mac & Linux Standalone**，将 **Target Platform** 设为 **Windows**，**Architecture** 设为 **x86_64**；如当前平台不同，点击 **Switch Platform** 并等待切换完成。
4. 如需调整窗口分辨率、全屏模式或产品名称，点击 **Player Settings** 修改。工程当前产品名称为 `jamBass`。
5. 点击 **Build**，选择独立输出目录，例如 `Builds/Windows/`，将程序命名为 `jamBass.exe`。也可点击 **Build And Run**，构建完成后直接运行。
6. 分发时打包整个构建输出目录，保留 `.exe`、对应的 `_Data` 文件夹、`UnityPlayer.dll` 及其他生成文件，不要仅发送 `.exe`。

### 导出其他平台

先为同一版本的 Unity 编辑器安装对应平台的构建支持模块，再在 **File > Build Settings** 中选择目标平台，点击 **Switch Platform**，完成该平台的 Player Settings 配置后构建。其他平台的兼容性需在实际导出后验证。

## 工程目录

```text
Assets/
  Scenes/          主菜单、游戏和说明场景
  Scripts/         C# 游戏逻辑
  Data/            建筑、单位等配置数据
  Prefabs/         建筑、单位和 UI 等预制体
  Sprites/         图片资源
  Audios/          音频资源
  Resources/       运行时加载资源
  DOTween/         动画补间插件
Packages/          Unity 包依赖配置
ProjectSettings/   引擎版本、构建场景及项目设置
```

## 常见问题

- **打开时提示版本不匹配**：检查所选编辑器版本是否为 `2022.3.62f3c1`。
- **包下载失败或脚本编译失败**：先检查网络连接与 Package Manager 的依赖解析状态，再根据 Console 中的首个错误处理。
- **目标平台无法构建**：检查该编辑器是否已安装对应的构建支持模块，以及所选脚本后端要求的编译工具链。
- **启动场景错误或场景切换失败**：检查 Build Settings 中的三个场景是否已勾选，并确认 `MainMenu` 位于索引 0。

以上步骤依据工程配置编写；本次文档整理未实际执行 Unity 构建。
