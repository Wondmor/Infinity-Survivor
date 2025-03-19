using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 敌人死亡状态
    /// </summary>
    public class EnemyDeathState : EnemyBaseState
    {
        public EnemyDeathState(Enemy enemy) : base(enemy)
        {
        }
        
        public override void OnEnter()
        {
            // 停止移动
            enemy.StopMoving();
            
            // 播放死亡动画
            if (enemy.EnemyAnimator != null)
                enemy.EnemyAnimator.SetTrigger("Death");
            
            // 禁用碰撞器
            Collider[] colliders = enemy.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }
        }
        
        public override void Update(float deltaTime)
        {
            // 死亡状态不执行任何逻辑
        }
    }
} 