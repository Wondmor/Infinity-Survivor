using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TrianCatStudio
{
    /// <summary>
    /// ��ɫ����������
    /// ��������ɫ�Ķ���״̬�Ͳ���������ɫ״̬ת��Ϊ��������
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimController : MonoBehaviour
    {
        #region ����������ϣֵ
        // ����������ϣֵ���棬�������
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

        #region ����״̬ö��
        /// <summary>
        /// ����״̬����
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

        #region �������
        [Header("�������")]
        [Tooltip("��ɫ���������")]
        [SerializeField] private Animator _animator;
        
        [Tooltip("��ɫ������")]
        [SerializeField] private Player _player;
        #endregion

        #region ��������
        [Header("��������")]
        [Tooltip("����ƽ������ʱ��")]
        [SerializeField] private float _animationDampTime = 0.1f;
        
        [Tooltip("��תƽ��ϵ��")]
        [SerializeField] private float _rotationSmoothTime = 0.1f;
        
        [Tooltip("��б��������")]
        [SerializeField] private AnimationCurve _leanCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        [Tooltip("ͷ����ת��������")]
        [SerializeField] private AnimationCurve _headLookCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField] private float _upperBodyLayerWeight = 0.8f;
        [SerializeField] private float _additiveLayerWeight = 0.5f;
        #endregion

        #region ����ʱ����
        // ��ǰ����״̬
        private AnimationState _currentAnimState = AnimationState.Idle;
        
        // �˶����
        private float _currentSpeed = 0f;
        private float _targetSpeed = 0f;
        private float _verticalVelocity = 0f;
        private float _horizontalSpeed = 0f;
        private bool _isMoving = false;
        private bool _isRunning = false;
        private bool _isGrounded = true;
        
        // ����״̬���
        private float _airTime = 0f;
        private float _jumpTime = 0f;
        private bool _isFloating = false;
        
        // ��׼���
        private bool _isAiming = false;
        
        // ��б����ת���
        private float _leanAmount = 0f;
        private float _headLookX = 0f;
        private float _headLookY = 0f;
        private Vector3 _previousRotation;
        private Vector3 _currentRotation;
        #endregion

        #region Unity��������
        private void Awake()
        {
            // ��ȡ�������
            if (_animator == null)
                _animator = GetComponent<Animator>();
                
            if (_player == null)
                _player = GetComponent<Player>();
                
            // ���ò�Ȩ��
            _animator.SetLayerWeight(1, 1.0f); // ������
            _animator.SetLayerWeight(2, _upperBodyLayerWeight); // �ϰ����
            _animator.SetLayerWeight(3, _additiveLayerWeight); // ���Ӳ�
        }

        private void Update()
        {
            UpdateAnimationParameters();
            
            // ȷ�����д�������ÿ֡����ʱ������
            ResetAllTriggers();
        }
        #endregion

        #region ��������
        /// <summary>
        /// ���ö���״̬
        /// </summary>
        /// <param name="state">Ŀ�궯��״̬</param>
        public void SetAnimationState(AnimationState state)
        {
            if (_currentAnimState == state)
                return;
                
            _currentAnimState = state;
            
            // ����״̬���ö�������
            _animator.SetInteger(_motionStateHash, (int)state);
            
            // ����״̬������Ӧ�Ķ���
            switch (state)
            {
                case AnimationState.Jump:
                    // �����ô�������ȷ�������ظ�����
                    _animator.ResetTrigger(_jumpTriggerHash);
                    _animator.SetTrigger(_jumpTriggerHash);
                    break;
                case AnimationState.DoubleJump:
                    // �����ô�������ȷ�������ظ�����
                    _animator.ResetTrigger(_doubleJumpTriggerHash);
                    _animator.SetTrigger(_doubleJumpTriggerHash);
                    break;
                case AnimationState.Fall:
                    // �����ô�������ȷ�������ظ�����
                    _animator.ResetTrigger(_fallTriggerHash);
                    _animator.SetTrigger(_fallTriggerHash);
                    break;
                case AnimationState.Land:
                    // �����ô�������ȷ�������ظ�����
                    _animator.ResetTrigger(_landTriggerHash);
                    _animator.SetTrigger(_landTriggerHash);
                    break;
                case AnimationState.HardLand:
                    // �����ô�������ȷ�������ظ�����
                    _animator.ResetTrigger(_hardLandTriggerHash);
                    _animator.SetTrigger(_hardLandTriggerHash);
                    break;
                case AnimationState.Roll:
                    // �����ô�������ȷ�������ظ�����
                    _animator.ResetTrigger(_rollTriggerHash);
                    _animator.SetTrigger(_rollTriggerHash);
                    break;
            }
        }

        /// <summary>
        /// ������Ծ����
        /// </summary>
        public void TriggerJump()
        {
            // �����ô�������ȷ�������ظ�����
            _animator.ResetTrigger(_jumpTriggerHash);
            _animator.SetTrigger(_jumpTriggerHash);
            _jumpTime = 0f;
        }

        /// <summary>
        /// ��������������
        /// </summary>
        public void TriggerDoubleJump()
        {
            // �����ô�������ȷ�������ظ�����
            _animator.ResetTrigger(_doubleJumpTriggerHash);
            _animator.SetTrigger(_doubleJumpTriggerHash);
            _jumpTime = 0f;
        }

        /// <summary>
        /// �������䶯��
        /// </summary>
        public void TriggerFall()
        {
            // �����ô�������ȷ�������ظ�����
            _animator.ResetTrigger(_fallTriggerHash);
            _animator.SetTrigger(_fallTriggerHash);
            _airTime = 0f;
        }

        /// <summary>
        /// ������½����
        /// </summary>
        public void TriggerLand()
        {
            // �����ô�������ȷ�������ظ�����
            _animator.ResetTrigger(_landTriggerHash);
            _animator.SetTrigger(_landTriggerHash);
        }

        /// <summary>
        /// ����Ӳ��½����
        /// </summary>
        public void TriggerHardLand()
        {
            // �����ô�������ȷ�������ظ�����
            _animator.ResetTrigger(_hardLandTriggerHash);
            _animator.SetTrigger(_hardLandTriggerHash);
        }

        /// <summary>
        /// ������������
        /// </summary>
        public void TriggerRoll()
        {
            // �����ô�������ȷ�������ظ�����
            _animator.ResetTrigger(_rollTriggerHash);
            _animator.SetTrigger(_rollTriggerHash);
        }

        /// <summary>
        /// �����¶�״̬
        /// </summary>
        /// <param name="isCrouching">�Ƿ��¶�</param>
        public void SetCrouchState(bool isCrouching)
        {
            Debug.Log($"PlayerAnimController.SetCrouchState: �����¶�״̬ = {isCrouching}");
            
            _animator.SetBool(_isCrouchingHash, isCrouching);
            
            if (isCrouching)
            {
                Debug.Log("PlayerAnimController: �����¶׶���״̬");
                SetAnimationState(AnimationState.Crouch);
            }
            else if (_currentAnimState == AnimationState.Crouch)
            {
                // �����ǰ���¶�״̬����ָ������ʵ��ƶ�״̬
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
        /// ֱ�Ӵ����¶׶��������ڵ��ԣ�
        /// </summary>
        public void TriggerCrouchAnimation()
        {
            Debug.Log("PlayerAnimController.TriggerCrouchAnimation: ֱ�Ӵ����¶׶���");
            
            // �����¶�״̬
            _animator.SetBool(_isCrouchingHash, true);
            
            // �����¶״�����
            _animator.SetTrigger(_crouchTriggerHash);
            
            // ���ö���״̬
            SetAnimationState(AnimationState.Crouch);
            
            // ��ӡAnimator����
            Debug.Log($"Animator���� - IsCrouching: {_animator.GetBool(_isCrouchingHash)}");
        }

        /// <summary>
        /// ������������
        /// </summary>
        public void TriggerSlide()
        {
            _animator.SetTrigger(_slideHash);
            SetAnimationState(AnimationState.Slide);
        }

        /// <summary>
        /// �����ƶ�״̬
        /// </summary>
        /// <param name="isMoving">�Ƿ��ƶ�</param>
        /// <param name="isRunning">�Ƿ���</param>
        /// <param name="speed">�ƶ��ٶ�</param>
        public void SetMovementState(bool isMoving, bool isRunning, float speed)
        {
            _isMoving = isMoving;
            _isRunning = isRunning;
            _targetSpeed = speed;
            
            // �����ƶ�״̬���ö���״̬
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
        /// ���ýӵ�״̬
        /// </summary>
        /// <param name="isGrounded">�Ƿ�ӵ�</param>
        /// <param name="verticalVelocity">��ֱ�ٶ�</param>
        public void SetGroundedState(bool isGrounded, float verticalVelocity)
        {
            _isGrounded = isGrounded;
            _verticalVelocity = verticalVelocity;
            
            // ���¿���ʱ��
            if (!_isGrounded)
            {
                _airTime += Time.deltaTime;
                
                // ����Ƿ�Ӧ�ý���Ư��״̬
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
        /// ������׼״̬
        /// </summary>
        /// <param name="isAiming">�Ƿ���׼</param>
        public void SetAimingState(bool isAiming)
        {
            _isAiming = isAiming;
        }

        /// <summary>
        /// ����ˮƽ�ٶ�
        /// </summary>
        /// <param name="horizontalSpeed">ˮƽ�ٶ�</param>
        public void SetHorizontalSpeed(float horizontalSpeed)
        {
            _horizontalSpeed = horizontalSpeed;
        }

        /// <summary>
        /// ������Ծʱ��
        /// </summary>
        /// <param name="deltaTime">ʱ������</param>
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
        /// ����Ư��״̬
        /// </summary>
        /// <param name="isFloating">�Ƿ���Ư��״̬</param>
        public void SetFloatingState(bool isFloating)
        {
            _isFloating = isFloating;
            _animator.SetBool(_isFloatingHash, isFloating);
        }

        /// <summary>
        /// ��������״̬
        /// </summary>
        public void ResetAllStates()
        {
            _isFloating = false;
            _animator.SetBool(_isFloatingHash, false);
            _airTime = 0f;
            _jumpTime = 0f;
        }

        /// <summary>
        /// ������Ծ������
        /// </summary>
        public void ResetJumpTrigger()
        {
            _animator.ResetTrigger(_jumpTriggerHash);
        }

        /// <summary>
        /// �������д�����
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

        #region ˽�з���
        /// <summary>
        /// ���¶�������
        /// </summary>
        private void UpdateAnimationParameters()
        {
            // ƽ�������ٶ�
            _currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, _animationDampTime * Time.deltaTime);
            
            // ���»����˶�����
            _animator.SetFloat(_speedHash, _currentSpeed);
            _animator.SetBool(_isMovingHash, _isMoving);
            _animator.SetBool(_isRunningHash, _isRunning);
            _animator.SetBool(_isGroundedHash, _isGrounded);
            _animator.SetFloat(_verticalVelocityHash, _verticalVelocity);
            _animator.SetFloat(_horizontalSpeedHash, _horizontalSpeed);
            
            // ���¿���״̬����
            _animator.SetBool(_isFloatingHash, _isFloating);
            _animator.SetFloat(_airTimeHash, _airTime);
            _animator.SetFloat(_jumpTimeHash, _jumpTime);
            
            // ������׼״̬
            _animator.SetBool(_isAimingHash, _isAiming);
            
            // ������б����ת
            CalculateRotationalAdditives();
        }

        /// <summary>
        /// ������ת��صĶ�������
        /// </summary>
        private void CalculateRotationalAdditives()
        {
            // ��ȡ��ǰ��ת
            _currentRotation = transform.forward;
            
            // ������ת����
            float rotationRate = 0f;
            if (_previousRotation != Vector3.zero)
            {
                rotationRate = Vector3.SignedAngle(_currentRotation, _previousRotation, Vector3.up) / Time.deltaTime * -1f;
            }
             
            // ������б��
            float targetLeanAmount = rotationRate * 0.01f;
            _leanAmount = Mathf.Lerp(_leanAmount, targetLeanAmount, _rotationSmoothTime * Time.deltaTime);
            
            // ����ͷ����ת
            float targetHeadLookX = rotationRate * 0.005f;
            _headLookX = Mathf.Lerp(_headLookX, targetHeadLookX, _rotationSmoothTime * Time.deltaTime);
            
            // Ӧ����б��ͷ����ת����
            float leanMultiplier = _leanCurve.Evaluate(Mathf.Abs(_leanAmount));
            float headLookMultiplier = _headLookCurve.Evaluate(Mathf.Abs(_headLookX));
            
            // ���ö�������
            // ע�⣺��Щ������Ҫ��Animator�ж���
            if (_animator.parameters.Any(p => p.name == "LeanAmount"))
                _animator.SetFloat("LeanAmount", _leanAmount * leanMultiplier);
                
            if (_animator.parameters.Any(p => p.name == "HeadLookX"))
                _animator.SetFloat("HeadLookX", _headLookX * headLookMultiplier);
                
            if (_animator.parameters.Any(p => p.name == "HeadLookY"))
                _animator.SetFloat("HeadLookY", _headLookY);
            
            // ���浱ǰ��תΪ��һ֡��ǰһ֡��ת
            _previousRotation = _currentRotation;
        }
        #endregion
    }
} 