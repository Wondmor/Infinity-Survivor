using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

namespace TrianCatStudio
{
    [RequireComponent(typeof(Rigidbody), typeof(Animator), typeof(GroundDetector))]
    public class Player : MonoBehaviour
    {
        // �������
        [Header("Component References")]
        [SerializeField] public Rigidbody Rb;
        [SerializeField] public Animator Animator;
        [SerializeField] public InputManager InputManager;
        [SerializeField] public Transform CameraPivot;
        [SerializeField] public PlayerAnimController AnimController; // ʹ��PlayerAnimController����
        [SerializeField] public GroundDetector GroundDetector; // �������������

        // �ӵ�����
        [Header("Shooting Settings")]
        [SerializeField] public GameObject BulletPrefab; // �ӵ�Ԥ����
        [SerializeField] public Transform FirePoint; // �����
        [SerializeField] public float BulletSpeed = 20f; // �ӵ��ٶ�
        [SerializeField] public float BulletLifetime = 5f; // �ӵ���������
        [SerializeField] public float FireRate = 0.2f; // ���Ƶ��
        [SerializeField] public float BulletDamage = 10f; // �ӵ������˺�
        private float lastFireTime; // �ϴ����ʱ��

        // �ƶ�����
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float rotationSpeed = 15f;
        [SerializeField] private float airControlMultiplier = 0.5f; // ���п���ϵ��
        [SerializeField] private float airAccelerationMultiplier = 0.7f; // ���м��ٶ�ϵ��
        [SerializeField] private float airMaxSpeedMultiplier = 0.8f; // ��������ٶ�ϵ��
        [SerializeField] private float airDirectionChangeMultiplier = 0.5f; // ���з���ı�ϵ��
        [SerializeField] private float landingRecoveryTime = 0.2f; // ��ػָ�ʱ��

        // ��Ծ����
        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float doubleJumpForce = 4f;
        [SerializeField] private float jumpCooldown = 0.1f;
        [SerializeField] public float coyoteTime = 0.2f; // �޸�Ϊ�����ֶ�
        [SerializeField] private int maxJumpCount = 2;

        // �¶׺ͻ�������
        [Header("Crouch and Slide Settings")]
        [SerializeField] private float crouchSpeed = 1.5f; // �¶��ƶ��ٶ�
        [SerializeField] private float slideSpeed = 8f; // ������ʼ�ٶ�
        [SerializeField] private float slideDeceleration = 5f; // �������ٶ�
        [SerializeField] private float minSpeedToSlide = 3f; // ������������С�ٶ�
        [SerializeField] private float slideCooldown = 1f; // ������ȴʱ��

        // ����ʱ״̬
        public bool IsGrounded => GroundDetector?.IsGrounded ?? false;
        public Vector3 MoveDirection { get; private set; }
        public Vector3 Velocity { get; private set; }
        public bool HasDoubleJumped { get; set; }
        public float lastGroundedTime => GroundDetector?.LastGroundedTime ?? 0f;
        public float lastJumpTime { get; set; }
        private float landingTime; // ���ʱ��
        public int jumpCount { get; set; }
        public bool isJumpRequested { get; set; }
        private float lastSlideTime; // �ϴλ���ʱ��
        public bool IsCrouching { get; private set; } // �Ƿ������¶�
        public bool IsSliding { get; private set; } // �Ƿ����ڻ���

        // ״̬��ϵͳ
        private PlayerStateManager stateManager;

