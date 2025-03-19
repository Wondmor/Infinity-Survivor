using UnityEngine;
using UnityEngine.AI;

namespace TrianCatStudio
{
    /// <summary>
    /// 敌人巡逻状态
    /// </summary>
    public class EnemyPatrolState : EnemyBaseState
    {
        private Vector3 patrolTarget;
        private float patrolWaitTimer;
        private bool isWaiting;
        private Vector3 startPosition;
        
        public EnemyPatrolState(Enemy enemy) : base(enemy)
        {
            startPosition = enemy.transform.position;
        }
        
        public override void OnEnter()
        {
            // 设置巡逻点
            SetNewPatrolTarget();
            
            // 播放走路动画
            if (enemy.EnemyAnimator != null)
                enemy.EnemyAnimator.SetTrigger("Walk");
                
            isWaiting = false;
        }
        
        public override void Update(float deltaTime)
        {
            // 如果找到目标，切换到追击状态
            if (enemy.Target != null && enemy.IsTargetInDetectRange())
            {
                enemy.StateManager.ChangeState(enemy.StateManager.ChaseState);
                return;
            }
            
            if (isWaiting)
            {
                // 等待一段时间后继续巡逻
                patrolWaitTimer += deltaTime;
                if (patrolWaitTimer >= 2f)
                {
                    isWaiting = false;
                    SetNewPatrolTarget();
                    
                    // 播放走路动画
                    if (enemy.EnemyAnimator != null)
                        enemy.EnemyAnimator.SetTrigger("Walk");
                }
            }
            else
            {
                // 检查是否到达巡逻点
                if (Vector3.Distance(enemy.transform.position, patrolTarget) < 0.5f)
                {
                    // 到达后等待
                    isWaiting = true;
                    patrolWaitTimer = 0f;
                    
                    // 停止移动
                    enemy.StopMoving();
                    
                    // 播放空闲动画
                    if (enemy.EnemyAnimator != null)
                        enemy.EnemyAnimator.SetTrigger("Idle");
                }
            }
        }
        
        private void SetNewPatrolTarget()
        {
            // 在起始点附近找一个随机位置
            float patrolRange = enemy.GetPatrolRange();
            Vector2 randomCircle = Random.insideUnitCircle * patrolRange;
            patrolTarget = startPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            // 尝试找到一个有效的导航点
            if (NavMesh.SamplePosition(patrolTarget, out NavMeshHit hit, patrolRange, NavMesh.AllAreas))
            {
                patrolTarget = hit.position;
                enemy.MoveTo(patrolTarget);
            }
            else
            {
                // 如果找不到有效点，返回起始位置
                patrolTarget = startPosition;
                enemy.MoveTo(patrolTarget);
            }
        }
        
        public override void OnExit()
        {
            enemy.StopMoving();
        }
    }
} 