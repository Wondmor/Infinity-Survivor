using UnityEngine;
using System.Collections.Generic;

namespace TrianCatStudio
{
    /// <summary>
    /// 子弹管理器 - 管理子弹对象池
    /// </summary>
    public class BulletManager : BaseManager<BulletManager>
    {
        #region 字段和属性
        
        // 子弹预制体字典
        private Dictionary<string, GameObject> _bulletPrefabs = new Dictionary<string, GameObject>();
        
        // 子弹对象池字典
        private Dictionary<string, ObjectPool> _bulletPools = new Dictionary<string, ObjectPool>();
        
        // 默认配置
        private int _defaultInitialSize = 20;
        private int _defaultMaxSize = 100;
        private float _defaultLifetime = 5f;
        private ObjectPoolManager.CacheLevel _defaultCacheLevel = ObjectPoolManager.CacheLevel.L1;
        
        #endregion
        
        #region 构造函数和初始化
        
        /// <summary>
        /// 私有构造函数，确保单例模式
        /// </summary>
        private BulletManager()
        {
            // 初始化
            Initialize();
        }
        
        /// <summary>
        /// 初始化子弹管理器
        /// </summary>
        private void Initialize()
        {
            Debug.Log("[BulletManager] 初始化子弹管理器");
            
            // 确保对象池管理器已初始化
            var poolManager = ObjectPoolManager.Instance;
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 注册子弹预制体
        /// </summary>
        /// <param name="bulletId">子弹ID</param>
        /// <param name="prefab">子弹预制体</param>
        /// <param name="initialSize">初始池大小</param>
        /// <param name="maxSize">最大池大小</param>
        /// <param name="lifetime">子弹生命周期</param>
        /// <param name="cacheLevel">缓存层级</param>
        /// <returns>是否注册成功</returns>
        public bool RegisterBullet(string bulletId, GameObject prefab, int initialSize = 0, int maxSize = 0, float lifetime = 0, ObjectPoolManager.CacheLevel cacheLevel = ObjectPoolManager.CacheLevel.L1)
        {
            if (string.IsNullOrEmpty(bulletId) || prefab == null)
            {
                Debug.LogError("[BulletManager] 注册子弹失败：ID或预制体为空");
                return false;
            }
            
            // 检查是否已注册
            if (_bulletPrefabs.ContainsKey(bulletId))
            {
                Debug.LogWarning($"[BulletManager] 子弹 '{bulletId}' 已注册，将被覆盖");
            }
            
            // 注册预制体
            _bulletPrefabs[bulletId] = prefab;
            
            // 使用默认值
            if (initialSize <= 0) initialSize = _defaultInitialSize;
            if (maxSize <= 0) maxSize = _defaultMaxSize;
            if (lifetime <= 0) lifetime = _defaultLifetime;
            
            // 创建对象池配置
            ObjectPoolManager.PoolConfig config = new ObjectPoolManager.PoolConfig
            {
                PoolId = $"Bullet_{bulletId}",
                Prefab = prefab,
                InitialSize = initialSize,
                MaxSize = maxSize,
                ExpansionRate = 0.2f,
                FragmentationThreshold = 0.15f,
                MaxLifetime = lifetime,
                Level = cacheLevel,
                PrewarmOnLoad = true
            };
            
            // 创建对象池
            ObjectPool pool = ObjectPoolManager.Instance.CreatePool(config);
            
            // 注册池
            _bulletPools[bulletId] = pool;
            
            Debug.Log($"[BulletManager] 注册子弹 '{bulletId}'：初始大小={initialSize}，最大大小={maxSize}，生命周期={lifetime}秒");
            
            return true;
        }
        
        /// <summary>
        /// 发射子弹
        /// </summary>
        /// <param name="bulletId">子弹ID</param>
        /// <param name="position">发射位置</param>
        /// <param name="direction">发射方向</param>
        /// <param name="speed">子弹速度</param>
        /// <param name="shooter">发射者</param>
        /// <param name="damage">伤害值</param>
        /// <param name="damageType">伤害类型</param>
        /// <returns>发射的子弹对象</returns>
        public GameObject FireBullet(string bulletId, Vector3 position, Vector3 direction, float speed, GameObject shooter = null, float damage = 0, DamageType damageType = DamageType.Physical)
        {
            // 检查子弹是否已注册
            if (!_bulletPools.TryGetValue(bulletId, out ObjectPool pool))
            {
                // 尝试自动注册
                if (_bulletPrefabs.TryGetValue(bulletId, out GameObject prefab))
                {
                    RegisterBullet(bulletId, prefab);
                    pool = _bulletPools[bulletId];
                }
                else
                {
                    Debug.LogError($"[BulletManager] 发射子弹失败：未注册的子弹 '{bulletId}'");
                    return null;
                }
            }
            
            // 从池中获取子弹
            GameObject bullet = pool.Get(position, Quaternion.LookRotation(direction));
            
            if (bullet == null)
            {
                Debug.LogError($"[BulletManager] 发射子弹失败：无法从池中获取子弹 '{bulletId}'");
                return null;
            }
            
            // 初始化子弹
            BulletBase bulletBase = bullet.GetComponent<BulletBase>();
            if (bulletBase != null)
            {
                // 初始化子弹
                if (shooter != null)
                {
                    bulletBase.Initialize(direction, speed, shooter);
                }
                else
                {
                    bulletBase.Initialize(direction, speed);
                }
                
                // 设置伤害
                if (damage > 0)
                {
                    bulletBase.SetDamage(damage, damageType);
                }
                
                // 特殊处理玩家子弹
                PooledPlayerBullet playerBullet = bullet.GetComponent<PooledPlayerBullet>();
                if (playerBullet != null)
                {
                    // 可以在这里设置玩家子弹的特殊属性
                    // playerBullet.SetPierceProperties(true, 2, 0.2f);
                }
            }
            else
            {
                Debug.LogWarning($"[BulletManager] 子弹 '{bulletId}' 没有BulletBase组件");
                
                // 尝试使用刚体
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = direction.normalized * speed;
                }
            }
            
            Debug.Log($"[BulletManager] 发射子弹 '{bulletId}'：位置={position}，方向={direction}，速度={speed}");
            
            return bullet;
        }
        
