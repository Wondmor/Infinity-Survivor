using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace TrianCatStudio
{
    /// <summary>
    /// 主菜单面板示例
    /// </summary>
    public class MainMenuPanel : BasePanel
    {
        [Header("Buttons")]
        [AutoBind("Buttons/StartButton")] private Button startButton;
        [AutoBind("Buttons/SettingsButton")] private Button settingsButton;
        [AutoBind("Buttons/QuitButton")] private Button quitButton;
        
        [Header("Other UI Elements")]
        [AutoBind("Title")] private TextMeshProUGUI titleText;
        [AutoBind("Version")] private TextMeshProUGUI versionText;
        
        protected override void PreInitialize()
        {
            base.PreInitialize();
            
            // 初始化按钮点击事件
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartButtonClicked);
            }
            
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitButtonClicked);
            }
            
            // 设置版本信息
            if (versionText != null)
            {
                versionText.text = $"版本: {Application.version}";
            }
        }
        
        public override void SetData(object data)
        {
            if (data is MainMenuData menuData)
            {
                // 设置标题
                if (titleText != null && !string.IsNullOrEmpty(menuData.Title))
                {
                    titleText.text = menuData.Title;
                }
            }
        }
        
        /// <summary>
        /// 开始按钮点击事件
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
        /// 退出按钮点击事件
        /// </summary>
        private void OnQuitButtonClicked()
        {
            MainMenuController controller = MainMenuController.Instance as MainMenuController;
            controller.QuitGame();
        }
        
        /// <summary>
        /// 自定义入场动画
        /// </summary>
        protected override IEnumerator PlayEnterAnimation()
        {
            // 自定义淡入动画
            if (startButton != null) startButton.gameObject.SetActive(false);
            if (settingsButton != null) settingsButton.gameObject.SetActive(false);
            if (quitButton != null) quitButton.gameObject.SetActive(false);
            
            // 先执行基类动画
            yield return base.PlayEnterAnimation();
            
            // 然后逐个显示按钮
            if (startButton != null)
            {
                startButton.gameObject.SetActive(true);
                startButton.transform.localScale = Vector3.zero;
                
                float duration = 0.2f;
                float startTime = Time.time;
                
                while (Time.time - startTime < duration)
                {
                    float t = (Time.time - startTime) / duration;
                    startButton.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                    yield return null;
                }
                
                startButton.transform.localScale = Vector3.one;
            }
            
            yield return new WaitForSeconds(0.1f);
            
            if (settingsButton != null)
            {
                settingsButton.gameObject.SetActive(true);
                settingsButton.transform.localScale = Vector3.zero;
                
                float duration = 0.2f;
                float startTime = Time.time;
                
                while (Time.time - startTime < duration)
                {
                    float t = (Time.time - startTime) / duration;
                    settingsButton.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                    yield return null;
                }
                
                settingsButton.transform.localScale = Vector3.one;
            }
            
            yield return new WaitForSeconds(0.1f);
            
            if (quitButton != null)
            {
                quitButton.gameObject.SetActive(true);
                quitButton.transform.localScale = Vector3.zero;
                
                float duration = 0.2f;
                float startTime = Time.time;
                
                while (Time.time - startTime < duration)
                {
                    float t = (Time.time - startTime) / duration;
                    quitButton.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                    yield return null;
                }
                
                quitButton.transform.localScale = Vector3.one;
            }
        }
    }
    
    /// <summary>
    /// 主菜单数据
    /// </summary>
    public class MainMenuData
    {
        public string Title { get; set; }
        
        public MainMenuData(string title)
        {
            Title = title;
        }
    }
} 