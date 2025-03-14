using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

namespace TrianCatStudio
{
    [RequireComponent(typeof(Rigidbody), typeof(Animator))]
    public class Player : MonoBehaviour
    {
        // 组件引用
        [Header("Component References")]
        [SerializeField] public Rigidbody Rb;
        [SerializeField] public Animator Animator;
        [SerializeField] public InputManager InputManager;
        [SerializeField] public Transform CameraPivot;

        // 移动参数
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float acceleration = 10f;



        // 运行时状态
        public bool IsGrounded { get; private set; }
        public Vector3 MoveDirection { get; private set; }
        public Vector3 Velocity { get; private set; }
        // 状态机系统
        private PlayerStateManager stateManager;

        private void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            Animator = GetComponent<Animator>();
            InputManager = GetComponent<InputManager>();
            stateManager = GetComponent<PlayerStateManager>();
            CameraPivot = FindObjectOfType<Camera>().transform;

            // 注册输入事件
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
            // 注销事件
            InputManager.OnJumpAction -= HandleJump;
            InputManager.OnAimAction -= HandleAim;
        }

        #region 输入处理
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

        #region 状态更新
        private void UpdateMovementParameters()
        {
            // 计算相机相对移动方向
            Vector3 cameraForward = Vector3.Scale(CameraPivot.forward, new Vector3(1, 0, 1)).normalized;
            MoveDirection = cameraForward * InputManager.MoveInput.y +
                           CameraPivot.right * InputManager.MoveInput.x;

            // 标准化输入向量
            if (InputManager.MoveInput.magnitude > 0.1f)
                MoveDirection.Normalize();
        }

        private void UpdateStateParameters()
        {
            // 同步输入到状态机
            stateManager.StateMachine.SetFloat("MoveInput", InputManager.GetInputMagnitude());
            stateManager.StateMachine.SetBool("IsRunning", InputManager.IsRunning);
        }

        #endregion

        #region 物理系统
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

        #region 动画系统
        private void UpdateAnimator()
        {
            Animator.SetFloat("Speed", Rb.velocity.magnitude / runSpeed);
            Animator.SetBool("IsGrounded", IsGrounded);
        }
        #endregion

        #region 辅助方法
        private bool CanPerformAction() => IsGrounded;
        #endregion
    }
}