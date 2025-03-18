using UnityEngine;
using System;

namespace TrianCatStudio
{
    /// <summary>
    /// UI框架使用示例
    /// </summary>
    public class UsageExample : MonoBehaviour
    {
        /// <summary>
        /// 打开主菜单
        /// </summary>
        public void OpenMainMenu()
        {
            // 打开主菜单面板
            var controller = MainMenuController.Instance as MainMenuController;
            controller.OpenPanel(panel => {
                // 面板打开后的回调
                Debug.Log("主菜单面板已打开");
                
                // 设置面板数据
                MainMenuData data = new MainMenuData("无限生存者");
                panel.SetData(data);
            });
        }
        
        /// <summary>
        /// 打开设置面板
        /// </summary>
        public void OpenSettings()
        {
            // 打开设置面板
            var controller = SettingsController.Instance as SettingsController;
            controller.OpenPanel();
        }
        
        /// <summary>
        /// 显示确认弹窗
        /// </summary>
        public void ShowConfirmationPopup()
        {
            // 创建弹窗数据
            MessagePopupData data = new MessagePopupData(
                "确认操作",
                "你确定要执行此操作吗？",
                () => Debug.Log("用户点击了确定按钮"),
                () => Debug.Log("用户点击了取消按钮")
            );
            
            // 设置按钮文本
            data.ConfirmText = "确定";
            data.CancelText = "取消";
            
            // 显示弹窗
            var popupSystem = PopupSystem.Instance as PopupSystem;
            popupSystem.ShowPopup<MessagePopup>(data, null);
        }
        
        /// <summary>
        /// 显示简单信息弹窗
        /// </summary>
        public void ShowInfoPopup()
        {
            // 创建弹窗数据（只有确认按钮）
            MessagePopupData data = new MessagePopupData(
                "信息",
                "这是一条重要信息，请注意查看。",
                () => Debug.Log("用户已查看信息")
            );
            
            // 设置按钮文本
            data.ConfirmText = "确定";
            
            // 显示弹窗
            var popupSystem = PopupSystem.Instance as PopupSystem;
            popupSystem.ShowPopup<MessagePopup>(data, null, PopupPosition.Top);
        }
        
        /// <summary>
        /// 展示连续弹窗（队列示例）
        /// </summary>
        public void ShowSequentialPopups()
        {
            // 第一个弹窗
            MessagePopupData data1 = new MessagePopupData(
                "第一条消息",
                "这是第一个弹窗，点击确定查看下一个。"
            );
            data1.ConfirmText = "确定";
            
            // 第二个弹窗
            MessagePopupData data2 = new MessagePopupData(
                "第二条消息",
                "这是第二个弹窗，点击确定查看下一个。"
            );
            data2.ConfirmText = "确定";
            
            // 第三个弹窗
            MessagePopupData data3 = new MessagePopupData(
                "第三条消息",
                "这是最后一个弹窗。",
                () => Debug.Log("所有弹窗已查看完毕")
            );
            data3.ConfirmText = "完成";
            
            // 依次显示弹窗
            var popupSystem = PopupSystem.Instance as PopupSystem;
            popupSystem.ShowPopup<MessagePopup>(data1, null);
            popupSystem.ShowPopup<MessagePopup>(data2, null);
            popupSystem.ShowPopup<MessagePopup>(data3, null);
        }
        
        /// <summary>
        /// 关闭所有UI
        /// </summary>
        public void CloseAllUI()
        {
            // 关闭所有面板
            var uiManager = UIManager.Instance as UIManager;
            uiManager.CloseAllPanels();
            
            // 清空弹窗队列
            var popupSystem = PopupSystem.Instance as PopupSystem;
            popupSystem.CloseAllPopups();
        }
    }
}