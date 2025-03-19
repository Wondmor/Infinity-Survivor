using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 敌人空闲状态
    /// </summary>
    public class EnemyIdleState : EnemyBaseState
    {
        private float idleTimer;
        private float idleDuration;
        
        public EnemyIdleState(Enemy enemy) : base(enemy)
        {
            idleDuration = Random.Range(1f, 3f);
        }
        
        public override void OnEnter()
        {
            // 停止移动
            enemy.StopMoving();
            
            // 播放空闲动画
            if (enemy.EnemyAnimator != null)
                enemy.EnemyAnimator.SetTrigger("Idle");
                
            // 重置计时器
            idleTimer = 0f;
            idleDuration = Random.Range(1f, 3f);
        }
        
        public override void Update(float deltaTime)
        {
            // 如果发现目标，切换到追击状态
            if (enemy.Target != null && enemy.IsTargetInDetectRange())
            {
                enemy.StateManager.ChangeState(enemy.StateManager.ChaseState);
                return;
            }
            
            // 空闲一段时间后切换到巡逻状态
            idleTimer += deltaTime;
            if (idleTimer >= idleDuration)
            {
                enemy.StateManager.ChangeState(enemy.StateManager.PatrolState);
            }
        }
    }
} 