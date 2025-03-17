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
        [SerializeField] public PlayerAnimController AnimController; // ʹ��PlayerAnimController����

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

        // ���������
        [Header("Ground Detection Settings")]
        [SerializeField] private GroundDetectionMethod groundDetectionMethod = GroundDetectionMethod.MultiRaycast; // �����ⷽ��
        [SerializeField] private float groundCheckDistance = 0.3f; // ���������
        [SerializeField] private float groundCheckRadius = 0.4f;   // ������뾶
        [SerializeField] private int groundRayCount = 5;           // ��������������
        [SerializeField] private float maxSlopeAngle = 45f;        // ��������б�½Ƕ�
        [SerializeField] private LayerMask groundLayer = -1;       // ����㼶
        [SerializeField] private bool showDebugRays = true;        // �Ƿ���ʾ��������

        // ¥�ݴ������
        [Header("Stairs Handling")]
        [SerializeField] private bool enableStairsHandling = true; // ����¥�ݴ���
        [SerializeField] private float stepHeight = 0.3f;          // ���̨�׸߶�
        [SerializeField] private float stepSmoothing = 0.1f;       // ̨��ƽ������ʱ��

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
        public bool IsGrounded { get; private set; }
        public Vector3 MoveDirection { get; private set; }
        public Vector3 Velocity { get; private set; }
        public bool HasDoubleJumped { get; set; }
        public float lastGroundedTime { get; private set; }
        public float lastJumpTime { get; set; }
        private float landingTime; // ���ʱ��
        private bool wasGrounded; // ��һ֡�Ƿ��ڵ���
        public int jumpCount { get; set; }
        private float lastSlideTime; // �ϴλ���ʱ��
        public bool IsCrouching { get; private set; } // �Ƿ������¶�
        public bool IsSliding { get; private set; } // �Ƿ����ڻ���

        // ״̬��ϵͳ
        private PlayerStateManager stateManager;

        // �����ⷽ��ö��
        public enum GroundDetectionMethod
        {
            SingleRaycast,  // �����߼�⣨ԭʼ������
            MultiRaycast,   // �����߼��
            SphereCast      // ���μ��
        }

        private void Awake()
        {
            try
            {
                Debug.Log("Player.Awake: ��ʼ��ʼ��");
                
                Rb = GetComponent<Rigidbody>();
                Animator = GetComponent<Animator>();
                InputManager = GetComponent<InputManager>();
                
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
            HandleGroundDetection();
            
            // ������׼����
            if (InputManager.IsAiming || InputManager.IsFirePressed)
            {
                UpdateAimDirection();
            }
        }

        private void FixedUpdate()
        {
            HandleGroundDetection();
            HandleMovement();
        }

        private void OnDestroy()
        {
            // ע���¼�
            InputManager.OnJumpAction -= HandleJumpInput;
            InputManager.OnAimAction -= HandleAimInput;
            InputManager.OnCrouchAction -= HandleCrouchInput;
            InputManager.OnFireAction -= HandleFireInput; // ע�����ο����¼�
            InputManager.OnFireActionChanged -= HandleFireInputChanged; // ע����������״̬�仯�¼�
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
            
            // ����AnimatorController���ƶ�״̬
            if (AnimController != null)
            {
                bool isMoving = InputManager.GetInputMagnitude() > 0.1f;
                bool isRunning = InputManager.IsRunning;
                float speed = isMoving ? (isRunning ? runSpeed : walkSpeed) : 0f;
                AnimController.SetMovementState(isMoving, isRunning, speed);
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

        #region ����ϵͳ
        private void HandleGroundDetection()
        {
            wasGrounded = IsGrounded;
            
            // �Ľ��ĵ����ⷽ����ʹ�ö�����߼��
            bool groundDetected = CheckGrounded();
            
            // ����ո���Ծ��ǿ������Ϊ�ǵ���״̬
            if (Time.time - lastJumpTime < 0.1f && Rb.velocity.y > 0)
            {
                groundDetected = false;
                Debug.Log("ǿ������Ϊ�ǵ���״̬ - �ո���Ծ");
            }
            
            IsGrounded = groundDetected;
            
            // �����ŵ�ʱ��
            if (IsGrounded)
            {
                // ����Ƿ�ո����
                if (!wasGrounded)
                {
                    landingTime = Time.time;
                    // ����Ӹߴ����£�����Ӳ��½״̬
                    if (Rb.velocity.y < -5f)
                    {
                        stateManager.TriggerHardLanding();
                        // ʹ��AnimatorController����Ӳ��½����
                        if (AnimController != null)
                        {
                            AnimController.TriggerHardLand();
                        }
                    }
                    else
                    {
                        stateManager.TriggerLanding();
                        // ʹ��AnimatorController������½����
                        if (AnimController != null)
                        {
                            AnimController.TriggerLand();
                        }
                    }
                }
                
                lastGroundedTime = Time.time;
                HasDoubleJumped = false;
                ResetJumpCount();
            }
            
            // ֪ͨ״̬������״̬�仯
            stateManager.SetGrounded(IsGrounded);
            stateManager.SetVerticalVelocity(Rb.velocity.y);
            
            // ����AnimatorController�Ľӵ�״̬
            if (AnimController != null)
            {
                AnimController.SetGroundedState(IsGrounded, Rb.velocity.y);
            }
            
            // ������뿪�����Ҵ�ֱ�ٶ�Ϊ������������״̬
            if (!IsGrounded && wasGrounded && Rb.velocity.y < 0)
            {
                // ���ٵ��� TriggerFalling����Ϊ�����Ѿ�ɾ���� FallingState
                // stateManager.TriggerFalling();
                
                // ʹ��AnimatorController�������䶯��
                if (AnimController != null)
                {
                    AnimController.TriggerFall();
                }
            }
            
            // �������
            if (IsGrounded != wasGrounded)
            {
                Debug.Log($"����״̬�仯: {wasGrounded} -> {IsGrounded}, ��ֱ�ٶ�: {Rb.velocity.y}");
            }
        }

        // �����ⷽ��
        private bool CheckGrounded()
        {
            switch (groundDetectionMethod)
            {
                case GroundDetectionMethod.SingleRaycast:
                    return CheckGroundedSingleRay();
                case GroundDetectionMethod.MultiRaycast:
                    return CheckGroundedMultiRay();
                case GroundDetectionMethod.SphereCast:
                    return CheckGroundedSphere();
                default:
                    return CheckGroundedMultiRay();
            }
        }

        // �����߼����棨ԭʼ������
        private bool CheckGroundedSingleRay()
        {
            float rayOriginHeight = 0.1f;
            Vector3 rayOrigin = transform.position + Vector3.up * rayOriginHeight;
            
            if (showDebugRays)
            {
                Debug.DrawRay(rayOrigin, Vector3.down * groundCheckDistance, Color.yellow);
            }
            
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
            {
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                return slopeAngle < maxSlopeAngle;
            }
            
            return false;
        }

        // �����߼�����
        private bool CheckGroundedMultiRay()
        {
            // ������߶� - ���Ӹ߶��Ա�������
            float rayOriginHeight = 0.15f;
            Vector3 rayOrigin = transform.position + Vector3.up * rayOriginHeight;
            
            // ��С��������룬ʹ������ȷ
            float effectiveGroundCheckDistance = groundCheckDistance * 0.8f;
            
            // �������߼��
            if (showDebugRays)
            {
                Debug.DrawRay(rayOrigin, Vector3.down * effectiveGroundCheckDistance, Color.green);
            }
            
            // �����ֱ�ٶ��������ϣ�ֱ����Ϊ���ڵ�����
            if (Rb.velocity.y > 0.5f)
            {
                return false;
            }
            
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, effectiveGroundCheckDistance, groundLayer))
            {
                // ����Ƕȣ�����б��
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (slopeAngle < maxSlopeAngle) // �����õ����б�½Ƕ�
                {
                    return true;
                }
            }
            
            // ��Χ������߼�⣬����¥�ݵȲ��������
            for (int i = 0; i < groundRayCount; i++)
            {
                // �������߷���Χ�ƽ�ɫ��Բ�ηֲ���
                float angle = i * (360f / groundRayCount);
                Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
                Vector3 rayStart = rayOrigin + direction * groundCheckRadius;
                
                // ���Ƶ�������
                if (showDebugRays)
                {
                    Debug.DrawRay(rayStart, Vector3.down * effectiveGroundCheckDistance, Color.red);
                }
                
                // ���߼��
                if (Physics.Raycast(rayStart, Vector3.down, out hit, effectiveGroundCheckDistance, groundLayer))
                {
                    float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                    if (slopeAngle < maxSlopeAngle) // �����õ����б�½Ƕ�
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        // ���μ�����
        private bool CheckGroundedSphere()
        {
            // ������߶�
            float rayOriginHeight = 0.1f;
            Vector3 sphereCenter = transform.position + Vector3.up * rayOriginHeight;
            
            // ���Ƶ�������
            if (showDebugRays)
            {
                Debug.DrawLine(sphereCenter, sphereCenter + Vector3.down * groundCheckDistance, Color.blue);
                // ��Scene��ͼ�л�������
                #if UNITY_EDITOR
                UnityEditor.Handles.color = Color.blue;
                UnityEditor.Handles.DrawWireDisc(sphereCenter, Vector3.up, groundCheckRadius);
                UnityEditor.Handles.DrawWireDisc(sphereCenter + Vector3.down * groundCheckDistance, Vector3.up, groundCheckRadius);
                #endif
            }
            
            // ���μ��
            if (Physics.SphereCast(sphereCenter, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
            {
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                return slopeAngle < maxSlopeAngle;
            }
            
            return false;
        }

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
                
                // ����¥��
                if (enableStairsHandling && MoveDirection.magnitude > 0.1f)
                {
                    HandleStairs();
                }
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
            if (InputManager.IsRunning)
                return runSpeed;
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

        // ����¥��
        private void HandleStairs()
        {
            // ǰ������
            Vector3 forwardPoint = transform.position + transform.forward * 0.5f;
            
            // ���ǰ���Ƿ����ϰ���
            if (Physics.Raycast(forwardPoint, Vector3.down, out RaycastHit downHit, stepHeight * 2f, groundLayer))
            {
                // ���ϰ����Ϸ�������������
                Vector3 stepCheckStart = new Vector3(forwardPoint.x, transform.position.y + stepHeight, forwardPoint.z);
                
                if (showDebugRays)
                {
                    Debug.DrawLine(forwardPoint, forwardPoint + Vector3.down * stepHeight * 2f, Color.yellow);
                    Debug.DrawLine(stepCheckStart, stepCheckStart + Vector3.down * stepHeight * 2f, Color.cyan);
                }
                
                // ����Ƿ���̨��
                if (Physics.Raycast(stepCheckStart, Vector3.down, out RaycastHit stepHit, stepHeight * 2f, groundLayer))
                {
                    // ����߶Ȳ�
                    float heightDifference = Mathf.Abs(downHit.point.y - stepHit.point.y);
                    
                    // ����߶Ȳ���̨�׷�Χ��
                    if (heightDifference > 0.01f && heightDifference < stepHeight)
                    {
                        // �ж�����¥�ݻ�����¥��
                        bool isAscending = stepHit.point.y > downHit.point.y;
                        
                        if (isAscending)
                        {
                            // ��¥�ݣ���΢������ɫλ��
                            Vector3 targetPosition = new Vector3(
                                transform.position.x,
                                stepHit.point.y + 0.05f, // ��΢̧��һ�㣬���⿨ס
                                transform.position.z
                            );
                            
                            // ƽ������
                            transform.position = Vector3.Lerp(
                                transform.position,
                                targetPosition,
                                stepSmoothing
                            );
                        }
                        else
                        {
                            // ��¥�ݣ����ֽӵ�״̬��������¥ʱ������Ծ
                            IsGrounded = true;
                        }
                    }
                }
            }
        }

        // �ڱ༭���п��ӻ������ⷶΧ
        private void OnDrawGizmosSelected()
        {
            // ������߶�
            float rayOriginHeight = 0.1f;
            Vector3 rayOrigin = transform.position + Vector3.up * rayOriginHeight;
            
            // ����Gizmos��ɫ
            Gizmos.color = Color.green;
            
            // ���ݲ�ͬ�ļ�ⷽ�����Ʋ�ͬ��Gizmos
            switch (groundDetectionMethod)
            {
                case GroundDetectionMethod.SingleRaycast:
                    // �����߼��
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundCheckDistance);
                    break;
                    
                case GroundDetectionMethod.MultiRaycast:
                    // ��������
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundCheckDistance);
                    
                    // ��Χ����
                    Gizmos.color = Color.red;
                    for (int i = 0; i < groundRayCount; i++)
                    {
                        float angle = i * (360f / groundRayCount);
                        Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                        Vector3 rayStart = rayOrigin + direction * groundCheckRadius;
                        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * groundCheckDistance);
                    }
                    
                    // ���Ƽ��뾶
                    Gizmos.color = new Color(1, 0, 0, 0.2f);
                    DrawCircle(rayOrigin, groundCheckRadius, 32);
                    break;
                    
                case GroundDetectionMethod.SphereCast:
                    // ���μ��
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundCheckDistance);
                    
                    // ��������
                    Gizmos.DrawWireSphere(rayOrigin, groundCheckRadius);
                    Gizmos.DrawWireSphere(rayOrigin + Vector3.down * groundCheckDistance, groundCheckRadius);
                    
                    // ��������·��
                    Gizmos.color = new Color(0, 0, 1, 0.2f);
                    DrawCylinder(
                        rayOrigin, 
                        rayOrigin + Vector3.down * groundCheckDistance,
                        groundCheckRadius
                    );
                    break;
            }
            
            // ���������¥�ݴ�������¥�ݼ��
            if (enableStairsHandling)
            {
                Gizmos.color = Color.cyan;
                Vector3 forwardPoint = transform.position + transform.forward * 0.5f;
                Gizmos.DrawLine(forwardPoint, forwardPoint + Vector3.down * stepHeight * 2f);
                
                Vector3 stepCheckStart = new Vector3(forwardPoint.x, transform.position.y + stepHeight, forwardPoint.z);
                Gizmos.DrawLine(stepCheckStart, stepCheckStart + Vector3.down * stepHeight * 2f);
            }
        }
        
        // ��������������Բ��
        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            float angle = 0f;
            float angleStep = 360f / segments;
            Vector3 previousPoint = center + new Vector3(Mathf.Sin(0) * radius, 0, Mathf.Cos(0) * radius);
            
            for (int i = 0; i <= segments; i++)
            {
                angle += angleStep;
                float radian = angle * Mathf.Deg2Rad;
                Vector3 currentPoint = center + new Vector3(Mathf.Sin(radian) * radius, 0, Mathf.Cos(radian) * radius);
                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }
        }
        
        // ��������������Բ����
        private static void DrawCylinder(Vector3 start, Vector3 end, float radius)
        {
            Vector3 up = (end - start).normalized * radius;
            Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
            Vector3 right = Vector3.Cross(up, forward).normalized * radius;
            
            float height = (end - start).magnitude;
            float sideLength = height / 2f;
            
            // ����Բ�������
            Vector3 middle = (start + end) / 2f;
            
            for (float i = 0; i < 360; i += 30)
            {
                float radian = i * Mathf.Deg2Rad;
                float radian2 = (i + 30) * Mathf.Deg2Rad;
                
                Vector3 point1 = start + Mathf.Sin(radian) * right + Mathf.Cos(radian) * forward;
                Vector3 point2 = end + Mathf.Sin(radian) * right + Mathf.Cos(radian) * forward;
                Vector3 point3 = end + Mathf.Sin(radian2) * right + Mathf.Cos(radian2) * forward;
                Vector3 point4 = start + Mathf.Sin(radian2) * right + Mathf.Cos(radian2) * forward;
                
                Gizmos.DrawLine(point1, point2);
                Gizmos.DrawLine(point2, point3);
                Gizmos.DrawLine(point3, point4);
                Gizmos.DrawLine(point4, point1);
            }
        }
        #endregion
    }
}