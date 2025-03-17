using UnityEngine;
using System.Collections.Generic;

namespace TrianCatStudio
{
    /// <summary>
    /// 池化子弹基类 - 继承BulletBase并实现IPoolable接口
    /// </summary>
    public abstract class PooledBulletBase : BulletBase, IPoolable
    {
        // 池化对象组件引用
        protected PooledObject pooledObject;
        
        protected override void Awake()
        {
            base.Awake();
            
            // 获取池化对象组件
            pooledObject = GetComponent<PooledObject>();
            if (pooledObject == null)
            {
                pooledObject = gameObject.AddComponent<PooledObject>();
            }
        }
        
        protected override void Start()
        {
            // 如果没有通过Initialize方法初始化，使用默认前向方向
            if (!isInitialized)
            {
                direction = transform.forward;
            }
            
            // 注意：不再使用Destroy来自动销毁，而是使用对象池的自动回收
            if (pooledObject != null && lifetime > 0)
            {
                pooledObject.StartAutoRelease(lifetime);
            }
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
            
            // 应用伤害逻辑（由子类实现具体细节）
            ApplyDamage(hitObject);
            
            // 生成击中特效
            SpawnHitEffect(hitPoint);
            
            // 回收子弹（而不是销毁）
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
        
        /// <summary>
        /// 生成击中特效
        /// </summary>
        /// <param name="hitPoint">击中点</param>
        protected override void SpawnHitEffect(Vector3 hitPoint)
        {
            if (hitEffectPrefab != null)
            {
                // 使用对象池获取特效
                GameObject effect = ObjectPoolManager.Instance.Get(hitEffectPrefab, hitPoint, Quaternion.LookRotation(direction));
                
                // 设置特效自动回收
                PooledObject effectPooled = effect.GetComponent<PooledObject>();
                if (effectPooled != null)
                {
                    effectPooled.StartAutoRelease(2f); // 2秒后自动回收特效
                }
            }
        }
        
        #region IPoolable接口实现
        
        /// <summary>
        /// 对象从池中获取时调用
        /// </summary>
        public virtual void OnPoolGet()
        {
            // 重置状态
            isInitialized = false;
            currentPierceCount = 0;
            
            // 启用碰撞器
            Collider bulletCollider = GetComponent<Collider>();
            if (bulletCollider != null)
            {
                bulletCollider.enabled = true;
            }
        }
        
        /// <summary>
        /// 对象释放回池时调用
        /// </summary>
        public virtual void OnPoolRelease()
        {
            // 重置状态
            isInitialized = false;
            shooter = null;
            direction = Vector3.zero;
            
            // 禁用碰撞器
            Collider bulletCollider = GetComponent<Collider>();
            if (bulletCollider != null)
            {
                bulletCollider.enabled = false;
            }
            
            // 重置伤害值
            baseDamage = 10f;
            damageType = DamageType.Physical;
        }
        
        #endregion
    }
} 