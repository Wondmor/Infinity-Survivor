using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 可伤害接口，实现此接口的对象可以接收伤害
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// 接收伤害
        /// </summary>
        /// <param name="damage">伤害值</param>
        void TakeDamage(float damage);
        
        /// <summary>
        /// 接收伤害（使用DamageData）
        /// </summary>
        /// <param name="damageData">伤害数据</param>
        void TakeDamage(DamageData damageData);
    }
} 