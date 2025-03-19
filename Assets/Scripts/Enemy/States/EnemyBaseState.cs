using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 敌人基础状态
    /// </summary>
    public abstract class EnemyBaseState : IState
    {
        protected Enemy enemy;
        
        public EnemyBaseState(Enemy enemy)
        {
            this.enemy = enemy;
        }
        
        // 实现IState接口
        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void Update(float deltaTime) { }
    }
} 