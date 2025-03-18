using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TrianCatStudio
{
    /// <summary>
    /// 主菜单面板
    /// </summary>
    public class MainMenuPanel : BasePanel
    {
        [Header("标题")]
        [AutoBind("Title/TitleText")] private TextMeshProUGUI titleText;
        
        [Header("按钮")]
        [AutoBind("Buttons/StartGameButton")] private Button startGameButton;
        [AutoBind("Buttons/SettingsButton")] private Button settingsButton;
        [AutoBind("Buttons/QuitGameButton")] private Button quitGameButton;
        
        /// <summary>
        /// 游戏版本文本
        /// </summary>
        [AutoBind("VersionText")] private TextMeshProUGUI versionText;
        
        protected override void PreInitialize()
        {
            base.PreInitialize();
            
            // 初始化按钮事件
            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(OnStartButtonClicked);
            }
            
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            }
            
            if (quitGameButton != null)
            {
                quitGameButton.onClick.AddListener(OnQuitButtonClicked);
            }
            
            // 设置版本信息
            if (versionText != null)
            {
                versionText.text = $"版本: {Application.version}";
            }
        }
        
        /// <summary>
        /// 设置菜单数据
        /// </summary>
        public override void SetData(object data)
        {
            if (data is MainMenuData menuData)
            {
                // 设置标题
                if (titleText != null)
                {
                    titleText.text = menuData.Title;
                }
            }
        }
        
        /// <summary>
        /// 开始游戏按钮点击事件
        /// </summary>
        private void OnStartButtonClicked()
        {
            MainMenuController controller = MainMenuController.Instance as MainMenuController;
            controller.StartGame();
        }
        
        /// <summary>
        /// 设置按钮点击事件
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            MainMenuController controller = MainMenuController.Instance as MainMenuController;
            controller.OpenSettings();
        }
        
        /// <summary>
        /// 退出游戏按钮点击事件
        /// </summary>
        private void OnQuitButtonClicked()
        {
            MainMenuController controller = MainMenuController.Instance as MainMenuController;
            controller.QuitGame();
        }
    }
    
    /// <summary>
    /// 主菜单数据
    /// </summary>
    public class MainMenuData
    {
        /// <summary>
        /// 菜单标题
        /// </summary>
        public string Title { get; private set; }
        
        public MainMenuData(string title)
        {
            Title = title;
        }
    }
} 