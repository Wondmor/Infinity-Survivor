using System;
using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// UI控制器接口
    /// </summary>
    public interface IUIController<TPanel> where TPanel : BasePanel
    {
        void OpenPanel(Action<TPanel> onOpened = null);
        void ClosePanel();
        void SetPanelData(object data);
    }
    
    /// <summary>
    /// UI控制器基类
    /// </summary>
    public abstract class BaseUIController<TPanel> : BaseManager<BaseUIController<TPanel>>, IUIController<TPanel> 
        where TPanel : BasePanel
    {
        private TPanel currentPanel;
        
        /// <summary>
        /// 面板预制体路径
        /// </summary>
        protected abstract string PanelPrefabPath { get; }
        
        /// <summary>
        /// 默认UI层级
        /// </summary>
        protected virtual UIManager.UILayer PanelLayer => UIManager.UILayer.Main;
        
        /// <summary>
        /// 当前面板是否已打开
        /// </summary>
        public bool IsPanelOpened => currentPanel != null;
        
        /// <summary>
        /// 打开面板
        /// </summary>
        public virtual void OpenPanel(Action<TPanel> onOpened = null)
        {
            // 如果已经打开，直接回调
            if (currentPanel != null)
            {
                onOpened?.Invoke(currentPanel);
                return;
            }
            
            // 加载预制体资源
            GameObject prefab = Resources.Load<GameObject>(PanelPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[{GetType().Name}] 打开面板失败：未找到预制体 {PanelPrefabPath}");
                return;
            }
            
            // 实例化面板
            GameObject panelObj = GameObject.Instantiate(prefab, UIManager.Instance.GetLayerTransform(PanelLayer));
            if (panelObj == null)
            {
                Debug.LogError($"[{GetType().Name}] 打开面板失败：实例化预制体失败");
                return;
            }
            
            // 获取面板组件
            currentPanel = panelObj.GetComponent<TPanel>();
            if (currentPanel == null)
            {
                Debug.LogError($"[{GetType().Name}] 打开面板失败：预制体没有 {typeof(TPanel).Name} 组件");
                GameObject.Destroy(panelObj);
                return;
            }
            
            // 初始化面板
            currentPanel.Initialize(() => {
                Debug.Log($"[{GetType().Name}] 面板已打开");
                onOpened?.Invoke(currentPanel);
            });
        }
        
        /// <summary>
        /// 关闭面板
        /// </summary>
        public virtual void ClosePanel()
        {
            if (currentPanel == null)
                return;
                
            currentPanel.Close();
            currentPanel = null;
        }
        
        /// <summary>
        /// 设置面板数据
        /// </summary>
        public virtual void SetPanelData(object data)
        {
            if (currentPanel == null)
            {
                Debug.LogWarning($"[{GetType().Name}] 设置数据失败：面板未打开");
                return;
            }
            
            currentPanel.SetData(data);
        }
        
        /// <summary>
        /// 刷新面板
        /// </summary>
        public virtual void RefreshPanel()
        {
            if (currentPanel == null)
            {
                Debug.LogWarning($"[{GetType().Name}] 刷新失败：面板未打开");
                return;
            }
            
            currentPanel.Refresh();
        }
    }
} 