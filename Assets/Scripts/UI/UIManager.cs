using UnityEngine;
using System.Collections.Generic;

namespace TrianCatStudio
{
    /// <summary>
    /// UI系统的核心管理器，负责管理UI层级和画布
    /// </summary>
    public class UIManager : BaseManager<UIManager>
    {
        // UI层级定义
        public enum UILayer
        {
            Background = 0, // 背景层
            Main = 1,       // 主界面层
            Popup = 2,      // 弹窗层
            Loading = 3     // 加载层
        }
        
        // 层级根节点映射
        private Dictionary<UILayer, Transform> layerRoots = new Dictionary<UILayer, Transform>();
        
        // 当前打开的面板
        private Dictionary<string, BasePanel> openedPanels = new Dictionary<string, BasePanel>();
        
        // 当前显示的弹窗栈
        private Stack<BasePopup> popupStack = new Stack<BasePopup>();
        
        // 主画布
        private Canvas mainCanvas;
        private Camera uiCamera;
        
        /// <summary>
        /// 初始化UI管理器
        /// </summary>
        /// <param name="mainCanvasObject">主画布对象</param>
        public void Initialize(GameObject mainCanvasObject)
        {
            Debug.Log("[UIManager] 初始化UI系统");
            
            if (mainCanvasObject == null)
            {
                Debug.LogError("[UIManager] 初始化失败：主画布对象为空");
                return;
            }
            
            mainCanvas = mainCanvasObject.GetComponent<Canvas>();
            if (mainCanvas == null)
            {
                Debug.LogError("[UIManager] 初始化失败：找不到Canvas组件");
                return;
            }
            
            uiCamera = mainCanvas.worldCamera;
            
            // 确保存在各个层级的根节点
            EnsureLayerRoots(mainCanvasObject.transform);
            
            Debug.Log("[UIManager] 初始化完成");
        }
        
        /// <summary>
        /// 确保各层级根节点存在
        /// </summary>
        private void EnsureLayerRoots(Transform canvasTransform)
        {
            // 枚举所有层级
            foreach (UILayer layer in System.Enum.GetValues(typeof(UILayer)))
            {
                string layerName = $"{layer}Layer";
                Transform layerRoot = canvasTransform.Find(layerName);
                
                // 如果层级不存在，创建它
                if (layerRoot == null)
                {
                    Debug.Log($"[UIManager] 创建层级: {layerName}");
                    
                    GameObject layerObj = new GameObject(layerName);
                    layerObj.transform.SetParent(canvasTransform, false);
                    
                    RectTransform rect = layerObj.AddComponent<RectTransform>();
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    
                    // 设置层级顺序
                    rect.SetSiblingIndex((int)layer);
                    
                    layerRoot = layerObj.transform;
                }
                
                layerRoots[layer] = layerRoot;
            }
        }
        
        /// <summary>
        /// 获取指定层级的变换
        /// </summary>
        public Transform GetLayerTransform(UILayer layer)
        {
            if (layerRoots.TryGetValue(layer, out Transform transform))
            {
                return transform;
            }
            
            Debug.LogError($"[UIManager] 未找到层级: {layer}");
            return null;
        }
        
        /// <summary>
        /// 注册已打开的面板
        /// </summary>
        public void RegisterPanel(string panelName, BasePanel panel)
        {
            if (string.IsNullOrEmpty(panelName) || panel == null)
                return;
                
            openedPanels[panelName] = panel;
        }
        
        /// <summary>
        /// 注销已关闭的面板
        /// </summary>
        public void UnregisterPanel(string panelName)
        {
            if (string.IsNullOrEmpty(panelName))
                return;
                
            if (openedPanels.ContainsKey(panelName))
            {
                openedPanels.Remove(panelName);
            }
        }
        
        /// <summary>
        /// 获取已打开的面板
        /// </summary>
        public T GetPanel<T>() where T : BasePanel
        {
            string panelName = typeof(T).Name;
            
            if (openedPanels.TryGetValue(panelName, out BasePanel panel))
            {
                return panel as T;
            }
            
            return null;
        }
        
        /// <summary>
        /// 检查面板是否已打开
        /// </summary>
        public bool IsPanelOpened<T>() where T : BasePanel
        {
            string panelName = typeof(T).Name;
            return openedPanels.ContainsKey(panelName);
        }
        
        /// <summary>
        /// 关闭所有打开的面板
        /// </summary>
        public void CloseAllPanels()
        {
            foreach (var panel in openedPanels.Values)
            {
                if (panel != null)
                {
                    panel.Close();
                }
            }
            
            openedPanels.Clear();
        }
        
        /// <summary>
        /// 将弹窗入栈
        /// </summary>
        public void PushPopup(BasePopup popup)
        {
            if (popup == null)
                return;
                
            popupStack.Push(popup);
        }
        
        /// <summary>
        /// 弹出栈顶弹窗
        /// </summary>
        public BasePopup PopPopup()
        {
            if (popupStack.Count > 0)
            {
                return popupStack.Pop();
            }
            
            return null;
        }
        
        /// <summary>
        /// 关闭所有弹窗
        /// </summary>
        public void CloseAllPopups()
        {
            while (popupStack.Count > 0)
            {
                BasePopup popup = popupStack.Pop();
                if (popup != null)
                {
                    popup.Close();
                }
            }
        }
    }
} 