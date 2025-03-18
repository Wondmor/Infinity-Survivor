# UI框架使用指南 (Unity 2021.3)

本指南将帮助你如何在Unity 2021.3中配置和使用UI框架。

## 目录

1. [准备工作](#准备工作)
2. [创建主菜单界面](#创建主菜单界面)
3. [创建设置界面](#创建设置界面)
4. [创建消息弹窗](#创建消息弹窗)
5. [创建暂停菜单](#创建暂停菜单)
6. [将UI管理器和控制器添加到场景](#将UI管理器和控制器添加到场景)
7. [配置场景加载和引用](#配置场景加载和引用)

## 准备工作

在开始创建UI界面之前，你需要确保：

1. 已安装TextMeshPro：如果尚未安装，请通过Window > Package Manager安装TextMeshPro
2. 创建Resources文件夹：在Assets下创建Resources文件夹，用于存放UI预制体

## 创建主菜单界面

### 步骤1：创建主菜单UI预制体

1. 在场景中创建一个Canvas（右键Hierarchy > UI > Canvas）
2. 确保Canvas设置如下：
   - Canvas Scaler设置为"Scale With Screen Size"
   - Reference Resolution设置为1920x1080
   - Match设置为0.5（这样可以在保持宽度的同时适应不同高度比例）

3. 创建主菜单面板层次结构：
   ```
   MainMenuPanel (Panel)
     |- Background (Image)
     |- Title (Panel)
     |   |- TitleText (TextMeshPro)
     |- Buttons (Panel)
     |   |- StartGameButton (Button-TextMeshPro)
     |   |- SettingsButton (Button-TextMeshPro)
     |   |- QuitGameButton (Button-TextMeshPro)
     |- VersionText (TextMeshPro)
   ```

4. 设计按钮和文本样式，并调整布局位置
5. 将MainMenuPanel拖拽到Resources目录下的UI文件夹中（如不存在，请创建），命名为"MainMenuPanel"

### 步骤2：添加MainMenuPanel组件

1. 在MainMenuPanel预制体上添加MainMenuPanel脚本组件
2. 确保没有重复的脚本组件

## 创建设置界面

### 步骤1：创建设置UI预制体

1. 在场景中创建一个新的Canvas
2. 创建设置面板层次结构：
   ```
   SettingsPanel (Panel)
     |- Background (Image)
     |- Title (Panel)
     |   |- TitleText (TextMeshPro - "设置")
     |- AudioSettings (Panel)
     |   |- MusicVolumeLabel (TextMeshPro - "音乐音量")
     |   |- MusicVolumeSlider (Slider)
     |   |- MusicVolumeText (TextMeshPro - "100%")
     |   |- SFXVolumeLabel (TextMeshPro - "音效音量")
     |   |- SFXVolumeSlider (Slider)
     |   |- SFXVolumeText (TextMeshPro - "100%")
     |- DisplaySettings (Panel)
     |   |- FullscreenLabel (TextMeshPro - "全屏")
     |   |- FullscreenToggle (Toggle)
     |   |- QualityLabel (TextMeshPro - "画质")
     |   |- QualityDropdown (TMP_Dropdown)
     |- Buttons (Panel)
     |   |- SaveButton (Button-TextMeshPro - "保存")
     |   |- ResetButton (Button-TextMeshPro - "重置")
     |   |- CloseButton (Button-TextMeshPro - "关闭")
   ```

3. 配置Slider组件：
   - 将Value Range设置为0-1
   - 为Slider添加适当的背景和填充色

4. 配置Toggle组件：
   - 添加背景和勾选标记
   - 设置默认值

5. 将完成的SettingsPanel预制体保存到Resources/UI/目录下，命名为"SettingsPanel"

### 步骤2：添加SettingsPanel组件

1. 在SettingsPanel预制体上添加SettingsPanel脚本组件
2. 确保路径与AutoBind特性中的路径一致

## 创建消息弹窗

### 步骤1：创建消息弹窗预制体

1. 在场景中创建一个新的Canvas
2. 创建消息弹窗层次结构：
   ```
   MessagePopup (Panel)
     |- Overlay (Image - 半透明黑色背景)
     |- PopupWindow (Panel - 弹窗主体)
     |   |- Title (Panel)
     |   |   |- Text (TextMeshPro)
     |   |- Content (Panel)
     |   |   |- Text (TextMeshPro)
     |   |- Buttons (Panel)
     |   |   |- ConfirmButton (Button-TextMeshPro - "确定")
     |   |   |- CancelButton (Button-TextMeshPro - "取消")
   ```

3. 为弹窗添加动画效果（可选）：
   - 添加Animator组件
   - 创建简单的进入/退出动画

4. 将MessagePopup预制体保存到Resources/UI/目录下，命名为"MessagePopup"

### 步骤2：添加MessagePopup组件

1. 在MessagePopup预制体上添加MessagePopup脚本组件
2. 确保路径与AutoBind特性中的路径一致

## 创建暂停菜单

### 步骤1：创建暂停菜单预制体

1. 在场景中创建一个新的Canvas
2. 创建暂停菜单层次结构：
   ```
   PauseMenuPanel (Panel)
     |- Overlay (Image - 半透明黑色背景)
     |- Title (TextMeshPro - "游戏暂停")
     |- Buttons (Panel)
     |   |- ResumeButton (Button-TextMeshPro - "继续游戏")
     |   |- SettingsButton (Button-TextMeshPro - "设置")
     |   |- MainMenuButton (Button-TextMeshPro - "主菜单")
     |   |- QuitButton (Button-TextMeshPro - "退出游戏")
   ```

3. 设置按钮和布局
4. 将PauseMenuPanel预制体保存到Resources/UI/目录下，命名为"PauseMenuPanel"

### 步骤2：添加PauseMenuPanel组件

1. 在PauseMenuPanel预制体上添加PauseMenuPanel脚本组件
2. 确保路径与AutoBind特性中的路径一致

## 将UI管理器和控制器添加到场景

要使UI框架正常工作，需要在场景中添加几个关键的管理器。最好在一个单独的GameObject上添加这些组件，例如可以命名为"GameManagers"。

1. 创建一个空的GameObject，命名为"GameManagers"
2. 添加以下组件：
   - UIManager
   - UIInitializer
   - PopupSystem
   - MainMenuController
   - SettingsController
   - AudioManager
   - PauseMenuController (仅游戏场景需要)

3. 配置每个控制器的设置：
   - 确保所有面板路径正确设置（例如："UI/MainMenuPanel"）
   - 配置场景名称引用（例如：主菜单场景名称、游戏场景名称）

## 配置场景加载和引用

1. 打开Build Settings（File > Build Settings）
2. 添加以下场景：
   - MainMenuScene（主菜单场景，索引0）
   - GameScene（游戏场景，索引1）
   - 其他游戏场景...

3. 确保主菜单控制器和暂停菜单控制器中的场景名称与Build Settings中的场景名称匹配

## 初始启动游戏

1. 确保主菜单场景是首先加载的场景（在Build Settings中将其设为索引0）
2. 在UIInitializer的Awake方法中，它会自动创建UI层级并初始化UI管理器
3. 如果想自动打开主菜单，可以在MainMenuController的Start方法中调用OpenPanel()
4. 测试所有UI导航流程，确保按钮和面板正常工作

## 特别说明

1. **关于自动绑定**：AutoBind特性依赖于正确的路径字符串，确保UI层次结构与代码中的路径匹配
2. **必须将预制体放在Resources目录下**：所有UI预制体必须放在Resources目录或其子目录下，才能被正确加载
3. **单例管理器**：所有管理器都是基于BaseManager实现的单例模式，应确保场景中只有一个实例
4. **调试技巧**：如果UI组件未正确显示，请检查Console窗口查看错误信息，常见问题包括路径不匹配或预制体缺失

按照本指南设置UI后，你将能够使用强大的可复用UI框架，包括主菜单、设置界面、消息弹窗和暂停菜单。 