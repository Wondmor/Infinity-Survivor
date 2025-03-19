using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 敌人眩晕状态
    /// </summary>
    public class EnemyStunnedState : EnemyBaseState
    {
        private float stunDuration;
        private float stunTimer;
        
        public EnemyStunnedState(Enemy enemy) : base(enemy)
        {
            stunDuration = 2f; // 默认眩晕2秒
        }
        
        public override void OnEnter()
        {
            // 停止移动
            enemy.StopMoving();
            
            // 播放眩晕动画
            if (enemy.EnemyAnimator != null)
                enemy.EnemyAnimator.SetTrigger("Stunned");
                
            stunTimer = 0f;
        }
        
        public override void Update(float deltaTime)
        {
            stunTimer += deltaTime;
            
            // 眩晕结束，返回上一个状态
            if (stunTimer >= stunDuration)
            {
                // 转换回追击或空闲状态
                if (enemy.Target != null && enemy.IsTargetInDetectRange())
                {
                    enemy.StateManager.ChangeState(enemy.StateManager.ChaseState);
                }
                else
                {
                    enemy.StateManager.ChangeState(enemy.StateManager.IdleState);
                }
            }
        }
    }
} 