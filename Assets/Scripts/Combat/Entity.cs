using UnityEngine;
using System;
using UnityEngine.AI;

namespace TrianCatStudio
{
    /// <summary>
    /// 基础实体类，实现生命值和伤害处理
    /// </summary>
    public class Entity : MonoBehaviour, IDamageable, IHealth
    {
        [Header("生命值设置")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float currentHealth;
        [SerializeField] protected bool isInvulnerable = false;
        
        [Header("伤害反馈")]
        [SerializeField] protected bool showDamageNumbers = true;
        [SerializeField] protected GameObject damageNumberPrefab;
        [SerializeField] protected GameObject deathEffectPrefab;
        [SerializeField] protected AudioClip hitSoundEffect;
        [SerializeField] protected AudioClip deathSoundEffect;
        
        [Header("防御属性")]
        [SerializeField] protected float physicalDefense = 0f;
        [SerializeField] protected float fireDefense = 0f;
        [SerializeField] protected float waterDefense = 0f;
        [SerializeField] protected float earthDefense = 0f;
        [SerializeField] protected float windDefense = 0f;
        [SerializeField] protected float lightningDefense = 0f;
        [SerializeField] protected float poisonDefense = 0f;
        
        // 事件
        public event Action<float> OnHealthChanged; // 生命值变化事件
        public event Action<GameObject> OnDeath; // 死亡事件
        public event Action<DamageEventArgs> OnDamageTaken; // 受伤事件
        public event Action<float> OnHeal; // 治疗事件
        
        // 是否已死亡
        protected bool isDead = false;
        
        protected virtual void Awake()
        {
            currentHealth = maxHealth;
        }
        
        #region IDamageable接口实现
        
        /// <summary>
        /// 受到伤害（实现IDamageable接口）
        /// </summary>
        public virtual float TakeDamage(float damage, DamageType damageType, GameObject source)
        {
            // 如果已经死亡或无敌，不受伤害
            if (isDead || isInvulnerable)
                return 0;
                
            // 创建伤害事件数据
            DamageEventArgs damageArgs = new DamageEventArgs(damage, damageType, source, gameObject);
            
            // 应用抵抗和防御
            ApplyDefenseAndResistance(damageArgs);
            
            // 触发受伤前事件，允许其他组件修改伤害
            OnDamageTaken?.Invoke(damageArgs);
            
            // 如果伤害被闪避或格挡，不扣血
            if (damageArgs.IsEvaded || (damageArgs.IsBlocked && damageArgs.ActualDamage <= 0))
                return 0;
            
            // 应用实际伤害
            float actualDamage = damageArgs.ActualDamage;
            currentHealth -= actualDamage;
            
            // 触发生命值变化事件
            OnHealthChanged?.Invoke(currentHealth / maxHealth);
            
            // 播放受伤效果
            PlayHitEffect(damageArgs);
            
            // 检查是否死亡
            if (currentHealth <= 0)
            {
                Die(source);
            }
            
            Debug.Log($"{gameObject.name} 受到 {actualDamage} 点{damageType}伤害，剩余生命值: {currentHealth}");
            
            return actualDamage;
        }
        
        /// <summary>
        /// 获取当前生命值
        /// </summary>
        public float GetCurrentHealth()
        {
            return currentHealth;
        }
        
        /// <summary>
        /// 获取最大生命值
        /// </summary>
        public float GetMaxHealth()
        {
            return maxHealth;
        }
        
        /// <summary>
        /// 是否已经死亡
        /// </summary>
        public bool IsDead()
        {
            return isDead;
        }
        
        #endregion
        
        #region 旧的伤害处理方法（兼容性保留）
        
        /// <summary>
        /// 接收伤害（简化版）
        /// </summary>
        public virtual void TakeDamage(float damage)
        {
            TakeDamage(damage, DamageType.Physical, null);
        }
        
        /// <summary>
        /// 接收伤害（使用DamageData）
        /// </summary>
        public virtual void TakeDamage(DamageData damageData)
        {
            if (isDead || isInvulnerable) return;
            
            // 计算总伤害（考虑防御）
            float totalDamage = CalculateDamage(damageData);
            
            // 应用伤害
            currentHealth -= totalDamage;
            
            // 触发生命值变化事件
            OnHealthChanged?.Invoke(currentHealth / maxHealth);
            
            // 显示伤害数字
            if (showDamageNumbers)
            {
                ShowDamageNumber(totalDamage);
            }
            
            // 检查是否死亡
            if (currentHealth <= 0)
            {
                Die(null);
            }
            
            Debug.Log($"{gameObject.name} 受到 {totalDamage} 点伤害，剩余生命值: {currentHealth}");
        }
        
        /// <summary>
        /// 计算最终伤害（考虑防御）
        /// </summary>
        protected virtual float CalculateDamage(DamageData damageData)
        {
            float totalDamage = 0f;
            
            // 计算物理伤害
            totalDamage += Mathf.Max(0, damageData.physical * (1f - physicalDefense / 100f));
            
            // 计算元素伤害
            totalDamage += Mathf.Max(0, damageData.fire * (1f - fireDefense / 100f));
            totalDamage += Mathf.Max(0, damageData.water * (1f - waterDefense / 100f));
            totalDamage += Mathf.Max(0, damageData.earth * (1f - earthDefense / 100f));
            totalDamage += Mathf.Max(0, damageData.wind * (1f - windDefense / 100f));
            totalDamage += Mathf.Max(0, damageData.lightning * (1f - lightningDefense / 100f));
            
            // 纯粹伤害不受防御影响
            totalDamage += damageData.pure;
            
            return totalDamage;
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 设置最大生命值
        /// </summary>
        public virtual void SetMaxHealth(float newMaxHealth)
        {
            maxHealth = Mathf.Max(1, newMaxHealth);
            
            // 如果当前生命值高于新的最大值，将其调整为最大值
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;
                
            // 触发生命值变化事件
            OnHealthChanged?.Invoke(currentHealth / maxHealth);
        }
        
        /// <summary>
        /// 治疗生命值
        /// </summary>
        public virtual void Heal(float amount)
        {
            if (isDead)
                return;
                
            float oldHealth = currentHealth;
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            
            float healAmount = currentHealth - oldHealth;
            if (healAmount > 0)
            {
                // 触发治疗事件
                OnHeal?.Invoke(healAmount);
                
                // 触发生命值变化事件
                OnHealthChanged?.Invoke(currentHealth / maxHealth);
            }
            
            Debug.Log($"{gameObject.name} 恢复 {amount} 点生命值，当前生命值: {currentHealth}");
        }
        
        /// <summary>
        /// 完全恢复生命值
        /// </summary>
        public virtual void FullHeal()
        {
            Heal(maxHealth - currentHealth);
        }
        
        /// <summary>
        /// 设置无敌状态
        /// </summary>
        public virtual void SetInvulnerable(bool invulnerable)
        {
            isInvulnerable = invulnerable;
        }
        
        /// <summary>
        /// 复活
        /// </summary>
        public virtual void Revive(float healthPercentage = 1f)
        {
            if (!isDead)
                return;
                
            isDead = false;
            currentHealth = maxHealth * Mathf.Clamp01(healthPercentage);
            
            // 重新启用组件
            EnableComponents();
            
            // 触发生命值变化事件
            OnHealthChanged?.Invoke(currentHealth / maxHealth);
        }
        
        #endregion
        
        #region 私有/保护方法
        
        /// <summary>
        /// 应用防御和抗性减免
        /// </summary>
        protected virtual void ApplyDefenseAndResistance(DamageEventArgs damageArgs)
        {
            float resistance = GetDefenseForDamageType(damageArgs.DamageType);
            
            // 计算实际伤害（伤害减免公式）
            float damageReduction = resistance / (resistance + 100f); // 抗性转换为百分比减伤
            damageArgs.ActualDamage = damageArgs.OriginalDamage * (1f - damageReduction);
        }
        
        /// <summary>
        /// 根据伤害类型获取对应的防御值
        /// </summary>
        protected virtual float GetDefenseForDamageType(DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Physical:
                    return physicalDefense;
                case DamageType.Fire:
                    return fireDefense;
                case DamageType.Ice:
                    return waterDefense; // 使用waterDefense作为ice的抗性
                case DamageType.Lightning:
                    return lightningDefense;
                case DamageType.Poison:
                    return poisonDefense; // 正确使用毒素抗性
                case DamageType.True:
                    return 0f; // 真实伤害无抗性
                default:
                    return 0f;
            }
        }
        
        /// <summary>
        /// 显示伤害数字
        /// </summary>
        protected virtual void ShowDamageNumber(float damage, bool isCritical = false)
        {
            if (damageNumberPrefab != null)
            {
                // 在实体上方生成伤害数字
                Vector3 spawnPosition = transform.position + Vector3.up * 2f;
                GameObject damageNumber = Instantiate(damageNumberPrefab, spawnPosition, Quaternion.identity);
                
                // 设置伤害数字的值
                DamageNumber damageNumberComponent = damageNumber.GetComponent<DamageNumber>();
                if (damageNumberComponent != null)
                {
                    damageNumberComponent.SetDamage(damage, isCritical);
                }
                else
                {
                    // 如果没有DamageNumber组件，尝试设置文本
                    TMPro.TextMeshPro textMesh = damageNumber.GetComponent<TMPro.TextMeshPro>();
                    if (textMesh != null)
                    {
                        textMesh.text = damage.ToString("0");
                    }
                }
                
                // 自动销毁
                Destroy(damageNumber, 2f);
            }
        }
        
        /// <summary>
        /// 播放受伤效果
        /// </summary>
        protected virtual void PlayHitEffect(DamageEventArgs damageArgs)
        {
            // 显示伤害数字
            if (showDamageNumbers)
            {
                ShowDamageNumber(damageArgs.ActualDamage, damageArgs.IsCritical);
            }
            
            // 播放受伤特效
            if (deathEffectPrefab != null && !isDead)
            {
                Vector3 hitPosition = transform.position + Vector3.up; // 可以根据模型调整位置
                Instantiate(deathEffectPrefab, hitPosition, Quaternion.identity);
            }
            
            // 播放受伤音效
            if (hitSoundEffect != null)
            {
                AudioSource.PlayClipAtPoint(hitSoundEffect, transform.position);
            }
        }
        
        /// <summary>
        /// 死亡处理
        /// </summary>
        protected virtual void Die(GameObject killer)
        {
            if (isDead) return;
            
            isDead = true;
            currentHealth = 0;
            
            // 播放死亡音效
            if (deathSoundEffect != null)
            {
                AudioSource.PlayClipAtPoint(deathSoundEffect, transform.position);
            }
            
            // 生成死亡特效
            if (deathEffectPrefab != null)
            {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // 禁用某些组件
            DisableComponents();
            
            // 触发死亡事件
            OnDeath?.Invoke(killer);
            
            Debug.Log($"{gameObject.name} 已死亡");
        }
        
        /// <summary>
        /// 禁用死亡后不需要的组件
        /// </summary>
        protected virtual void DisableComponents()
        {
            // 禁用碰撞器
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }
            
            // 可以禁用其他组件，如AI、移动控制器等
            NavMeshAgent navAgent = GetComponent<NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.enabled = false;
            }
        }
        
        /// <summary>
        /// 重新启用死亡时禁用的组件
        /// </summary>
        protected virtual void EnableComponents()
        {
            // 启用碰撞器
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = true;
            }
            
            // 启用其他组件
            NavMeshAgent navAgent = GetComponent<NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.enabled = true;
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 伤害数字组件
    /// </summary>
    public class DamageNumber : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshPro textMesh;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color criticalColor = Color.red;
        
        private void Awake()
        {
            if (textMesh == null)
            {
                textMesh = GetComponent<TMPro.TextMeshPro>();
            }
        }
        
        /// <summary>
        /// 设置伤害数值
        /// </summary>
        public void SetDamage(float damage, bool isCritical = false)
        {
            if (textMesh != null)
            {
                textMesh.text = damage.ToString("0");
                textMesh.color = isCritical ? criticalColor : normalColor;
                
                // 如果是暴击，放大文本
                if (isCritical)
                {
                    textMesh.fontSize *= 1.5f;
                }
            }
        }
    }
} 