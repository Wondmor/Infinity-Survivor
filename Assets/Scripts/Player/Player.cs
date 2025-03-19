using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

namespace TrianCatStudio
{
    [RequireComponent(typeof(Rigidbody), typeof(Animator), typeof(GroundDetector))]
    public class Player : MonoBehaviour
    {
        // 组件引用
        [Header("Component References")]
        [SerializeField] public Rigidbody Rb;
        [SerializeField] public Animator Animator;
        [SerializeField] public InputManager InputManager;
        [SerializeField] public Transform CameraPivot;
        [SerializeField] public PlayerAnimController AnimController; // 使用PlayerAnimController类型
        [SerializeField] public GroundDetector GroundDetector; // 地面检测组件引用

        // 子弹设置
        [Header("Shooting Settings")]
        [SerializeField] public GameObject BulletPrefab; // 子弹预制体
        [SerializeField] public Transform FirePoint; // 发射点
        [SerializeField] public float BulletSpeed = 20f; // 子弹速度
        [SerializeField] public float BulletLifetime = 5f; // 子弹生命周期
        [SerializeField] public float FireRate = 0.2f; // 射击频率
        [SerializeField] public float BulletDamage = 10f; // 子弹基础伤害
        private float lastFireTime; // 上次射击时间

        // 移动参数
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float rotationSpeed = 15f;
        [SerializeField] private float airControlMultiplier = 0.5f; // 空中控制系数
        [SerializeField] private float airAccelerationMultiplier = 0.7f; // 空中加速度系数
        [SerializeField] private float airMaxSpeedMultiplier = 0.8f; // 空中最大速度系数
        [SerializeField] private float airDirectionChangeMultiplier = 0.5f; // 空中方向改变系数
        [SerializeField] private float landingRecoveryTime = 0.2f; // 落地恢复时间

        // 跳跃参数
        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float doubleJumpForce = 4f;
        [SerializeField] private float jumpCooldown = 0.1f;
        [SerializeField] public float coyoteTime = 0.2f; // 修改为公共字段
        [SerializeField] private int maxJumpCount = 2;

        // 下蹲和滑铲参数
        [Header("Crouch and Slide Settings")]
        [SerializeField] private float crouchSpeed = 1.5f; // 下蹲移动速度
        [SerializeField] private float slideSpeed = 8f; // 滑铲初始速度
        [SerializeField] private float slideDeceleration = 5f; // 滑铲减速度
        [SerializeField] private float minSpeedToSlide = 3f; // 触发滑铲的最小速度
        [SerializeField] private float slideCooldown = 1f; // 滑铲冷却时间

        // 运行时状态
        public bool IsGrounded => GroundDetector?.IsGrounded ?? false;
        public Vector3 MoveDirection { get; private set; }
        public Vector3 Velocity { get; private set; }
        public bool HasDoubleJumped { get; set; }
        public float lastGroundedTime => GroundDetector?.LastGroundedTime ?? 0f;
        public float lastJumpTime { get; set; }
        private float landingTime; // 落地时间
        public int jumpCount { get; set; }
        public bool isJumpRequested { get; set; }
        private float lastSlideTime; // 上次滑铲时间
        public bool IsCrouching { get; private set; } // 是否正在下蹲
        public bool IsSliding { get; private set; } // 是否正在滑铲

        // 状态机系统
        private PlayerStateManager stateManager;

