using UnityEngine;
using UnityEngine.UI;

namespace TrianCatStudio
{
    /// <summary>
    /// 设置系统测试脚本，用于演示设置面板的使用
    /// </summary>
    public class SettingsTest : MonoBehaviour
    {
        [Header("UI初始化")]
        [SerializeField] private GameObject mainCanvasPrefab;
        [SerializeField] private Button openSettingsButton;
        
        private void Start()
        {
            // 初始化UI系统
            InitializeUISystem();
            
            // 设置按钮事件
            if (openSettingsButton != null)
            {
                openSettingsButton.onClick.AddListener(OpenSettingsPanel);
            }
        }
        
        /// <summary>
        /// 初始化UI系统
        /// </summary>
        private void InitializeUISystem()
        {
            // 检查UIManager是否已存在
            if (UIManager.Instance == null)
            {
                Debug.LogError("[SettingsTest] UIManager不存在，请确保UIManager已初始化");
                return;
            }
            
            // 检查主画布是否存在
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            bool mainCanvasExists = false;
            
            foreach (Canvas canvas in canvases)
            {
                if (canvas.name == "MainCanvas" || canvas.name == "UICanvas")
                {
                    // 已存在主画布，初始化UIManager
                    UIManager.Instance.Initialize(canvas.gameObject);
                    mainCanvasExists = true;
                    break;
                }
            }
            
            // 如果不存在主画布，创建一个
            if (!mainCanvasExists && mainCanvasPrefab != null)
            {
                GameObject canvasObj = Instantiate(mainCanvasPrefab);
                canvasObj.name = "MainCanvas";
                UIManager.Instance.Initialize(canvasObj);
                
                Debug.Log("[SettingsTest] 创建并初始化主画布");
            }
            else if (!mainCanvasExists && mainCanvasPrefab == null)
            {
                // 如果没有预制体，则创建一个基本的画布
                GameObject canvasObj = new GameObject("MainCanvas");
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                
                UIManager.Instance.Initialize(canvasObj);
                
                Debug.Log("[SettingsTest] 创建并初始化基本画布");
            }
            
            // 初始化必要的控制器实例
            InitializeControllers();
        }
        
        /// <summary>
        /// 初始化必要的控制器实例
        /// </summary>
        private void InitializeControllers()
        {
            // 通过访问Instance属性确保AudioManager被初始化
            var audioManager = AudioManager.Instance;
            Debug.Log("[SettingsTest] 确保AudioManager实例已创建");
            
            // 通过访问Instance属性确保PopupSystem被初始化
            var popupSystem = PopupSystem.Instance;
            
            // 添加一些基本的弹窗配置
            var messagePopupConfig = new PopupSystem.PopupConfig
            {
                PrefabPath = "UI/MessagePopup",
                DefaultPosition = PopupPosition.Center,
                UseOverlay = true,
                OverlayOpacity = 0.5f
            };
            
            var configs = new System.Collections.Generic.Dictionary<string, PopupSystem.PopupConfig>
            {
                { "MessagePopup", messagePopupConfig }
            };
            
            popupSystem.InitializeConfigs(configs);
            Debug.Log("[SettingsTest] 确保PopupSystem实例已创建并配置");
            
            // 通过访问Instance属性确保SettingsController被初始化
            var settingsController = SettingsController.Instance;
            Debug.Log("[SettingsTest] 确保SettingsController实例已创建");
        }
        
        /// <summary>
        /// 打开设置面板
        /// </summary>
        public void OpenSettingsPanel()
        {
            // 打开设置面板
            var settingsController = SettingsController.Instance;
            if (settingsController != null)
            {
                settingsController.OpenPanel(panel => {
                    Debug.Log("[SettingsTest] 设置面板已打开");
                });
            }
            else
            {
                Debug.LogError("[SettingsTest] 设置控制器实例不存在");
            }
        }
    }
} 