using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 主菜单UI控制器示例
    /// </summary>
    public class MainMenuController : BaseUIController<MainMenuPanel>
    {
        /// <summary>
        /// 面板预制体路径
        /// </summary>
        protected override string PanelPrefabPath => "UI/Panels/MainMenuPanel";
        
        /// <summary>
        /// 打开设置面板
        /// </summary>
        public void OpenSettings()
        {
            // 检查设置控制器并打开面板
            SettingsController.Instance.OpenPanel();
        }
        
        /// <summary>
        /// 显示消息弹窗
        /// </summary>
        public void ShowMessagePopup(string title, string content, System.Action onConfirm = null, System.Action onCancel = null)
        {
            // 创建弹窗数据
            MessagePopupData data = new MessagePopupData(title, content, onConfirm, onCancel);
            
            // 显示弹窗
            PopupSystem.Instance.ShowPopup<MessagePopup>(data);
        }
        
        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartGame()
        {
            Debug.Log("[MainMenuController] 开始游戏");
            
            // 关闭主菜单面板
            ClosePanel();
            
            // 加载游戏场景（示例）
            // SceneManager.LoadScene("GameScene");
        }
        
        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
            // 显示确认弹窗
            ShowMessagePopup(
                "退出游戏", 
                "确定要退出游戏吗？", 
                () => {
                    Debug.Log("[MainMenuController] 退出游戏");
                    
                    #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                    #else
                    Application.Quit();
                    #endif
                }
            );
        }
    }
} 