using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 可池化子弹 - 实现IPoolable接口的子弹
    /// </summary>
    public class PoolableBullet : MonoBehaviour, IPoolable
    {
        [Header("子弹设置")]
        [SerializeField] private float speed = 20f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private GameObject hitEffectPrefab;
        
        [Header("特效设置")]
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private ParticleSystem particleSystem;
        
        // 子弹方向
        private Vector3 _direction;
        
        // 是否已初始化
        private bool _isInitialized = false;
        
        private void Update()
        {
            if (!_isInitialized)
                return;
                
            // 移动子弹
            transform.position += _direction * speed * Time.deltaTime;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // 处理碰撞
            HandleHit(other.gameObject, other.ClosestPoint(transform.position));
        }
        
        /// <summary>
        /// 初始化子弹
        /// </summary>
        /// <param name="direction">方向</param>
        /// <param name="bulletSpeed">速度</param>
        public void Initialize(Vector3 direction, float bulletSpeed)
        {
            _direction = direction.normalized;
            speed = bulletSpeed;
            _isInitialized = true;
            
            // 设置子弹朝向
            transform.rotation = Quaternion.LookRotation(_direction);
            
            // 启动粒子系统
            if (particleSystem != null)
            {
                particleSystem.Play();
            }
            
            Debug.Log($"[PoolableBullet] 初始化子弹：方向={_direction}，速度={speed}");
        }
        
        /// <summary>
        /// 处理击中逻辑
        /// </summary>
        /// <param name="hitObject">被击中的对象</param>
        /// <param name="hitPoint">击中点</param>
        private void HandleHit(GameObject hitObject, Vector3 hitPoint)
        {
            // 生成击中特效
            if (hitEffectPrefab != null)
            {
                GameObject hitEffect = ObjectPoolManager.Instance.Get(hitEffectPrefab, hitPoint, Quaternion.LookRotation(_direction));
                
                // 5秒后自动回收特效
                PooledObject pooledEffect = hitEffect.GetComponent<PooledObject>();
                if (pooledEffect != null)
                {
                    pooledEffect.StartAutoRelease(5f);
                }
            }
            
            Debug.Log($"[PoolableBullet] 子弹击中 {hitObject.name}，位置={hitPoint}");
            
            // 回收子弹
            PooledObject pooledObj = GetComponent<PooledObject>();
            if (pooledObj != null)
            {
                pooledObj.Release();
            }
        }
        
        #region IPoolable 接口实现
        
        /// <summary>
        /// 对象从池中获取时调用
        /// </summary>
        public void OnPoolGet()
        {
            // 重置状态
            _isInitialized = false;
            
            // 启用拖尾渲染器
            if (trailRenderer != null)
            {
                trailRenderer.enabled = true;
            }
            
            Debug.Log("[PoolableBullet] 子弹从对象池获取");
        }
        
        /// <summary>
        /// 对象释放回池时调用
        /// </summary>
        public void OnPoolRelease()
        {
            // 重置状态
            _isInitialized = false;
            
            // 禁用拖尾渲染器
            if (trailRenderer != null)
            {
                trailRenderer.enabled = false;
            }
            
            // 停止粒子系统
            if (particleSystem != null)
            {
                particleSystem.Stop();
                particleSystem.Clear();
            }
            
            Debug.Log("[PoolableBullet] 子弹释放回对象池");
        }
        
        #endregion
    }
} 