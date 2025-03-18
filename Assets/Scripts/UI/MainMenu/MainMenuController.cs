using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace TrianCatStudio
{
    /// <summary>
    /// 主菜单控制器
    /// </summary>
    public class MainMenuController : BaseUIController<MainMenuPanel>
    {
        // 改为常量或配置项
        private const string DEFAULT_GAME_SCENE_NAME = "GameScene"; // 游戏场景名称
        private const string DEFAULT_PANEL_PREFAB_PATH = "UI/MainMenuPanel"; // 主菜单面板预制体路径
        
        // 可配置的场景名称和预制体路径
        private string gameSceneName = DEFAULT_GAME_SCENE_NAME;
        private string panelPrefabPath = DEFAULT_PANEL_PREFAB_PATH;
        
        // 实现基类抽象属性
        protected override string PanelPrefabPath => panelPrefabPath;
        
        /// <summary>
        /// 设置游戏场景名称
        /// </summary>
        public void SetGameSceneName(string sceneName)
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                gameSceneName = sceneName;
            }
        }
        
        /// <summary>
        /// 设置面板预制体路径
        /// </summary>
        public void SetPanelPrefabPath(string prefabPath)
        {
            if (!string.IsNullOrEmpty(prefabPath))
            {
                panelPrefabPath = prefabPath;
            }
        }
        
        /// <summary>
        /// 启动游戏
        /// </summary>
        public void StartGame()
        {
            Debug.Log("[MainMenuController] 开始游戏");
            
            // 关闭主菜单
            ClosePanel();
            
            // 使用MonoBehaviourProxy加载游戏场景
            MonoBehaviourProxy.Instance.StartCoroutine(LoadGameSceneCoroutine());
        }
        
        /// <summary>
        /// 加载游戏场景的协程
        /// </summary>
        private System.Collections.IEnumerator LoadGameSceneCoroutine()
        {
            // 加载游戏场景
            var operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(gameSceneName);
            yield return operation;
        }
        
        /// <summary>
        /// 打开设置
        /// </summary>
        public void OpenSettings()
        {
            Debug.Log("[MainMenuController] 打开设置");
            
            // 打开设置面板
            var settingsController = SettingsController.Instance as SettingsController;
            if (settingsController != null)
            {
                settingsController.OpenPanel();
            }
            else
            {
                Debug.LogError("[MainMenuController] 设置控制器未找到");
            }
        }
        
        /// <summary>
        /// 显示退出确认
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("[MainMenuController] 退出游戏确认");
            
            // 创建确认弹窗
            MessagePopupData data = new MessagePopupData(
                "退出游戏",
                "确定要退出游戏吗？",
                () => {
                    // 确认退出
                    Debug.Log("[MainMenuController] 用户确认退出游戏");
                    
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
                    Debug.Log("[MainMenuController] 用户取消退出游戏");
                }
            );
            
            // 设置按钮文本
            data.ConfirmText = "确定";
            data.CancelText = "取消";
            
            // 显示弹窗
            var popupSystem = PopupSystem.Instance;
            if (popupSystem != null)
            {
                popupSystem.ShowPopup<MessagePopup>(data);
            }
            else
            {
                Debug.LogError("[MainMenuController] 弹窗系统未找到");
            }
        }
    }
}