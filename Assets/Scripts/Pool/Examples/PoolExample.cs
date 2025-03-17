using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 对象池示例 - 展示如何使用对象池
    /// </summary>
    public class PoolExample : MonoBehaviour
    {
        [Header("对象池配置")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private int initialPoolSize = 20;
        [SerializeField] private int maxPoolSize = 100;
        [SerializeField] private ObjectPoolManager.CacheLevel cacheLevel = ObjectPoolManager.CacheLevel.L1;
        
        [Header("发射配置")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.1f;
        [SerializeField] private float bulletSpeed = 20f;
        [SerializeField] private float bulletLifetime = 5f;
        [SerializeField] private bool autoFire = false;
        
        // 对象池引用
        private ObjectPool _bulletPool;
        
        // 发射计时器
        private float _fireTimer = 0f;
        
        private void Start()
        {
            // 确保对象池更新器存在
            if (FindObjectOfType<ObjectPoolUpdater>() == null)
            {
                GameObject updaterObj = new GameObject("ObjectPoolUpdater");
                updaterObj.AddComponent<ObjectPoolUpdater>();
            }
            
            // 创建对象池
            ObjectPoolManager.PoolConfig config = new ObjectPoolManager.PoolConfig
            {
                PoolId = "BulletPool",
                Prefab = bulletPrefab,
                InitialSize = initialPoolSize,
                MaxSize = maxPoolSize,
                ExpansionRate = 0.2f,
                FragmentationThreshold = 0.15f,
                MaxLifetime = bulletLifetime,
                Level = cacheLevel,
                PrewarmOnLoad = true
            };
            
            _bulletPool = ObjectPoolManager.Instance.CreatePool(config);
            
            Debug.Log($"[PoolExample] 创建子弹对象池：初始大小={initialPoolSize}，最大大小={maxPoolSize}");
        }
        
        private void Update()
        {
            // 更新发射计时器
            _fireTimer += Time.deltaTime;
            
            // 自动发射或按下空格键发射
            if ((autoFire || Input.GetKeyDown(KeyCode.Space)) && _fireTimer >= fireRate)
            {
                FireBullet();
                _fireTimer = 0f;
            }
        }
        
        /// <summary>
        /// 发射子弹
        /// </summary>
        private void FireBullet()
        {
            if (_bulletPool == null || firePoint == null)
                return;
                
            // 从池中获取子弹
            GameObject bullet = _bulletPool.Get(firePoint.position, firePoint.rotation);
            
            if (bullet == null)
                return;
                
            // 设置子弹速度
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = firePoint.forward * bulletSpeed;
            }
            
            Debug.Log($"[PoolExample] 发射子弹：位置={firePoint.position}，方向={firePoint.forward}，速度={bulletSpeed}");
        }
        
        /// <summary>
        /// 清空对象池
        /// </summary>
        public void ClearPool()
        {
            if (_bulletPool != null)
            {
                _bulletPool.Clear();
                Debug.Log("[PoolExample] 清空子弹对象池");
            }
        }
        
        /// <summary>
        /// 显示对象池统计信息
        /// </summary>
        public void ShowPoolStats()
        {
            if (_bulletPool != null)
            {
                Debug.Log($"[PoolExample] 子弹对象池统计：总大小={_bulletPool.Size}，活跃={_bulletPool.ActiveCount}，空闲={_bulletPool.InactiveCount}，空闲率={_bulletPool.GetIdleRate():P2}");
            }
        }
    }
} 