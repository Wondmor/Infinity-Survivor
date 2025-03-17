using System;
using System.Collections.Generic;
using System.Linq;
using Unity;
using UnityEngine;

namespace TrianCatStudio
{
    public class PlayerStateManager : MonoBehaviour
    {
        public StateMachine StateMachine { get; private set; }
        public Player Player { get; private set; }

        // 状态实例
        // 基础状态
        public IdleState IdleState { get; private set; }
        public WalkState WalkState { get; private set; }
        public RunState RunState { get; private set; }
        
        // 动作层状态
        public JumpState JumpState { get; private set; }
        public DoubleJumpState DoubleJumpState { get; private set; }
        public LandingState LandingState { get; private set; }
        public HardLandingState HardLandingState { get; private set; }
        public RollState RollState { get; private set; }
        public CrouchState CrouchState { get; private set; }
        public SlideState SlideState { get; private set; }
        
        // 上半身层状态
        public AimState AimState { get; private set; }
        public FireState FireState { get; private set; }

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
                // 确保组件启用时正确初始化
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
                LandingState = new LandingState(this);
                HardLandingState = new HardLandingState(this);
                RollState = new RollState(this);
                CrouchState = new CrouchState(this);
                SlideState = new SlideState(this);
                
                // 上半身层状态
                AimState = new AimState(this);
                FireState = new FireState(this);
                
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
            try
            {
                // 确保StateMachine已初始化
                if (StateMachine == null)
                {
                    Debug.LogError("Update: StateMachine为空，重新初始化");
                    Init();
                    return;
                }
                
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
            try
            {
                // 主层状态输入处理
                if (StateMachine.CurrentState is PlayerBaseState baseState)
                {
                    baseState.HandleInput();
                }
                
                // 处理各层状态的输入
                var layerStates = StateMachine.LayerStates.ToList();
                foreach (var layerState in layerStates)
                {
                    if (layerState.Value is PlayerBaseState playerState)
                    {
                        playerState.HandleInput();
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"HandleLayerInput发生异常: {e.Message}\n{e.StackTrace}");
            }
        }
        
        private void HandleLayerPhysicsUpdate()
        {
            try
            {
                float deltaTime = Time.fixedDeltaTime;
                
                // 主层状态物理更新处理
                if (StateMachine.CurrentState is PlayerBaseState baseState)
                {
                    baseState.PhysicsUpdate(deltaTime);
                }
                
                // 处理各层状态的物理更新
                var layerStates = StateMachine.LayerStates.ToList();
                foreach (var layerState in layerStates)
                {
                    if (layerState.Value is PlayerBaseState playerState)
                    {
                        playerState.PhysicsUpdate(deltaTime);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"HandleLayerPhysicsUpdate发生异常: {e.Message}\n{e.StackTrace}");
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
            
            // 获取当前动作层状态
            var currentActionState = StateMachine.GetCurrentStateInLayer((int)StateLayerType.Action);
            bool isInJumpState = currentActionState?.GetType() == typeof(JumpState);
            
            // 检查是否可以二段跳 - 修改逻辑，不再依赖IsGrounded
            bool canDoubleJump = Player.jumpCount == 1 && !Player.HasDoubleJumped;
            
            // 如果在跳跃状态，尝试二段跳
            if (isInJumpState)
            {
                // 已经在空中，检查是否可以二段跳
                if (canDoubleJump)
                {
                    Debug.Log($"PlayerStateManager.TriggerJump: 检测到二段跳条件满足 - jumpCount={Player.jumpCount}, HasDoubleJumped={Player.HasDoubleJumped}");
                    TriggerDoubleJump();
                }
                else
                {
                    Debug.Log($"PlayerStateManager.TriggerJump: 二段跳条件不满足 - jumpCount={Player.jumpCount}, HasDoubleJumped={Player.HasDoubleJumped}, IsGrounded={Player.IsGrounded}");
                }
            }
            // 如果在地面上或处于土狼时间内，执行普通跳跃
            else if (Player.IsGrounded || (Time.time - Player.lastGroundedTime <= Player.coyoteTime))
            {
                // 在地面上，正常跳跃
                Debug.Log($"PlayerStateManager.TriggerJump: 执行普通跳跃 - IsGrounded={Player.IsGrounded}, lastGroundedTime={Time.time - Player.lastGroundedTime:F3}s, coyoteTime={Player.coyoteTime:F3}s");
                StateMachine.SetTrigger("Jump");
            }
            else
            {
                Debug.Log($"PlayerStateManager.TriggerJump: 不满足任何跳跃条件 - 当前状态: {currentActionState?.GetType().Name ?? "无状态"}, IsGrounded={Player.IsGrounded}, jumpCount={Player.jumpCount}");
            }
        }
        
        public void TriggerDoubleJump()
        {
            Debug.Log("PlayerStateManager.TriggerDoubleJump: 触发二段跳");
            
            // 立即更新状态，确保不会重复触发
            Player.HasDoubleJumped = true;
            Player.jumpCount = 2;
            
            // 设置触发器
            StateMachine.SetTrigger("DoubleJump");
            
            // 直接切换到二段跳状态，确保立即响应
            ChangeLayerState((int)StateLayerType.Action, DoubleJumpState);
            
            // 触发动画
            if (Player.AnimController != null)
            {
                Player.AnimController.TriggerDoubleJump();
            }
        }
        
        public void TriggerLanding()
        {
            StateMachine.SetTrigger("Landing");
        }
        
        public void TriggerHardLanding()
        {
            StateMachine.SetTrigger("HardLanding");
        }
        
        public void TriggerRoll()
        {
            StateMachine.SetTrigger("Roll");
        }
        
        public void TriggerCrouch(bool isCrouching)
        {
            SetCrouching(isCrouching);
            StateMachine.SetBool("IsCrouching", isCrouching);
            
            if (isCrouching)
            {
                StateMachine.SetTrigger("Crouch");
            }
        }
        
        public void TriggerSlide()
        {
            StateMachine.SetTrigger("Slide");
        }
        
        public void TriggerFire()
        {
            Debug.Log("PlayerStateManager.TriggerFire: 触发开火");
            
            // 设置触发器
            StateMachine.SetTrigger("Fire");
            
            // 直接切换到开火状态，确保立即响应
            ChangeLayerState((int)StateLayerType.UpperBody, FireState);
        }
        
        public void StopFiring()
        {
            Debug.Log("PlayerStateManager.StopFiring: 停止开火");
            
            // 如果当前上半身层状态是开火状态，退出该层状态
            if (StateMachine.GetCurrentStateInLayer((int)StateLayerType.UpperBody) == FireState)
            {
                // 通知 FireState 停止开火
                if (FireState is FireState fireState)
                {
                    fireState.StopFiring();
                }
                
                // 如果正在瞄准，恢复瞄准状态
                if (isAiming)
                {
                    ChangeLayerState((int)StateLayerType.UpperBody, AimState);
                }
                else
                {
                    // 否则退出上半身层状态
                    ChangeLayerState((int)StateLayerType.UpperBody, null);
                }
            }
        }
        
        public void ExitFireState()
        {
            // 退出开火状态
            if (StateMachine.GetCurrentStateInLayer((int)StateLayerType.UpperBody) == FireState)
            {
                // 如果当前上半身层状态是开火状态，退出该层状态
                // 将 null 传递给 ChangeLayerState 方法，表示退出当前层状态
                ChangeLayerState((int)StateLayerType.UpperBody, null);
                
                // 如果正在瞄准，恢复瞄准状态
                if (isAiming)
                {
                    ChangeLayerState((int)StateLayerType.UpperBody, AimState);
                }
            }
        }

        private void RegisterTransitions()
        {
            try
            {
                RegisterBaseLayerTransitions();
                RegisterActionLayerTransitions();
                RegisterUpperBodyLayerTransitions();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"注册状态转换时发生错误: {e.Message}\n{e.StackTrace}");
            }
        }
        
        private void RegisterBaseLayerTransitions()
        {
            // 确保基础状态已被正确初始化
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
            // 确保动作状态已被正确初始化
            if (JumpState == null || DoubleJumpState == null || 
                LandingState == null || HardLandingState == null || RollState == null)
            {
                Debug.LogError("RegisterActionLayerTransitions: 一些状态未初始化");
                InitializeStates(); // 尝试重新初始化状态
            }
            
            // 跳跃状态转换
            AddGlobalTransition((int)StateLayerType.Action, JumpState,
                new Condition("Jump", ParameterType.Trigger, ComparisonType.Equals, true),
                new Condition("IsGrounded", ParameterType.Bool, ComparisonType.Equals, true));
            
            // 注意：二段跳转换现在由TriggerDoubleJump方法直接处理，不再依赖触发器转换

            // 着陆状态转换 - 直接从跳跃状态转换到着陆状态
            AddTransition(JumpState, LandingState,
                new Condition("Landing", ParameterType.Trigger, ComparisonType.Equals, true));
        
            AddTransition(DoubleJumpState, LandingState,
                new Condition("Landing", ParameterType.Trigger, ComparisonType.Equals, true));
                
            // 硬着陆转换
            AddTransition(JumpState, HardLandingState,
                new Condition("HardLanding", ParameterType.Trigger, ComparisonType.Equals, true));
                
            AddTransition(DoubleJumpState, HardLandingState,
                new Condition("HardLanding", ParameterType.Trigger, ComparisonType.Equals, true));
        
            // 着陆恢复
            AddTransition(LandingState, null,
                new Condition("RecoveryComplete", ParameterType.Trigger, ComparisonType.Equals, true));
        
            AddTransition(HardLandingState, null,
                new Condition("RecoveryComplete", ParameterType.Trigger, ComparisonType.Equals, true));
                
            // 允许在着陆状态直接跳跃
            AddTransition(LandingState, JumpState,
                new Condition("Jump", ParameterType.Trigger, ComparisonType.Equals, true));
                
            AddTransition(HardLandingState, JumpState,
                new Condition("Jump", ParameterType.Trigger, ComparisonType.Equals, true));
        }
        
        private void RegisterUpperBodyLayerTransitions()
        {
            // 确保上身状态已被正确初始化
            if (AimState == null || FireState == null)
            {
                Debug.LogError("RegisterUpperBodyLayerTransitions: 上半身层状态未初始化");
                InitializeStates(); // 尝试重新初始化状态
            }
            
            // 瞄准状态转换
            AddGlobalTransition((int)StateLayerType.UpperBody, AimState,
                new Condition("IsAiming", ParameterType.Bool, ComparisonType.Equals, true));
            
            // 从瞄准状态恢复
            AddTransition(AimState, null,
                new Condition("IsAiming", ParameterType.Bool, ComparisonType.Equals, false));
                
            // 开火状态转换
            AddGlobalTransition((int)StateLayerType.UpperBody, FireState,
                new Condition("Fire", ParameterType.Trigger, ComparisonType.Equals, true));
        }

        private void AddTransition(IState from, IState to, params Condition[] conditions)
        {
            // 如果from为空，记录警告但不添加转换
            if (from == null)
            {
                Debug.LogWarning("尝试为空状态添加转换");
                return;
            }
            
            // 如果to为空，表示退出当前状态
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