        private void Awake()
        {
            try
            {
                Debug.Log("Player.Awake: ��ʼ��ʼ��");
                
                Rb = GetComponent<Rigidbody>();
                Animator = GetComponent<Animator>();
                InputManager = GetComponent<InputManager>();
                
                // ��ȡ���������
                GroundDetector = GetComponent<GroundDetector>();
                if (GroundDetector == null)
                {
                    Debug.LogError("Player.Awake: δ�ҵ�GroundDetector���");
                    GroundDetector = gameObject.AddComponent<GroundDetector>();
                }
                
                // ���ĵ������¼�
                GroundDetector.OnGroundStateChanged += HandleGroundStateChanged;
                GroundDetector.OnLanding += HandleLanding;
                GroundDetector.OnHardLanding += HandleHardLanding;
                
                // ȷ��PlayerStateManager������ڲ�����
                stateManager = GetComponent<PlayerStateManager>();
                if (stateManager == null)
                {
                    Debug.Log("Player.Awake: ���PlayerStateManager���");
                    stateManager = gameObject.AddComponent<PlayerStateManager>();
                }
                
                // ȷ�����������
                if (stateManager != null && !stateManager.enabled)
                {
                    Debug.Log("Player.Awake: ����PlayerStateManager���");
                    stateManager.enabled = true;
                }
                
                if (AnimController == null)
                {
                    AnimController = GetComponent<PlayerAnimController>();
                }
                
                CameraPivot = FindObjectOfType<Camera>()?.transform;
                if (CameraPivot == null)
                {
                    Debug.LogWarning("Player.Awake: δ�ҵ����");
                }

                // ע�������¼�
                if (InputManager != null)
                {
                    InputManager.OnJumpAction += HandleJumpInput;
                    InputManager.OnAimAction += HandleAimInput;
                    InputManager.OnCrouchAction += HandleCrouchInput;
                    InputManager.OnFireAction += HandleFireInput; // ���ο����¼�
                    InputManager.OnFireActionChanged += HandleFireInputChanged; // ��������״̬�仯�¼�
                }
                else
                {
                    Debug.LogError("Player.Awake: InputManagerΪ��");
                }
                
                // ��ʼ�������
                if (FirePoint == null)
                {
                    // ���û��ָ������㣬����һ��Ĭ�ϵķ����
                    GameObject firePointObj = new GameObject("FirePoint");
                    firePointObj.transform.SetParent(transform);
                    firePointObj.transform.localPosition = new Vector3(0, 1.5f, 0.5f); // �����ڽ�ɫǰ��ƫ�ϵ�λ��
                    FirePoint = firePointObj.transform;
                }
                
                Debug.Log("Player.Awake: ��ʼ�����");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Player.Awake: ��ʼ��ʧ�� - {e.Message}\n{e.StackTrace}");
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
            
            // ������׼����
            if (InputManager.IsAiming || InputManager.IsFirePressed)
            {
                UpdateAimDirection();
            }
        }

        private void FixedUpdate()
        {
            HandleMovement();
            
            // ����¥��
            if (IsGrounded && MoveDirection.magnitude > 0.1f)
            {
                GroundDetector.HandleStairs(MoveDirection);
            }
        }

        private void OnDestroy()
        {
            // ע���������¼�
            if (GroundDetector != null)
            {
                GroundDetector.OnGroundStateChanged -= HandleGroundStateChanged;
                GroundDetector.OnLanding -= HandleLanding;
                GroundDetector.OnHardLanding -= HandleHardLanding;
            }
            
            // ע�������¼�
            if (InputManager != null)
            {
                InputManager.OnJumpAction -= HandleJumpInput;
                InputManager.OnAimAction -= HandleAimInput;
                InputManager.OnCrouchAction -= HandleCrouchInput;
                InputManager.OnFireAction -= HandleFireInput; // ע�����ο����¼�
                InputManager.OnFireActionChanged -= HandleFireInputChanged; // ע����������״̬�仯�¼�
            }
        }

        #region ���봦��
        private void HandleJumpInput()
        {
            // ����Ƿ������Ծ
            bool canJump = IsGrounded || Time.time - lastGroundedTime <= coyoteTime;
            bool canDoubleJump = CanDoubleJump();
            
            // ��¼��Ծ����ʱ��
            lastJumpTime = Time.time;
            
            // ֪ͨ״̬��������Ծ
            if (canJump)
            {
                // �ڵ����ϻ�����ʱ���ڣ�ִ��һ����
                jumpCount = 0; // ȷ����0��ʼ����
                HasDoubleJumped = false;
                
                // ������Ծ����
                jumpCount = 1;
                
                Debug.Log("Player.HandleJumpInput: ����һ����");
                stateManager.TriggerJump();
            }
            else if (canDoubleJump)
            {
                // �ڿ������Ѿ�����һ�Σ�ִ�ж�����
                Debug.Log($"Player.HandleJumpInput: ���������� - jumpCount={jumpCount}, HasDoubleJumped={HasDoubleJumped}");
                
                // ��������״̬��ȷ�������ظ�����
                HasDoubleJumped = true;
                jumpCount = 2;
                
                // ����������
                stateManager.TriggerDoubleJump();
            }
            else
            {
                Debug.Log($"Player.HandleJumpInput: �޷���Ծ - IsGrounded={IsGrounded}, jumpCount={jumpCount}, HasDoubleJumped={HasDoubleJumped}");
            }
        }

        private void HandleAimInput(bool isAiming)
        {
            // ֪ͨ״̬��
            stateManager.SetAiming(isAiming);
            
            // ����AnimatorController����׼״̬
            if (AnimController != null)
            {
                AnimController.SetAimingState(isAiming);
            }
        }

        private void HandleCrouchInput(bool isCrouching)
        {
            Debug.Log($"Player.HandleCrouchInput: �յ��¶����� = {isCrouching}");
            
            // ֪ͨ״̬��
            stateManager.TriggerCrouch(isCrouching);
            
            // ����AnimatorController���¶�״̬
            if (AnimController != null)
            {
                AnimController.SetCrouchState(isCrouching);
                
                // ������¶ף�ֱ�Ӵ����¶׶��������ڵ��ԣ�
                if (isCrouching)
                {
                    AnimController.TriggerCrouchAnimation();
                }
            }
            
            // ������ܲ�״̬�°����¶ף���������
            if (isCrouching && IsGrounded && Rb.velocity.magnitude > minSpeedToSlide && Time.time > lastSlideTime + slideCooldown)
            {
                TriggerSlide();
            }
            
            // �����¶�״̬
            IsCrouching = isCrouching;
            
            // ��ӡ��ǰ״̬
            Debug.Log($"Player״̬ - IsCrouching: {IsCrouching}, IsGrounded: {IsGrounded}, �ٶ�: {Rb.velocity.magnitude}");
        }
        
        private void TriggerSlide()
        {
            // ��¼����ʱ��
            lastSlideTime = Time.time;
            
            // ���û���״̬
            IsSliding = true;
            
            // ֪ͨ״̬��
            stateManager.TriggerSlide();
            
            Debug.Log("��������");
        }

        private void HandleFireInput()
        {
            // ��������ȴ
            if (Time.time - lastFireTime < FireRate)
            {
                Debug.Log($"Player.HandleFireInput: �����ȴ�� - ʣ��ʱ��: {FireRate - (Time.time - lastFireTime):F2}��");
                return;
            }
            
            // �����ϴ����ʱ��
            lastFireTime = Time.time;
            
            Debug.Log("Player.HandleFireInput: �������");
            
            // ֪ͨ״̬���������
            stateManager.TriggerFire();
        }
        
        // �����������״̬�仯
        private void HandleFireInputChanged(bool isFiring)
        {
            Debug.Log($"Player.HandleFireInputChanged: ����״̬�仯 = {isFiring}");
            
            if (isFiring)
            {
                // ��ʼ���������״δ����� HandleFireInput ����
                // ���ﲻ��Ҫ���⴦��
            }
            else
            {
                // ֹͣ����
                stateManager.StopFiring();
            }
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
            
            // ���ƶ����򴫵ݸ�����������
            if (AnimController != null && MoveDirection.magnitude > 0.1f)
            {
                AnimController.SetMoveDirection(MoveDirection);
            }
        }

        private void UpdateStateParameters()
        {
            // ͬ�����뵽״̬�����������ö�������
            stateManager.SetMoveInput(InputManager.GetInputMagnitude());
            stateManager.SetRunning(InputManager.IsRunning);
            stateManager.SetGrounded(IsGrounded);
            stateManager.SetVerticalVelocity(Rb.velocity.y);
            
            // ����AnimatorController���ƶ�״̬
            if (AnimController != null)
            {
                bool isMoving = InputManager.GetInputMagnitude() > 0.1f;
                bool isRunning = InputManager.IsRunning;
                float speed = isMoving ? (isRunning ? runSpeed : walkSpeed) : 0f;
                AnimController.SetMovementState(isMoving, isRunning, speed);
                
                // ���½ӵ�״̬
                AnimController.SetGroundedState(IsGrounded, Rb.velocity.y);
            }
        }
        
        // ������׼����
        private void UpdateAimDirection()
        {
            if (AnimController == null || CameraPivot == null)
                return;
            
            // ʹ�����ǰ����Ϊ��׼����
            Vector3 aimDirection = CameraPivot.forward;
            
            // ����׼���򴫵ݸ�����������
            AnimController.SetAimDirection(aimDirection);
            
            // ����׼�򿪻�ʱ��ֱ���ý�ɫ������׼����
            if (InputManager.IsAiming || InputManager.IsFirePressed)
            {
                // ��ȡˮƽ���򣨺���Y�ᣩ
                Vector3 horizontalAimDirection = new Vector3(aimDirection.x, 0, aimDirection.z).normalized;
                
                // ֱ�����ý�ɫ����
                if (horizontalAimDirection.magnitude > 0.1f)
                {
                    transform.rotation = Quaternion.LookRotation(horizontalAimDirection);
                }
            }
        }
        #endregion

        #region �������¼�����
        private void HandleGroundStateChanged(bool isGrounded)
        {
            // ֪ͨ״̬������״̬�仯
            stateManager.SetGrounded(isGrounded);
            
            if (isGrounded)
            {
                // ���ŵ�ʱ�Ĵ���
                HasDoubleJumped = false;
                ResetJumpCount();
            }
            else if (Rb.velocity.y < 0)
            {
                // �뿪��������������ʱ
                if (AnimController != null)
                {
                    AnimController.TriggerFall();
                }
            }
        }
        
        private void HandleLanding(float fallSpeed)
        {
            // ��¼��½ʱ��
            landingTime = Time.time;
            
            // ����������½״̬
            stateManager.TriggerLanding();
            
            // ʹ��AnimatorController������½����
            if (AnimController != null)
            {
                AnimController.TriggerLand();
            }
        }
        
        private void HandleHardLanding(float fallSpeed)
        {
            // ��¼��½ʱ��
            landingTime = Time.time;
            
            // ����Ӳ��½״̬
            stateManager.TriggerHardLanding();
            
            // ʹ��AnimatorController����Ӳ��½����
            if (AnimController != null)
            {
                AnimController.TriggerHardLand();
            }
        }
        #endregion

        #region ����ϵͳ
        private void HandleMovement()
        {
            float targetSpeed = GetTargetSpeed();
            
            // ����Ŀ���ٶ�
            Vector3 targetVelocity = MoveDirection * targetSpeed;
            
            // �������ٶȱ���
            float currentAcceleration;
            
            // �����ƶ�����
            if (!IsGrounded)
            {
                // ���浱ǰˮƽ�ٶ�
                Vector3 currentHorizontalVelocity = new Vector3(Rb.velocity.x, 0, Rb.velocity.z);
                float currentHorizontalSpeed = currentHorizontalVelocity.magnitude;
                
                // ���ƿ�������ٶ�
                float airMaxSpeed = targetSpeed * airMaxSpeedMultiplier;
                
                // �����ǰ�ٶ��Ѿ�������������ٶȣ����ֵ�ǰ�ٶ�
                if (currentHorizontalSpeed > airMaxSpeed)
                {
                    // ���ֵ�ǰ���򣬵������ٶ�
                    targetVelocity = currentHorizontalVelocity.normalized * currentHorizontalSpeed;
                }
                else
                {
                    // ���㷽��仯�̶� (0-1)��1��ʾ��ȫ�෴����
                    float directionChange = 0;
                    if (currentHorizontalSpeed > 0.1f)
                    {
                        directionChange = 1f - Vector3.Dot(currentHorizontalVelocity.normalized, MoveDirection.normalized) * 0.5f;
                    }
                    
                    // ���ݷ���仯��������ϵ��
                    float effectiveAirControl = airControlMultiplier * (1f - directionChange * airDirectionChangeMultiplier);
                    
                    // Ӧ�ÿ��п���
                    targetVelocity = Vector3.Lerp(currentHorizontalVelocity, MoveDirection * airMaxSpeed, effectiveAirControl);
                }
                
                // ʹ�ý�С�ļ��ٶ�
                currentAcceleration = acceleration * airAccelerationMultiplier;
            }
            else
            {
                currentAcceleration = acceleration;
            }
            
            // ������ֱ�ٶ�
            targetVelocity.y = Rb.velocity.y;
            
            // ��ػ����ڼ�����ƶ��ٶ�
            if (IsGrounded && Time.time - landingTime < landingRecoveryTime)
            {
                float recoveryProgress = (Time.time - landingTime) / landingRecoveryTime;
                targetVelocity.x *= recoveryProgress;
                targetVelocity.z *= recoveryProgress;
            }

            // Ӧ���ٶ�
            Rb.velocity = Vector3.Lerp(Rb.velocity, targetVelocity,
                currentAcceleration * Time.fixedDeltaTime);
            Velocity = Rb.velocity;

            // ������ת
            if (MoveDirection.magnitude > 0.1f && !(InputManager.IsAiming || InputManager.IsFirePressed))
            {
                Quaternion targetRotation = Quaternion.LookRotation(MoveDirection);
                
                // �ڿ��м�����ת�ٶ�
                float currentRotationSpeed = IsGrounded ? rotationSpeed : rotationSpeed * airControlMultiplier;
                
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 
                    currentRotationSpeed * Time.fixedDeltaTime);
            }
            
            // ����״̬���еĴ�ֱ�ٶ�
            stateManager.SetVerticalVelocity(Velocity.y);
            
            // ����AnimatorController��ˮƽ�ٶ�
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

        #region ��������
        private void ResetJumpCount()
        {
            jumpCount = 0;
        }

        public float GetJumpForce()
        {
            return jumpCount <= 1 ? jumpForce : doubleJumpForce;
        }

        // ����Ƿ���Զ�����
        private bool CanDoubleJump()
        {
            // ��������IsGrounded���ԣ�ֻ�����Ծ�����Ͷ�����״̬
            return jumpCount == 1 && !HasDoubleJumped;
        }
        #endregion
    }
}