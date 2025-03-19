using UnityEngine;
using System;
using System.Collections.Generic;

namespace TrianCatStudio
{
    /// <summary>
    /// 敌人状态管理器 - 负责管理敌人的状态和转换
    /// </summary>
    public class EnemyStateManager : MonoBehaviour
    {
        public StateMachine StateMachine { get; private set; }
        public Enemy Enemy { get; private set; }
        
        // 状态层级定义
        public enum StateLayerType
        {
            Base = 0,           // 基础层：负责移动和基本行为
            Action = 1,         // 动作层：负责特殊动作（如受击、攻击等）
            Effect = 2          // 效果层：负责状态效果（如眩晕、减速等）
        }

        // 状态实例
        // 基础状态
        public EnemyIdleState IdleState { get; private set; }
        public EnemyPatrolState PatrolState { get; private set; }
        public EnemyChaseState ChaseState { get; private set; }
        public EnemyAttackState AttackState { get; private set; }
        
        // 特殊状态
        public EnemyStunnedState StunnedState { get; private set; }
        public EnemyDeathState DeathState { get; private set; }

        // 运行时状态
        private bool isTargetInRange;
        private bool isAttacking;
        private bool isStunned;
        private bool isDead;

        private void Awake()
        {
            try
            {
                Debug.Log("EnemyStateManager.Awake: 开始初始化");
                Init();
                Debug.Log("EnemyStateManager.Awake: 初始化完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"EnemyStateManager.Awake: 初始化失败 - {e.Message}\n{e.StackTrace}");
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
                    Debug.Log("EnemyStateManager.OnEnable: StateMachine为空，重新初始化");
                    Init();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"EnemyStateManager.OnEnable: 发生错误 - {e.Message}\n{e.StackTrace}");
                enabled = false;
            }
        }

        protected void Init()
        {
            try
            {
                StateMachine = new StateMachine();
                Enemy = GetComponent<Enemy>();
                
                // 确保Enemy组件存在
                if (Enemy == null)
                {
                    Debug.LogError("EnemyStateManager无法找到Enemy组件");
                    enabled = false;
                    return;
                }
                
                InitializeStates();
                RegisterTransitions();
                SetInitialState();
                
                // 订阅事件
                SubscribeToEnemyEvents();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"EnemyStateManager初始化失败: {e.Message}\n{e.StackTrace}");
                enabled = false;
            }
        }

        private void InitializeStates()
        {
            // 基础层状态
            IdleState = new EnemyIdleState(Enemy);
            PatrolState = new EnemyPatrolState(Enemy);
            ChaseState = new EnemyChaseState(Enemy);
            AttackState = new EnemyAttackState(Enemy);
            
            // 特殊状态
            StunnedState = new EnemyStunnedState(Enemy);
            DeathState = new EnemyDeathState(Enemy);
        }

        private void RegisterTransitions()
        {
            // 添加基础层状态转换
            RegisterBaseLayerTransitions();
            
            // 添加动作层状态转换
            RegisterActionLayerTransitions();
            
            // 添加效果层状态转换
            RegisterEffectLayerTransitions();
        }
        
        private void RegisterBaseLayerTransitions()
        {
            // 空闲状态 -> 巡逻状态
            AddTransition(IdleState, PatrolState,
                new Condition("IdleComplete", ParameterType.Trigger, ComparisonType.Equals, true));
                
            // 巡逻状态 -> 空闲状态
            AddTransition(PatrolState, IdleState,
                new Condition("PatrolComplete", ParameterType.Trigger, ComparisonType.Equals, true));
                
            // 任何状态 -> 追击状态（当检测到目标时）
            AddGlobalTransition(ChaseState,
                new Condition("TargetInRange", ParameterType.Bool, ComparisonType.Equals, true));
                
            // 追击状态 -> 攻击状态（当目标在攻击范围内）
            AddTransition(ChaseState, AttackState,
                new Condition("TargetInAttackRange", ParameterType.Bool, ComparisonType.Equals, true));
                
            // 攻击状态 -> 追击状态（当目标不在攻击范围内）
            AddTransition(AttackState, ChaseState,
                new Condition("TargetInAttackRange", ParameterType.Bool, ComparisonType.Equals, false),
                new Condition("TargetInRange", ParameterType.Bool, ComparisonType.Equals, true));
                
            // 追击状态 -> 空闲状态（当丢失目标时）
            AddTransition(ChaseState, IdleState,
                new Condition("TargetInRange", ParameterType.Bool, ComparisonType.Equals, false));
        }
        
