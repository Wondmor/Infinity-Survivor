using System.Collections.Generic;
using Unity;
using UnityEngine;

namespace TrianCatStudio
{
    public class PlayerStateManager : MonoBehaviour
    {
        public StateMachine StateMachine { get; private set; }
        public Player Player { get; private set; }

        // 状态实例
        // 基础层状态
        public IdleState IdleState { get; private set; }
        public WalkState WalkState { get; private set; }
        public RunState RunState { get; private set; }
        
        // 动作层状态
        public JumpState JumpState { get; private set; }
        public DoubleJumpState DoubleJumpState { get; private set; }
        public FallingState FallingState { get; private set; }
        public LandingState LandingState { get; private set; }
        public HardLandingState HardLandingState { get; private set; }
        public RollState RollState { get; private set; }
        public CrouchState CrouchState { get; private set; }
        public SlideState SlideState { get; private set; }
        
        // 上半身层状态
        public AimState AimState { get; private set; }

        // 运行时状态
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
                Debug.Log("PlayerStateManager.Awake: 开始初始化");
                Init(); 
                Debug.Log("PlayerStateManager.Awake: 初始化完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"PlayerStateManager.Awake: 初始化失败 - {e.Message}\n{e.StackTrace}");
                enabled = false;
            }
        }
        
