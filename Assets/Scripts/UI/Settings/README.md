# 设置界面系统使用指南

本文档将指导您如何设置和使用游戏设置界面系统。

## 1. 系统概述

设置界面系统包含以下核心组件：

- **SettingsController**：负责管理设置的加载、保存和应用
- **SettingsPanel**：负责展示和编辑设置的UI界面
- **SettingsData**：包含所有设置数据的模型类
- **SettingsTest**：测试脚本，用于快速初始化和测试设置界面

系统支持以下设置项：

- 音频设置（音乐音量、音效音量）
- 显示设置（全屏模式、分辨率、画质）
- 语言设置

## 2. 预制体设置

### 主画布预制体

在Resources目录下创建UI画布预制体 `Assets/Resources/UI/MainCanvas.prefab`：

1. 创建一个新的 Canvas 对象
2. 设置 Canvas 的 RenderMode 为 ScreenSpaceOverlay
3. 添加 CanvasScaler 组件并设置为 Scale With Screen Size
4. 添加 GraphicRaycaster 组件
5. 在 Canvas 下创建以下子对象：
   - Background (UI层级0)
   - Main (UI层级1)
   - Popup (UI层级2)
   - Loading (UI层级3)

### 设置面板预制体

在Resources目录下创建设置面板预制体 `Assets/Resources/UI/SettingsPanel.prefab`：

1. 创建设置面板UI布局，包含以下组件：
   - 标题文本 (TitleText)
   - 音频设置区域 (AudioSettings)
     - 音乐音量滑块 (MusicVolumeSlider)
     - 音乐音量文本 (MusicVolumeText)
     - 音效音量滑块 (SFXVolumeSlider)
     - 音效音量文本 (SFXVolumeText)
     - 测试音效按钮 (TestSFXButton)
   - 显示设置区域 (DisplaySettings)
     - 全屏切换开关 (FullscreenToggle)
     - 分辨率下拉框 (ResolutionDropdown)
     - 画质下拉框 (QualityDropdown)
   - 其他设置区域 (OtherSettings)
     - 语言下拉框 (LanguageDropdown)
   - 按钮区域 (Buttons)
     - 应用按钮 (ApplyButton)
     - 重置按钮 (ResetButton)
     - 关闭按钮 (CloseButton)
2. 添加 SettingsPanel 脚本到面板根对象
3. 将各UI组件拖拽到脚本相应的序列化字段

### 消息弹窗预制体

在Resources目录下创建消息弹窗预制体 `Assets/Resources/UI/MessagePopup.prefab`：

1. 创建消息弹窗UI布局，包含以下组件：
   - 标题文本 (TitleText)
   - 消息文本 (MessageText)
   - 确认按钮 (ConfirmButton)
   - 取消按钮 (CancelButton)
2. 添加 MessagePopup 脚本到弹窗根对象

## 3. 使用方法

### 基本使用流程

1. 确保场景中已初始化UI系统
2. 获取 SettingsController 实例
3. 调用 OpenPanel 方法打开设置面板
4. 面板会自动加载当前设置

```csharp
// 打开设置面板示例
var settingsController = SettingsController.Instance as SettingsController;
if (settingsController != null)
{
    settingsController.OpenPanel();
}
```

### 监听设置变更事件

您可以监听 SettingsController 的 OnSettingsChanged 事件来响应设置变化：

```csharp
// 监听设置变更事件
var settingsController = SettingsController.Instance as SettingsController;
if (settingsController != null)
{
    settingsController.OnSettingsChanged += OnSettingsChanged;
}

// 设置变更处理方法
private void OnSettingsChanged(SettingsData settings)
{
    Debug.Log("设置已变更");
    // 处理设置变更逻辑
}
```

### 手动加载和保存设置

您可以手动加载和保存设置：

```csharp
// 加载设置
var settingsController = SettingsController.Instance as SettingsController;
SettingsData settings = settingsController.LoadSettings();

// 修改设置
// ...

// 保存设置
settingsController.SaveSettings(settings);
```

## 4. 快速测试

1. 在场景中创建一个空游戏对象
2. 添加 SettingsTest 脚本
3. 可选：将主画布预制体拖拽到 mainCanvasPrefab 字段
4. 可选：将打开设置按钮拖拽到 openSettingsButton 字段
5. 运行场景，点击按钮或调用 SettingsTest.OpenSettingsPanel() 方法打开设置面板

## 5. 自定义和扩展

### 添加新的设置项

1. 在 SettingsData 类中添加新的属性
2. 在 SettingsController 中添加对应的存储键和默认值
3. 修改 SettingsController 的 SaveSettings、LoadSettings 和 ApplySettings 方法
4. 在 SettingsPanel 中添加对应的UI组件和事件处理

### 自定义样式

您可以自由修改预制体的外观和布局，只需确保保持组件的名称和层级结构与代码匹配。

## 6. 注意事项

- 确保所有预制体都放在 Resources 目录下，以便动态加载
- 确保场景中已正确初始化 UIManager
- 确保 AudioManager 实例可用，以便应用音频设置

## 7. 故障排除

**问题**: 设置面板无法打开
- 检查 UIManager 是否已初始化
- 检查设置面板预制体是否在正确的路径
- 检查控制台是否有错误日志

**问题**: 设置无法保存
- 检查 PlayerPrefs 是否可用
- 确保正确调用了 SaveSettings 方法

**问题**: 音频设置不生效
- 确保 AudioManager 实例已创建
- 确保 AudioManager 有正确实现 SetMusicVolume 和 SetSFXVolume 方法 