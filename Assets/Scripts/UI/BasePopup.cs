using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TrianCatStudio
{
    /// <summary>
    /// 弹窗位置枚举
    /// </summary>
    public enum PopupPosition
    {
        Center,
        Top,
        Bottom,
        Left,
        Right
    }
    
    /// <summary>
    /// 所有弹窗的基类
    /// </summary>
    public abstract class BasePopup : MonoBehaviour
    {
        [SerializeField] private PopupPosition position = PopupPosition.Center;
        [SerializeField] private Vector2 offset = Vector2.zero;
        [SerializeField] private bool useOverlay = true;
        [SerializeField] private float overlayOpacity = 0.5f;
        
        private CanvasGroup canvasGroup;
        private GameObject overlay;
        private Image overlayImage;
        
        // 属性
        public PopupPosition Position => position;
        public Vector2 Offset => offset;
        
        #region 生命周期方法
        
        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            // 如果使用遮罩，创建遮罩
            if (useOverlay)
            {
                CreateOverlay();
            }
        }
        
        /// <summary>
        /// 初始化弹窗
        /// </summary>
        public void Initialize(Action onInitialized = null)
        {
            Debug.Log($"[{GetType().Name}] 初始化弹窗");
            
            // 自动绑定组件
            AutoBindComponents();
            
            // 设置弹窗位置
            SetPosition(position, offset);
            
            // 弹窗初始化前的准备
            PreInitialize();
            
            // 初始化完成回调
            onInitialized?.Invoke();
            
            // 播放入场动画
            StartCoroutine(PlayEnterAnimation());
            
            // 将弹窗入栈
            UIManager.Instance.PushPopup(this);
        }
        
        /// <summary>
        /// 关闭弹窗
        /// </summary>
        public void Close()
        {
            Debug.Log($"[{GetType().Name}] 关闭弹窗");
            
            // 播放退出动画，动画完成后销毁
            StartCoroutine(PlayExitAnimationAndDestroy());
        }
        
        /// <summary>
        /// 销毁弹窗
        /// </summary>
        private void DestroyPopup()
        {
            // 从UIManager弹出弹窗
            UIManager.Instance.PopPopup();
            
            // 销毁游戏对象
            Destroy(gameObject);
        }
        
        #endregion
        
        #region 可重写的方法
        
        /// <summary>
        /// 弹窗初始化前的准备工作
        /// </summary>
        protected virtual void PreInitialize() { }
        
        /// <summary>
        /// 自定义数据设置
        /// </summary>
        public virtual void SetData(object data) { }
        
        /// <summary>
        /// 播放入场动画
        /// </summary>
        public virtual IEnumerator PlayEnterAnimation()
        {
            // 默认缩放入场动画
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                transform.localScale = Vector3.one * 0.5f;
                
                // 显示遮罩
                if (overlay != null)
                {
                    overlayImage.color = new Color(0, 0, 0, 0);
                    overlay.SetActive(true);
                }
                
                float duration = 0.3f;
                float startTime = Time.time;
                
                while (Time.time - startTime < duration)
                {
                    float t = (Time.time - startTime) / duration;
                    canvasGroup.alpha = t;
                    transform.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one, t);
                    
                    // 淡入遮罩
                    if (overlay != null)
                    {
                        overlayImage.color = new Color(0, 0, 0, t * overlayOpacity);
                    }
                    
                    yield return null;
                }
                
                canvasGroup.alpha = 1;
                transform.localScale = Vector3.one;
                
                // 设置最终遮罩透明度
                if (overlay != null)
                {
                    overlayImage.color = new Color(0, 0, 0, overlayOpacity);
                }
            }
            
            yield break;
        }
        
        /// <summary>
        /// 播放退出动画
        /// </summary>
        public virtual IEnumerator PlayExitAnimationAndDestroy()
        {
            // 默认缩放退出动画
            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                
                float duration = 0.2f;
                float startTime = Time.time;
                
                while (Time.time - startTime < duration)
                {
                    float t = 1 - (Time.time - startTime) / duration;
                    canvasGroup.alpha = t;
                    transform.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one, t);
                    
                    // 淡出遮罩
                    if (overlay != null)
                    {
                        overlayImage.color = new Color(0, 0, 0, t * overlayOpacity);
                    }
                    
                    yield return null;
                }
                
                // 隐藏遮罩
                if (overlay != null)
                {
                    overlay.SetActive(false);
                }
            }
            
            // 销毁弹窗
            DestroyPopup();
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 创建背景遮罩
        /// </summary>
        private void CreateOverlay()
        {
            // 创建遮罩对象
            overlay = new GameObject("Overlay");
            overlay.transform.SetParent(transform.parent);
            overlay.transform.SetSiblingIndex(transform.GetSiblingIndex());
            
            // 设置遮罩填满父对象
            RectTransform rectTransform = overlay.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // 添加图片组件
            overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0, 0, 0, overlayOpacity);
            
            // 添加按钮组件，点击背景关闭弹窗
            Button overlayButton = overlay.AddComponent<Button>();
            overlayButton.transition = Selectable.Transition.None;
            overlayButton.onClick.AddListener(Close);
            
            // 暂时隐藏遮罩
            overlay.SetActive(false);
        }
        
        /// <summary>
        /// 设置弹窗位置
        /// </summary>
        public void SetPosition(PopupPosition pos, Vector2 offset = default)
        {
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform == null)
                return;
            
            // 设置锚点为中心
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            
            // 根据位置设置偏移
            switch (pos)
            {
                case PopupPosition.Center:
                    rectTransform.anchoredPosition = offset;
                    break;
                case PopupPosition.Top:
                    rectTransform.anchoredPosition = new Vector2(offset.x, Screen.height * 0.4f - rectTransform.rect.height * 0.5f) + offset;
                    break;
                case PopupPosition.Bottom:
                    rectTransform.anchoredPosition = new Vector2(offset.x, -Screen.height * 0.4f + rectTransform.rect.height * 0.5f) + offset;
                    break;
                case PopupPosition.Left:
                    rectTransform.anchoredPosition = new Vector2(-Screen.width * 0.4f + rectTransform.rect.width * 0.5f, offset.y) + offset;
                    break;
                case PopupPosition.Right:
                    rectTransform.anchoredPosition = new Vector2(Screen.width * 0.4f - rectTransform.rect.width * 0.5f, offset.y) + offset;
                    break;
            }
        }
        
        /// <summary>
        /// 自动绑定标记了AutoBindAttribute的字段
        /// </summary>
        private void AutoBindComponents()
        {
            // 利用与BasePanel相同的反射机制
            var fields = GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                AutoBindAttribute attr = System.Reflection.CustomAttributeExtensions.GetCustomAttribute<AutoBindAttribute>(field);
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
} 