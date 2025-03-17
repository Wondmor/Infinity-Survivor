using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 对象池管理器 - 负责管理所有对象池
    /// 实现了三级缓存架构和智能容量控制
    /// </summary>
    public class ObjectPoolManager : BaseManager<ObjectPoolManager>
    {
        #region 内部类和枚举

        /// <summary>
        /// 缓存层级枚举
        /// </summary>
        public enum CacheLevel
        {
            L1, // 场景动态对象（子弹等），随场景卸载销毁
            L2, // 全局共享对象（UI等），游戏进程生命周期
            L3  // 大型资源（BOSS模型），按需加载/卸载
        }

        /// <summary>
        /// 对象池配置
        /// </summary>
        [Serializable]
        public class PoolConfig
        {
            public string PoolId;                // 池标识符
            public GameObject Prefab;            // 预制体
            public int InitialSize = 10;         // 初始大小
            public int MaxSize = 100;            // 最大大小
            public float ExpansionRate = 0.2f;   // 扩容比率
            public float FragmentationThreshold = 0.15f; // 碎片整理阈值
            public float MaxLifetime = 5f;       // 最大存活时间
            public CacheLevel Level = CacheLevel.L1; // 缓存层级
            public bool PrewarmOnLoad = true;    // 是否在加载时预热
        }

        #endregion

        #region 字段和属性

        // 对象池字典 - 按池ID索引
        private readonly Dictionary<string, ObjectPool> _pools = new Dictionary<string, ObjectPool>();
        
        // 对象池字典 - 按预制体索引
        private readonly Dictionary<GameObject, ObjectPool> _prefabPools = new Dictionary<GameObject, ObjectPool>();
        
        // 缓存层级字典 - 按层级索引池列表
        private readonly Dictionary<CacheLevel, List<ObjectPool>> _levelPools = new Dictionary<CacheLevel, List<ObjectPool>>
        {
            { CacheLevel.L1, new List<ObjectPool>() },
            { CacheLevel.L2, new List<ObjectPool>() },
            { CacheLevel.L3, new List<ObjectPool>() }
        };

        // 性能监控数据
        private readonly Queue<int> _recentPeakRequests = new Queue<int>(10); // 最近10次请求峰值
        private int _currentFrameRequests = 0;                               // 当前帧请求数
        private int _maxFrameRequests = 0;                                   // 最大帧请求数
        private float _expansionSensitivity = 0.7f;                          // 扩容敏感度
        private float _fragmentationThreshold = 0.15f;                       // 碎片整理阈值
        private bool _isInitialized = false;                                 // 是否已初始化

        // 全局配置
        public float DefaultExpansionRate { get; set; } = 0.2f;              // 默认扩容比率
        public float DefaultFragmentationThreshold { get; set; } = 0.15f;    // 默认碎片整理阈值
        public float DefaultMaxLifetime { get; set; } = 5f;                  // 默认最大存活时间

        #endregion

        #region 构造函数和初始化

        /// <summary>
        /// 私有构造函数，确保单例模式
        /// </summary>
        private ObjectPoolManager()
        {
            // 初始化
            Initialize();
        }

        /// <summary>
        /// 初始化对象池管理器
        /// </summary>
        private void Initialize()
        {
            if (_isInitialized)
                return;

            Debug.Log("[ObjectPoolManager] 初始化对象池管理器");
            
            // 创建根游戏对象
            GameObject rootObj = new GameObject("ObjectPools");
            GameObject.DontDestroyOnLoad(rootObj);
            
            // 为每个缓存层级创建容器
            foreach (CacheLevel level in Enum.GetValues(typeof(CacheLevel)))
            {
                GameObject levelObj = new GameObject(level.ToString());
                levelObj.transform.SetParent(rootObj.transform);
            }
            
            // 添加场景管理组件
            GameObject sceneManagerObj = new GameObject("PoolSceneManager");
            sceneManagerObj.transform.SetParent(rootObj.transform);
            PoolSceneManager sceneManager = sceneManagerObj.AddComponent<PoolSceneManager>();
            sceneManager.Initialize(this);
            
            _isInitialized = true;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <param name="config">池配置</param>
        /// <returns>创建的对象池</returns>
        public ObjectPool CreatePool(PoolConfig config)
        {
            if (config == null || config.Prefab == null)
            {
                Debug.LogError("[ObjectPoolManager] 创建对象池失败：配置或预制体为空");
                return null;
            }

            // 检查池是否已存在
            if (_pools.ContainsKey(config.PoolId))
            {
                Debug.LogWarning($"[ObjectPoolManager] 对象池 '{config.PoolId}' 已存在，返回现有池");
                return _pools[config.PoolId];
            }

            // 创建新池
            ObjectPool pool = new ObjectPool(config);
            
            // 注册池
            _pools[config.PoolId] = pool;
            _prefabPools[config.Prefab] = pool;
            _levelPools[config.Level].Add(pool);
            
            // 预热池
            if (config.PrewarmOnLoad)
            {
                pool.Prewarm();
            }
            
            Debug.Log($"[ObjectPoolManager] 创建对象池：{config.PoolId}，初始大小：{config.InitialSize}，最大大小：{config.MaxSize}");
            
            return pool;
        }

        /// <summary>
        /// 获取对象池（按ID）
        /// </summary>
        /// <param name="poolId">池ID</param>
        /// <returns>对象池</returns>
        public ObjectPool GetPool(string poolId)
        {
            if (_pools.TryGetValue(poolId, out ObjectPool pool))
            {
                return pool;
            }
            
            Debug.LogWarning($"[ObjectPoolManager] 未找到对象池：{poolId}");
            return null;
        }

        /// <summary>
        /// 获取对象池（按预制体）
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <returns>对象池</returns>
        public ObjectPool GetPool(GameObject prefab)
        {
            if (_prefabPools.TryGetValue(prefab, out ObjectPool pool))
            {
                return pool;
            }
            
            Debug.LogWarning($"[ObjectPoolManager] 未找到预制体的对象池：{prefab.name}");
            return null;
        }

        /// <summary>
        /// 获取或创建对象池
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="initialSize">初始大小</param>
        /// <param name="maxSize">最大大小</param>
        /// <param name="level">缓存层级</param>
        /// <returns>对象池</returns>
        public ObjectPool GetOrCreatePool(GameObject prefab, int initialSize = 10, int maxSize = 100, CacheLevel level = CacheLevel.L1)
        {
            // 尝试获取现有池
            if (_prefabPools.TryGetValue(prefab, out ObjectPool existingPool))
            {
                return existingPool;
            }
            
            // 创建新池配置
            PoolConfig config = new PoolConfig
            {
                PoolId = $"Pool_{prefab.name}_{Guid.NewGuid().ToString().Substring(0, 8)}",
                Prefab = prefab,
                InitialSize = initialSize,
                MaxSize = maxSize,
                ExpansionRate = DefaultExpansionRate,
                FragmentationThreshold = DefaultFragmentationThreshold,
                MaxLifetime = DefaultMaxLifetime,
                Level = level,
                PrewarmOnLoad = true
            };
            
            // 创建并返回新池
            return CreatePool(config);
        }

        /// <summary>
        /// 从池中获取对象
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <returns>池化对象</returns>
        public GameObject Get(GameObject prefab, Vector3 position = default, Quaternion rotation = default)
        {
            _currentFrameRequests++;
            
            // 获取或创建池
            ObjectPool pool = GetOrCreatePool(prefab);
            
            // 从池中获取对象
            return pool.Get(position, rotation);
        }

        /// <summary>
        /// 从池中获取对象（按池ID）
        /// </summary>
        /// <param name="poolId">池ID</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <returns>池化对象</returns>
        public GameObject Get(string poolId, Vector3 position = default, Quaternion rotation = default)
        {
            _currentFrameRequests++;
            
            // 获取池
            ObjectPool pool = GetPool(poolId);
            if (pool == null)
            {
                Debug.LogError($"[ObjectPoolManager] 获取对象失败：未找到对象池 '{poolId}'");
                return null;
            }
            
            // 从池中获取对象
            return pool.Get(position, rotation);
        }

        /// <summary>
        /// 释放对象回池
        /// </summary>
        /// <param name="obj">要释放的对象</param>
        public void Release(GameObject obj)
        {
            if (obj == null)
                return;
                
            // 获取池化组件
            PooledObject pooledObj = obj.GetComponent<PooledObject>();
            if (pooledObj == null)
            {
                Debug.LogWarning($"[ObjectPoolManager] 释放对象失败：对象 '{obj.name}' 不是池化对象");
                GameObject.Destroy(obj);
                return;
            }
            
            // 释放回池
            pooledObj.Pool.Release(obj);
        }

        /// <summary>
        /// 清空指定层级的所有池
        /// </summary>
        /// <param name="level">缓存层级</param>
        public void ClearLevel(CacheLevel level)
        {
            foreach (ObjectPool pool in _levelPools[level])
            {
                pool.Clear();
            }
            
            Debug.Log($"[ObjectPoolManager] 已清空 {level} 层级的所有对象池");
        }

        /// <summary>
        /// 清空所有对象池
        /// </summary>
        public void ClearAll()
        {
            foreach (ObjectPool pool in _pools.Values)
            {
                pool.Clear();
            }
            
            Debug.Log("[ObjectPoolManager] 已清空所有对象池");
        }

        /// <summary>
        /// 预热指定层级的所有池
        /// </summary>
        /// <param name="level">缓存层级</param>
        public void PrewarmLevel(CacheLevel level)
        {
            foreach (ObjectPool pool in _levelPools[level])
            {
                pool.Prewarm();
            }
            
            Debug.Log($"[ObjectPoolManager] 已预热 {level} 层级的所有对象池");
        }

        /// <summary>
        /// 场景加载处理
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public void HandleSceneLoaded(string sceneName)
        {
            // 预热L1层级的池
            PrewarmLevel(CacheLevel.L1);
            
            Debug.Log($"[ObjectPoolManager] 场景 '{sceneName}' 已加载，已预热L1层级对象池");
        }

        /// <summary>
        /// 场景卸载处理
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public void HandleSceneUnloaded(string sceneName)
        {
            // 清空L1层级的池
            ClearLevel(CacheLevel.L1);
            
            Debug.Log($"[ObjectPoolManager] 场景 '{sceneName}' 已卸载，已清空L1层级对象池");
        }

        /// <summary>
        /// 更新性能监控数据
        /// </summary>
        public void UpdateStats()
        {
            // 更新请求峰值
            if (_currentFrameRequests > _maxFrameRequests)
            {
                _maxFrameRequests = _currentFrameRequests;
            }
            
            // 每10帧记录一次峰值
            if (Time.frameCount % 10 == 0)
            {
                // 保持队列大小为10
                if (_recentPeakRequests.Count >= 10)
                {
                    _recentPeakRequests.Dequeue();
                }
                
                _recentPeakRequests.Enqueue(_maxFrameRequests);
                _maxFrameRequests = 0;
            }
            
            // 重置当前帧请求数
            _currentFrameRequests = 0;
            
            // 检查是否需要扩容
            CheckExpansionNeeds();
            
            // 检查是否需要碎片整理
            CheckFragmentation();
        }

        /// <summary>
        /// 检查是否需要扩容
        /// </summary>
        private void CheckExpansionNeeds()
        {
            // 计算平均峰值
            int sum = 0;
            foreach (int peak in _recentPeakRequests)
            {
                sum += peak;
            }
            
            if (_recentPeakRequests.Count == 0)
                return;
                
            float avgPeak = (float)sum / _recentPeakRequests.Count;
            
            // 检查每个池是否需要扩容
            foreach (ObjectPool pool in _pools.Values)
            {
                // 如果空闲率低于阈值，触发扩容
                float idleRate = pool.GetIdleRate();
                if (idleRate < (1 - _expansionSensitivity))
                {
                    pool.Expand();
                }
            }
        }

        /// <summary>
        /// 检查是否需要碎片整理
        /// </summary>
        private void CheckFragmentation()
        {
            foreach (ObjectPool pool in _pools.Values)
            {
                // 如果碎片率高于阈值，触发整理
                float fragmentationRate = pool.GetFragmentationRate();
                if (fragmentationRate > _fragmentationThreshold)
                {
                    pool.Defragment();
                }
            }
        }

        #endregion
    }
    
    /// <summary>
    /// 对象池场景管理器 - 处理场景加载和卸载事件
    /// </summary>
    public class PoolSceneManager : MonoBehaviour
    {
        private ObjectPoolManager _poolManager;
        private string _currentSceneName;
        private int _currentLevelIndex;
        
        public void Initialize(ObjectPoolManager poolManager)
        {
            _poolManager = poolManager;
            
            // 获取当前场景名称
            _currentSceneName = Application.loadedLevelName;
            _currentLevelIndex = Application.loadedLevel;
            
            // 处理当前场景
            _poolManager.HandleSceneLoaded(_currentSceneName);
        }
        
        private void Update()
        {
            // 检查场景是否已更改
            string sceneName = Application.loadedLevelName;
            int levelIndex = Application.loadedLevel;
            
            if (sceneName != _currentSceneName || levelIndex != _currentLevelIndex)
            {
                // 处理场景卸载
                _poolManager.HandleSceneUnloaded(_currentSceneName);
                
                // 更新当前场景
                _currentSceneName = sceneName;
                _currentLevelIndex = levelIndex;
                
                // 处理场景加载
                _poolManager.HandleSceneLoaded(_currentSceneName);
            }
        }
    }
} 