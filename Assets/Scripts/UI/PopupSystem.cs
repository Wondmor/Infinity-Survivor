using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 弹窗管理系统
    /// </summary>
    public class PopupSystem : BaseManager<PopupSystem>
    {
        // 弹窗配置
        [Serializable]
        public class PopupConfig
        {
            public string PrefabPath;                            // 预制体路径
            public PopupPosition DefaultPosition = PopupPosition.Center; // 默认位置
            public Vector2 Offset = Vector2.zero;                // 偏移量
            public bool UseOverlay = true;                       // 是否使用遮罩
            public float OverlayOpacity = 0.5f;                  // 遮罩透明度
        }
        
        // 弹窗配置字典
        private Dictionary<string, PopupConfig> popupConfigs = new Dictionary<string, PopupConfig>();
        
        // 弹窗队列
        private Queue<PopupQueueItem> popupQueue = new Queue<PopupQueueItem>();
        
        // 是否有弹窗正在显示
        private bool isPopupShowing = false;
        
        // 弹窗队列项
        private class PopupQueueItem
        {
            public string PopupName;              // 弹窗名称
            public string PrefabPath;             // 预制体路径
            public PopupPosition Position;        // 弹窗位置
            public Vector2 Offset;                // 偏移
            public object Data;                   // 数据
            public Action<BasePopup> OnPopupCreated; // 弹窗创建回调
            
            public PopupQueueItem(string popupName, string prefabPath, PopupPosition position, Vector2 offset, object data, Action<BasePopup> onPopupCreated)
            {
                PopupName = popupName;
                PrefabPath = prefabPath;
                Position = position;
                Offset = offset;
                Data = data;
                OnPopupCreated = onPopupCreated;
            }
        }
        
        /// <summary>
        /// 初始化弹窗配置
        /// </summary>
        public void InitializeConfigs(Dictionary<string, PopupConfig> configs)
        {
            if (configs == null)
                return;
                
            popupConfigs = configs;
            Debug.Log($"[PopupSystem] 初始化了 {configs.Count} 个弹窗配置");
        }
        
        /// <summary>
        /// 添加弹窗配置
        /// </summary>
        public void AddPopupConfig(string popupName, PopupConfig config)
        {
            if (string.IsNullOrEmpty(popupName) || config == null)
                return;
                
            popupConfigs[popupName] = config;
            Debug.Log($"[PopupSystem] 添加弹窗配置: {popupName}");
        }
        
        /// <summary>
        /// 显示弹窗（泛型方法）
        /// </summary>
        public void ShowPopup<T>(Action<T> onCreated = null, PopupPosition position = PopupPosition.Center) where T : BasePopup
        {
            string popupName = typeof(T).Name;
            
            // 创建一个适配器将泛型回调转换为非泛型回调
            Action<BasePopup> adapter = null;
            if (onCreated != null)
            {
                adapter = (popup) => 
                {
                    if (popup is T typedPopup)
                    {
                        onCreated(typedPopup);
                    }
                };
            }
            
            ShowPopup(popupName, null, adapter, position);
        }
        
        /// <summary>
        /// 显示弹窗（带数据）
        /// </summary>
        public void ShowPopup<T>(object data, Action<T> onCreated = null, PopupPosition position = PopupPosition.Center) where T : BasePopup
        {
            string popupName = typeof(T).Name;
            
            // 创建一个适配器将泛型回调转换为非泛型回调
            Action<BasePopup> adapter = null;
            if (onCreated != null)
            {
                adapter = (popup) => 
                {
                    if (popup is T typedPopup)
                    {
                        onCreated(typedPopup);
                    }
                };
            }
            
            ShowPopup(popupName, data, adapter, position);
        }
        
        /// <summary>
        /// 显示弹窗（非泛型方法）
        /// </summary>
        public void ShowPopup(string popupName, object data = null, Action<BasePopup> onCreated = null, PopupPosition position = PopupPosition.Center)
        {
            // 获取弹窗配置
            if (!popupConfigs.TryGetValue(popupName, out PopupConfig config))
            {
                Debug.LogError($"[PopupSystem] 显示弹窗失败: 未找到弹窗配置 {popupName}");
                return;
            }
            
            // 使用弹窗配置的默认位置
            if (position == PopupPosition.Center)
            {
                position = config.DefaultPosition;
            }
            
            // 将弹窗添加到队列
            popupQueue.Enqueue(new PopupQueueItem(
                popupName,
                config.PrefabPath,
                position,
                config.Offset,
                data,
                onCreated
            ));
            
            // 如果当前没有弹窗显示，则显示队列中的第一个弹窗
            if (!isPopupShowing)
            {
                ShowNextPopupInQueue();
            }
        }
        
        /// <summary>
        /// 显示队列中的下一个弹窗
        /// </summary>
        private void ShowNextPopupInQueue()
        {
            if (popupQueue.Count == 0)
            {
                isPopupShowing = false;
                return;
            }
            
            isPopupShowing = true;
            
            // 获取队列中的下一个弹窗
            PopupQueueItem item = popupQueue.Dequeue();
            
            // 启动协程显示弹窗
            MonoBehaviourProxy.Instance.StartCoroutine(ShowPopupCoroutine(item));
        }
        
        /// <summary>
        /// 显示弹窗的协程
        /// </summary>
        private IEnumerator ShowPopupCoroutine(PopupQueueItem item)
        {
            Debug.Log($"[PopupSystem] 显示弹窗: {item.PopupName}");
            
            // 加载预制体
            GameObject prefab = Resources.Load<GameObject>(item.PrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[PopupSystem] 显示弹窗失败: 未找到预制体 {item.PrefabPath}");
                isPopupShowing = false;
                ShowNextPopupInQueue();
                yield break;
            }
            
            // 实例化弹窗
            var uiManager = UIManager.Instance as UIManager;
            GameObject popupObj = GameObject.Instantiate(prefab, uiManager.GetLayerTransform(UIManager.UILayer.Popup));
            if (popupObj == null)
            {
                Debug.LogError($"[PopupSystem] 显示弹窗失败: 实例化预制体失败 {item.PrefabPath}");
                isPopupShowing = false;
                ShowNextPopupInQueue();
                yield break;
            }
            
            // 获取弹窗组件
            BasePopup popup = popupObj.GetComponent<BasePopup>();
            if (popup == null)
            {
                Debug.LogError($"[PopupSystem] 显示弹窗失败: 预制体没有BasePopup组件 {item.PrefabPath}");
                GameObject.Destroy(popupObj);
                isPopupShowing = false;
                ShowNextPopupInQueue();
                yield break;
            }
            
            // 设置弹窗位置
            popup.SetPosition(item.Position, item.Offset);
            
            // 设置数据
            if (item.Data != null)
            {
                popup.SetData(item.Data);
            }
            
            // 初始化弹窗
            popup.Initialize();
            
            // 调用回调
            if (item.OnPopupCreated != null)
            {
                try
                {
                    item.OnPopupCreated(popup);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[PopupSystem] 弹窗回调异常: {e.Message}\n{e.StackTrace}");
                }
            }
            
            // 监听弹窗关闭事件
            MonoBehaviourProxy.Instance.StartCoroutine(WaitForPopupClose(popup));
            
            yield break;
        }
        
        /// <summary>
        /// 等待弹窗关闭的协程
        /// </summary>
        private IEnumerator WaitForPopupClose(BasePopup popup)
        {
            // 等待弹窗被销毁
            while (popup != null && popup.gameObject != null)
            {
                yield return null;
            }
            
            // 弹窗已关闭，显示队列中的下一个弹窗
            isPopupShowing = false;
            ShowNextPopupInQueue();
        }
        
        /// <summary>
        /// 关闭所有弹窗并清空队列
        /// </summary>
        public void CloseAllPopups()
        {
            // 清空弹窗队列
            popupQueue.Clear();
            
            // 关闭所有已打开的弹窗
            var uiManager = UIManager.Instance as UIManager;
            uiManager.CloseAllPopups();
            
            isPopupShowing = false;
        }
    }
    
    /// <summary>
    /// MonoBehaviour代理，用于在非MonoBehaviour类中执行协程
    /// </summary>
    public class MonoBehaviourProxy : MonoBehaviour
    {
        private static MonoBehaviourProxy instance;
        
        public static MonoBehaviourProxy Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("MonoBehaviourProxy");
                    DontDestroyOnLoad(go);
                    instance = go.AddComponent<MonoBehaviourProxy>();
                }
                return instance;
            }
        }
    }
} 