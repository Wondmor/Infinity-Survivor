using UnityEngine;
using System;

namespace TrianCatStudio
{
    /// <summary>
    /// 游戏设置控制器，负责管理游戏设置的加载、保存和应用
    /// </summary>
    public class SettingsController : BaseUIController<SettingsPanel>
    {
        // 预制体路径
        [SerializeField] private string panelPrefabPath = "UI/SettingsPanel";
        
        // 设置存储键
        private const string MUSIC_VOLUME_KEY = "MusicVolume";
        private const string SFX_VOLUME_KEY = "SFXVolume";
        private const string FULLSCREEN_KEY = "Fullscreen";
        private const string QUALITY_LEVEL_KEY = "QualityLevel";
        private const string RESOLUTION_INDEX_KEY = "ResolutionIndex";
        private const string LANGUAGE_KEY = "Language";
        
        // 默认设置值
        private const float DEFAULT_MUSIC_VOLUME = 0.8f;
        private const float DEFAULT_SFX_VOLUME = 0.8f;
        private const bool DEFAULT_FULLSCREEN = true;
        private const int DEFAULT_QUALITY_LEVEL = 2; // 中等画质
        private const int DEFAULT_RESOLUTION_INDEX = 0;
        private const string DEFAULT_LANGUAGE = "简体中文";
        
        // 面板预制体路径
        protected override string PanelPrefabPath => panelPrefabPath;
        
        /// <summary>
        /// 保存设置
        /// </summary>
        public void SaveSettings(SettingsData settings)
        {
            // 保存设置到PlayerPrefs
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, settings.MusicVolume);
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, settings.SFXVolume);
            PlayerPrefs.SetInt(FULLSCREEN_KEY, settings.Fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(QUALITY_LEVEL_KEY, settings.QualityLevel);
            PlayerPrefs.SetInt(RESOLUTION_INDEX_KEY, settings.ResolutionIndex);
            PlayerPrefs.SetString(LANGUAGE_KEY, settings.Language);
            
            // 保存更改
            PlayerPrefs.Save();
            
            Debug.Log("[SettingsController] 设置已保存");
            
            // 应用设置
            ApplySettings(settings);
            
            // 发送设置更改事件
            OnSettingsChanged?.Invoke(settings);
        }
        
        /// <summary>
        /// 加载设置
        /// </summary>
        public SettingsData LoadSettings()
        {
            // 从PlayerPrefs加载设置，如果不存在则使用默认值
            float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, DEFAULT_MUSIC_VOLUME);
            float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, DEFAULT_SFX_VOLUME);
            bool fullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, DEFAULT_FULLSCREEN ? 1 : 0) == 1;
            int qualityLevel = PlayerPrefs.GetInt(QUALITY_LEVEL_KEY, DEFAULT_QUALITY_LEVEL);
            int resolutionIndex = PlayerPrefs.GetInt(RESOLUTION_INDEX_KEY, DEFAULT_RESOLUTION_INDEX);
            string language = PlayerPrefs.GetString(LANGUAGE_KEY, DEFAULT_LANGUAGE);
            
            Debug.Log("[SettingsController] 设置已加载");
            
            return new SettingsData(
                musicVolume,
                sfxVolume,
                fullscreen,
                qualityLevel,
                resolutionIndex,
                language
            );
        }
        
        /// <summary>
        /// 重置设置为默认值
        /// </summary>
        public SettingsData ResetSettings()
        {
            // 创建默认设置
            SettingsData defaultSettings = new SettingsData(
                DEFAULT_MUSIC_VOLUME,
                DEFAULT_SFX_VOLUME,
                DEFAULT_FULLSCREEN,
                DEFAULT_QUALITY_LEVEL,
                DEFAULT_RESOLUTION_INDEX,
                DEFAULT_LANGUAGE
            );
            
            Debug.Log("[SettingsController] 设置已重置为默认值");
            
            // 保存默认设置
            SaveSettings(defaultSettings);
            
            return defaultSettings;
        }
        
        /// <summary>
        /// 应用设置
        /// </summary>
        private void ApplySettings(SettingsData settings)
        {
            // 应用音频设置
            var audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                audioManager.SetMusicVolume(settings.MusicVolume);
                audioManager.SetSFXVolume(settings.SFXVolume);
            }
            
            // 应用显示设置
            Screen.fullScreen = settings.Fullscreen;
            QualitySettings.SetQualityLevel(settings.QualityLevel);
            
            // 应用分辨率设置
            if (settings.ResolutionIndex >= 0 && settings.ResolutionIndex < Screen.resolutions.Length)
            {
                Resolution resolution = Screen.resolutions[settings.ResolutionIndex];
                Screen.SetResolution(resolution.width, resolution.height, settings.Fullscreen);
            }
            
            // 应用语言设置 (需要多语言系统支持)
            // TODO: 实现语言切换
            
            Debug.Log($"[SettingsController] 设置已应用: 音乐音量={settings.MusicVolume}, 音效音量={settings.SFXVolume}, 全屏={settings.Fullscreen}, 画质={settings.QualityLevel}");
        }
        
        /// <summary>
        /// 打开设置面板
        /// </summary>
        public override void OpenPanel(Action<SettingsPanel> onPanelCreated = null)
        {
            // 创建一个新的回调，用于在面板打开时设置数据
            Action<SettingsPanel> callback = panel => {
                // 执行原回调
                onPanelCreated?.Invoke(panel);
                
                // 加载设置并设置到面板
                SettingsData settings = LoadSettings();
                panel.SetData(settings);
            };
            
            // 使用新回调调用基类方法
            base.OpenPanel(callback);
        }
        
        /// <summary>
        /// 设置更改事件
        /// </summary>
        public event Action<SettingsData> OnSettingsChanged;
    }
    
    /// <summary>
    /// 设置数据模型
    /// </summary>
    [System.Serializable]
    public class SettingsData
    {
        public float MusicVolume { get; private set; }
        public float SFXVolume { get; private set; }
        public bool Fullscreen { get; private set; }
        public int QualityLevel { get; private set; }
        public int ResolutionIndex { get; private set; }
        public string Language { get; private set; }
        
        public SettingsData(float musicVolume, float sfxVolume, bool fullscreen, int qualityLevel, int resolutionIndex, string language)
        {
            MusicVolume = musicVolume;
            SFXVolume = sfxVolume;
            Fullscreen = fullscreen;
            QualityLevel = qualityLevel;
            ResolutionIndex = resolutionIndex;
            Language = language;
        }
    }
}