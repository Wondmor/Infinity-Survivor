using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 设置控制器示例
    /// </summary>
    public class SettingsController : BaseUIController<SettingsPanel>
    {
        /// <summary>
        /// 面板预制体路径
        /// </summary>
        protected override string PanelPrefabPath => "UI/Panels/SettingsPanel";
        
        /// <summary>
        /// 保存设置
        /// </summary>
        public void SaveSettings(float musicVolume, float sfxVolume, bool fullscreen, int qualityLevel)
        {
            // 保存音频设置
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            
            // 保存显示设置
            PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
            PlayerPrefs.SetInt("QualityLevel", qualityLevel);
            
            // 应用设置
            PlayerPrefs.Save();
            ApplySettings(musicVolume, sfxVolume, fullscreen, qualityLevel);
            
            Debug.Log("[SettingsController] 设置已保存");
        }
        
        /// <summary>
        /// 加载设置
        /// </summary>
        public SettingsData LoadSettings()
        {
            // 从PlayerPrefs加载保存的设置，如果不存在则使用默认值
            float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            int qualityLevel = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
            
            Debug.Log("[SettingsController] 设置已加载");
            
            return new SettingsData(musicVolume, sfxVolume, fullscreen, qualityLevel);
        }
        
        /// <summary>
        /// 应用设置
        /// </summary>
        private void ApplySettings(float musicVolume, float sfxVolume, bool fullscreen, int qualityLevel)
        {
            // 应用音频设置（实际项目中可能需要一个音频管理器）
            // AudioManager.Instance.SetMusicVolume(musicVolume);
            // AudioManager.Instance.SetSFXVolume(sfxVolume);
            
            // 应用显示设置
            Screen.fullScreen = fullscreen;
            QualitySettings.SetQualityLevel(qualityLevel);
            
            Debug.Log($"[SettingsController] 设置已应用 - 音乐: {musicVolume}, 音效: {sfxVolume}, 全屏: {fullscreen}, 画质: {qualityLevel}");
        }
        
        /// <summary>
        /// 重置设置为默认值
        /// </summary>
        public void ResetSettings()
        {
            // 默认设置值
            float defaultMusicVolume = 0.75f;
            float defaultSFXVolume = 0.75f;
            bool defaultFullscreen = true;
            int defaultQualityLevel = 2; // 中等画质
            
            // 保存默认设置
            SaveSettings(defaultMusicVolume, defaultSFXVolume, defaultFullscreen, defaultQualityLevel);
            
            // 如果面板已打开，更新界面
            if (IsPanelOpened)
            {
                SettingsData defaultData = new SettingsData(
                    defaultMusicVolume, 
                    defaultSFXVolume, 
                    defaultFullscreen, 
                    defaultQualityLevel
                );
                
                SetPanelData(defaultData);
            }
            
            Debug.Log("[SettingsController] 设置已重置为默认值");
        }
    }
    
    /// <summary>
    /// 设置数据
    /// </summary>
    public class SettingsData
    {
        public float MusicVolume { get; private set; }
        public float SFXVolume { get; private set; }
        public bool Fullscreen { get; private set; }
        public int QualityLevel { get; private set; }
        
        public SettingsData(float musicVolume, float sfxVolume, bool fullscreen, int qualityLevel)
        {
            MusicVolume = musicVolume;
            SFXVolume = sfxVolume;
            Fullscreen = fullscreen;
            QualityLevel = qualityLevel;
        }
    }
} 