        private void OnEnable()
        {
            try
            {
                // 确保组件在启用时正确初始化
                if (StateMachine == null)
                {
                    Debug.Log("PlayerStateManager.OnEnable: StateMachine为空，重新初始化");
                    Init();
                }
                else
                {
                    Debug.Log("PlayerStateManager.OnEnable: StateMachine已存在");
                }
                
                // 打印调试信息
                Debug.Log("PlayerStateManager已启用");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"PlayerStateManager.OnEnable: 发生错误 - {e.Message}\n{e.StackTrace}");
                enabled = false;
            }
        }
        
        private void OnDisable()
        {
            // 记录组件被禁用的原因
            Debug.LogWarning("PlayerStateManager被禁用，这可能会导致角色无法正常移动和动画");
        }

        protected void Init()
        {
            try
            {
                Debug.Log("开始初始化PlayerStateManager");
                
                StateMachine = new StateMachine();
                if (StateMachine == null)
                {
                    Debug.LogError("StateMachine创建失败");
                    enabled = false;
                    return;
                }
                
                Player = GetComponent<Player>();
                
                // 确保Player组件存在
                if (Player == null)
                {
                    Player = GetComponent<Player>();
                    if (Player == null)
                    {
                        Debug.LogError("PlayerStateManager无法找到Player组件");
                        enabled = false;
                        return;
                    }
                }
                
                Debug.Log("开始初始化状态");
                InitializeStates();
                
                Debug.Log("开始注册状态转换");
                RegisterTransitions();
                
                Debug.Log("开始设置初始状态");
                SetInitialState();
                
                // 打印调试信息
                Debug.Log("PlayerStateManager初始化完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"PlayerStateManager初始化失败: {e.Message}\n{e.StackTrace}");
                enabled = false;
            }
        }

        private void InitializeStates()
        {
            try
            {
                // 基础层状态
                IdleState = new IdleState(this);
                WalkState = new WalkState(this);
                RunState = new RunState(this);
                
                // 动作层状态
                JumpState = new JumpState(this);
                DoubleJumpState = new DoubleJumpState(this);
                FallingState = new FallingState(this);
                LandingState = new LandingState(this);
                HardLandingState = new HardLandingState(this);
                RollState = new RollState(this);
                CrouchState = new CrouchState(this);
                SlideState = new SlideState(this);
                
                // 上半身层状态
                AimState = new AimState(this);
                
                // 验证关键状态是否创建成功
                if (IdleState == null)
                {
                    Debug.LogError("初始化状态失败: IdleState为空");
                }
                
                Debug.Log("所有状态初始化完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"初始化状态时发生错误: {e.Message}\n{e.StackTrace}");
                enabled = false;
            }
        }

        private void Update()
        {
            // 确保StateMachine已初始化
            if (StateMachine == null)
            {
                Debug.LogError("Update: StateMachine为空，重新初始化");
                Init();
                return;
            }
            
            try
            {
                SyncStateParameters();
                
                // 更新当前状态
                StateMachine.Update(Time.deltaTime);
                
                // 处理各层状态的输入
                HandleLayerInput();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Update中发生异常: {e.Message}\n{e.StackTrace}");
            }
        }
        
        private void FixedUpdate()
        {
            // 确保StateMachine已初始化
            if (StateMachine == null)
            {
                Debug.LogError("FixedUpdate: StateMachine为空，重新初始化");
                Init();
                return;
            }
            
            try
            {
                // 处理各层状态的物理更新
                HandleLayerPhysicsUpdate();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"FixedUpdate中发生异常: {e.Message}\n{e.StackTrace}");
            }
        }
        
        private void HandleLayerInput()
        {
            // 处理主状态的输入
            if (StateMachine.CurrentState is PlayerBaseState baseState)
            {
                baseState.HandleInput();
            }
            
            // 处理各层状态的输入
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
            
            // 处理主状态的物理更新
            if (StateMachine.CurrentState is PlayerBaseState baseState)
            {
                baseState.PhysicsUpdate(deltaTime);
            }
            
            // 处理各层状态的物理更新
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
            // 同步状态到状态机
            StateMachine.SetFloat("MoveInput", moveInput);
            StateMachine.SetBool("IsRunning", isRunning);
            StateMachine.SetBool("IsGrounded", isGrounded);
            StateMachine.SetFloat("VerticalVelocity", verticalVelocity);
            StateMachine.SetBool("IsAiming", isAiming);
            StateMachine.SetBool("IsCrouching", isCrouching);
        }

        // 状态设置方法
        public void SetMoveInput(float value) => moveInput = value;
        public void SetRunning(bool value) => isRunning = value;
        public void SetGrounded(bool value) => isGrounded = value;
        public void SetVerticalVelocity(float value) => verticalVelocity = value;
        public void SetAiming(bool value) => isAiming = value;
        public void SetCrouching(bool value) => isCrouching = value;

        // 触发器方法
        public void TriggerJump()
        {
            Debug.Log("PlayerStateManager.TriggerJump: 触发跳跃");
            StateMachine.ResetTrigger("Jump"); // 先重置触发器
            StateMachine.SetTrigger("Jump");
        }
        
        public void TriggerDoubleJump()
        {
            Debug.Log("PlayerStateManager.TriggerDoubleJump: 触发二段跳");
            StateMachine.ResetTrigger("DoubleJump"); // 先重置触发器
            StateMachine.SetTrigger("DoubleJump");
        }
        
        public void TriggerFalling()
        {
            StateMachine.ResetTrigger("StartFalling"); // 先重置触发器
            StateMachine.SetTrigger("StartFalling");
        }
        
        public void TriggerLanding()
        {
            StateMachine.ResetTrigger("Landing"); // 先重置触发器
            StateMachine.SetTrigger("Landing");
        }
        
        public void TriggerHardLanding()
        {
            StateMachine.ResetTrigger("HardLanding"); // 先重置触发器
            StateMachine.SetTrigger("HardLanding");
        }
        
        public void TriggerRoll()
        {
            StateMachine.ResetTrigger("Roll"); // 先重置触发器
            StateMachine.SetTrigger("Roll");
        }
        
        public void TriggerCrouch(bool isCrouching)
        {
            Debug.Log($"PlayerStateManager.TriggerCrouch: 设置下蹲状态 = {isCrouching}");
            
            // 设置下蹲状态参数
            StateMachine.SetBool("IsCrouching", isCrouching);
            SetCrouching(isCrouching);
            
            if (isCrouching)
            {
                Debug.Log("PlayerStateManager: 触发下蹲触发器");
                StateMachine.ResetTrigger("Crouch"); // 先重置触发器
                StateMachine.SetTrigger("Crouch");
                
                // 打印当前参数状态
                Debug.Log($"StateMachine参数 - IsCrouching: {StateMachine.GetBool("IsCrouching")}, Crouch触发器: {StateMachine.GetBool("Crouch")}");
            }
        }
        
        public void TriggerSlide()
        {
            StateMachine.ResetTrigger("Slide"); // 先重置触发器
            StateMachine.SetTrigger("Slide");
        }

        private void RegisterTransitions()
        {
            try
            {
                RegisterBaseLayerTransitions();
                RegisterActionLayerTransitions();
                RegisterUpperBodyLayerTransitions();
                
                Debug.Log("所有状态转换注册完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"注册状态转换时发生错误: {e.Message}\n{e.StackTrace}");
            }
        }
        
        private void RegisterBaseLayerTransitions()
        {
            // 确保所有状态都已正确初始化
            if (IdleState == null || WalkState == null || RunState == null)
            {
                Debug.LogError("RegisterBaseLayerTransitions: 一些状态未初始化");
                InitializeStates(); // 尝试重新初始化状态
            }
            
            // 基础移动状态转换
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
            // 确保所有状态都已正确初始化
            if (JumpState == null || DoubleJumpState == null || FallingState == null || 
                LandingState == null || HardLandingState == null || RollState == null)
            {
                Debug.LogError("RegisterActionLayerTransitions: 一些状态未初始化");
                InitializeStates(); // 尝试重新初始化状态
            }
            
            // 跳跃状态转换
            AddGlobalTransition((int)StateLayerType.Action, JumpState,
                new Condition("Jump", ParameterType.Trigger, ComparisonType.Equals, true),
                new Condition("IsGrounded", ParameterType.Bool, ComparisonType.Equals, true));
        
            // 二段跳状态转换
            AddTransition(JumpState, DoubleJumpState,
                new Condition("DoubleJump", ParameterType.Trigger, ComparisonType.Equals, true),
                new Condition("IsGrounded", ParameterType.Bool, ComparisonType.Equals, false));

            // 下落状态转换
            AddTransition(JumpState, FallingState,
                new Condition("StartFalling", ParameterType.Trigger, ComparisonType.Equals, true));
        
            AddTransition(DoubleJumpState, FallingState,
                new Condition("StartFalling", ParameterType.Trigger, ComparisonType.Equals, true));
        
            // 自然下落（从边缘走下）
            AddGlobalTransition((int)StateLayerType.Action, FallingState,
                new Condition("IsGrounded", ParameterType.Bool, ComparisonType.Equals, false),
                new Condition("VerticalVelocity", ParameterType.Float, ComparisonType.LessThan, -1f));
        
            // 着陆状态转换
            AddTransition(FallingState, LandingState,
                new Condition("Landing", ParameterType.Trigger, ComparisonType.Equals, true));
        
            AddTransition(FallingState, HardLandingState,
                new Condition("HardLanding", ParameterType.Trigger, ComparisonType.Equals, true));
        
            // 着陆恢复
            AddTransition(LandingState, null,
                new Condition("RecoveryComplete", ParameterType.Trigger, ComparisonType.Equals, true));
        
            AddTransition(HardLandingState, null,
                new Condition("RecoveryComplete", ParameterType.Trigger, ComparisonType.Equals, true));

            // 翻滚全局中断
            AddGlobalTransition((int)StateLayerType.Action, RollState,
                new Condition("Roll", ParameterType.Trigger, ComparisonType.Equals, true),
                new Condition("IsGrounded", ParameterType.Bool, ComparisonType.Equals, true));
        
            // 从翻滚状态返回
            AddTransition(RollState, null,
                new Condition("RollComplete", ParameterType.Trigger, ComparisonType.Equals, true));
                
            // 下蹲状态转换
            if (CrouchState != null)
            {
                AddGlobalTransition((int)StateLayerType.Action, CrouchState,
                    new Condition("Crouch", ParameterType.Trigger, ComparisonType.Equals, true),
                    new Condition("IsGrounded", ParameterType.Bool, ComparisonType.Equals, true),
                    new Condition("IsCrouching", ParameterType.Bool, ComparisonType.Equals, true));
                
                // 从下蹲状态返回
                AddTransition(CrouchState, null,
                    new Condition("IsCrouching", ParameterType.Bool, ComparisonType.Equals, false));
            }
            
            // 滑铲状态转换
            if (SlideState != null)
            {
                AddGlobalTransition((int)StateLayerType.Action, SlideState,
                    new Condition("Slide", ParameterType.Trigger, ComparisonType.Equals, true),
                    new Condition("IsGrounded", ParameterType.Bool, ComparisonType.Equals, true));
                
                // 从滑铲状态返回到下蹲状态
                AddTransition(SlideState, CrouchState,
                    new Condition("SlideComplete", ParameterType.Trigger, ComparisonType.Equals, true),
                    new Condition("IsCrouching", ParameterType.Bool, ComparisonType.Equals, true));
                
                // 从滑铲状态直接返回
                AddTransition(SlideState, null,
                    new Condition("SlideComplete", ParameterType.Trigger, ComparisonType.Equals, true),
                    new Condition("IsCrouching", ParameterType.Bool, ComparisonType.Equals, false));
            }
        }
        
        private void RegisterUpperBodyLayerTransitions()
        {
            // 确保所有状态都已正确初始化
            if (AimState == null)
            {
                Debug.LogError("RegisterUpperBodyLayerTransitions: AimState未初始化");
                InitializeStates(); // 尝试重新初始化状态
            }
            
            // 瞄准状态转换
            AddGlobalTransition((int)StateLayerType.UpperBody, AimState,
                new Condition("IsAiming", ParameterType.Bool, ComparisonType.Equals, true));
            
            // 从瞄准状态返回
            AddTransition(AimState, null,
                new Condition("IsAiming", ParameterType.Bool, ComparisonType.Equals, false));
        }

        private void AddTransition(IState from, IState to, params Condition[] conditions)
        {
            // 如果from为空，记录警告但不添加转换
            if (from == null)
            {
                Debug.LogWarning("尝试为空状态添加转换");
                return;
            }
            
            // 允许to为空，表示退出当前状态
            StateMachine.AddTransition(from, to, conditions);
        }

        private void AddGlobalTransition(IState to, params Condition[] conditions)
        {
            // 如果to为空，记录警告但不添加转换
            if (to == null)
            {
                Debug.LogWarning("尝试添加到空状态的全局转换");
                return;
            }
            
            StateMachine.AddGlobalTransition(to, conditions);
        }
        
        private void AddGlobalTransition(int layer, IState to, params Condition[] conditions)
        {
            // 如果to为空，记录警告但不添加转换
            if (to == null)
            {
                Debug.LogWarning($"尝试为层 {layer} 添加到空状态的全局转换");
                return;
            }
            
            StateMachine.AddGlobalTransition(layer, to, conditions);
        }

        private void SetInitialState()
        {
            // 初始化主状态
            StateMachine.Initialize(IdleState);
            
            // 初始化各层状态
            Dictionary<int, IState> initialLayerStates = new Dictionary<int, IState>();
            
            // 只有当字典不为空时才初始化层
            if (initialLayerStates.Count > 0)
            {
                StateMachine.InitializeLayers(initialLayerStates);
            }
        }
        
        // 切换状态方法
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