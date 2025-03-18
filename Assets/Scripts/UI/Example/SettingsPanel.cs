using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace TrianCatStudio
{
    /// <summary>
    /// 设置面板示例
    /// </summary>
    public class SettingsPanel : BasePanel
    {
        [Header("Audio Settings")]
        [AutoBind("AudioSettings/MusicVolumeSlider")] private Slider musicVolumeSlider;
        [AutoBind("AudioSettings/MusicVolumeText")] private TextMeshProUGUI musicVolumeText;
        [AutoBind("AudioSettings/SFXVolumeSlider")] private Slider sfxVolumeSlider;
        [AutoBind("AudioSettings/SFXVolumeText")] private TextMeshProUGUI sfxVolumeText;
        
        [Header("Display Settings")]
        [AutoBind("DisplaySettings/FullscreenToggle")] private Toggle fullscreenToggle;
        [AutoBind("DisplaySettings/QualityDropdown")] private TMP_Dropdown qualityDropdown;
        
        [Header("Buttons")]
        [AutoBind("Buttons/SaveButton")] private Button saveButton;
        [AutoBind("Buttons/ResetButton")] private Button resetButton;
        [AutoBind("Buttons/CloseButton")] private Button closeButton;
        
        // 当前设置值
        private float currentMusicVolume;
        private float currentSFXVolume;
        private bool currentFullscreen;
        private int currentQualityLevel;
        
        // 是否已修改但未保存
        private bool hasUnsavedChanges = false;
        
        protected override void PreInitialize()
        {
            base.PreInitialize();
            
            // 初始化UI组件事件
            InitializeUIEvents();
            
            // 加载当前设置
            LoadCurrentSettings();
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
            
            // 全屏切换事件
            if (fullscreenToggle != null)
            {
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
            }
            
            // 画质下拉框事件
            if (qualityDropdown != null)
            {
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }
            
            // 按钮点击事件
            if (saveButton != null)
            {
                saveButton.onClick.AddListener(OnSaveClicked);
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
        /// 加载当前设置
        /// </summary>
        private void LoadCurrentSettings()
        {
            // 从设置控制器加载设置
            var controller = SettingsController.Instance as SettingsController;
            SettingsData settings = controller.LoadSettings();
            
            // 更新UI和当前值
            UpdateUIWithSettings(settings);
            
            // 初始状态标记为无未保存更改
            hasUnsavedChanges = false;
            UpdateSaveButtonInteractable();
        }
        
        /// <summary>
        /// 设置数据
        /// </summary>
        public override void SetData(object data)
        {
            if (data is SettingsData settings)
            {
                UpdateUIWithSettings(settings);
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
            if (qualityDropdown != null)
            {
                // 确保下拉框有足够的选项
                if (qualityDropdown.options.Count == 0)
                {
                    PopulateQualityDropdown();
                }
                
                qualityDropdown.value = currentQualityLevel;
            }
        }
        
        /// <summary>
        /// 填充画质下拉框选项
        /// </summary>
        private void PopulateQualityDropdown()
        {
            if (qualityDropdown == null)
                return;
            
            qualityDropdown.ClearOptions();
            
            string[] qualityNames = QualitySettings.names;
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            
            foreach (string qualityName in qualityNames)
            {
                options.Add(new TMP_Dropdown.OptionData(qualityName));
            }
            
            qualityDropdown.AddOptions(options);
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
        /// 更新保存按钮交互状态
        /// </summary>
        private void UpdateSaveButtonInteractable()
        {
            if (saveButton != null)
            {
                saveButton.interactable = hasUnsavedChanges;
            }
        }
        
        #region UI事件处理
        
        /// <summary>
        /// 音乐音量变化
        /// </summary>
        private void OnMusicVolumeChanged(float value)
        {
            currentMusicVolume = value;
            UpdateVolumeTexts();
            
            // 临时应用音量，但不保存（实际项目中可能需要一个音频管理器）
            // AudioManager.Instance.SetMusicVolume(value);
            
            hasUnsavedChanges = true;
            UpdateSaveButtonInteractable();
        }
        
        /// <summary>
        /// 音效音量变化
        /// </summary>
        private void OnSFXVolumeChanged(float value)
        {
            currentSFXVolume = value;
            UpdateVolumeTexts();
            
            // 临时应用音量，但不保存（实际项目中可能需要一个音频管理器）
            // AudioManager.Instance.SetSFXVolume(value);
            
            hasUnsavedChanges = true;
            UpdateSaveButtonInteractable();
        }
        
        /// <summary>
        /// 全屏切换
        /// </summary>
        private void OnFullscreenToggled(bool isOn)
        {
            currentFullscreen = isOn;
            
            // 临时应用，但不保存
            Screen.fullScreen = isOn;
            
            hasUnsavedChanges = true;
            UpdateSaveButtonInteractable();
        }
        
        /// <summary>
        /// 画质变化
        /// </summary>
        private void OnQualityChanged(int index)
        {
            currentQualityLevel = index;
            
            // 临时应用，但不保存
            QualitySettings.SetQualityLevel(index);
            
            hasUnsavedChanges = true;
            UpdateSaveButtonInteractable();
        }
        
        /// <summary>
        /// 保存按钮点击
        /// </summary>
        private void OnSaveClicked()
        {
            // 保存设置
            var controller = SettingsController.Instance as SettingsController;
            controller.SaveSettings(
                currentMusicVolume,
                currentSFXVolume,
                currentFullscreen,
                currentQualityLevel
            );
            
            // 更新UI状态
            hasUnsavedChanges = false;
            UpdateSaveButtonInteractable();
            
            // 显示保存成功提示（在实际项目中可能是一个提示弹窗）
            Debug.Log("[SettingsPanel] 设置已保存");
        }
        
        /// <summary>
        /// 重置按钮点击
        /// </summary>
        private void OnResetClicked()
        {
            // 调用控制器重置设置
            var controller = SettingsController.Instance as SettingsController;
            controller.ResetSettings();
            
            // 面板数据会通过SetData自动更新
        }
        
        /// <summary>
        /// 关闭按钮点击
        /// </summary>
        private void OnCloseClicked()
        {
            // 如果有未保存的更改，显示确认弹窗
            if (hasUnsavedChanges)
            {
                // 创建弹窗数据
                MessagePopupData data = new MessagePopupData(
                    "未保存的更改",
                    "你有未保存的设置更改，是否保存后关闭？",
                    () => {
                        // 保存后关闭
                        OnSaveClicked();
                        var controller = SettingsController.Instance as SettingsController;
                        controller.ClosePanel();
                    },
                    () => {
                        // 不保存直接关闭
                        var controller = SettingsController.Instance as SettingsController;
                        controller.ClosePanel();
                    }
                );
                
                // 设置按钮文本
                data.ConfirmText = "保存并关闭";
                data.CancelText = "不保存关闭";
                
                // 显示弹窗
                var popupSystem = PopupSystem.Instance as PopupSystem;
                popupSystem.ShowPopup<MessagePopup>(data);
            }
            else
            {
                // 直接关闭
                var controller = SettingsController.Instance as SettingsController;
                controller.ClosePanel();
            }
        }
        
        #endregion
    }
} 