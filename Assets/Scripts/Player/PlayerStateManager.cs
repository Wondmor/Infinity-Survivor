using System.Collections.Generic;
using Unity;
using UnityEngine;

namespace TrianCatStudio
{
    public class PlayerStateManager : MonoBehaviour
    {
        public StateMachine StateMachine { get; private set; }
        public Player Player { get; private set; }

        // ״̬ʵ��
        // ������״̬
        public IdleState IdleState { get; private set; }
        public WalkState WalkState { get; private set; }
        public RunState RunState { get; private set; }
        
        // ������״̬
        public JumpState JumpState { get; private set; }
        public DoubleJumpState DoubleJumpState { get; private set; }
        public FallingState FallingState { get; private set; }
        public LandingState LandingState { get; private set; }
        public HardLandingState HardLandingState { get; private set; }
        public RollState RollState { get; private set; }
        public CrouchState CrouchState { get; private set; }
        public SlideState SlideState { get; private set; }
        
        // �ϰ����״̬
        public AimState AimState { get; private set; }

        // ����ʱ״̬
        private float moveInput;
        private bool isRunning;
        private bool isGrounded;
        private float verticalVelocity;
        private bool isAiming;
        private bool isCrouching;

        private void Awake()
        {
            try
            {
                Debug.Log("PlayerStateManager.Awake: ��ʼ��ʼ��");
                Init(); 
                Debug.Log("PlayerStateManager.Awake: ��ʼ�����");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"PlayerStateManager.Awake: ��ʼ��ʧ�� - {e.Message}\n{e.StackTrace}");
                enabled = false;
            }
        }
        
