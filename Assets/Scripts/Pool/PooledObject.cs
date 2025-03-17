using System.Collections;
using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 池化对象组件 - 附加到池化对象上，管理对象的生命周期
    /// </summary>
    public class PooledObject : MonoBehaviour
    {
        #region 字段和属性

        // 对象池引用
        public ObjectPool Pool { get; set; }
        
        // 自动回收协程
        private Coroutine _autoReleaseCoroutine;
        
        // 是否已经初始化
        private bool _isInitialized = false;
        
        // 对象创建时间
        private float _createTime;
        
        // 对象最后使用时间
        private float _lastUseTime;
        
        // 可池化组件缓存
        private IPoolable[] _poolables;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            // 记录创建时间
            _createTime = Time.time;
            _lastUseTime = _createTime;
            
            // 缓存可池化组件
            _poolables = GetComponentsInChildren<IPoolable>(true);
            
            _isInitialized = true;
        }

        private void OnEnable()
        {
            // 更新最后使用时间
            _lastUseTime = Time.time;
        }

        private void OnDisable()
        {
            // 停止自动回收
            StopAutoRelease();
        }

        private void OnDestroy()
        {
            // 停止自动回收
            StopAutoRelease();
        }

        private void OnBecameInvisible()
        {
            // 如果对象离开摄像机视野，延迟回收
            if (gameObject.activeInHierarchy && Pool != null)
            {
                // 延迟200ms后回收
                StartCoroutine(DelayedReleaseCoroutine(0.2f));
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 对象从池中获取时调用
        /// </summary>
        public virtual void OnGet()
        {
            // 更新最后使用时间
            _lastUseTime = Time.time;
            
            // 重置组件状态
            ResetComponents();
            
            // 通知可池化组件
            if (_poolables != null)
            {
                foreach (IPoolable poolable in _poolables)
                {
                    poolable.OnPoolGet();
                }
            }
        }

        /// <summary>
        /// 对象释放回池时调用
        /// </summary>
        public virtual void OnRelease()
        {
            // 停止所有协程
            StopAllCoroutines();
            
            // 通知可池化组件
            if (_poolables != null)
            {
                foreach (IPoolable poolable in _poolables)
                {
                    poolable.OnPoolRelease();
                }
            }
            
            // 重置组件状态
            ResetComponents();
        }

        /// <summary>
        /// 开始自动回收
        /// </summary>
        /// <param name="lifetime">生命周期（秒）</param>
        public void StartAutoRelease(float lifetime)
        {
            // 停止现有协程
            StopAutoRelease();
            
            // 启动新协程
            if (lifetime > 0)
            {
                _autoReleaseCoroutine = StartCoroutine(AutoReleaseCoroutine(lifetime));
            }
        }

        /// <summary>
        /// 停止自动回收
        /// </summary>
        public void StopAutoRelease()
        {
            if (_autoReleaseCoroutine != null)
            {
                StopCoroutine(_autoReleaseCoroutine);
                _autoReleaseCoroutine = null;
            }
        }

        /// <summary>
        /// 手动释放对象回池
        /// </summary>
        public void Release()
        {
            if (Pool != null)
            {
                Pool.Release(gameObject);
            }
            else
            {
                Debug.LogWarning($"[PooledObject] 对象 '{gameObject.name}' 没有关联的对象池，直接销毁");
                Destroy(gameObject);
            }
        }

        #endregion

        #region 内部方法

        /// <summary>
        /// 自动回收协程
        /// </summary>
        /// <param name="lifetime">生命周期（秒）</param>
        private IEnumerator AutoReleaseCoroutine(float lifetime)
        {
            // 等待指定时间
            yield return new WaitForSeconds(lifetime);
            
            // 回收对象
            Release();
        }

        /// <summary>
        /// 延迟回收协程
        /// </summary>
        /// <param name="delay">延迟时间（秒）</param>
        private IEnumerator DelayedReleaseCoroutine(float delay)
        {
            // 等待指定时间
            yield return new WaitForSeconds(delay);
            
            // 如果对象仍然不可见，回收对象
            if (!IsVisibleToCamera() && gameObject.activeInHierarchy)
            {
                Release();
            }
        }

        /// <summary>
        /// 检查对象是否对任何摄像机可见
        /// </summary>
        /// <returns>是否可见</returns>
        private bool IsVisibleToCamera()
        {
            // 获取所有渲染器
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            
            // 检查每个渲染器是否可见
            foreach (Renderer renderer in renderers)
            {
                if (renderer.isVisible)
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// 重置组件状态
        /// </summary>
        private void ResetComponents()
        {
            // 重置粒子系统
            ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                ps.Stop();
                ps.Clear();
            }
            
            // 重置音频源
            AudioSource[] audioSources = GetComponentsInChildren<AudioSource>();
            foreach (AudioSource audioSource in audioSources)
            {
                audioSource.Stop();
                audioSource.time = 0f;
            }
            
            // 重置动画器
            Animator[] animators = GetComponentsInChildren<Animator>();
            foreach (Animator animator in animators)
            {
                animator.Rebind();
                animator.Update(0f);
            }
            
            // 重置拖尾渲染器
            TrailRenderer[] trailRenderers = GetComponentsInChildren<TrailRenderer>();
            foreach (TrailRenderer trailRenderer in trailRenderers)
            {
                trailRenderer.Clear();
            }
        }

        #endregion
    }
} 