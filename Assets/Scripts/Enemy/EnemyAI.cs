using UnityEngine;
using System.Collections.Generic;

namespace TrianCatStudio
{
    /// <summary>
    /// 敌人AI决策系统 - 负责处理敌人的高级决策逻辑
    /// </summary>
    [RequireComponent(typeof(Enemy), typeof(EnemyPerception), typeof(EnemyPathfinding))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("引用")]
        private Enemy enemy;
        private EnemyStateManager stateManager;
        private EnemyPerception perception;
        private EnemyPathfinding pathfinding;
        
        [Header("AI设置")]
        [SerializeField] private float decisionInterval = 0.5f; // 决策间隔
        [SerializeField] private float targetLostMemoryTime = 3f; // 目标丢失后记忆时间
        [SerializeField] private float suspicionTime = 5f; // 可疑状态持续时间
        
        // AI状态
        private enum AIState { Idle, Patrolling, Suspicious, Alert, Attacking, Fleeing }
        private AIState currentAIState = AIState.Idle;
        
        // 内部状态
        private float lastDecisionTime;
        private float lastTargetSeenTime;
        private Vector3 lastKnownTargetPosition;
        private float suspicionTimer;
        private bool isInvestigating;
        private List<Vector3> interestPoints = new List<Vector3>(); // 兴趣点（如声音来源）
        
        private void Awake()
        {
            // 获取必要组件
            enemy = GetComponent<Enemy>();
            stateManager = GetComponent<EnemyStateManager>();
            perception = GetComponent<EnemyPerception>();
            pathfinding = GetComponent<EnemyPathfinding>();
        }
        
        private void OnEnable()
        {
            // 订阅事件
            if (perception != null)
            {
                perception.OnTargetDetected += HandleTargetDetected;
                perception.OnTargetLost += HandleTargetLost;
                perception.OnNoiseHeard += HandleNoiseHeard;
            }
        }
        
        private void OnDisable()
        {
            // 取消事件订阅
            if (perception != null)
            {
                perception.OnTargetDetected -= HandleTargetDetected;
                perception.OnTargetLost -= HandleTargetLost;
                perception.OnNoiseHeard -= HandleNoiseHeard;
            }
        }
        
        private void Update()
        {
            if (enemy.IsDead) return;
            
            // 按决策间隔更新决策
            if (Time.time >= lastDecisionTime + decisionInterval)
            {
                MakeDecision();
                lastDecisionTime = Time.time;
            }
            
            // 更新当前AI状态
            UpdateAIState();
        }
        
        #region 决策逻辑
        
        /// <summary>
        /// 主要决策方法
        /// </summary>
        private void MakeDecision()
        {
            // 基于当前状态和感知信息，决定下一步行动
            switch (currentAIState)
            {
                case AIState.Idle:
                    DecideFromIdle();
                    break;
                    
                case AIState.Patrolling:
                    DecideFromPatrolling();
                    break;
                    
                case AIState.Suspicious:
                    DecideFromSuspicious();
                    break;
                    
                case AIState.Alert:
                    DecideFromAlert();
                    break;
                    
                case AIState.Attacking:
                    DecideFromAttacking();
                    break;
                    
                case AIState.Fleeing:
                    DecideFromFleeing();
                    break;
            }
        }
        
        private void DecideFromIdle()
        {
            // 如果目标可见，直接进入警戒状态
            if (perception.IsTargetVisible)
            {
                ChangeAIState(AIState.Alert);
                return;
            }
            
            // 有兴趣点，进入可疑状态
            if (interestPoints.Count > 0)
            {
                ChangeAIState(AIState.Suspicious);
                return;
            }
            
            // 随机决定是否巡逻
            if (Random.value < 0.3f)
            {
                ChangeAIState(AIState.Patrolling);
            }
        }
        
        private void DecideFromPatrolling()
        {
            // 如果目标可见，直接进入警戒状态
            if (perception.IsTargetVisible)
            {
                ChangeAIState(AIState.Alert);
                return;
            }
            
            // 有兴趣点，进入可疑状态
            if (interestPoints.Count > 0)
            {
                ChangeAIState(AIState.Suspicious);
                return;
            }
            
            // 随机决定是否休息
            if (Random.value < 0.1f)
            {
                ChangeAIState(AIState.Idle);
            }
        }
        