        private void OnEnable()
        {
            try
            {
                // ȷ�����������ʱ��ȷ��ʼ��
                if (StateMachine == null)
                {
                    Debug.Log("PlayerStateManager.OnEnable: StateMachineΪ�գ����³�ʼ��");
                    Init();
                }
                else
                {
                    Debug.Log("PlayerStateManager.OnEnable: StateMachine�Ѵ���");
                }
                
                // ��ӡ������Ϣ
                Debug.Log("PlayerStateManager������");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"PlayerStateManager.OnEnable: �������� - {e.Message}\n{e.StackTrace}");
                enabled = false;
            }
        }
        
        private void OnDisable()
        {
            // ��¼��������õ�ԭ��
            Debug.LogWarning("PlayerStateManager�����ã�����ܻᵼ�½�ɫ�޷������ƶ��Ͷ���");
        }

        protected void Init()
        {
            try
            {
                Debug.Log("��ʼ��ʼ��PlayerStateManager");
                
                StateMachine = new StateMachine();
                if (StateMachine == null)
                {
                    Debug.LogError("StateMachine����ʧ��");
                    enabled = false;
                    return;
                }
                
                Player = GetComponent<Player>();
                
                // ȷ��Player�������
                if (Player == null)
                {
                    Player = GetComponent<Player>();
                    if (Player == null)
                    {
                        Debug.LogError("PlayerStateManager�޷��ҵ�Player���");
                        enabled = false;
                        return;
                    }
                }
                
                Debug.Log("��ʼ��ʼ��״̬");
                InitializeStates();
                
                Debug.Log("��ʼע��״̬ת��");
                RegisterTransitions();
                
                Debug.Log("��ʼ���ó�ʼ״̬");
                SetInitialState();
                
                // ��ӡ������Ϣ
                Debug.Log("PlayerStateManager��ʼ�����");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"PlayerStateManager��ʼ��ʧ��: {e.Message}\n{e.StackTrace}");
                enabled = false;
            }
        }

        private void InitializeStates()
        {
            try
            {
                // ������״̬
                IdleState = new IdleState(this);
                WalkState = new WalkState(this);
                RunState = new RunState(this);
                
                // ������״̬
                JumpState = new JumpState(this);
                DoubleJumpState = new DoubleJumpState(this);
                FallingState = new FallingState(this);
                LandingState = new LandingState(this);
                HardLandingState = new HardLandingState(this);
                RollState = new RollState(this);
                CrouchState = new CrouchState(this);
                SlideState = new SlideState(this);
                
                // �ϰ����״̬
                AimState = new AimState(this);
                
                // ��֤�ؼ�״̬�Ƿ񴴽��ɹ�
                if (IdleState == null)
                {
                    Debug.LogError("��ʼ��״̬ʧ��: IdleStateΪ��");
                }
                
                Debug.Log("����״̬��ʼ�����");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"��ʼ��״̬ʱ��������: {e.Message}\n{e.StackTrace}");
                enabled = false;
            }
        }

        private void Update()
        {
            // ȷ��StateMachine�ѳ�ʼ��
            if (StateMachine == null)
            {
                Debug.LogError("Update: StateMachineΪ�գ����³�ʼ��");
                Init();
                return;
            }
            
            try
            {
                SyncStateParameters();
                
                // ���µ�ǰ״̬
                StateMachine.Update(Time.deltaTime);
                
                // �������״̬������
                HandleLayerInput();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Update�з����쳣: {e.Message}\n{e.StackTrace}");
            }
        }
        
        private void FixedUpdate()
        {
            // ȷ��StateMachine�ѳ�ʼ��
            if (StateMachine == null)
            {
                Debug.LogError("FixedUpdate: StateMachineΪ�գ����³�ʼ��");
                Init();
                return;
            }
            
            try
            {
                // �������״̬���������
                HandleLayerPhysicsUpdate();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"FixedUpdate�з����쳣: {e.Message}\n{e.StackTrace}");
            }
        }
        
        private void HandleLayerInput()
        {
            // ������״̬������
            if (StateMachine.CurrentState is PlayerBaseState baseState)
            {
                baseState.HandleInput();
            }
            
            // �������״̬������
            foreach (var layerState in StateMachine.LayerStates)
            {
                if (layerState.Value is PlayerBaseState playerState)
                {
                    playerState.HandleInput();
                }
            }
        }
        
        private void HandleLayerPhysicsUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;
            
            // ������״̬���������
            if (StateMachine.CurrentState is PlayerBaseState baseState)
            {
                baseState.PhysicsUpdate(deltaTime);
            }
            
            // �������״̬���������
            foreach (var layerState in StateMachine.LayerStates)
            {
                if (layerState.Value is PlayerBaseState playerState)
                {
                    playerState.PhysicsUpdate(deltaTime);
                }
            }
        }

        private void SyncStateParameters()
        {
            // ͬ��״̬��״̬��
            StateMachine.SetFloat("MoveInput", moveInput);
            StateMachine.SetBool("IsRunning", isRunning);
            StateMachine.SetBool("IsGrounded", isGrounded);
            StateMachine.SetFloat("VerticalVelocity", verticalVelocity);
            StateMachine.SetBool("IsAiming", isAiming);
            StateMachine.SetBool("IsCrouching", isCrouching);
        }

        // ״̬���÷���
        public void SetMoveInput(float value) => moveInput = value;
        public void SetRunning(bool value) => isRunning = value;
        public void SetGrounded(bool value) => isGrounded = value;
        public void SetVerticalVelocity(float value) => verticalVelocity = value;
        public void SetAiming(bool value) => isAiming = value;
        public void SetCrouching(bool value) => isCrouching = value;

        // ����������
        public void TriggerJump()
        {
            Debug.Log("PlayerStateManager.TriggerJump: ������Ծ");
            StateMachine.ResetTrigger("Jump"); // �����ô�����
            StateMachine.SetTrigger("Jump");
        }
        
        public void TriggerDoubleJump()
        {
            Debug.Log("PlayerStateManager.TriggerDoubleJump: ����������");
            StateMachine.ResetTrigger("DoubleJump"); // �����ô�����
            StateMachine.SetTrigger("DoubleJump");
        }
        
        public void TriggerFalling()
        {
            StateMachine.ResetTrigger("StartFalling"); // �����ô�����
            StateMachine.SetTrigger("StartFalling");
        }
        
        public void TriggerLanding()
        {
            StateMachine.ResetTrigger("Landing"); // �����ô�����
            StateMachine.SetTrigger("Landing");
        }
        
        public void TriggerHardLanding()
        {
            StateMachine.ResetTrigger("HardLanding"); // �����ô�����
            StateMachine.SetTrigger("HardLanding");
        }
        
        public void TriggerRoll()
        {
            StateMachine.ResetTrigger("Roll"); // �����ô�����
            StateMachine.SetTrigger("Roll");
        }
        
        public void TriggerCrouch(bool isCrouching)
        {
            Debug.Log($"PlayerStateManager.TriggerCrouch: �����¶�״̬ = {isCrouching}");
            
            // �����¶�״̬����
            StateMachine.SetBool("IsCrouching", isCrouching);
            SetCrouching(isCrouching);
            
            if (isCrouching)
            {
                Debug.Log("PlayerStateManager: �����¶״�����");
                StateMachine.ResetTrigger("Crouch"); // �����ô�����
                StateMachine.SetTrigger("Crouch");
                
                // ��ӡ��ǰ����״̬
                Debug.Log($"StateMachine���� - IsCrouching: {StateMachine.GetBool("IsCrouching")}, Crouch������: {StateMachine.GetBool("Crouch")}");
            }
        }
        
        public void TriggerSlide()
        {
            StateMachine.ResetTrigger("Slide"); // �����ô�����
            StateMachine.SetTrigger("Slide");
        }

        private void RegisterTransitions()
        {
            try
            {
                RegisterBaseLayerTransitions();
                RegisterActionLayerTransitions();
                RegisterUpperBodyLayerTransitions();
                
                Debug.Log("����״̬ת��ע�����");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ע��״̬ת��ʱ��������: {e.Message}\n{e.StackTrace}");
            }
        }
        
        private void RegisterBaseLayerTransitions()
        {
            // ȷ������״̬������ȷ��ʼ��
            if (IdleState == null || WalkState == null || RunState == null)
            {
                Debug.LogError("RegisterBaseLayerTransitions: һЩ״̬δ��ʼ��");
                InitializeStates(); // �������³�ʼ��״̬
            }
            
            // �����ƶ�״̬ת��
            AddTransition(IdleState, WalkState,
                new Condition("MoveInput", ParameterType.Float, ComparisonType.GreaterThan, 0.1f));

            AddTransition(WalkState, IdleState,
                new Condition("MoveInput", ParameterType.Float, ComparisonType.LessOrEqual, 0.1f));

            AddTransition(WalkState, RunState,
                new Condition("IsRunning", ParameterType.Bool, ComparisonType.Equals, true),
                new Condition("Stamina", ParameterType.Float, ComparisonType.GreaterThan, 0f));

            AddTransition(RunState, WalkState,
                new Condition("IsRunning", ParameterType.Bool, ComparisonType.Equals, false));
        }
        
        private void RegisterActionLayerTransitions()
        {
            // ȷ������״̬������ȷ��ʼ��
            if (JumpState == null || DoubleJumpState == null || FallingState == null || 
                LandingState == null || HardLandingState == null || RollState == null)
            {
                Debug.LogError("RegisterActionLayerTransitions: һЩ״̬δ��ʼ��");
                InitializeStates(); // �������³�ʼ��״̬
            }
            
            // ��Ծ״̬ת��
            AddGlobalTransition((int)StateLayerType.Action, JumpState,
                new Condition("Jump", ParameterType.Trigger, ComparisonType.Equals, true),
                new Condition("IsGrounded", ParameterType.Bool, ComparisonType.Equals, true));
        
            // ������״̬ת��
            AddTransition(JumpState, DoubleJumpState,
                new Condition("DoubleJump", ParameterType.Trigger, ComparisonType.Equals, true),
                new Condition("IsGrounded", ParameterType.Bool, ComparisonType.Equals, false));

            // ����״̬ת��
            AddTransition(JumpState, FallingState,
                new Condition("StartFalling", ParameterType.Trigger, ComparisonType.Equals, true));
        
            AddTransition(DoubleJumpState, FallingState,
                new Condition("StartFalling", ParameterType.Trigger, ComparisonType.Equals, true));
        
            // ��Ȼ���䣨�ӱ�Ե���£�
            AddGlobalTransition((int)StateLayerType.Action, FallingState,
                new Condition("IsGrounded", ParameterType.Bool, ComparisonType.Equals, false),
                new Condition("VerticalVelocity", ParameterType.Float, ComparisonType.LessThan, -1f));
        
            // ��½״̬ת��
            AddTransition(FallingState, LandingState,
                new Condition("Landing", ParameterType.Trigger, ComparisonType.Equals, true));
        
            AddTransition(FallingState, HardLandingState,
                new Condition("HardLanding", ParameterType.Trigger, ComparisonType.Equals, true));
        
            // ��½�ָ�
            AddTransition(LandingState, null,
                new Condition("RecoveryComplete", ParameterType.Trigger, ComparisonType.Equals, true));
        
            AddTransition(HardLandingState, null,
                new Condition("RecoveryComplete", ParameterType.Trigger, ComparisonType.Equals, true));

            // ����ȫ���ж�
            AddGlobalTransition((int)StateLayerType.Action, RollState,
                new Condition("Roll", ParameterType.Trigger, ComparisonType.Equals, true),
                new Condition("IsGrounded", ParameterType.Bool, ComparisonType.Equals, true));
        
            // �ӷ���״̬����
            AddTransition(RollState, null,
                new Condition("RollComplete", ParameterType.Trigger, ComparisonType.Equals, true));
                
            // �¶�״̬ת��
            if (CrouchState != null)
            {
                AddGlobalTransition((int)StateLayerType.Action, CrouchState,
                    new Condition("Crouch", ParameterType.Trigger, ComparisonType.Equals, true),
                    new Condition("IsGrounded", ParameterType.Bool, ComparisonType.Equals, true),
                    new Condition("IsCrouching", ParameterType.Bool, ComparisonType.Equals, true));
                
                // ���¶�״̬����
                AddTransition(CrouchState, null,
                    new Condition("IsCrouching", ParameterType.Bool, ComparisonType.Equals, false));
            }
            
            // ����״̬ת��
            if (SlideState != null)
            {
                AddGlobalTransition((int)StateLayerType.Action, SlideState,
                    new Condition("Slide", ParameterType.Trigger, ComparisonType.Equals, true),
                    new Condition("IsGrounded", ParameterType.Bool, ComparisonType.Equals, true));
                
                // �ӻ���״̬���ص��¶�״̬
                AddTransition(SlideState, CrouchState,
                    new Condition("SlideComplete", ParameterType.Trigger, ComparisonType.Equals, true),
                    new Condition("IsCrouching", ParameterType.Bool, ComparisonType.Equals, true));
                
                // �ӻ���״ֱ̬�ӷ���
                AddTransition(SlideState, null,
                    new Condition("SlideComplete", ParameterType.Trigger, ComparisonType.Equals, true),
                    new Condition("IsCrouching", ParameterType.Bool, ComparisonType.Equals, false));
            }
        }
        
        private void RegisterUpperBodyLayerTransitions()
        {
            // ȷ������״̬������ȷ��ʼ��
            if (AimState == null)
            {
                Debug.LogError("RegisterUpperBodyLayerTransitions: AimStateδ��ʼ��");
                InitializeStates(); // �������³�ʼ��״̬
            }
            
            // ��׼״̬ת��
            AddGlobalTransition((int)StateLayerType.UpperBody, AimState,
                new Condition("IsAiming", ParameterType.Bool, ComparisonType.Equals, true));
            
            // ����׼״̬����
            AddTransition(AimState, null,
                new Condition("IsAiming", ParameterType.Bool, ComparisonType.Equals, false));
        }

        private void AddTransition(IState from, IState to, params Condition[] conditions)
        {
            // ���fromΪ�գ���¼���浫�����ת��
            if (from == null)
            {
                Debug.LogWarning("����Ϊ��״̬���ת��");
                return;
            }
            
            // ����toΪ�գ���ʾ�˳���ǰ״̬
            StateMachine.AddTransition(from, to, conditions);
        }

        private void AddGlobalTransition(IState to, params Condition[] conditions)
        {
            // ���toΪ�գ���¼���浫�����ת��
            if (to == null)
            {
                Debug.LogWarning("������ӵ���״̬��ȫ��ת��");
                return;
            }
            
            StateMachine.AddGlobalTransition(to, conditions);
        }
        
        private void AddGlobalTransition(int layer, IState to, params Condition[] conditions)
        {
            // ���toΪ�գ���¼���浫�����ת��
            if (to == null)
            {
                Debug.LogWarning($"����Ϊ�� {layer} ��ӵ���״̬��ȫ��ת��");
                return;
            }
            
            StateMachine.AddGlobalTransition(layer, to, conditions);
        }

        private void SetInitialState()
        {
            // ��ʼ����״̬
            StateMachine.Initialize(IdleState);
            
            // ��ʼ������״̬
            Dictionary<int, IState> initialLayerStates = new Dictionary<int, IState>();
            
            // ֻ�е��ֵ䲻Ϊ��ʱ�ų�ʼ����
            if (initialLayerStates.Count > 0)
            {
                StateMachine.InitializeLayers(initialLayerStates);
            }
        }
        
        // �л�״̬����
        public void ChangeState(IState newState)
        {
            StateMachine.ChangeState(newState);
        }
        
        public void ChangeLayerState(int layer, IState newState)
        {
            StateMachine.ChangeState(layer, newState);
        }
    }
}