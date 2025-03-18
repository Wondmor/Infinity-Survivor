using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TrianCatStudio
{
    /// <summary>
    /// 游戏暂停菜单面板
    /// </summary>
    public class PauseMenuPanel : BasePanel
    {
        [Header("按钮")]
        [AutoBind("Buttons/ResumeButton")] private Button resumeButton;
        [AutoBind("Buttons/SettingsButton")] private Button settingsButton;
        [AutoBind("Buttons/MainMenuButton")] private Button mainMenuButton;
        [AutoBind("Buttons/QuitButton")] private Button quitButton;
        
        protected override void PreInitialize()
        {
            base.PreInitialize();
            
            // 初始化按钮事件
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnResumeButtonClicked);
            }
            
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            }
            
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitButtonClicked);
            }
            
            // 暂停游戏
            Time.timeScale = 0f;
        }
        
        /// <summary>
        /// 面板关闭时恢复游戏
        /// </summary>
        private void OnDestroy()
        {
            // 当面板被销毁时恢复游戏运行
            Time.timeScale = 1f;
        }
        
        /// <summary>
        /// 继续游戏按钮点击事件
        /// </summary>
        private void OnResumeButtonClicked()
        {
            // 直接关闭面板，会自动恢复游戏
            PauseMenuController controller = PauseMenuController.Instance as PauseMenuController;
            controller.ResumeGame();
        }
        
        /// <summary>
        /// 设置按钮点击事件
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            Debug.Log("[PauseMenuPanel] 打开设置");
            
            // 打开设置面板
            SettingsController settingsController = SettingsController.Instance as SettingsController;
            if (settingsController != null)
            {
                settingsController.OpenPanel();
            }
            else
            {
                Debug.LogError("[PauseMenuPanel] 设置控制器未找到");
            }
        }
        
        /// <summary>
        /// 主菜单按钮点击事件
        /// </summary>
        private void OnMainMenuButtonClicked()
        {
            Debug.Log("[PauseMenuPanel] 返回主菜单确认");
            
            // 创建确认弹窗
            MessagePopupData data = new MessagePopupData(
                "返回主菜单",
                "确定要返回主菜单吗？当前进度将会丢失。",
                () => {
                    // 确认返回
                    Debug.Log("[PauseMenuPanel] 用户确认返回主菜单");
                    
                    // 恢复正常时间流
                    Time.timeScale = 1f;
                    
                    // 返回主菜单
                    PauseMenuController controller = PauseMenuController.Instance as PauseMenuController;
                    controller.ReturnToMainMenu();
                },
                () => {
                    // 取消返回
                    Debug.Log("[PauseMenuPanel] 用户取消返回主菜单");
                }
            );
            
            // 设置按钮文本
            data.ConfirmText = "确定";
            data.CancelText = "取消";
            
            // 显示弹窗
            PopupSystem popupSystem = PopupSystem.Instance as PopupSystem;
            if (popupSystem != null)
            {
                popupSystem.ShowPopup<MessagePopup>(data);
            }
            else
            {
                Debug.LogError("[PauseMenuPanel] 弹窗系统未找到");
            }
        }
        
        /// <summary>
        /// 退出游戏按钮点击事件
        /// </summary>
        private void OnQuitButtonClicked()
        {
            Debug.Log("[PauseMenuPanel] 退出游戏确认");
            
            // 创建确认弹窗
            MessagePopupData data = new MessagePopupData(
                "退出游戏",
                "确定要退出游戏吗？当前进度将会丢失。",
                () => {
                    // 确认退出
                    Debug.Log("[PauseMenuPanel] 用户确认退出游戏");
                    
                    // 恢复正常时间流
                    Time.timeScale = 1f;
                    
                    #if UNITY_EDITOR
                    // 在编辑器中停止播放模式
                    UnityEditor.EditorApplication.isPlaying = false;
                    #else
                    // 在构建版本中退出应用
                    Application.Quit();
                    #endif
                },
                () => {
                    // 取消退出
                    Debug.Log("[PauseMenuPanel] 用户取消退出游戏");
                }
            );
            
            // 设置按钮文本
            data.ConfirmText = "确定";
            data.CancelText = "取消";
            
            // 显示弹窗
            PopupSystem popupSystem = PopupSystem.Instance as PopupSystem;
            if (popupSystem != null)
            {
                popupSystem.ShowPopup<MessagePopup>(data);
            }
            else
            {
                Debug.LogError("[PauseMenuPanel] 弹窗系统未找到");
            }
        }
    }
} 