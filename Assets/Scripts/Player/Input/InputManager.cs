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
        // �����¼�ί��
        public event Action<bool> OnAimAction;
        public event Action OnJumpAction;
        public event Action OnDashAction;
        public event Action<bool> OnCrouchAction;
        public event Action OnFireAction; // ���ο����¼�
        public event Action<bool> OnFireActionChanged; // ��ӳ�������״̬�仯�¼�

        // ��ǰ����״̬
        public Vector2 MoveInput { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool JumpTriggered { get; private set; }
        public bool IsJumpPressed { get; private set; } // ��Ծ���Ƿ�հ���
        public bool IsAiming { get; private set; }
        public bool IsDashPressed { get; private set; } // ��̼��Ƿ�հ���
        public bool IsFirePressed { get; private set; } // ������Ƿ��£�����״̬��

        private PlayerInput playerInput;
        private InputActionMap actionMap;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            actionMap = playerInput.actions.FindActionMap("Player");

            // ��ʼ�������
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
                
                Debug.Log("Jump���������£�IsJumpPressed = true");
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
            
            // �޸Ŀ������봦��֧�ֳ�������
            actionMap["Fire"].started += _ =>
            {
                IsFirePressed = true;
                OnFireAction?.Invoke(); // �������ο����¼�
                OnFireActionChanged?.Invoke(true); // ��������״̬�仯�¼�
                Debug.Log("Fire���������£�IsFirePressed = true");
            };
            
            // ��ӿ���ȡ���¼�����
            actionMap["Fire"].canceled += _ =>
            {
                IsFirePressed = false;
                OnFireActionChanged?.Invoke(false); // ��������״̬�仯�¼�
                Debug.Log("Fire�������ͷţ�IsFirePressed = false");
            };
        }
        
        private void LateUpdate()
        {
            // ���ô�����״̬
            JumpTriggered = false;
            IsJumpPressed = false;
            IsDashPressed = false;
            // �������� IsFirePressed���������ǳ���״̬
        }

        private void OnEnable()
        {
            // ȷ������ϵͳ����
            if (actionMap != null && !actionMap.enabled)
            {
                actionMap.Enable();
                Debug.Log("InputManager: ����ϵͳ������");
            }
        }
        
        private void OnDisable()
        {
            // ��������ϵͳ
            if (actionMap != null && actionMap.enabled)
            {
                actionMap.Disable();
                Debug.Log("InputManager: ����ϵͳ�ѽ���");
            }
        }

        public float GetInputMagnitude() =>
            Mathf.Clamp01(new Vector2(MoveInput.x, MoveInput.y).magnitude);
    }
}