        /// <summary>
        /// 预热子弹池
        /// </summary>
        /// <param name="bulletId">子弹ID</param>
        public void PrewarmBulletPool(string bulletId)
        {
            if (_bulletPools.TryGetValue(bulletId, out ObjectPool pool))
            {
                pool.Prewarm();
                Debug.Log($"[BulletManager] 预热子弹池 '{bulletId}'");
            }
            else
            {
                Debug.LogWarning($"[BulletManager] 预热子弹池失败：未注册的子弹 '{bulletId}'");
            }
        }
        
        /// <summary>
        /// 清空子弹池
        /// </summary>
        /// <param name="bulletId">子弹ID</param>
        public void ClearBulletPool(string bulletId)
        {
            if (_bulletPools.TryGetValue(bulletId, out ObjectPool pool))
            {
                pool.Clear();
                Debug.Log($"[BulletManager] 清空子弹池 '{bulletId}'");
            }
            else
            {
                Debug.LogWarning($"[BulletManager] 清空子弹池失败：未注册的子弹 '{bulletId}'");
            }
        }
        
        /// <summary>
        /// 清空所有子弹池
        /// </summary>
        public void ClearAllBulletPools()
        {
            foreach (var pool in _bulletPools.Values)
            {
                pool.Clear();
            }
            
            Debug.Log("[BulletManager] 清空所有子弹池");
        }
        
        /// <summary>
        /// 设置默认配置
        /// </summary>
        /// <param name="initialSize">默认初始池大小</param>
        /// <param name="maxSize">默认最大池大小</param>
        /// <param name="lifetime">默认子弹生命周期</param>
        /// <param name="cacheLevel">默认缓存层级</param>
        public void SetDefaultConfig(int initialSize, int maxSize, float lifetime, ObjectPoolManager.CacheLevel cacheLevel)
        {
            _defaultInitialSize = initialSize;
            _defaultMaxSize = maxSize;
            _defaultLifetime = lifetime;
            _defaultCacheLevel = cacheLevel;
            
            Debug.Log($"[BulletManager] 设置默认配置：初始大小={initialSize}，最大大小={maxSize}，生命周期={lifetime}秒，缓存层级={cacheLevel}");
        }
        
        #endregion
    }
} 