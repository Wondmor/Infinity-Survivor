using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TrianCatStudio
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputManager : MonoBehaviour
    {
        // 输入事件委托
        public event Action<bool> OnAimAction;
        public event Action OnJumpAction;
        public event Action OnDashAction;
        public event Action<bool> OnCrouchAction;
        public event Action OnFireAction; // 单次开火事件
        public event Action<bool> OnFireActionChanged; // 添加持续开火状态变化事件

        // 当前输入状态
        public Vector2 MoveInput { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool JumpTriggered { get; private set; }
        public bool IsJumpPressed { get; private set; } // 跳跃键是否刚按下
        public bool IsAiming { get; private set; }
        public bool IsDashPressed { get; private set; } // 冲刺键是否刚按下
        public bool IsFirePressed { get; private set; } // 开火键是否按下（持续状态）

        private PlayerInput playerInput;
        private InputActionMap actionMap;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            actionMap = playerInput.actions.FindActionMap("Player");

            // 初始化输入绑定
            actionMap["Move"].performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
            actionMap["Move"].canceled += _ => MoveInput = Vector2.zero;

            actionMap["Run"].started += _ => IsRunning = true;
            actionMap["Run"].canceled += _ => IsRunning = false;

            actionMap["Crouch"].started += _ => 
            {
                IsCrouching = true;
                OnCrouchAction?.Invoke(true);
            };
            
            actionMap["Crouch"].canceled += _ => 
            {
                IsCrouching = false;
                OnCrouchAction?.Invoke(false);
            };

            actionMap["Jump"].started += _ =>
            {
                JumpTriggered = true;
                IsJumpPressed = true;
                OnJumpAction?.Invoke();
                
                Debug.Log("Jump按键被按下：IsJumpPressed = true");
            };

            actionMap["Dash"].started += _ =>
            {
                IsDashPressed = true;
                OnDashAction?.Invoke();
            };

            actionMap["Aim"].started += _ =>
            {
                IsAiming = true;
                OnAimAction?.Invoke(true);
            };

            actionMap["Aim"].canceled += _ =>
            {
                IsAiming = false;
                OnAimAction?.Invoke(false);
            };
            
            // 修改开火输入处理，支持持续开火
            actionMap["Fire"].started += _ =>
            {
                IsFirePressed = true;
                OnFireAction?.Invoke(); // 触发单次开火事件
                OnFireActionChanged?.Invoke(true); // 触发开火状态变化事件
                Debug.Log("Fire按键被按下：IsFirePressed = true");
            };
            
            // 添加开火取消事件处理
            actionMap["Fire"].canceled += _ =>
            {
                IsFirePressed = false;
                OnFireActionChanged?.Invoke(false); // 触发开火状态变化事件
                Debug.Log("Fire按键被释放：IsFirePressed = false");
            };
        }
        
        private void LateUpdate()
        {
            // 重置触发型状态
            JumpTriggered = false;
            IsJumpPressed = false;
            IsDashPressed = false;
            // 不再重置 IsFirePressed，它现在是持续状态
        }

        private void OnEnable()
        {
            // 确保输入系统启用
            if (actionMap != null && !actionMap.enabled)
            {
                actionMap.Enable();
                Debug.Log("InputManager: 输入系统已启用");
            }
        }
        
        private void OnDisable()
        {
            // 禁用输入系统
            if (actionMap != null && actionMap.enabled)
            {
                actionMap.Disable();
                Debug.Log("InputManager: 输入系统已禁用");
            }
        }

        public float GetInputMagnitude() =>
            Mathf.Clamp01(new Vector2(MoveInput.x, MoveInput.y).magnitude);
    }
}
