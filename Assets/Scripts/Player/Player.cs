using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

namespace TrianCatStudio
{
    [RequireComponent(typeof(Rigidbody), typeof(Animator))]
    public class Player : MonoBehaviour
    {
        // �������
        [Header("Component References")]
        [SerializeField] public Rigidbody Rb;
        [SerializeField] public Animator Animator;
        [SerializeField] public InputManager InputManager;
        [SerializeField] public Transform CameraPivot;

        // �ƶ�����
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float acceleration = 10f;



        // ����ʱ״̬
        public bool IsGrounded { get; private set; }
        public Vector3 MoveDirection { get; private set; }
        public Vector3 Velocity { get; private set; }
        // ״̬��ϵͳ
        private PlayerStateManager stateManager;

        private void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            Animator = GetComponent<Animator>();
            InputManager = GetComponent<InputManager>();
            stateManager = GetComponent<PlayerStateManager>();
            CameraPivot = FindObjectOfType<Camera>().transform;

            // ע�������¼�
            InputManager.OnJumpAction += HandleJump;
            InputManager.OnAimAction += HandleAim;
        }

        private void Update()
        {
            UpdateMovementParameters();
            UpdateStateParameters();
            UpdateAnimator();
        }

        private void FixedUpdate()
        {
            HandleGroundDetection();
            HandleMovement();
        }

        private void OnDestroy()
        {
            // ע���¼�
            InputManager.OnJumpAction -= HandleJump;
            InputManager.OnAimAction -= HandleAim;
        }

        #region ���봦��
        private void HandleJump()
        {
            if (CanPerformAction())
                stateManager.TriggerJump();
        }

        private void HandleAim(bool isAiming)
        {
            stateManager.StateMachine.SetBool("IsAiming", isAiming);
            Animator.SetBool("IsAiming", isAiming);
        }
        #endregion

        #region ״̬����
        private void UpdateMovementParameters()
        {
            // �����������ƶ�����
            Vector3 cameraForward = Vector3.Scale(CameraPivot.forward, new Vector3(1, 0, 1)).normalized;
            MoveDirection = cameraForward * InputManager.MoveInput.y +
                           CameraPivot.right * InputManager.MoveInput.x;

            // ��׼����������
            if (InputManager.MoveInput.magnitude > 0.1f)
                MoveDirection.Normalize();
        }

        private void UpdateStateParameters()
        {
            // ͬ�����뵽״̬��
            stateManager.StateMachine.SetFloat("MoveInput", InputManager.GetInputMagnitude());
            stateManager.StateMachine.SetBool("IsRunning", InputManager.IsRunning);
        }

        #endregion

        #region ����ϵͳ
        private void HandleGroundDetection()
        {
            const float groundCheckDistance = 0.2f;
            IsGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f,
                Vector3.down, out _, groundCheckDistance);
            stateManager.StateMachine.SetBool("IsGrounded", IsGrounded);
        }

        private void HandleMovement()
        {
            float targetSpeed = GetTargetSpeed();
            Vector3 targetVelocity = MoveDirection * targetSpeed;
            targetVelocity.y = Rb.velocity.y;

            Rb.velocity = Vector3.Lerp(Rb.velocity, targetVelocity,
                acceleration * Time.fixedDeltaTime);
            Velocity = Rb.velocity;
        }

        private float GetTargetSpeed()
        {
            if (!IsGrounded) return Rb.velocity.magnitude;
            if (InputManager.IsRunning)
                return runSpeed;
            return walkSpeed;
        }

        #endregion

        #region ����ϵͳ
        private void UpdateAnimator()
        {
            Animator.SetFloat("Speed", Rb.velocity.magnitude / runSpeed);
            Animator.SetBool("IsGrounded", IsGrounded);
        }
        #endregion

        #region ��������
        private bool CanPerformAction() => IsGrounded;
        #endregion
    }
}