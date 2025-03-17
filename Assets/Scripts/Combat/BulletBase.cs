using UnityEngine;
using System.Collections.Generic;

namespace TrianCatStudio
{
    /// <summary>
    /// 子弹基类，处理子弹的基本移动和伤害逻辑
    /// </summary>
    public abstract class BulletBase : MonoBehaviour
    {
        [Header("基本设置")]
        [SerializeField] protected float speed = 20f;
        [SerializeField] protected float lifetime = 5f;
        [SerializeField] protected GameObject hitEffectPrefab;
        
        [Header("伤害设置")]
        [SerializeField] protected float baseDamage = 10f; // 基础伤害值
        [SerializeField] protected float criticalChance = 0.1f;  // 暴击几率
        [SerializeField] protected float criticalMultiplier = 2.0f;  // 暴击倍率
        [SerializeField] protected string[] damageTags = new string[] { "Projectile", "Direct" };  // 伤害标签
        
        protected Vector3 direction;
        protected bool isInitialized = false;
        protected GameObject shooter; // 发射者引用
        protected DamageType damageType = DamageType.Physical; // 默认物理伤害
        protected int currentPierceCount = 0; // 当前已穿透数量
        
        protected virtual void Awake()
        {
            // 基类Awake方法，可以被子类重写
        }
        
        protected virtual void Start()
        {
            // 如果没有通过Initialize方法初始化，使用默认前向方向
            if (!isInitialized)
            {
                direction = transform.forward;
            }
            
            // 设置自动销毁
            Destroy(gameObject, lifetime);
        }
        
        protected virtual void Update()
        {
            // 移动子弹
            transform.position += direction * speed * Time.deltaTime;
        }
        
        /// <summary>
        /// 初始化子弹
        /// </summary>
        /// <param name="dir">子弹方向</param>
        /// <param name="spd">子弹速度</param>
        public virtual void Initialize(Vector3 dir, float spd)
        {
            direction = dir.normalized;
            speed = spd;
            isInitialized = true;
            
            // 设置子弹朝向
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        /// <summary>
        /// 初始化子弹（带发射者）
        /// </summary>
        /// <param name="dir">子弹方向</param>
        /// <param name="spd">子弹速度</param>
        /// <param name="shooterObj">发射者对象</param>
        public virtual void Initialize(Vector3 dir, float spd, GameObject shooterObj)
        {
            Initialize(dir, spd);
            shooter = shooterObj;
            
            // 忽略与发射者的碰撞
            IgnoreShooterCollision();
        }
        
        /// <summary>
        /// 忽略与发射者的碰撞
        /// </summary>
        protected virtual void IgnoreShooterCollision()
        {
            if (shooter != null)
            {
                // 获取发射者的所有碰撞体
                Collider[] shooterColliders = shooter.GetComponentsInChildren<Collider>();
                Collider bulletCollider = GetComponent<Collider>();
                
                if (bulletCollider != null)
                {
                    foreach (Collider col in shooterColliders)
                    {
                        // 忽略子弹与发射者之间的碰撞
                        Physics.IgnoreCollision(bulletCollider, col);
                    }
                }
            }
        }
        
        /// <summary>
        /// 设置子弹伤害数据
        /// </summary>
        /// <param name="damage">伤害值</param>
        /// <param name="type">伤害类型（物理、元素等）</param>
        public virtual void SetDamage(float damage, DamageType type = DamageType.Physical)
        {
            baseDamage = damage;
            damageType = type;
        }
        
        protected virtual void OnCollisionEnter(Collision collision)
        {
            HandleHit(collision.gameObject, collision.contacts[0].point);
        }
        
        protected virtual void OnTriggerEnter(Collider other)
        {
            HandleHit(other.gameObject, other.ClosestPoint(transform.position));
        }
        
        /// <summary>
        /// 处理击中逻辑
        /// </summary>
        protected virtual void HandleHit(GameObject hitObject, Vector3 hitPoint)
        {
            // 检查是否击中发射者
            if (shooter != null && (hitObject == shooter || hitObject.transform.IsChildOf(shooter.transform)))
            {
                // 如果击中发射者，不造成伤害
                return;
            }
            
            // 应用伤害逻辑（由子类实现具体细节）
            ApplyDamage(hitObject);
            
            // 生成击中特效
            SpawnHitEffect(hitPoint);
            
            // 销毁子弹
            Destroy(gameObject);
        }
        
        /// <summary>
        /// 应用伤害逻辑（由子类实现）
        /// </summary>
        /// <param name="hitObject">被击中的对象</param>
        protected abstract void ApplyDamage(GameObject hitObject);
        
        /// <summary>
        /// 生成击中特效
        /// </summary>
        /// <param name="hitPoint">击中点</param>
        protected virtual void SpawnHitEffect(Vector3 hitPoint)
        {
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, hitPoint, Quaternion.LookRotation(direction));
            }
        }
    }
} 