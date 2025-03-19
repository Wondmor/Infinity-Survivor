using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 池化玩家子弹类 - 继承PooledBulletBase并实现玩家子弹特有的功能
    /// </summary>
    public class PooledPlayerBullet : PooledBulletBase
    {
        [Header("玩家子弹特有设置")]
        [SerializeField] private bool canPierce = false; // 是否可以穿透敌人
        [SerializeField] private int maxPierceCount = 0; // 最大穿透数量
        [SerializeField] private float damageReductionPerPierce = 0.2f; // 每次穿透后的伤害衰减
        
        [Header("特效设置")]
        [SerializeField] private TrailRenderer trailRenderer; // 拖尾渲染器
        [SerializeField] private ParticleSystem bulletParticleSystem; // 粒子系统
        
        protected override void Start()
        {
            base.Start();
            
            // 玩家子弹特有的初始化逻辑
            currentPierceCount = 0;
        }
        
        /// <summary>
        /// 应用伤害逻辑
        /// </summary>
        /// <param name="hitObject">被击中的对象</param>
        protected override void ApplyDamage(GameObject hitObject)
        {
            // 检查是否击中可伤害对象
            IDamageable damageable = hitObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // 计算实际伤害
                float actualDamage = CalculateDamage();
                
                // 应用伤害
                damageable.TakeDamage(actualDamage, DamageType.Physical, hitObject);
                
                // 输出调试信息
                Debug.Log($"玩家子弹击中 {hitObject.name}，造成 {actualDamage} 点伤害");
                
                // 处理穿透逻辑
                if (canPierce && currentPierceCount < maxPierceCount)
                {
                    currentPierceCount++;
                    // 减少伤害
                    baseDamage *= (1f - damageReductionPerPierce);
                    // 不销毁子弹
                    return;
                }
            }
            
            // 如果没有穿透或已达到最大穿透次数，子弹会在HandleHit方法中被回收
        }
        
        /// <summary>
        /// 计算实际伤害（考虑暴击等因素）
        /// </summary>
        /// <returns>实际伤害值</returns>
        private float CalculateDamage()
        {
            // 检查是否暴击
            bool isCritical = Random.value < criticalChance;
            float damage = baseDamage;
            
            // 如果暴击，应用暴击倍率
            if (isCritical)
            {
                damage *= criticalMultiplier;
                Debug.Log("暴击！伤害翻倍");
            }
            
            return damage;
        }
        
        /// <summary>
        /// 处理击中逻辑
        /// </summary>
        protected override void HandleHit(GameObject hitObject, Vector3 hitPoint)
        {
            // 检查是否击中发射者
            if (shooter != null && (hitObject == shooter || hitObject.transform.IsChildOf(shooter.transform)))
            {
                // 如果击中发射者，不造成伤害
                return;
            }
            
            // 应用伤害逻辑
            ApplyDamage(hitObject);
            
            // 生成击中特效
            SpawnHitEffect(hitPoint);
            
            // 检查是否需要回收子弹（如果没有穿透或已达到最大穿透次数）
            if (!canPierce || currentPierceCount >= maxPierceCount)
            {
                // 回收子弹
                if (pooledObject != null)
                {
                    pooledObject.Release();
                }
                else
                {
                    // 如果没有池化组件，则销毁
                    Destroy(gameObject);
                }
            }
        }
        
        /// <summary>
        /// 设置穿透属性
        /// </summary>
        /// <param name="canPierceEnemies">是否可以穿透</param>
        /// <param name="maxPierce">最大穿透数量</param>
        /// <param name="damageReduction">每次穿透后的伤害衰减</param>
        public void SetPierceProperties(bool canPierceEnemies, int maxPierce = 1, float damageReduction = 0.2f)
        {
            canPierce = canPierceEnemies;
            maxPierceCount = maxPierce;
            damageReductionPerPierce = damageReduction;
        }
        
        #region IPoolable接口实现
        
        /// <summary>
        /// 对象从池中获取时调用
        /// </summary>
        public override void OnPoolGet()
        {
            base.OnPoolGet();
            
            // 重置穿透计数
            currentPierceCount = 0;
            
            // 启用拖尾渲染器
            if (trailRenderer != null)
            {
                trailRenderer.enabled = true;
                trailRenderer.Clear();
            }
            
            // 启用粒子系统
            if (bulletParticleSystem != null)
            {
                bulletParticleSystem.Play();
            }
        }
        
        /// <summary>
        /// 对象释放回池时调用
        /// </summary>
        public override void OnPoolRelease()
        {
            base.OnPoolRelease();
            
            // 重置穿透属性
            currentPierceCount = 0;
            
            // 禁用拖尾渲染器
            if (trailRenderer != null)
            {
                trailRenderer.enabled = false;
                trailRenderer.Clear();
            }
            
            // 停止粒子系统
            if (bulletParticleSystem != null)
            {
                bulletParticleSystem.Stop();
                bulletParticleSystem.Clear();
            }
        }
        
        #endregion
    }
} 