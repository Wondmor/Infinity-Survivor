using UnityEngine;
using System;

namespace TrianCatStudio
{
    /// <summary>
    /// 伤害修饰器 - 修改进出伤害数值并提供抗性
    /// </summary>
    public class DamageModifier : MonoBehaviour
    {
        [Header("伤害修饰")]
        [SerializeField] private float incomingDamageMultiplier = 1.0f;
        [SerializeField] private float outgoingDamageMultiplier = 1.0f;
        
        [Header("抗性设置")]
        [SerializeField, Range(0f, 100f)] private float physicalResistance = 0f;
        [SerializeField, Range(0f, 100f)] private float fireResistance = 0f;
        [SerializeField, Range(0f, 100f)] private float iceResistance = 0f;
        [SerializeField, Range(0f, 100f)] private float lightningResistance = 0f;
        [SerializeField, Range(0f, 100f)] private float poisonResistance = 0f;
        
        // 引用实体对象
        private Entity entity;
        
        // 事件
        public event Action<DamageEventArgs> OnBeforeDamageApplied;
        
        private void Awake()
        {
            entity = GetComponent<Entity>();
            
            // 订阅实体的伤害事件
            if (entity != null)
            {
                entity.OnDamageTaken += ModifyIncomingDamage;
            }
        }
        
        private void OnDestroy()
        {
            // 解除订阅
            if (entity != null)
            {
                entity.OnDamageTaken -= ModifyIncomingDamage;
            }
        }
        
        /// <summary>
        /// 修改进入的伤害
        /// </summary>
        private void ModifyIncomingDamage(DamageEventArgs damageArgs)
        {
            // 应用抗性减免
            ApplyResistance(damageArgs);
            
            // 应用全局伤害修饰
            damageArgs.ActualDamage *= incomingDamageMultiplier;
            
            // 触发伤害前事件，允许其他组件进一步修改
            OnBeforeDamageApplied?.Invoke(damageArgs);
        }
        
        /// <summary>
        /// 应用抗性减免
        /// </summary>
        private void ApplyResistance(DamageEventArgs damageArgs)
        {
            float resistance = 0f;
            
            // 根据伤害类型应用不同的抗性
            switch (damageArgs.DamageType)
            {
                case DamageType.Physical:
                    resistance = physicalResistance;
                    break;
                case DamageType.Fire:
                    resistance = fireResistance;
                    break;
                case DamageType.Ice:
                    resistance = iceResistance;
                    break;
                case DamageType.Lightning:
                    resistance = lightningResistance;
                    break;
                case DamageType.Poison:
                    resistance = poisonResistance;
                    break;
                case DamageType.True:
                    // 真实伤害无视抗性
                    resistance = 0f;
                    break;
            }
            
            // 伤害减免计算公式（伤害减免百分比 = 抗性 / (抗性 + 100)）
            // 例如：100抗性提供50%减伤，200抗性提供66.7%减伤
            float damageReduction = resistance / (resistance + 100f);
            damageArgs.ActualDamage *= (1f - damageReduction);
        }
        
        /// <summary>
        /// 修改输出的伤害（由攻击者使用）
        /// </summary>
        public float ModifyOutgoingDamage(float damage)
        {
            return damage * outgoingDamageMultiplier;
        }
        
        #region 设置方法
        
        /// <summary>
        /// 设置物理抗性
        /// </summary>
        public void SetPhysicalResistance(float value)
        {
            physicalResistance = Mathf.Max(0f, value);
        }
        
        /// <summary>
        /// 设置火焰抗性
        /// </summary>
        public void SetFireResistance(float value)
        {
            fireResistance = Mathf.Max(0f, value);
        }
        
        /// <summary>
        /// 设置冰冻抗性
        /// </summary>
        public void SetIceResistance(float value)
        {
            iceResistance = Mathf.Max(0f, value);
        }
        
        /// <summary>
        /// 设置闪电抗性
        /// </summary>
        public void SetLightningResistance(float value)
        {
            lightningResistance = Mathf.Max(0f, value);
        }
        
        /// <summary>
        /// 设置毒素抗性
        /// </summary>
        public void SetPoisonResistance(float value)
        {
            poisonResistance = Mathf.Max(0f, value);
        }
        
        /// <summary>
        /// 设置进入伤害乘数
        /// </summary>
        public void SetIncomingDamageMultiplier(float value)
        {
            incomingDamageMultiplier = Mathf.Max(0f, value);
        }
        
        /// <summary>
        /// 设置输出伤害乘数
        /// </summary>
        public void SetOutgoingDamageMultiplier(float value)
        {
            outgoingDamageMultiplier = Mathf.Max(0f, value);
        }
        
        #endregion
        
        #region 获取方法
        
        public float GetPhysicalResistance() => physicalResistance;
        public float GetFireResistance() => fireResistance;
        public float GetIceResistance() => iceResistance;
        public float GetLightningResistance() => lightningResistance;
        public float GetPoisonResistance() => poisonResistance;
        
        public float GetIncomingDamageMultiplier() => incomingDamageMultiplier;
        public float GetOutgoingDamageMultiplier() => outgoingDamageMultiplier;
        
        #endregion
    }
} 