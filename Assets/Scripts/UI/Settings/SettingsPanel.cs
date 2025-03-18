using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace TrianCatStudio
{
    /// <summary>
    /// 游戏设置面板，负责显示和编辑游戏设置
    /// </summary>
    public class SettingsPanel : BasePanel
    {
        [Header("标题设置")]
        [SerializeField] private TextMeshProUGUI titleText;
        
        [Header("音频设置")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private TextMeshProUGUI musicVolumeText;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TextMeshProUGUI sfxVolumeText;
        [SerializeField] private Button testSfxButton;
        
        [Header("显示设置")]
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private TMP_Dropdown qualityDropdown;
        
        [Header("其他设置")]
        [SerializeField] private TMP_Dropdown languageDropdown;
        
        [Header("按钮")]
        [SerializeField] private Button applyButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button closeButton;
        
        // 当前设置值
        private float currentMusicVolume;
        private float currentSFXVolume;
        private bool currentFullscreen;
        private int currentQualityLevel;
        private int currentResolutionIndex;
        private string currentLanguage;
        
        // 所有可用的分辨率选项
        private Resolution[] availableResolutions;
        
        // 是否已修改但未保存
        private bool hasUnsavedChanges = false;
        
        // 原始设置数据（用于取消更改）
        private SettingsData originalSettings;
        
        /// <summary>
        /// 初始化前的准备工作
        /// </summary>
        protected override void PreInitialize()
        {
            base.PreInitialize();
            
            // 初始化组件引用（AutoBind代替）
            if (titleText == null) titleText = transform.FindDeepChild<TextMeshProUGUI>("TitleText");
            if (musicVolumeSlider == null) musicVolumeSlider = transform.FindDeepChild<Slider>("AudioSettings/MusicVolumeSlider");
            if (musicVolumeText == null) musicVolumeText = transform.FindDeepChild<TextMeshProUGUI>("AudioSettings/MusicVolumeText");
            if (sfxVolumeSlider == null) sfxVolumeSlider = transform.FindDeepChild<Slider>("AudioSettings/SFXVolumeSlider");
            if (sfxVolumeText == null) sfxVolumeText = transform.FindDeepChild<TextMeshProUGUI>("AudioSettings/SFXVolumeText");
            if (testSfxButton == null) testSfxButton = transform.FindDeepChild<Button>("AudioSettings/TestSFXButton");
            if (fullscreenToggle == null) fullscreenToggle = transform.FindDeepChild<Toggle>("DisplaySettings/FullscreenToggle");
            if (resolutionDropdown == null) resolutionDropdown = transform.FindDeepChild<TMP_Dropdown>("DisplaySettings/ResolutionDropdown");
            if (qualityDropdown == null) qualityDropdown = transform.FindDeepChild<TMP_Dropdown>("DisplaySettings/QualityDropdown");
            if (languageDropdown == null) languageDropdown = transform.FindDeepChild<TMP_Dropdown>("OtherSettings/LanguageDropdown");
            if (applyButton == null) applyButton = transform.FindDeepChild<Button>("Buttons/ApplyButton");
            if (resetButton == null) resetButton = transform.FindDeepChild<Button>("Buttons/ResetButton");
            if (closeButton == null) closeButton = transform.FindDeepChild<Button>("Buttons/CloseButton");
            
            // 初始化UI组件事件
            InitializeUIEvents();
            
            // 初始化下拉菜单选项
            InitializeDropdowns();
        }
        
        /// <summary>
        /// 初始化UI组件事件
        /// </summary>
        private void InitializeUIEvents()
        {
            // 音量滑块事件
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }
            
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
            
            // 测试音效按钮
            if (testSfxButton != null)
            {
                testSfxButton.onClick.AddListener(OnTestSFXClicked);
            }
            
            // 全屏切换事件
            if (fullscreenToggle != null)
            {
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
            }
            
            // 分辨率下拉框事件
            if (resolutionDropdown != null)
            {
                resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            }
            
            // 画质下拉框事件
            if (qualityDropdown != null)
            {
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }
            
            // 语言下拉框事件
            if (languageDropdown != null)
            {
                languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
            }
            
            // 按钮点击事件
            if (applyButton != null)
            {
                applyButton.onClick.AddListener(OnApplyClicked);
            }
            
            if (resetButton != null)
            {
                resetButton.onClick.AddListener(OnResetClicked);
            }
            
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }
        }
        
        /// <summary>
        /// 初始化下拉菜单选项
        /// </summary>
        private void InitializeDropdowns()
        {
            // 初始化画质下拉框
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                
                string[] qualityNames = QualitySettings.names;
                List<TMP_Dropdown.OptionData> qualityOptions = new List<TMP_Dropdown.OptionData>();
                
                foreach (string qualityName in qualityNames)
                {
                    qualityOptions.Add(new TMP_Dropdown.OptionData(qualityName));
                }
                
                qualityDropdown.AddOptions(qualityOptions);
            }
            
            // 初始化分辨率下拉框
            if (resolutionDropdown != null)
            {
                resolutionDropdown.ClearOptions();
                
                // 获取所有可用分辨率
                availableResolutions = Screen.resolutions;
                
                List<TMP_Dropdown.OptionData> resolutionOptions = new List<TMP_Dropdown.OptionData>();
                for (int i = 0; i < availableResolutions.Length; i++)
                {
                    Resolution resolution = availableResolutions[i];
                    string option = $"{resolution.width} x {resolution.height} @{resolution.refreshRate}Hz";
                    resolutionOptions.Add(new TMP_Dropdown.OptionData(option));
                }
                
                resolutionDropdown.AddOptions(resolutionOptions);
            }
            
            // 初始化语言下拉框
            if (languageDropdown != null)
            {
                languageDropdown.ClearOptions();
                
                // 添加支持的语言选项
                List<TMP_Dropdown.OptionData> languageOptions = new List<TMP_Dropdown.OptionData>
                {
                    new TMP_Dropdown.OptionData("简体中文"),
                    new TMP_Dropdown.OptionData("English"),
                    new TMP_Dropdown.OptionData("日本語")
                };
                
                languageDropdown.AddOptions(languageOptions);
            }
        }
        
        /// <summary>
        /// 设置面板数据
        /// </summary>
        public override void SetData(object data)
        {
            if (data is SettingsData settings)
            {
                // 保存原始设置，用于取消更改
                originalSettings = settings;
                
                // 更新UI和当前值
                UpdateUIWithSettings(settings);
                
                // 重置未保存更改状态
                hasUnsavedChanges = false;
                UpdateApplyButtonInteractable();
            }
        }
        
        /// <summary>
        /// 使用设置数据更新UI
        /// </summary>
        private void UpdateUIWithSettings(SettingsData settings)
        {
            // 保存当前设置值
            currentMusicVolume = settings.MusicVolume;
            currentSFXVolume = settings.SFXVolume;
            currentFullscreen = settings.Fullscreen;
            currentQualityLevel = settings.QualityLevel;
            currentResolutionIndex = settings.ResolutionIndex;
            currentLanguage = settings.Language;
            
            // 更新音量滑块
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = currentMusicVolume;
            }
            
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = currentSFXVolume;
            }
            
            // 更新音量文本
            UpdateVolumeTexts();
            
            // 更新全屏切换
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = currentFullscreen;
            }
            
            // 更新画质下拉框
            if (qualityDropdown != null && qualityDropdown.options.Count > 0)
            {
                qualityDropdown.value = Mathf.Clamp(currentQualityLevel, 0, qualityDropdown.options.Count - 1);
            }
            
            // 更新分辨率下拉框
            if (resolutionDropdown != null && resolutionDropdown.options.Count > 0)
            {
                resolutionDropdown.value = Mathf.Clamp(currentResolutionIndex, 0, resolutionDropdown.options.Count - 1);
            }
            
            // 更新语言下拉框
            if (languageDropdown != null && languageDropdown.options.Count > 0)
            {
                // 查找语言索引
                int languageIndex = 0;
                for (int i = 0; i < languageDropdown.options.Count; i++)
                {
                    if (languageDropdown.options[i].text == currentLanguage)
                    {
                        languageIndex = i;
                        break;
                    }
                }
                
                languageDropdown.value = languageIndex;
            }
        }
        
        /// <summary>
        /// 更新音量文本显示
        /// </summary>
        private void UpdateVolumeTexts()
        {
            if (musicVolumeText != null)
            {
                musicVolumeText.text = $"{Mathf.RoundToInt(currentMusicVolume * 100)}%";
            }
            
            if (sfxVolumeText != null)
            {
                sfxVolumeText.text = $"{Mathf.RoundToInt(currentSFXVolume * 100)}%";
            }
        }
        
        /// <summary>
        /// 更新应用按钮交互状态
        /// </summary>
        private void UpdateApplyButtonInteractable()
        {
            if (applyButton != null)
            {
                applyButton.interactable = hasUnsavedChanges;
            }
        }
        
        /// <summary>
        /// 标记设置已更改
        /// </summary>
        private void MarkSettingsChanged()
        {
            hasUnsavedChanges = true;
            UpdateApplyButtonInteractable();
        }
        
        /// <summary>
        /// 创建当前设置数据对象
        /// </summary>
        private SettingsData CreateCurrentSettingsData()
        {
            return new SettingsData(
                currentMusicVolume,
                currentSFXVolume,
                currentFullscreen,
                currentQualityLevel,
                currentResolutionIndex,
                currentLanguage
            );
        }
        
        #region UI事件处理
        
        /// <summary>
        /// 音乐音量变化处理
        /// </summary>
        private void OnMusicVolumeChanged(float value)
        {
            currentMusicVolume = value;
            UpdateVolumeTexts();
            
            // 实时应用音乐音量变化
            var audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                audioManager.SetMusicVolume(value);
            }
            
            MarkSettingsChanged();
        }
        
        /// <summary>
        /// 音效音量变化处理
        /// </summary>
        private void OnSFXVolumeChanged(float value)
        {
            currentSFXVolume = value;
            UpdateVolumeTexts();
            
            // 实时应用音效音量变化
            var audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                audioManager.SetSFXVolume(value);
            }
            
            MarkSettingsChanged();
        }
        
        /// <summary>
        /// 测试音效按钮点击处理
        /// </summary>
        private void OnTestSFXClicked()
        {
            // 播放测试音效
            var audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                // 使用默认测试音效或加载专门的测试音效
                AudioClip testClip = Resources.Load<AudioClip>("Audio/UI/ButtonClick");
                if (testClip != null)
                {
                    audioManager.PlaySFX(testClip);
                }
                else
                {
                    Debug.LogWarning("[SettingsPanel] 测试音效资源未找到");
                }
            }
        }
        
        /// <summary>
        /// 全屏切换处理
        /// </summary>
        private void OnFullscreenToggled(bool isOn)
        {
            currentFullscreen = isOn;
            MarkSettingsChanged();
        }
        
        /// <summary>
        /// 分辨率变化处理
        /// </summary>
        private void OnResolutionChanged(int index)
        {
            currentResolutionIndex = index;
            MarkSettingsChanged();
        }
        
        /// <summary>
        /// 画质变化处理
        /// </summary>
        private void OnQualityChanged(int index)
        {
            currentQualityLevel = index;
            MarkSettingsChanged();
        }
        
        /// <summary>
        /// 语言变化处理
        /// </summary>
        private void OnLanguageChanged(int index)
        {
            if (index >= 0 && index < languageDropdown.options.Count)
            {
                currentLanguage = languageDropdown.options[index].text;
                MarkSettingsChanged();
            }
        }
        
        /// <summary>
        /// 应用按钮点击处理
        /// </summary>
        private void OnApplyClicked()
        {
            // 创建设置数据
            SettingsData settings = CreateCurrentSettingsData();
            
            // 保存设置
            var controller = SettingsController.Instance as SettingsController;
            if (controller != null)
            {
                controller.SaveSettings(settings);
                
                // 更新原始设置
                originalSettings = settings;
                
                // 设置已保存，没有未保存的更改
                hasUnsavedChanges = false;
                UpdateApplyButtonInteractable();
                
                Debug.Log("[SettingsPanel] 设置已应用");
            }
        }
        
        /// <summary>
        /// 重置按钮点击处理
        /// </summary>
        private void OnResetClicked()
        {
            // 重置为默认设置
            var controller = SettingsController.Instance as SettingsController;
            if (controller != null)
            {
                SettingsData defaultSettings = controller.ResetSettings();
                
                // 更新UI
                UpdateUIWithSettings(defaultSettings);
                
                // 更新原始设置
                originalSettings = defaultSettings;
                
                // 设置已重置，没有未保存的更改
                hasUnsavedChanges = false;
                UpdateApplyButtonInteractable();
                
                Debug.Log("[SettingsPanel] 设置已重置为默认值");
            }
        }
        
        /// <summary>
        /// 关闭按钮点击处理
        /// </summary>
        private void OnCloseClicked()
        {
            if (hasUnsavedChanges)
            {
                // 有未保存的更改，显示确认弹窗
                MessagePopupData data = new MessagePopupData(
                    "未保存的更改",
                    "您有未保存的设置更改，是否保存？",
                    () => {
                        // 保存并关闭
                        OnApplyClicked();
                        Close();
                    },
                    () => {
                        // 放弃更改并关闭
                        if (originalSettings != null)
                        {
                            // 恢复原始设置
                            var controller = SettingsController.Instance as SettingsController;
                            if (controller != null)
                            {
                                controller.SaveSettings(originalSettings);
                            }
                        }
                        Close();
                    }
                );
                
                data.ConfirmText = "保存";
                data.CancelText = "放弃";
                
                // 显示弹窗
                var popupSystem = PopupSystem.Instance as PopupSystem;
                if (popupSystem != null)
                {
                    popupSystem.ShowPopup<MessagePopup>(data);
                }
                else
                {
                    // 弹窗系统不可用，直接关闭面板
                    Close();
                }
            }
            else
            {
                // 没有未保存的更改，直接关闭
                Close();
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Transform扩展方法，用于查找深层子对象
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// 根据路径查找深层子对象
        /// </summary>
        public static Transform FindDeepChild(this Transform parent, string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
                
            return parent.Find(path);
        }
        
        /// <summary>
        /// 根据路径查找深层子对象组件
        /// </summary>
        public static T FindDeepChild<T>(this Transform parent, string path) where T : Component
        {
            Transform child = parent.FindDeepChild(path);
            if (child != null)
                return child.GetComponent<T>();
                
            return null;
        }
    }
} 