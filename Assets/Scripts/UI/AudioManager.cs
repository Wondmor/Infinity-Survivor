using UnityEngine;
using System.Collections.Generic;

namespace TrianCatStudio
{
    /// <summary>
    /// 音频管理器
    /// </summary>
    public class AudioManager : SingletonAutoMono<AudioManager>
    {
        // 音频设置
        [System.Serializable]
        public class AudioSettings
        {
            [Range(0f, 1f)] public float masterVolume = 1.0f;
            [Range(0f, 1f)] public float musicVolume = 0.8f;
            [Range(0f, 1f)] public float sfxVolume = 0.8f;
        }
        
        [Header("音频设置")]
        [SerializeField] private AudioSettings settings = new AudioSettings();
        
        [Header("音乐设置")]
        [SerializeField] private AudioSource musicSource; // 背景音乐
        [SerializeField] private float musicFadeDuration = 1.0f; // 音乐淡入淡出时间
        
        [Header("音效设置")]
        [SerializeField] private int sfxPoolSize = 5; // 音效对象池大小
        
        // 音效对象池
        private List<AudioSource> sfxPool = new List<AudioSource>();
        
        // 当前播放的背景音乐
        private AudioClip currentMusic;
        
        // 是否正在淡入淡出
        private bool isFading = false;
        private float fadeStartTime;
        private float fadeStartVolume;
        private float fadeTargetVolume;
        
        private void Awake()
        {
            // 初始化背景音乐
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.parent = transform;
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            
            // 初始化音效对象池
            for (int i = 0; i < sfxPoolSize; i++)
            {
                CreateSFXSource();
            }
            
            // 应用初始音量设置
            ApplyVolumeSettings();
        }
        
        private void Update()
        {
            // 处理音乐淡入淡出
            if (isFading)
            {
                float elapsed = Time.time - fadeStartTime;
                float t = Mathf.Clamp01(elapsed / musicFadeDuration);
                
                musicSource.volume = Mathf.Lerp(fadeStartVolume, fadeTargetVolume, t);
                
                if (t >= 1.0f)
                {
                    isFading = false;
                }
            }
        }
        
        /// <summary>
        /// 创建一个音效音频源
        /// </summary>
        private AudioSource CreateSFXSource()
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.parent = transform;
            
            AudioSource sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            
            sfxPool.Add(sfxSource);
            
            return sfxSource;
        }
        
        /// <summary>
        /// 应用音量设置
        /// </summary>
        private void ApplyVolumeSettings()
        {
            // 应用音乐音量
            if (musicSource != null)
            {
                musicSource.volume = settings.masterVolume * settings.musicVolume;
            }
            
            // 应用音效音量（预设值，实际播放时会应用）
            foreach (AudioSource sfxSource in sfxPool)
            {
                sfxSource.volume = settings.masterVolume * settings.sfxVolume;
            }
        }
        
        /// <summary>
        /// 播放背景音乐
        /// </summary>
        public void PlayMusic(AudioClip musicClip, bool fadeIn = true)
        {
            if (musicClip == null)
                return;
                
            // 如果已经在播放这个音乐，不做任何操作
            if (currentMusic == musicClip && musicSource.isPlaying)
                return;
                
            currentMusic = musicClip;
            
            if (fadeIn)
            {
                // 淡入新音乐
                musicSource.clip = musicClip;
                musicSource.volume = 0f;
                musicSource.Play();
                
                // 开始淡入
                StartFade(0f, settings.masterVolume * settings.musicVolume);
            }
            else
            {
                // 直接播放
                musicSource.clip = musicClip;
                musicSource.volume = settings.masterVolume * settings.musicVolume;
                musicSource.Play();
            }
            
            Debug.Log($"[AudioManager] 开始播放音乐: {musicClip.name}");
        }
        
        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public void StopMusic(bool fadeOut = true)
        {
            if (musicSource.isPlaying)
            {
                if (fadeOut)
                {
                    // 淡出后停止
                    StartFade(musicSource.volume, 0f);
                    
                    // 创建一个协程来在淡出后停止音乐
                    StartCoroutine(StopMusicAfterFade());
                }
                else
                {
                    // 直接停止
                    musicSource.Stop();
                }
                
                Debug.Log("[AudioManager] 停止播放音乐");
            }
        }
        
        /// <summary>
        /// 淡出后停止音乐的协程
        /// </summary>
        private System.Collections.IEnumerator StopMusicAfterFade()
        {
            // 等待淡出完成
            yield return new WaitForSeconds(musicFadeDuration);
            
            // 如果音量已经为0，停止音乐
            if (musicSource.volume <= 0.01f)
            {
                musicSource.Stop();
            }
        }
        
        /// <summary>
        /// 开始淡入淡出
        /// </summary>
        private void StartFade(float startVolume, float targetVolume)
        {
            isFading = true;
            fadeStartTime = Time.time;
            fadeStartVolume = startVolume;
            fadeTargetVolume = targetVolume;
        }
        
        /// <summary>
        /// 播放音效
        /// </summary>
        public void PlaySFX(AudioClip sfxClip, float volumeScale = 1.0f)
        {
            if (sfxClip == null)
                return;
                
            // 获取一个可用的音效音频源
            AudioSource sfxSource = GetAvailableSFXSource();
            
            // 设置音效并播放
            sfxSource.clip = sfxClip;
            sfxSource.volume = settings.masterVolume * settings.sfxVolume * volumeScale;
            sfxSource.Play();
            
            Debug.Log($"[AudioManager] 播放音效: {sfxClip.name}");
        }
        
        /// <summary>
        /// 获取一个可用的音效音频源
        /// </summary>
        private AudioSource GetAvailableSFXSource()
        {
            // 尝试找到一个没有在播放的音频源
            foreach (AudioSource source in sfxPool)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            
            // 如果都在播放，创建一个新的
            return CreateSFXSource();
        }
        
        /// <summary>
        /// 设置主音量
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            settings.masterVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }
        
        /// <summary>
        /// 设置音乐音量
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            settings.musicVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }
        
        /// <summary>
        /// 设置音效音量
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            settings.sfxVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }
        
        /// <summary>
        /// 获取音乐音量
        /// </summary>
        public float GetMusicVolume()
        {
            return settings.musicVolume;
        }
        
        /// <summary>
        /// 获取音效音量
        /// </summary>
        public float GetSFXVolume()
        {
            return settings.sfxVolume;
        }
    }
} 