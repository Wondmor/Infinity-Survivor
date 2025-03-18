using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using TrianCatStudio;

namespace TrianCatStudio
{
    /// <summary>
    /// 游戏暂停菜单控制器
    /// </summary>
    public class PauseMenuController : BaseUIController<PauseMenuPanel>
    {
        [Header("设置")]
        [SerializeField] private string mainMenuSceneName = "MainMenuScene"; // 主菜单场景名称
        [SerializeField] private string panelPrefabPath = "UI/PauseMenuPanel"; // 面板预制体路径
        
        [Header("按键设置")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape; // 暂停按键
        
        // 是否允许暂停
        private bool canPause = true;
        
        // 修正属性名为PanelPrefabPath
        protected override string PanelPrefabPath => panelPrefabPath;
        
        private void Update()
        {
            // 检测暂停按键
            if (canPause && Input.GetKeyDown(pauseKey))
            {
                TogglePause();
            }
        }
        
        /// <summary>
        /// 切换暂停状态
        /// </summary>
        public void TogglePause()
        {
            if (IsPanelOpened)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
        
        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            Debug.Log("[PauseMenuController] 暂停游戏");
            
            // 打开暂停菜单
            OpenPanel();
            
            // 暂停时间将在面板的PreInitialize中处理
        }
        
        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void ResumeGame()
        {
            Debug.Log("[PauseMenuController] 恢复游戏");
            
            // 关闭暂停菜单
            ClosePanel();
            
            // 时间恢复将在面板的OnClose中处理
        }
        
        /// <summary>
        /// 返回主菜单
        /// </summary>
        public void ReturnToMainMenu()
        {
            Debug.Log("[PauseMenuController] 返回主菜单");
            
            // 关闭所有UI
            UIManager uiManager = UIManager.Instance as UIManager;
            if (uiManager != null)
            {
                uiManager.CloseAllPanels();
                
                PopupSystem popupSystem = PopupSystem.Instance as PopupSystem;
                if (popupSystem != null)
                {
                    popupSystem.CloseAllPopups();
                }
            }
            
            // 加载主菜单场景
            SceneController.Instance.LoadMainMenu();
        }
        
        /// <summary>
        /// 启用/禁用暂停功能
        /// </summary>
        public void SetPauseEnabled(bool enabled)
        {
            canPause = enabled;
            
            // 如果禁用暂停功能，同时当前处于暂停状态，则恢复游戏
            if (!enabled && IsPanelOpened)
            {
                ResumeGame();
            }
            
            Debug.Log($"[PauseMenuController] 暂停功能已{(enabled ? "启用" : "禁用")}");
        }
    }
} 