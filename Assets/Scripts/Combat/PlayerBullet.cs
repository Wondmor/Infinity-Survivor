using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 玩家子弹类，处理玩家发射的子弹特有逻辑
    /// </summary>
    public class PlayerBullet : BulletBase
    {
        [Header("玩家子弹特有设置")]
        [SerializeField] private bool canPierce = false; // 是否可以穿透敌人
        [SerializeField] private int maxPierceCount = 0; // 最大穿透数量
        [SerializeField] private float damageReductionPerPierce = 0.2f; // 每次穿透后的伤害衰减
        
        private int currentPierceCount = 0; // 当前已穿透数量
        
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
                damageable.TakeDamage(actualDamage);
                
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
            
            // 如果没有穿透或已达到最大穿透次数，子弹会在HandleHit方法中被销毁
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
            
            // 检查是否需要销毁子弹（如果没有穿透或已达到最大穿透次数）
            if (!canPierce || currentPierceCount >= maxPierceCount)
            {
                // 销毁子弹
                Destroy(gameObject);
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
    }
} 