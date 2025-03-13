using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TrianCatStudio
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputManager : SingletonAutoMono<InputManager>
    {
        // �����¼�ί��
        public event Action<bool> OnAimAction;
        public event Action OnJumpAction;

        // ��ǰ����״̬
        public Vector2 MoveInput { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool JumpTriggered { get; private set; }
        public bool IsAiming { get; private set; }


        private PlayerInput playerInput;
        private InputActionMap actionMap;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            actionMap = playerInput.actions.FindActionMap("Player");

            // ��ʼ�������
            actionMap["Move"].performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
            actionMap["Move"].canceled += _ => MoveInput = Vector2.zero;

            actionMap["Sprint"].started += _ => IsSprinting = true;
            actionMap["Sprint"].canceled += _ => IsSprinting = false;

            actionMap["Jump"].started += _ =>
            {
                JumpTriggered = true;
                OnJumpAction?.Invoke();
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
        }
        private void LateUpdate()
        {
            // ���ô�����״̬
            JumpTriggered = false;
        }

        public float GetInputMagnitude() =>
            Mathf.Clamp01(new Vector2(MoveInput.x, MoveInput.y).magnitude);
    }
}