        private void Awake()
        {
            try
            {
                Debug.Log("Player.Awake: 开始初始化");
                
                Rb = GetComponent<Rigidbody>();
                Animator = GetComponent<Animator>();
                InputManager = GetComponent<InputManager>();
                
                // 获取地面检测组件
                GroundDetector = GetComponent<GroundDetector>();
                if (GroundDetector == null)
                {
                    Debug.LogError("Player.Awake: 未找到GroundDetector组件");
                    GroundDetector = gameObject.AddComponent<GroundDetector>();
                }
                
                // 订阅地面检测事件
                GroundDetector.OnGroundStateChanged += HandleGroundStateChanged;
                GroundDetector.OnLanding += HandleLanding;
                GroundDetector.OnHardLanding += HandleHardLanding;
                
                // 确保PlayerStateManager组件存在并启用
                stateManager = GetComponent<PlayerStateManager>();
                if (stateManager == null)
                {
                    Debug.Log("Player.Awake: 添加PlayerStateManager组件");
                    stateManager = gameObject.AddComponent<PlayerStateManager>();
                }
                
                // 确保组件已启用
                if (stateManager != null && !stateManager.enabled)
                {
                    Debug.Log("Player.Awake: 启用PlayerStateManager组件");
                    stateManager.enabled = true;
                }
                
                if (AnimController == null)
                {
                    AnimController = GetComponent<PlayerAnimController>();
                }
                
                CameraPivot = FindObjectOfType<Camera>()?.transform;
                if (CameraPivot == null)
                {
                    Debug.LogWarning("Player.Awake: 未找到相机");
                }

                // 注册输入事件
                if (InputManager != null)
                {
                    InputManager.OnJumpAction += HandleJumpInput;
                    InputManager.OnAimAction += HandleAimInput;
                    InputManager.OnCrouchAction += HandleCrouchInput;
                    InputManager.OnFireAction += HandleFireInput; // 单次开火事件
                    InputManager.OnFireActionChanged += HandleFireInputChanged; // 持续开火状态变化事件
                }
                else
                {
                    Debug.LogError("Player.Awake: InputManager为空");
                }
                
                // 初始化发射点
                if (FirePoint == null)
                {
                    // 如果没有指定发射点，创建一个默认的发射点
                    GameObject firePointObj = new GameObject("FirePoint");
                    firePointObj.transform.SetParent(transform);
                    firePointObj.transform.localPosition = new Vector3(0, 1.5f, 0.5f); // 设置在角色前方偏上的位置
                    FirePoint = firePointObj.transform;
                }
                
                Debug.Log("Player.Awake: 初始化完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Player.Awake: 初始化失败 - {e.Message}\n{e.StackTrace}");
            }
        }

        private void Start()
        {
            lastJumpTime = -jumpCooldown;
            ResetJumpCount();
        }

        private void Update()
        {
            UpdateMovementParameters();
            UpdateStateParameters();
            
            // 更新瞄准方向
            if (InputManager.IsAiming || InputManager.IsFirePressed)
            {
                UpdateAimDirection();
            }
        }

        private void FixedUpdate()
        {
            HandleMovement();
            
            // 处理楼梯
            if (IsGrounded && MoveDirection.magnitude > 0.1f)
            {
                GroundDetector.HandleStairs(MoveDirection);
            }
        }

        private void OnDestroy()
        {
            // 注销地面检测事件
            if (GroundDetector != null)
            {
                GroundDetector.OnGroundStateChanged -= HandleGroundStateChanged;
                GroundDetector.OnLanding -= HandleLanding;
                GroundDetector.OnHardLanding -= HandleHardLanding;
            }
            
            // 注销输入事件
            if (InputManager != null)
            {
                InputManager.OnJumpAction -= HandleJumpInput;
                InputManager.OnAimAction -= HandleAimInput;
                InputManager.OnCrouchAction -= HandleCrouchInput;
                InputManager.OnFireAction -= HandleFireInput; // 注销单次开火事件
                InputManager.OnFireActionChanged -= HandleFireInputChanged; // 注销持续开火状态变化事件
            }
        }

        #region 输入处理
        private void HandleJumpInput()
        {
            // 检查是否可以跳跃
            bool canJump = IsGrounded || Time.time - lastGroundedTime <= coyoteTime;
            bool canDoubleJump = CanDoubleJump();
            
            // 记录跳跃输入时间
            lastJumpTime = Time.time;
            
            // 通知状态机处理跳跃
            if (canJump)
            {
                // 在地面上或土狼时间内，执行一段跳
                jumpCount = 0; // 确保从0开始计数
                HasDoubleJumped = false;
                
                // 增加跳跃计数
                jumpCount = 1;
                
                Debug.Log("Player.HandleJumpInput: 触发一段跳");
                stateManager.TriggerJump();
            }
            else if (canDoubleJump)
            {
                // 在空中且已经跳过一次，执行二段跳
                Debug.Log($"Player.HandleJumpInput: 触发二段跳 - jumpCount={jumpCount}, HasDoubleJumped={HasDoubleJumped}");
                
                // 立即更新状态，确保不会重复触发
                HasDoubleJumped = true;
                jumpCount = 2;
                
                // 触发二段跳
                stateManager.TriggerDoubleJump();
            }
            else
            {
                Debug.Log($"Player.HandleJumpInput: 无法跳跃 - IsGrounded={IsGrounded}, jumpCount={jumpCount}, HasDoubleJumped={HasDoubleJumped}");
            }
        }

        private void HandleAimInput(bool isAiming)
        {
            // 通知状态机
            stateManager.SetAiming(isAiming);
            
            // 更新AnimatorController的瞄准状态
            if (AnimController != null)
            {
                AnimController.SetAimingState(isAiming);
            }
        }