        private void DecideFromSuspicious()
        {
            // 如果目标可见，直接进入警戒状态
            if (perception.IsTargetVisible)
            {
                ChangeAIState(AIState.Alert);
                return;
            }
            
            // 可疑时间结束，回到巡逻
            if (suspicionTimer >= suspicionTime)
            {
                interestPoints.Clear();
                ChangeAIState(AIState.Patrolling);
            }
            
            // 如果正在调查点且到达了该点，移除该点
            if (isInvestigating && interestPoints.Count > 0)
            {
                float distToPoint = Vector3.Distance(transform.position, interestPoints[0]);
                if (distToPoint < 1.5f)
                {
                    interestPoints.RemoveAt(0);
                    isInvestigating = false;
                    
                    // 四处观望（动画触发）
                    LookAround();
                }
            }
            
            // 如果还有兴趣点，但不在调查中，开始调查下一个点
            if (!isInvestigating && interestPoints.Count > 0)
            {
                InvestigatePoint(interestPoints[0]);
            }
        }
        
        private void DecideFromAlert()
        {
            // 如果目标可见且在攻击范围内，进入攻击状态
            if (perception.IsTargetVisible && enemy.IsTargetInAttackRange())
            {
                ChangeAIState(AIState.Attacking);
                return;
            }
            
            // 目标可见但不在攻击范围，继续追击
            if (perception.IsTargetVisible)
            {
                ChaseTarget();
                lastKnownTargetPosition = perception.LastKnownTargetPosition;
                lastTargetSeenTime = Time.time;
                return;
            }
            
            // 目标不可见，但在记忆时间内，前往最后已知位置
            if (Time.time - lastTargetSeenTime < targetLostMemoryTime)
            {
                pathfinding.SetDestination(lastKnownTargetPosition);
                return;
            }
            
            // 记忆时间结束，进入可疑状态
            if (interestPoints.Count == 0)
            {
                // 添加最后已知位置作为兴趣点
                interestPoints.Add(lastKnownTargetPosition);
            }
            ChangeAIState(AIState.Suspicious);
        }
        
        private void DecideFromAttacking()
        {
            // 如果目标不可见或超出攻击范围，回到警戒状态
            if (!perception.IsTargetVisible || !enemy.IsTargetInAttackRange())
            {
                ChangeAIState(AIState.Alert);
                return;
            }
            
            // 否则继续攻击
            // 攻击逻辑在StateManager中处理
        }
        
        private void DecideFromFleeing()
        {
            // 血量恢复到安全值，回到警戒状态
            if (enemy.CurrentHealth / enemy.MaxHealth > 0.3f)
            {
                ChangeAIState(AIState.Alert);
                return;
            }
            
            // 继续逃离
            ContinueFleeing();
        }
        
        #endregion
        
        #region 状态更新与行为
        
        private void UpdateAIState()
        {
            // 更新各状态特定的计时器和逻辑
            switch (currentAIState)
            {
                case AIState.Suspicious:
                    suspicionTimer += Time.deltaTime;
                    break;
                    
                case AIState.Alert:
                    // 检查是否需要逃跑（血量过低）
                    if (enemy.CurrentHealth / enemy.MaxHealth < 0.2f)
                    {
                        ChangeAIState(AIState.Fleeing);
                    }
                    break;
            }
        }
        
        private void ChangeAIState(AIState newState)
        {
            // 退出当前状态的逻辑
            switch (currentAIState)
            {
                case AIState.Suspicious:
                    suspicionTimer = 0f;
                    isInvestigating = false;
                    break;
            }
            
            // 进入新状态的逻辑
            switch (newState)
            {
                case AIState.Idle:
                    stateManager.ChangeState(stateManager.IdleState);
                    break;
                    
                case AIState.Patrolling:
                    stateManager.ChangeState(stateManager.PatrolState);
                    break;
                    
                case AIState.Suspicious:
                    // 可以使用某种"警觉"状态，或者复用巡逻状态
                    stateManager.ChangeState(stateManager.PatrolState);
                    suspicionTimer = 0f;
                    break;
                    
                case AIState.Alert:
                    stateManager.ChangeState(stateManager.ChaseState);
                    break;
                    
                case AIState.Attacking:
                    stateManager.ChangeState(stateManager.AttackState);
                    break;
                    
                case AIState.Fleeing:
                    // 可以添加逃跑状态，或暂时用巡逻状态模拟
                    FleeFromTarget();
                    break;
            }
            
            // 更新当前状态
            currentAIState = newState;
            Debug.Log($"{gameObject.name} 切换AI状态: {newState}");
        }
        
