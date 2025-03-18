using UnityEngine;
using System.Collections.Generic;

namespace TrianCatStudio
{
    /// <summary>
    /// UI系统初始化器，负责初始化UI相关的管理器
    /// </summary>
    public class UIInitializer : MonoBehaviour
    {
        [Header("UI Canvas")]
        [SerializeField] private GameObject mainCanvas;
        
        [Header("Popup Configs")]
        [SerializeField] private List<PopupConfigItem> popupConfigs = new List<PopupConfigItem>();
        
        // 弹窗配置项
        [System.Serializable]
        public class PopupConfigItem
        {
            public string PopupName;
            public PopupSystem.PopupConfig Config;
        }
        
        private void Awake()
        {
            // 初始化UI管理器
            InitializeUIManager();
            
            // 初始化弹窗系统
            InitializePopupSystem();
            
            Debug.Log("[UIInitializer] UI系统初始化完成");
        }
        
        /// <summary>
        /// 初始化UI管理器
        /// </summary>
        private void InitializeUIManager()
        {
            if (mainCanvas == null)
            {
                Debug.LogError("[UIInitializer] 初始化UI管理器失败：主画布为空");
                return;
            }
            
            UIManager.Instance.Initialize(mainCanvas);
        }
        
        /// <summary>
        /// 初始化弹窗系统
        /// </summary>
        private void InitializePopupSystem()
        {
            // 初始化弹窗配置
            Dictionary<string, PopupSystem.PopupConfig> configs = new Dictionary<string, PopupSystem.PopupConfig>();
            
            foreach (var item in popupConfigs)
            {
                if (string.IsNullOrEmpty(item.PopupName) || item.Config == null)
                    continue;
                    
                configs[item.PopupName] = item.Config;
            }
            
            PopupSystem.Instance.InitializeConfigs(configs);
        }
    }
} 