        private void HandleCrouchInput(bool isCrouching)
        {
            Debug.Log($"Player.HandleCrouchInput: 收到下蹲输入 = {isCrouching}");
            
            // 通知状态机
            stateManager.TriggerCrouch(isCrouching);
            
            // 更新AnimatorController的下蹲状态
            if (AnimController != null)
            {
                AnimController.SetCrouchState(isCrouching);
                
                // 如果是下蹲，直接触发下蹲动画（用于调试）
                if (isCrouching)
                {
                    AnimController.TriggerCrouchAnimation();
                }
            }
            
            // 如果在跑步状态下按下下蹲，触发滑铲
            if (isCrouching && IsGrounded && Rb.velocity.magnitude > minSpeedToSlide && Time.time > lastSlideTime + slideCooldown)
            {
                TriggerSlide();
            }
            
            // 更新下蹲状态
            IsCrouching = isCrouching;
            
            // 打印当前状态
            Debug.Log($"Player状态 - IsCrouching: {IsCrouching}, IsGrounded: {IsGrounded}, 速度: {Rb.velocity.magnitude}");
        }
        
        private void TriggerSlide()
        {
            // 记录滑铲时间
            lastSlideTime = Time.time;
            
            // 设置滑铲状态
            IsSliding = true;
            
            // 通知状态机
            stateManager.TriggerSlide();
            
            Debug.Log("触发滑铲");
        }

        private void HandleFireInput()
        {
            // 检查射击冷却
            if (Time.time - lastFireTime < FireRate)
            {
                Debug.Log($"Player.HandleFireInput: 射击冷却中 - 剩余时间: {FireRate - (Time.time - lastFireTime):F2}秒");
                return;
            }
            
            // 更新上次射击时间
            lastFireTime = Time.time;
            
            Debug.Log("Player.HandleFireInput: 触发射击");
            
            // 通知状态机处理射击
            stateManager.TriggerFire();
        }
        
        // 处理持续开火状态变化
        private void HandleFireInputChanged(bool isFiring)
        {
            Debug.Log($"Player.HandleFireInputChanged: 开火状态变化 = {isFiring}");
            
            if (isFiring)
            {
                // 开始持续开火，首次触发由 HandleFireInput 处理
                // 这里不需要额外处理
            }
            else
            {
                // 停止开火
                stateManager.StopFiring();
            }
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
            
            // 将移动方向传递给动画控制器
            if (AnimController != null && MoveDirection.magnitude > 0.1f)
            {
                AnimController.SetMoveDirection(MoveDirection);
            }
        }

        private void UpdateStateParameters()
        {
            // 同步输入到状态机，但不设置动画参数
            stateManager.SetMoveInput(InputManager.GetInputMagnitude());
            stateManager.SetRunning(InputManager.IsRunning);
            stateManager.SetGrounded(IsGrounded);
            stateManager.SetVerticalVelocity(Rb.velocity.y);
            
            // 更新AnimatorController的移动状态
            if (AnimController != null)
            {
                bool isMoving = InputManager.GetInputMagnitude() > 0.1f;
                bool isRunning = InputManager.IsRunning;
                float speed = isMoving ? (isRunning ? runSpeed : walkSpeed) : 0f;
                AnimController.SetMovementState(isMoving, isRunning, speed);
                
                // 更新接地状态
                AnimController.SetGroundedState(IsGrounded, Rb.velocity.y);
            }
        }
        
        // 更新瞄准方向
        private void UpdateAimDirection()
        {
            if (AnimController == null || CameraPivot == null)
                return;
            
            // 使用相机前方作为瞄准方向
            Vector3 aimDirection = CameraPivot.forward;
            
            // 将瞄准方向传递给动画控制器
            AnimController.SetAimDirection(aimDirection);
            
            // 当瞄准或开火时，直接让角色朝向瞄准方向
            if (InputManager.IsAiming || InputManager.IsFirePressed)
            {
                // 获取水平方向（忽略Y轴）
                Vector3 horizontalAimDirection = new Vector3(aimDirection.x, 0, aimDirection.z).normalized;
                
                // 直接设置角色朝向
                if (horizontalAimDirection.magnitude > 0.1f)
                {
                    transform.rotation = Quaternion.LookRotation(horizontalAimDirection);
                }
            }
        }
        #endregion

        #region 地面检测事件处理
        private void HandleGroundStateChanged(bool isGrounded)
        {
            // 通知状态机地面状态变化
            stateManager.SetGrounded(isGrounded);
            
            if (isGrounded)
            {
                // 刚着地时的处理
                HasDoubleJumped = false;
                ResetJumpCount();
            }
            else if (Rb.velocity.y < 0)
            {
                // 离开地面且正在下落时
                if (AnimController != null)
                {
                    AnimController.TriggerFall();
                }
            }
        }
        
