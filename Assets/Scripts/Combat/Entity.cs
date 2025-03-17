using UnityEngine;

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
        
        [Header("伤害反馈")]
        [SerializeField] protected bool showDamageNumbers = true;
        [SerializeField] protected GameObject damageNumberPrefab;
        [SerializeField] protected GameObject deathEffectPrefab;
        
        [Header("防御属性")]
        [SerializeField] protected float physicalDefense = 0f;
        [SerializeField] protected float fireDefense = 0f;
        [SerializeField] protected float waterDefense = 0f;
        [SerializeField] protected float earthDefense = 0f;
        [SerializeField] protected float windDefense = 0f;
        [SerializeField] protected float lightningDefense = 0f;
        
        // 是否已死亡
        protected bool isDead = false;
        
        protected virtual void Awake()
        {
            currentHealth = maxHealth;
        }
        
        /// <summary>
        /// 接收伤害
        /// </summary>
        public virtual void TakeDamage(float damage)
        {
            if (isDead) return;
            
            // 应用伤害
            currentHealth -= damage;
            
            // 显示伤害数字
            if (showDamageNumbers)
            {
                ShowDamageNumber(damage);
            }
            
            // 检查是否死亡
            if (currentHealth <= 0)
            {
                Die();
            }
            
            Debug.Log($"{gameObject.name} 受到 {damage} 点伤害，剩余生命值: {currentHealth}");
        }
        
        /// <summary>
        /// 接收伤害（使用DamageData）
        /// </summary>
        public virtual void TakeDamage(DamageData damageData)
        {
            if (isDead) return;
            
            // 计算总伤害（考虑防御）
            float totalDamage = CalculateDamage(damageData);
            
            // 应用伤害
            currentHealth -= totalDamage;
            
            // 显示伤害数字
            if (showDamageNumbers)
            {
                ShowDamageNumber(totalDamage);
            }
            
            // 检查是否死亡
            if (currentHealth <= 0)
            {
                Die();
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
        
        /// <summary>
        /// 显示伤害数字
        /// </summary>
        protected virtual void ShowDamageNumber(float damage)
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
                    damageNumberComponent.SetDamage(damage);
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
        /// 死亡处理
        /// </summary>
        protected virtual void Die()
        {
            if (isDead) return;
            
            isDead = true;
            currentHealth = 0;
            
            // 生成死亡特效
            if (deathEffectPrefab != null)
            {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }
            
            Debug.Log($"{gameObject.name} 已死亡");
            
            // 延迟销毁对象
            Destroy(gameObject, 2f);
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
        /// 检查是否已死亡
        /// </summary>
        public bool IsDead()
        {
            return isDead;
        }
        
        /// <summary>
        /// 治疗
        /// </summary>
        public virtual void Heal(float amount)
        {
            if (isDead) return;
            
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            Debug.Log($"{gameObject.name} 恢复 {amount} 点生命值，当前生命值: {currentHealth}");
        }
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