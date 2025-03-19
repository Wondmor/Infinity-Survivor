using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 敌人攻击状态
    /// </summary>
    public class EnemyAttackState : EnemyBaseState
    {
        private float stateTimer;
        
        public EnemyAttackState(Enemy enemy) : base(enemy)
        {
        }
        
        public override void OnEnter()
        {
            stateTimer = 0f;
            
            // 尝试攻击
            enemy.TryAttack();
        }
        
        public override void Update(float deltaTime)
        {
            stateTimer += deltaTime;
            
            // 检查目标是否存在
            if (enemy.Target == null)
            {
                enemy.StateManager.ChangeState(enemy.StateManager.IdleState);
                return;
            }
            
            // 如果目标不在攻击范围内，切换到追击状态
            if (!enemy.IsTargetInAttackRange())
            {
                enemy.StateManager.ChangeState(enemy.StateManager.ChaseState);
                return;
            }
            
            // 正面朝向目标
            Vector3 targetDirection = (enemy.Target.position - enemy.transform.position).normalized;
            targetDirection.y = 0f;
            if (targetDirection != Vector3.zero)
            {
                enemy.transform.rotation = Quaternion.Slerp(
                    enemy.transform.rotation,
                    Quaternion.LookRotation(targetDirection),
                    10f * deltaTime
                );
            }
            
            // 尝试攻击
            if (stateTimer >= enemy.GetAttackCooldown())
            {
                enemy.TryAttack();
                stateTimer = 0f;
            }
        }
    }
} 