using System;
using System.Collections;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TrianCatStudio
{
    /// <summary>
    /// 所有UI面板的基类
    /// </summary>
    public abstract class BasePanel : MonoBehaviour
    {
        [SerializeField] private UIManager.UILayer layer = UIManager.UILayer.Main;
        
        private CanvasGroup canvasGroup;
        private bool isInitialized = false;
        
        // 属性
        public UIManager.UILayer Layer => layer;
        public bool IsInitialized => isInitialized;
        
        #region 生命周期方法
        
        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        /// <summary>
        /// 初始化面板
        /// </summary>
        public void Initialize(Action onInitialized = null)
        {
            if (isInitialized)
                return;
                
            Debug.Log($"[{GetType().Name}] 初始化面板");
            
            // 自动绑定组件
            AutoBindComponents();
            
            // 面板初始化前的准备
            PreInitialize();
            
            // 标记为已初始化
            isInitialized = true;
            
            // 初始化完成回调
            onInitialized?.Invoke();
            
            // 播放入场动画
            StartCoroutine(PlayEnterAnimation());
            
            // 注册到UIManager
            UIManager.Instance.RegisterPanel(GetType().Name, this);
        }
        
        /// <summary>
        /// 关闭面板
        /// </summary>
        public void Close()
        {
            Debug.Log($"[{GetType().Name}] 关闭面板");
            
            // 播放退出动画，动画完成后销毁
            StartCoroutine(PlayExitAnimationAndDestroy());
        }
        
        /// <summary>
        /// 销毁面板
        /// </summary>
        private void DestroyPanel()
        {
            // 从UIManager注销
            UIManager.Instance.UnregisterPanel(GetType().Name);
            
            // 销毁游戏对象
            Destroy(gameObject);
        }
        
        #endregion
        
        #region 可重写的方法
        
        /// <summary>
        /// 面板初始化前的准备工作
        /// </summary>
        protected virtual void PreInitialize() { }
        
        /// <summary>
        /// 自定义数据设置
        /// </summary>
        public virtual void SetData(object data) { }
        
        /// <summary>
        /// 刷新面板内容
        /// </summary>
        public virtual void Refresh() { }
        
        #endregion
        
        #region 动画相关
        
        /// <summary>
        /// 播放入场动画
        /// </summary>
        protected virtual IEnumerator PlayEnterAnimation()
        {
            // 默认淡入动画
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                
                float duration = 0.3f;
                float startTime = Time.time;
                
                while (Time.time - startTime < duration)
                {
                    float t = (Time.time - startTime) / duration;
                    canvasGroup.alpha = t;
                    yield return null;
                }
                
                canvasGroup.alpha = 1;
            }
            
            yield break;
        }
        
        /// <summary>
        /// 播放退出动画然后销毁
        /// </summary>
        protected virtual IEnumerator PlayExitAnimationAndDestroy()
        {
            // 默认淡出动画
            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                
                float duration = 0.3f;
                float startTime = Time.time;
                
                while (Time.time - startTime < duration)
                {
                    float t = 1 - (Time.time - startTime) / duration;
                    canvasGroup.alpha = t;
                    yield return null;
                }
            }
            
            // 销毁面板
            DestroyPanel();
        }
        
        #endregion
        
        #region 组件绑定
        
        /// <summary>
        /// 自动绑定标记了AutoBindAttribute的字段
        /// </summary>
        private void AutoBindComponents()
        {
            FieldInfo[] fields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                AutoBindAttribute attr = field.GetCustomAttribute<AutoBindAttribute>();
                if (attr == null)
                    continue;
                
                string bindPath = string.IsNullOrEmpty(attr.Path) ? field.Name : attr.Path;
                Transform target = transform.Find(bindPath);
                
                if (target != null)
                {
                    // 获取正确的组件类型
                    Component component = null;
                    
                    if (field.FieldType == typeof(GameObject))
                    {
                        field.SetValue(this, target.gameObject);
                        continue;
                    }
                    else if (field.FieldType == typeof(Transform) || field.FieldType == typeof(RectTransform))
                    {
                        component = target;
                    }
                    else
                    {
                        component = target.GetComponent(field.FieldType);
                    }
                    
                    if (component != null)
                    {
                        field.SetValue(this, component);
                    }
                    else
                    {
                        Debug.LogWarning($"[{GetType().Name}] 无法绑定组件 {field.Name}: 未找到类型为 {field.FieldType.Name} 的组件");
                    }
                }
                else
                {
                    Debug.LogWarning($"[{GetType().Name}] 无法绑定组件 {field.Name}: 未找到路径 {bindPath}");
                }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 自动绑定特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class AutoBindAttribute : Attribute
    {
        public string Path { get; private set; }
        
        public AutoBindAttribute(string path = null)
        {
            Path = path;
        }
    }
} 