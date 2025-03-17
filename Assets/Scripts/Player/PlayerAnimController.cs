using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TrianCatStudio
{
    /// <summary>
    /// 角色动画控制器
    /// 负责处理所有与动画相关的状态转换和参数设置
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimController : MonoBehaviour
    {
        #region 动画参数哈希值
        // 缓存动画参数哈希值，提高性能
        private readonly int _speedHash = Animator.StringToHash("Speed");
        private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");
        private readonly int _verticalVelocityHash = Animator.StringToHash("VerticalVelocity");
        private readonly int _horizontalSpeedHash = Animator.StringToHash("HorizontalSpeed");
        private readonly int _isMovingHash = Animator.StringToHash("IsMoving");
        private readonly int _isRunningHash = Animator.StringToHash("IsRunning");
        private readonly int _isAimingHash = Animator.StringToHash("IsAiming");
        private readonly int _motionStateHash = Animator.StringToHash("MotionState");
        private readonly int _jumpTriggerHash = Animator.StringToHash("Jump");
        private readonly int _doubleJumpTriggerHash = Animator.StringToHash("DoubleJump");
        private readonly int _fallTriggerHash = Animator.StringToHash("Fall");
        private readonly int _landTriggerHash = Animator.StringToHash("Land");
        private readonly int _hardLandTriggerHash = Animator.StringToHash("HardLand");
        private readonly int _rollTriggerHash = Animator.StringToHash("Roll");
        private readonly int _isCrouchingHash = Animator.StringToHash("IsCrouching");
        private readonly int _slideHash = Animator.StringToHash("Slide");
        private readonly int _isFloatingHash = Animator.StringToHash("IsFloating");
        private readonly int _airTimeHash = Animator.StringToHash("AirTime");
        private readonly int _jumpTimeHash = Animator.StringToHash("JumpTime");
        private readonly int _crouchTriggerHash = Animator.StringToHash("Crouch");
        private readonly int _isFireingHash = Animator.StringToHash("IsFiring");
        private readonly int _fireTriggerHash = Animator.StringToHash("Fire");
        
        // 移动方向相关的哈希值（相对于瞄准方向）
        private readonly int _moveDirectionXHash = Animator.StringToHash("MoveDirectionX"); // 横向移动（相对于瞄准方向）
        private readonly int _moveDirectionZHash = Animator.StringToHash("MoveDirectionZ"); // 纵向移动（相对于瞄准方向）
        private readonly int _aimingOrFiringHash = Animator.StringToHash("IsAimingOrFiring"); // 是否瞄准或开火
        #endregion

        #region 动画状态枚举
        /// <summary>
        /// 动画状态枚举
        /// </summary>
        public enum AnimationState
        {
            Idle = 0,
            Walk = 1,
            Run = 2,
            Jump = 3,
            DoubleJump = 4,
            Fall = 5,
            Land = 6,
            HardLand = 7,
            Roll = 8,
            Aim = 9,
            Crouch = 10,
            Slide = 11,
            Fire = 12
        }
        #endregion

        #region 组件引用
        [Header("组件引用")]
        [Tooltip("角色动画控制器")]
        [SerializeField] private Animator _animator;
        
        [Tooltip("角色控制器")]
        [SerializeField] private Player _player;
        #endregion

        #region 动画设置
        [Header("动画设置")]
        [Tooltip("动画平滑过渡时间")]
        [SerializeField] private float _animationDampTime = 0.1f;
        
        [SerializeField] private float _upperBodyLayerWeight = 0.8f;
        [SerializeField] private float _additiveLayerWeight = 0.5f;
        #endregion

        #region 运行时变量
        // 当前动画状态
        private AnimationState _currentAnimState = AnimationState.Idle;
        
        // 速度变量
        private float _currentSpeed = 0f;
        private float _targetSpeed = 0f;
        private float _verticalVelocity = 0f;
        private float _horizontalSpeed = 0f;
        private bool _isMoving = false;
        private bool _isRunning = false;
        private bool _isGrounded = true;
        
        // 空中状态变量
        private float _airTime = 0f;
        private float _jumpTime = 0f;
        private bool _isFloating = false;
        
        // 瞄准变量
        private bool _isAiming = false;
        private bool _isFiring = false;
        
        // 移动方向
        private Vector3 _moveDirection = Vector3.forward;
        private Vector3 _lastMoveDirection = Vector3.forward;
        private Vector3 _aimDirection = Vector3.forward;
        #endregion

        #region Unity生命周期方法
        private void Awake()
        {
            // 获取必要组件
            if (_animator == null)
                _animator = GetComponent<Animator>();
                
            if (_player == null)
                _player = GetComponent<Player>();
                
            // 设置层权重
            _animator.SetLayerWeight(1, 1.0f); // 动作层
            _animator.SetLayerWeight(2, _upperBodyLayerWeight); // 上半身层
            _animator.SetLayerWeight(3, _additiveLayerWeight); // 附加层
        }

        private void Update()
        {
            UpdateAnimationParameters();
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 设置动画状态
        /// </summary>
        /// <param name="state">目标动画状态</param>
        public void SetAnimationState(AnimationState state)
        {
            if (_currentAnimState == state)
                return;
                
            _currentAnimState = state;
            
            // 设置状态到动画控制器
            _animator.SetInteger(_motionStateHash, (int)state);
            
            // 根据状态触发对应的动画
            switch (state)
            {
                case AnimationState.Jump:
                    _animator.SetTrigger(_jumpTriggerHash);
                    // 立即重置触发器，防止持续触发
                    StartCoroutine(ResetTriggerNextFrame(_jumpTriggerHash));
                    break;
                case AnimationState.DoubleJump:
                    _animator.SetTrigger(_doubleJumpTriggerHash);
                    StartCoroutine(ResetTriggerNextFrame(_doubleJumpTriggerHash));
                    break;
                case AnimationState.Fall:
                    _animator.SetTrigger(_fallTriggerHash);
                    StartCoroutine(ResetTriggerNextFrame(_fallTriggerHash));
                    break;
                case AnimationState.Land:
                    _animator.SetTrigger(_landTriggerHash);
                    StartCoroutine(ResetTriggerNextFrame(_landTriggerHash));
                    break;
                case AnimationState.HardLand:
                    _animator.SetTrigger(_hardLandTriggerHash);
                    StartCoroutine(ResetTriggerNextFrame(_hardLandTriggerHash));
                    break;
                case AnimationState.Roll:
                    _animator.SetTrigger(_rollTriggerHash);
                    StartCoroutine(ResetTriggerNextFrame(_rollTriggerHash));
                    break;
            }
        }

        /// <summary>
        /// 触发跳跃动画
        /// </summary>
        public void TriggerJump()
        {
            _animator.SetTrigger(_jumpTriggerHash);
            StartCoroutine(ResetTriggerNextFrame(_jumpTriggerHash));
            _jumpTime = 0f;
        }

        /// <summary>
        /// 触发二段跳动画
        /// </summary>
        public void TriggerDoubleJump()
        {
            _animator.SetTrigger(_doubleJumpTriggerHash);
            StartCoroutine(ResetTriggerNextFrame(_doubleJumpTriggerHash));
            _jumpTime = 0f;
        }

        /// <summary>
        /// 触发下落动画
        /// </summary>
        public void TriggerFall()
        {
            _animator.SetTrigger(_fallTriggerHash);
            StartCoroutine(ResetTriggerNextFrame(_fallTriggerHash));
            _airTime = 0f;
        }

        /// <summary>
        /// 触发着陆动画
        /// </summary>
        public void TriggerLand()
        {
            _animator.SetTrigger(_landTriggerHash);
            StartCoroutine(ResetTriggerNextFrame(_landTriggerHash));
        }

        /// <summary>
        /// 触发硬着陆动画
        /// </summary>
        public void TriggerHardLand()
        {
            _animator.SetTrigger(_hardLandTriggerHash);
            StartCoroutine(ResetTriggerNextFrame(_hardLandTriggerHash));
        }

        /// <summary>
        /// 触发翻滚动画
        /// </summary>
        public void TriggerRoll()
        {
            _animator.SetTrigger(_rollTriggerHash);
            StartCoroutine(ResetTriggerNextFrame(_rollTriggerHash));
        }

        /// <summary>
        /// 设置下蹲状态
        /// </summary>
        /// <param name="isCrouching">是否下蹲</param>
        public void SetCrouchState(bool isCrouching)
        {
            Debug.Log($"PlayerAnimController.SetCrouchState: 设置下蹲状态 = {isCrouching}");
            
            _animator.SetBool(_isCrouchingHash, isCrouching);
            
            if (isCrouching)
            {
                Debug.Log("PlayerAnimController: 设置为下蹲动画状态");
                SetAnimationState(AnimationState.Crouch);
            }
            else if (_currentAnimState == AnimationState.Crouch)
            {
                // 根据当前是否移动来恢复到合适的动画状态
                if (_isMoving)
                {
                    if (_isRunning)
                        SetAnimationState(AnimationState.Run);
                    else
                        SetAnimationState(AnimationState.Walk);
                }
                else
                {
                    SetAnimationState(AnimationState.Idle);
                }
            }
        }
        
        /// <summary>
        /// 直接触发下蹲动画（用于过渡）
        /// </summary>
        public void TriggerCrouchAnimation()
        {
            Debug.Log("PlayerAnimController.TriggerCrouchAnimation: 直接触发下蹲动画");
            
            // 设置下蹲状态
            _animator.SetBool(_isCrouchingHash, true);
            
            // 触发下蹲触发器
            _animator.SetTrigger(_crouchTriggerHash);
            StartCoroutine(ResetTriggerNextFrame(_crouchTriggerHash));
            
            // 设置动画状态
            SetAnimationState(AnimationState.Crouch);
            
            // 打印Animator参数
            Debug.Log($"Animator参数 - IsCrouching: {_animator.GetBool(_isCrouchingHash)}");
        }

        /// <summary>
        /// 触发滑行动画
        /// </summary>
        public void TriggerSlide()
        {
            _animator.SetTrigger(_slideHash);
            StartCoroutine(ResetTriggerNextFrame(_slideHash));
            SetAnimationState(AnimationState.Slide);
        }

        /// <summary>
        /// 设置移动状态
        /// </summary>
        /// <param name="isMoving">是否移动</param>
        /// <param name="isRunning">是否奔跑</param>
        /// <param name="speed">移动速度</param>
        public void SetMovementState(bool isMoving, bool isRunning, float speed)
        {
            _isMoving = isMoving;
            _isRunning = isRunning;
            _targetSpeed = speed;
            
            // 根据移动状态设置动画状态
            if (!_isMoving)
            {
                SetAnimationState(AnimationState.Idle);
            }
            else if (_isRunning)
            {
                SetAnimationState(AnimationState.Run);
            }
            else
            {
                SetAnimationState(AnimationState.Walk);
            }
        }

        /// <summary>
        /// 设置接地状态
        /// </summary>
        /// <param name="isGrounded">是否接地</param>
        /// <param name="verticalVelocity">垂直速度</param>
        public void SetGroundedState(bool isGrounded, float verticalVelocity)
        {
            _isGrounded = isGrounded;
            _verticalVelocity = verticalVelocity;
            
            // 更新空中时间
            if (!_isGrounded)
            {
                _airTime += Time.deltaTime;
                
                // 长时间空中则进入漂浮状态
                if (_airTime > 0.8f)
                {
                    _isFloating = true;
                }
            }
            else
            {
                _airTime = 0f;
                _isFloating = false;
            }
        }

        /// <summary>
        /// 设置瞄准状态
        /// </summary>
        /// <param name="isAiming">是否瞄准</param>
        public void SetAimingState(bool isAiming)
        {
            _isAiming = isAiming;
            
            // 更新是否瞄准或开火状态
            UpdateAimingOrFiringState();
        }

        /// <summary>
        /// 设置水平速度
        /// </summary>
        /// <param name="horizontalSpeed">水平速度</param>
        public void SetHorizontalSpeed(float horizontalSpeed)
        {
            _horizontalSpeed = horizontalSpeed;
        }

        /// <summary>
        /// 更新跳跃时间
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        public void UpdateJumpTime(float deltaTime)
        {
            _jumpTime += deltaTime;
        }

        public void SetUpperBodyLayerWeight(float weight)
        {
            _upperBodyLayerWeight = Mathf.Clamp01(weight);
            _animator.SetLayerWeight(2, _upperBodyLayerWeight);
        }

        public void SetAdditiveLayerWeight(float weight)
        {
            _additiveLayerWeight = Mathf.Clamp01(weight);
            _animator.SetLayerWeight(3, _additiveLayerWeight);
        }

        /// <summary>
        /// 设置漂浮状态
        /// </summary>
        /// <param name="isFloating">是否处于漂浮状态</param>
        public void SetFloatingState(bool isFloating)
        {
            _isFloating = isFloating;
            _animator.SetBool(_isFloatingHash, isFloating);
        }

        /// <summary>
        /// 设置开火状态
        /// </summary>
        /// <param name="isFiring">是否开火</param>
        public void SetFiringState(bool isFiring)
        {
            _isFiring = isFiring;
                
            // 更新是否瞄准或开火状态
            UpdateAimingOrFiringState();
            
            // 设置动画参数
            _animator.SetBool(_isFireingHash, isFiring);
            
            if (isFiring)
            {
                SetAnimationState(AnimationState.Fire);
            }
            else if (_currentAnimState == AnimationState.Fire)
            {
                // 如果当前是开火状态，根据其他状态恢复
                if (_isAiming)
                {
                    SetAnimationState(AnimationState.Aim);
                }
                else if (_isMoving)
                {
                    if (_isRunning)
                        SetAnimationState(AnimationState.Run);
                    else
                        SetAnimationState(AnimationState.Walk);
                }
                else
                {
                    SetAnimationState(AnimationState.Idle);
                }
            }
        }
        
        /// <summary>
        /// 触发开火动画
        /// </summary>
        public void TriggerFire()
        {
            Debug.Log("PlayerAnimController.TriggerFire: 触发开火动画");
            
            _animator.SetTrigger(_fireTriggerHash);
            StartCoroutine(ResetTriggerNextFrame(_fireTriggerHash));
            
            // 确保开火状态为true
            if (!_isFiring)
            {
                SetFiringState(true);
            }
        }

        /// <summary>
        /// 重置所有状态
        /// </summary>
        public void ResetAllStates()
        {
            _currentAnimState = AnimationState.Idle;
            _animator.SetInteger(_motionStateHash, (int)_currentAnimState);
            
            _isMoving = false;
            _isRunning = false;
            _isGrounded = true;
            _isAiming = false;
            _isFiring = false;
            
            _animator.SetBool(_isMovingHash, _isMoving);
            _animator.SetBool(_isRunningHash, _isRunning);
            _animator.SetBool(_isGroundedHash, _isGrounded);
            _animator.SetBool(_isAimingHash, _isAiming);
            _animator.SetBool(_isFireingHash, _isFiring);
            
            _animator.SetFloat(_speedHash, 0f);
            _animator.SetFloat(_verticalVelocityHash, 0f);
            _animator.SetFloat(_horizontalSpeedHash, 0f);
            
            ResetAnimatorTriggers();
        }

        /// <summary>
        /// 更新是否瞄准或开火状态
        /// </summary>
        private void UpdateAimingOrFiringState()
        {
            bool isAimingOrFiring = _isAiming || _isFiring;
            _animator.SetBool(_aimingOrFiringHash, isAimingOrFiring);
        }
        
        /// <summary>
        /// 设置瞄准方向
        /// </summary>
        /// <param name="aimDirection">瞄准方向</param>
        public void SetAimDirection(Vector3 aimDirection)
        {
            if (aimDirection.magnitude < 0.1f)
                return;
                
            _aimDirection = aimDirection.normalized;
            
            // 如果有移动方向，更新相对于瞄准方向的移动
            if (_moveDirection.magnitude > 0.1f)
            {
                UpdateRelativeMovement();
            }
        }
        
        /// <summary>
        /// 设置移动方向
        /// </summary>
        /// <param name="moveDirection">移动方向</param>
        public void SetMoveDirection(Vector3 moveDirection)
        {
            if (moveDirection.magnitude < 0.1f)
                return;
                
            _lastMoveDirection = _moveDirection;
            _moveDirection = moveDirection.normalized;
            
            // 更新相对于瞄准方向的移动
            UpdateRelativeMovement();
        }
        
        /// <summary>
        /// 更新相对于瞄准方向的移动
        /// </summary>
        private void UpdateRelativeMovement()
        {
            // 获取瞄准方向的水平分量
            Vector3 horizontalAimDir = new Vector3(_aimDirection.x, 0, _aimDirection.z).normalized;
            
            // 如果瞄准方向或移动方向无效，直接返回
            if (horizontalAimDir.magnitude < 0.1f || _moveDirection.magnitude < 0.1f)
                return;
                
            // 计算瞄准方向的右向量
            Vector3 aimRight = Vector3.Cross(Vector3.up, horizontalAimDir).normalized;
            
            // 计算移动方向在瞄准前方和右方的投影
            float forwardAmount = Vector3.Dot(_moveDirection, horizontalAimDir);
            float rightAmount = Vector3.Dot(_moveDirection, aimRight);
            
            // 设置移动方向参数（相对于瞄准方向）
            _animator.SetFloat(_moveDirectionXHash, rightAmount);
            _animator.SetFloat(_moveDirectionZHash, forwardAmount);
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 更新动画参数
        /// </summary>
        private void UpdateAnimationParameters()
        {
            // 平滑过渡速度
            _currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, _animationDampTime * Time.deltaTime);
            
            // 更新基本运动参数
            _animator.SetFloat(_speedHash, _currentSpeed);
            _animator.SetBool(_isMovingHash, _isMoving);
            _animator.SetBool(_isRunningHash, _isRunning);
            _animator.SetBool(_isGroundedHash, _isGrounded);
            _animator.SetFloat(_verticalVelocityHash, _verticalVelocity);
            _animator.SetFloat(_horizontalSpeedHash, _horizontalSpeed);
            
            // 更新空中状态参数
            _animator.SetBool(_isFloatingHash, _isFloating);
            _animator.SetFloat(_airTimeHash, _airTime);
            _animator.SetFloat(_jumpTimeHash, _jumpTime);
            
            // 更新瞄准状态
            _animator.SetBool(_isAimingHash, _isAiming);
        }

        /// <summary>
        /// 重置所有触发器
        /// </summary>
        private void ResetAnimatorTriggers()
        {
            _animator.ResetTrigger(_jumpTriggerHash);
            _animator.ResetTrigger(_doubleJumpTriggerHash);
            _animator.ResetTrigger(_fallTriggerHash);
            _animator.ResetTrigger(_landTriggerHash);
            _animator.ResetTrigger(_hardLandTriggerHash);
            _animator.ResetTrigger(_rollTriggerHash);
            _animator.ResetTrigger(_crouchTriggerHash);
            _animator.ResetTrigger(_fireTriggerHash);
        }

        /// <summary>
        /// 在下一帧重置触发器
        /// </summary>
        private IEnumerator ResetTriggerNextFrame(int triggerHash)
        {
            // 等待一帧，确保动画系统已经接收到触发器
            yield return null;
            
            // 重置触发器
            _animator.ResetTrigger(triggerHash);
        }
        #endregion
    }
} 