using UnityEngine;
using System;

namespace TrianCatStudio
{
    /// <summary>
    /// 可受伤害接口 - 所有可以受到伤害的实体都应实现该接口
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// 受到伤害
        /// </summary>
        /// <param name="damage">伤害值</param>
        /// <param name="damageType">伤害类型</param>
        /// <param name="source">伤害来源</param>
        /// <returns>实际造成的伤害值</returns>
        float TakeDamage(float damage, DamageType damageType, GameObject source);
        
        /// <summary>
        /// 获取当前生命值
        /// </summary>
        float GetCurrentHealth();
        
        /// <summary>
        /// 获取最大生命值
        /// </summary>
        float GetMaxHealth();
        
        /// <summary>
        /// 是否已经死亡
        /// </summary>
        bool IsDead();
    }
    
    /// <summary>
    /// 伤害类型枚举
    /// </summary>
    public enum DamageType
    {
        Physical,   // 物理伤害
        Fire,       // 火焰伤害
        Ice,        // 冰冻伤害
        Lightning,  // 闪电伤害
        Poison,     // 毒素伤害
        True        // 真实伤害（无视防御）
    }
    
    /// <summary>
    /// 伤害事件参数
    /// </summary>
    public class DamageEventArgs : EventArgs
    {
        public float OriginalDamage { get; private set; } // 原始伤害值
        public float ActualDamage { get; set; } // 实际伤害值（可被修改）
        public DamageType DamageType { get; private set; } // 伤害类型
        public GameObject Source { get; private set; } // 伤害来源
        public GameObject Target { get; private set; } // 伤害目标
        public bool IsCritical { get; set; } // 是否暴击
        public bool IsBlocked { get; set; } // 是否被格挡
        public bool IsEvaded { get; set; } // 是否被闪避
        
        public DamageEventArgs(float damage, DamageType damageType, GameObject source, GameObject target)
        {
            OriginalDamage = damage;
            ActualDamage = damage;
            DamageType = damageType;
            Source = source;
            Target = target;
            IsCritical = false;
            IsBlocked = false;
            IsEvaded = false;
        }
    }
} 