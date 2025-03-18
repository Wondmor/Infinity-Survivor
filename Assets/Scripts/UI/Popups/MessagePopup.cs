using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TrianCatStudio
{
    /// <summary>
    /// 消息弹窗
    /// </summary>
    public class MessagePopup : BasePopup
    {
        [AutoBind("Title/Text")] private TextMeshProUGUI titleText;
        [AutoBind("Content/Text")] private TextMeshProUGUI contentText;
        [AutoBind("Buttons/ConfirmButton")] private Button confirmButton;
        [AutoBind("Buttons/CancelButton")] private Button cancelButton;
        
        private System.Action onConfirm;
        private System.Action onCancel;
        
        protected override void PreInitialize()
        {
            base.PreInitialize();
            
            // 初始化按钮事件
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmClicked);
            }
            
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelClicked);
            }
        }
        
        /// <summary>
        /// 设置弹窗数据
        /// </summary>
        public override void SetData(object data)
        {
            if (data is MessagePopupData messageData)
            {
                // 设置标题
                if (titleText != null)
                {
                    titleText.text = messageData.Title;
                }
                
                // 设置内容
                if (contentText != null)
                {
                    contentText.text = messageData.Content;
                }
                
                // 设置回调
                onConfirm = messageData.OnConfirm;
                onCancel = messageData.OnCancel;
                
                // 设置按钮文本
                if (confirmButton != null && !string.IsNullOrEmpty(messageData.ConfirmText))
                {
                    TextMeshProUGUI buttonText = confirmButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = messageData.ConfirmText;
                    }
                }
                
                if (cancelButton != null)
                {
                    // 如果没有设置取消回调，隐藏取消按钮
                    cancelButton.gameObject.SetActive(messageData.OnCancel != null);
                    
                    // 设置取消按钮文本
                    if (!string.IsNullOrEmpty(messageData.CancelText))
                    {
                        TextMeshProUGUI buttonText = cancelButton.GetComponentInChildren<TextMeshProUGUI>();
                        if (buttonText != null)
                        {
                            buttonText.text = messageData.CancelText;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 确认按钮点击事件
        /// </summary>
        private void OnConfirmClicked()
        {
            onConfirm?.Invoke();
            Close();
        }
        
        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        private void OnCancelClicked()
        {
            onCancel?.Invoke();
            Close();
        }
    }
    
    /// <summary>
    /// 消息弹窗数据
    /// </summary>
    public class MessagePopupData
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string ConfirmText { get; set; } = "确定";
        public string CancelText { get; set; } = "取消";
        public System.Action OnConfirm { get; set; }
        public System.Action OnCancel { get; set; }
        
        // 支持只有标题和内容的简单构造
        public MessagePopupData(string title, string content)
        {
            Title = title;
            Content = content;
        }
        
        // 支持带确认回调的构造
        public MessagePopupData(string title, string content, System.Action onConfirm)
        {
            Title = title;
            Content = content;
            OnConfirm = onConfirm;
        }
        
        // 支持带确认和取消回调的构造
        public MessagePopupData(string title, string content, System.Action onConfirm, System.Action onCancel)
        {
            Title = title;
            Content = content;
            OnConfirm = onConfirm;
            OnCancel = onCancel;
        }
    }
} 