        private void RegisterActionLayerTransitions()
        {
            // 添加效果层的全局转换（眩晕）
            AddGlobalTransition((int)StateLayerType.Action, StunnedState,
                new Condition("IsStunned", ParameterType.Bool, ComparisonType.Equals, true));
                
            // 眩晕状态 -> 无状态（当眩晕结束时）
            AddTransition(StunnedState, null,
                new Condition("IsStunned", ParameterType.Bool, ComparisonType.Equals, false));
        }
        
        private void RegisterEffectLayerTransitions()
        {
            // 效果层暂无转换规则
        }

        private void SetInitialState()
        {
            // 初始化主状态为空闲状态
            StateMachine.Initialize(IdleState);
            
            // 初始化各层状态
            Dictionary<int, IState> initialLayerStates = new Dictionary<int, IState>();
            // 暂时不需要初始化其他层状态
            
            if (initialLayerStates.Count > 0)
            {
                StateMachine.InitializeLayers(initialLayerStates);
            }
        }
        
        private void SubscribeToEnemyEvents()
        {
            if (Enemy != null)
            {
                // 可以在这里订阅Enemy的事件
                // 例如死亡事件、受伤事件等
                Enemy.OnDeath += HandleEnemyDeath;
                // 其他事件订阅...
            }
        }
        
        private void HandleEnemyDeath(Enemy enemy)
        {
            // 敌人死亡时，切换到死亡状态
            ChangeState(DeathState);
            
            // 设置死亡标志
            isDead = true;
            StateMachine.SetBool("IsDead", true);
        }

        private void Update()
        {
            try
            {
                if (StateMachine == null || Enemy == null) return;
                
                // 如果敌人已死亡，不更新状态
                if (isDead) return;
                
                // 更新状态参数
                UpdateStateParameters();
                
                // 更新状态机
                StateMachine.Update(Time.deltaTime);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"EnemyStateManager.Update发生异常: {e.Message}\n{e.StackTrace}");
            }
        }

        private void UpdateStateParameters()
        {
            // 更新目标检测
            isTargetInRange = Enemy.IsTargetInDetectRange();
            StateMachine.SetBool("TargetInRange", isTargetInRange);
            
            // 更新攻击范围检测
            bool targetInAttackRange = Enemy.IsTargetInAttackRange();
            StateMachine.SetBool("TargetInAttackRange", targetInAttackRange);
            
            // 更新其他参数...
            StateMachine.SetBool("IsStunned", isStunned);
            StateMachine.SetBool("IsDead", isDead);
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
        
        // 状态触发器方法
        public void TriggerIdleComplete()
        {
            StateMachine.SetTrigger("IdleComplete");
        }
        
        public void TriggerPatrolComplete()
        {
            StateMachine.SetTrigger("PatrolComplete");
        }
        
        public void SetStunned(bool stunned, float duration = 2f)
        {
            isStunned = stunned;
            StateMachine.SetBool("IsStunned", stunned);
            
            if (stunned)
            {
                // 如果设置为眩晕，启动计时器来自动解除眩晕
                CancelInvoke("ResetStun");
                Invoke("ResetStun", duration);
            }
        }
        
        private void ResetStun()
        {
            isStunned = false;
            StateMachine.SetBool("IsStunned", false);
        }
        
        // 辅助方法
        private void AddTransition(IState from, IState to, params Condition[] conditions)
        {
            StateMachine.AddTransition(from, to, conditions);
        }
        
        private void AddGlobalTransition(IState to, params Condition[] conditions)
        {
            StateMachine.AddGlobalTransition(to, conditions);
        }
        
        private void AddGlobalTransition(int layer, IState to, params Condition[] conditions)
        {
            StateMachine.AddGlobalTransition(layer, to, conditions);
        }
        
        private void OnDestroy()
        {
            // 解除事件订阅
            if (Enemy != null)
            {
                Enemy.OnDeath -= HandleEnemyDeath;
                // 解除其他事件订阅...
            }
        }
    }
} 
