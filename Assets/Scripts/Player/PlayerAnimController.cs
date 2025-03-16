using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TrianCatStudio
{
    /// <summary>
    /// 角色动画控制器
    /// 负责管理角色的动画状态和参数，将角色状态转换为动画表现
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimController : MonoBehaviour
    {
        #region 动画参数哈希值
        // 动画参数哈希值缓存，提高性能
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
        #endregion

        #region 动画状态枚举
        /// <summary>
        /// 动画状态类型
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
            Slide = 11
        }
        #endregion

        #region 组件引用
        [Header("组件引用")]
        [Tooltip("角色动画器组件")]
        [SerializeField] private Animator _animator;
        
        [Tooltip("角色控制器")]
        [SerializeField] private Player _player;
        #endregion

        #region 动画设置
        [Header("动画设置")]
        [Tooltip("动画平滑过渡时间")]
        [SerializeField] private float _animationDampTime = 0.1f;
        
        [Tooltip("旋转平滑系数")]
        [SerializeField] private float _rotationSmoothTime = 0.1f;
        
        [Tooltip("倾斜动画曲线")]
        [SerializeField] private AnimationCurve _leanCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        [Tooltip("头部旋转动画曲线")]
        [SerializeField] private AnimationCurve _headLookCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField] private float _upperBodyLayerWeight = 0.8f;
        [SerializeField] private float _additiveLayerWeight = 0.5f;
        #endregion

        #region 运行时变量
        // 当前动画状态
        private AnimationState _currentAnimState = AnimationState.Idle;
        
        // 运动相关
        private float _currentSpeed = 0f;
        private float _targetSpeed = 0f;
        private float _verticalVelocity = 0f;
        private float _horizontalSpeed = 0f;
        private bool _isMoving = false;
        private bool _isRunning = false;
        private bool _isGrounded = true;
        
        // 空中状态相关
        private float _airTime = 0f;
        private float _jumpTime = 0f;
        private bool _isFloating = false;
        
        // 瞄准相关
        private bool _isAiming = false;
        
        // 倾斜和旋转相关
        private float _leanAmount = 0f;
        private float _headLookX = 0f;
        private float _headLookY = 0f;
        private Vector3 _previousRotation;
        private Vector3 _currentRotation;
        #endregion

        #region Unity生命周期
        private void Awake()
        {
            // 获取组件引用
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
            
            // 确保所有触发器在每帧结束时被重置
            ResetAllTriggers();
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
            
            // 根据状态设置动画参数
            _animator.SetInteger(_motionStateHash, (int)state);
            
            // 根据状态触发相应的动画
            switch (state)
            {
                case AnimationState.Jump:
                    // 先重置触发器，确保不会重复触发
                    _animator.ResetTrigger(_jumpTriggerHash);
                    _animator.SetTrigger(_jumpTriggerHash);
                    break;
                case AnimationState.DoubleJump:
                    // 先重置触发器，确保不会重复触发
                    _animator.ResetTrigger(_doubleJumpTriggerHash);
                    _animator.SetTrigger(_doubleJumpTriggerHash);
                    break;
                case AnimationState.Fall:
                    // 先重置触发器，确保不会重复触发
                    _animator.ResetTrigger(_fallTriggerHash);
                    _animator.SetTrigger(_fallTriggerHash);
                    break;
                case AnimationState.Land:
                    // 先重置触发器，确保不会重复触发
                    _animator.ResetTrigger(_landTriggerHash);
                    _animator.SetTrigger(_landTriggerHash);
                    break;
                case AnimationState.HardLand:
                    // 先重置触发器，确保不会重复触发
                    _animator.ResetTrigger(_hardLandTriggerHash);
                    _animator.SetTrigger(_hardLandTriggerHash);
                    break;
                case AnimationState.Roll:
                    // 先重置触发器，确保不会重复触发
                    _animator.ResetTrigger(_rollTriggerHash);
                    _animator.SetTrigger(_rollTriggerHash);
                    break;
            }
        }

        /// <summary>
        /// 触发跳跃动画
        /// </summary>
        public void TriggerJump()
        {
            // 先重置触发器，确保不会重复触发
            _animator.ResetTrigger(_jumpTriggerHash);
            _animator.SetTrigger(_jumpTriggerHash);
            _jumpTime = 0f;
        }

        /// <summary>
        /// 触发二段跳动画
        /// </summary>
        public void TriggerDoubleJump()
        {
            // 先重置触发器，确保不会重复触发
            _animator.ResetTrigger(_doubleJumpTriggerHash);
            _animator.SetTrigger(_doubleJumpTriggerHash);
            _jumpTime = 0f;
        }

        /// <summary>
        /// 触发下落动画
        /// </summary>
        public void TriggerFall()
        {
            // 先重置触发器，确保不会重复触发
            _animator.ResetTrigger(_fallTriggerHash);
            _animator.SetTrigger(_fallTriggerHash);
            _airTime = 0f;
        }

        /// <summary>
        /// 触发着陆动画
        /// </summary>
        public void TriggerLand()
        {
            // 先重置触发器，确保不会重复触发
            _animator.ResetTrigger(_landTriggerHash);
            _animator.SetTrigger(_landTriggerHash);
        }

        /// <summary>
        /// 触发硬着陆动画
        /// </summary>
        public void TriggerHardLand()
        {
            // 先重置触发器，确保不会重复触发
            _animator.ResetTrigger(_hardLandTriggerHash);
            _animator.SetTrigger(_hardLandTriggerHash);
        }

        /// <summary>
        /// 触发翻滚动画
        /// </summary>
        public void TriggerRoll()
        {
            // 先重置触发器，确保不会重复触发
            _animator.ResetTrigger(_rollTriggerHash);
            _animator.SetTrigger(_rollTriggerHash);
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
                Debug.Log("PlayerAnimController: 触发下蹲动画状态");
                SetAnimationState(AnimationState.Crouch);
            }
            else if (_currentAnimState == AnimationState.Crouch)
            {
                // 如果当前是下蹲状态，则恢复到合适的移动状态
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
        /// 直接触发下蹲动画（用于调试）
        /// </summary>
        public void TriggerCrouchAnimation()
        {
            Debug.Log("PlayerAnimController.TriggerCrouchAnimation: 直接触发下蹲动画");
            
            // 设置下蹲状态
            _animator.SetBool(_isCrouchingHash, true);
            
            // 触发下蹲触发器
            _animator.SetTrigger(_crouchTriggerHash);
            
            // 设置动画状态
            SetAnimationState(AnimationState.Crouch);
            
            // 打印Animator参数
            Debug.Log($"Animator参数 - IsCrouching: {_animator.GetBool(_isCrouchingHash)}");
        }

        /// <summary>
        /// 触发滑铲动画
        /// </summary>
        public void TriggerSlide()
        {
            _animator.SetTrigger(_slideHash);
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
                
                // 检测是否应该进入漂浮状态
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
        /// 重置所有状态
        /// </summary>
        public void ResetAllStates()
        {
            _isFloating = false;
            _animator.SetBool(_isFloatingHash, false);
            _airTime = 0f;
            _jumpTime = 0f;
        }

        /// <summary>
        /// 重置跳跃触发器
        /// </summary>
        public void ResetJumpTrigger()
        {
            _animator.ResetTrigger(_jumpTriggerHash);
        }

        /// <summary>
        /// 重置所有触发器
        /// </summary>
        public void ResetAllTriggers()
        {
            _animator.ResetTrigger(_jumpTriggerHash);
            _animator.ResetTrigger(_doubleJumpTriggerHash);
            _animator.ResetTrigger(_fallTriggerHash);
            _animator.ResetTrigger(_landTriggerHash);
            _animator.ResetTrigger(_hardLandTriggerHash);
            _animator.ResetTrigger(_rollTriggerHash);
            _animator.ResetTrigger(_slideHash);
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
            
            // 计算倾斜和旋转
            CalculateRotationalAdditives();
        }

        /// <summary>
        /// 计算旋转相关的动画参数
        /// </summary>
        private void CalculateRotationalAdditives()
        {
            // 获取当前旋转
            _currentRotation = transform.forward;
            
            // 计算旋转速率
            float rotationRate = 0f;
            if (_previousRotation != Vector3.zero)
            {
                rotationRate = Vector3.SignedAngle(_currentRotation, _previousRotation, Vector3.up) / Time.deltaTime * -1f;
            }
             
            // 计算倾斜量
            float targetLeanAmount = rotationRate * 0.01f;
            _leanAmount = Mathf.Lerp(_leanAmount, targetLeanAmount, _rotationSmoothTime * Time.deltaTime);
            
            // 计算头部旋转
            float targetHeadLookX = rotationRate * 0.005f;
            _headLookX = Mathf.Lerp(_headLookX, targetHeadLookX, _rotationSmoothTime * Time.deltaTime);
            
            // 应用倾斜和头部旋转曲线
            float leanMultiplier = _leanCurve.Evaluate(Mathf.Abs(_leanAmount));
            float headLookMultiplier = _headLookCurve.Evaluate(Mathf.Abs(_headLookX));
            
            // 设置动画参数
            // 注意：这些参数需要在Animator中定义
            if (_animator.parameters.Any(p => p.name == "LeanAmount"))
                _animator.SetFloat("LeanAmount", _leanAmount * leanMultiplier);
                
            if (_animator.parameters.Any(p => p.name == "HeadLookX"))
                _animator.SetFloat("HeadLookX", _headLookX * headLookMultiplier);
                
            if (_animator.parameters.Any(p => p.name == "HeadLookY"))
                _animator.SetFloat("HeadLookY", _headLookY);
            
            // 保存当前旋转为下一帧的前一帧旋转
            _previousRotation = _currentRotation;
        }
        #endregion
    }
} 