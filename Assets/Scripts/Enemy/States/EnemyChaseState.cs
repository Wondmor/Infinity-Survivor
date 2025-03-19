using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 敌人追击状态
    /// </summary>
    public class EnemyChaseState : EnemyBaseState
    {
        private float chaseTimer;
        private float maxChaseTime;
        private float nextPathUpdateTime;
        
        public EnemyChaseState(Enemy enemy) : base(enemy)
        {
            maxChaseTime = enemy.GetChaseTimeout();
        }
        
        public override void OnEnter()
        {
            // 重置计时器
            chaseTimer = 0f;
            nextPathUpdateTime = 0f;
            
            // 播放跑步动画
            if (enemy.EnemyAnimator != null)
                enemy.EnemyAnimator.SetTrigger("Run");
                
            UpdatePath();
        }
        
        public override void Update(float deltaTime)
        {
            if (enemy.Target == null)
            {
                // 如果没有目标，返回空闲状态
                enemy.StateManager.ChangeState(enemy.StateManager.IdleState);
                return;
            }
            
            // 更新追击计时器
            chaseTimer += deltaTime;
            
            // 如果超过最大追击时间，返回空闲状态
            if (chaseTimer > maxChaseTime && !enemy.IsTargetInDetectRange())
            {
                enemy.StateManager.ChangeState(enemy.StateManager.IdleState);
                return;
            }
            
            // 如果目标在攻击范围内，切换到攻击状态
            if (enemy.IsTargetInAttackRange())
            {
                enemy.StateManager.ChangeState(enemy.StateManager.AttackState);
                return;
            }
            
            // 定期更新路径
            if (Time.time >= nextPathUpdateTime)
            {
                UpdatePath();
            }
        }
        
        private void UpdatePath()
        {
            if (enemy.Target != null)
            {
                // 更新追击路径
                enemy.MoveTo(enemy.Target.position);
                
                // 设置下一次路径更新时间
                nextPathUpdateTime = Time.time + 0.5f;
            }
        }
        
        public override void OnExit()
        {
            enemy.StopMoving();
        }
    }
} 