        private void HandleLanding(float fallSpeed)
        {
            // 记录着陆时间
            landingTime = Time.time;
            
            // 触发正常着陆状态
            stateManager.TriggerLanding();
            
            // 使用AnimatorController触发着陆动画
            if (AnimController != null)
            {
                AnimController.TriggerLand();
            }
        }
        
        private void HandleHardLanding(float fallSpeed)
        {
            // 记录着陆时间
            landingTime = Time.time;
            
            // 触发硬着陆状态
            stateManager.TriggerHardLanding();
            
            // 使用AnimatorController触发硬着陆动画
            if (AnimController != null)
            {
                AnimController.TriggerHardLand();
            }
        }
        #endregion

        #region 物理系统
        private void HandleMovement()
        {
            float targetSpeed = GetTargetSpeed();
            
            // 计算目标速度
            Vector3 targetVelocity = MoveDirection * targetSpeed;
            
            // 声明加速度变量
            float currentAcceleration;
            
            // 空中移动控制
            if (!IsGrounded)
            {
                // 保存当前水平速度
                Vector3 currentHorizontalVelocity = new Vector3(Rb.velocity.x, 0, Rb.velocity.z);
                float currentHorizontalSpeed = currentHorizontalVelocity.magnitude;
                
                // 限制空中最大速度
                float airMaxSpeed = targetSpeed * airMaxSpeedMultiplier;
                
                // 如果当前速度已经超过空中最大速度，保持当前速度
                if (currentHorizontalSpeed > airMaxSpeed)
                {
                    // 保持当前方向，但限制速度
                    targetVelocity = currentHorizontalVelocity.normalized * currentHorizontalSpeed;
                }
                else
                {
                    // 计算方向变化程度 (0-1)，1表示完全相反方向
                    float directionChange = 0;
                    if (currentHorizontalSpeed > 0.1f)
                    {
                        directionChange = 1f - Vector3.Dot(currentHorizontalVelocity.normalized, MoveDirection.normalized) * 0.5f;
                    }
                    
                    // 根据方向变化调整控制系数
                    float effectiveAirControl = airControlMultiplier * (1f - directionChange * airDirectionChangeMultiplier);
                    
                    // 应用空中控制
                    targetVelocity = Vector3.Lerp(currentHorizontalVelocity, MoveDirection * airMaxSpeed, effectiveAirControl);
                }
                
                // 使用较小的加速度
                currentAcceleration = acceleration * airAccelerationMultiplier;
            }
            else
            {
                currentAcceleration = acceleration;
            }
            
            // 保留垂直速度
            targetVelocity.y = Rb.velocity.y;
            
            // 落地缓冲期间减少移动速度
            if (IsGrounded && Time.time - landingTime < landingRecoveryTime)
            {
                float recoveryProgress = (Time.time - landingTime) / landingRecoveryTime;
                targetVelocity.x *= recoveryProgress;
                targetVelocity.z *= recoveryProgress;
            }

            // 应用速度
            Rb.velocity = Vector3.Lerp(Rb.velocity, targetVelocity,
                currentAcceleration * Time.fixedDeltaTime);
            Velocity = Rb.velocity;

            // 处理旋转
            if (MoveDirection.magnitude > 0.1f && !(InputManager.IsAiming || InputManager.IsFirePressed))
            {
                Quaternion targetRotation = Quaternion.LookRotation(MoveDirection);
                
                // 在空中减少旋转速度
                float currentRotationSpeed = IsGrounded ? rotationSpeed : rotationSpeed * airControlMultiplier;
                
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 
                    currentRotationSpeed * Time.fixedDeltaTime);
            }
            
            // 更新状态机中的垂直速度
            stateManager.SetVerticalVelocity(Velocity.y);
            
            // 更新AnimatorController的水平速度
            if (AnimController != null)
            {
                Vector3 horizontalVelocity = new Vector3(Velocity.x, 0, Velocity.z);
                AnimController.SetHorizontalSpeed(horizontalVelocity.magnitude);
            }
        }

        private float GetTargetSpeed()
        {
            if (!IsGrounded) return Rb.velocity.magnitude;
            if (IsCrouching) return crouchSpeed;
            if (InputManager.IsRunning) return runSpeed;
            return walkSpeed;
        }
        #endregion

        #region 辅助方法
        private void ResetJumpCount()
        {
            jumpCount = 0;
        }

        public float GetJumpForce()
        {
            return jumpCount <= 1 ? jumpForce : doubleJumpForce;
        }

        // 检查是否可以二段跳
        private bool CanDoubleJump()
        {
            // 不再依赖IsGrounded属性，只检查跳跃计数和二段跳状态
            return jumpCount == 1 && !HasDoubleJumped;
        }
        #endregion
    }
}