        #endregion
        
        #region 行为实现
        
        private void ChaseTarget()
        {
            if (perception.CurrentTarget != null)
            {
                enemy.SetTarget(perception.CurrentTarget);
                pathfinding.SetDestination(perception.CurrentTarget.position);
            }
        }
        
        private void InvestigatePoint(Vector3 point)
        {
            isInvestigating = true;
            pathfinding.SetDestination(point);
        }
        
        private void LookAround()
        {
            // 触发观望动画或行为
            // 例如，可以在这里触发一个随机旋转的协程
            StartCoroutine(LookAroundCoroutine());
        }
        
        private System.Collections.IEnumerator LookAroundCoroutine()
        {
            float lookTime = 2f;
            float startTime = Time.time;
            
            // 随机选择一个方向
            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(randomDirection);
            
            while (Time.time < startTime + lookTime)
            {
                // 缓慢旋转
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
                yield return null;
            }
            
            // 看完了，重置调查状态
            isInvestigating = false;
        }
        
        private void FleeFromTarget()
        {
            if (perception.CurrentTarget == null) return;
            
            // 计算逃跑方向（远离目标）
            Vector3 fleeDirection = transform.position - perception.CurrentTarget.position;
            fleeDirection.y = 0;
            fleeDirection.Normalize();
            
            // 找一个远离目标的点
            Vector3 fleeDestination = transform.position + fleeDirection * 10f;
            pathfinding.SetDestination(fleeDestination);
        }
        
        private void ContinueFleeing()
        {
            // 检查是否需要更新逃跑点
            float distToDestination = pathfinding.GetDistanceToDestination();
            if (distToDestination < 2f || !pathfinding.HasPath())
            {
                FleeFromTarget();
            }
        }
        
        #endregion
        
        #region 事件处理
        
        private void HandleTargetDetected(Transform target)
        {
            // 在感知到目标时触发
            enemy.SetTarget(target);
            
            if (currentAIState == AIState.Idle || currentAIState == AIState.Patrolling || currentAIState == AIState.Suspicious)
            {
                ChangeAIState(AIState.Alert);
            }
            
            // 更新最后已知位置
            lastKnownTargetPosition = target.position;
            lastTargetSeenTime = Time.time;
        }
        
        private void HandleTargetLost(Transform target)
        {
            // 更新最后已知位置
            lastKnownTargetPosition = target.position;
            lastTargetSeenTime = Time.time;
            
            // 在警戒状态下，暂时不改变状态
            // MakeDecision方法会处理记忆时间逻辑
        }
        
        private void HandleNoiseHeard(Vector3 noisePosition, float volume)
        {
            // 将声音源添加为兴趣点
            // 可以基于音量决定优先级
            if (volume > 0.5f)
            {
                // 重要声音，放在列表最前面
                interestPoints.Insert(0, noisePosition);
                
                // 如果声音很重要且不在警戒状态，切换到可疑状态
                if (currentAIState != AIState.Alert && currentAIState != AIState.Attacking)
                {
                    ChangeAIState(AIState.Suspicious);
                }
            }
            else
            {
                // 普通声音，添加到列表末尾
                interestPoints.Add(noisePosition);
            }
        }
        
        #endregion
        
        #region 调试
        
        private void OnDrawGizmos()
        {
            // 可视化当前AI状态
            if (!Application.isPlaying) return;
            
            // 绘制兴趣点
            Gizmos.color = Color.yellow;
            foreach (var point in interestPoints)
            {
                Gizmos.DrawSphere(point, 0.5f);
            }
            
            // 绘制最后已知目标位置
            if (Time.time - lastTargetSeenTime < targetLostMemoryTime)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(lastKnownTargetPosition, 0.7f);
            }
            
            // 根据AI状态绘制不同颜色
            switch (currentAIState)
            {
                case AIState.Idle:
                    Gizmos.color = Color.gray;
                    break;
                case AIState.Patrolling:
                    Gizmos.color = Color.green;
                    break;
                case AIState.Suspicious:
                    Gizmos.color = Color.yellow;
                    break;
                case AIState.Alert:
                    Gizmos.color = Color.red;
                    break;
                case AIState.Attacking:
                    Gizmos.color = Color.magenta;
                    break;
                case AIState.Fleeing:
                    Gizmos.color = Color.blue;
                    break;
            }
            
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);
        }
        
        #endregion
    }
} 