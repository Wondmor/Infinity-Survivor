using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 对象池 - 管理特定预制体的对象实例
    /// </summary>
    public class ObjectPool
    {
        #region 字段和属性

        // 池配置
        private readonly ObjectPoolManager.PoolConfig _config;
        
        // 对象容器
        private readonly Transform _container;
        
        // 空闲对象栈
        private readonly Stack<GameObject> _inactive = new Stack<GameObject>();
        
        // 活跃对象列表
        private readonly List<GameObject> _active = new List<GameObject>();
        
        // 统计数据
        private int _totalCreated = 0;
        private int _peakActive = 0;
        private int _expansionCount = 0;
        private int _fragmentationCount = 0;
        
        // 属性
        public string PoolId => _config.PoolId;
        public GameObject Prefab => _config.Prefab;
        public int Size => _inactive.Count + _active.Count;
        public int ActiveCount => _active.Count;
        public int InactiveCount => _inactive.Count;
        public ObjectPoolManager.CacheLevel Level => _config.Level;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config">池配置</param>
        public ObjectPool(ObjectPoolManager.PoolConfig config)
        {
            _config = config;
            
            // 创建容器
            GameObject containerObj = new GameObject($"Pool_{config.Prefab.name}");
            
            // 根据缓存层级设置父对象
            GameObject parent = GameObject.Find($"ObjectPools/{config.Level}");
            if (parent != null)
            {
                containerObj.transform.SetParent(parent.transform);
            }
            else
            {
                if (config.Level == ObjectPoolManager.CacheLevel.L2)
                {
                    GameObject.DontDestroyOnLoad(containerObj);
                }
            }
            
            _container = containerObj.transform;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 预热对象池 - 预先创建对象
        /// </summary>
        public void Prewarm()
        {
            // 计算需要创建的对象数量
            int createCount = _config.InitialSize - Size;
            
            if (createCount <= 0)
                return;
                
            // 创建对象
            for (int i = 0; i < createCount; i++)
            {
                CreateObject();
            }
            
            Debug.Log($"[ObjectPool] 预热对象池 '{PoolId}'：创建 {createCount} 个对象");
        }

        /// <summary>
        /// 从池中获取对象
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <returns>池化对象</returns>
        public GameObject Get(Vector3 position = default, Quaternion rotation = default)
        {
            GameObject obj;
            
            // 如果没有空闲对象，尝试创建新对象
            if (_inactive.Count == 0)
            {
                // 如果达到最大大小，尝试扩容
                if (Size >= _config.MaxSize)
                {
                    Debug.LogWarning($"[ObjectPool] 对象池 '{PoolId}' 已达到最大大小 {_config.MaxSize}，无法创建更多对象");
                    
                    // 如果没有可用对象，返回null
                    if (_inactive.Count == 0)
                    {
                        Debug.LogError($"[ObjectPool] 对象池 '{PoolId}' 没有可用对象");
                        return null;
                    }
                }
                else
                {
                    // 扩容
                    Expand();
                }
            }
            
            // 从栈中弹出对象
            obj = _inactive.Pop();
            
            // 如果对象为null（可能被意外销毁），创建新对象
            if (obj == null)
            {
                obj = CreateObject();
                
                // 如果创建失败，返回null
                if (obj == null)
                {
                    Debug.LogError($"[ObjectPool] 对象池 '{PoolId}' 创建对象失败");
                    return null;
                }
                
                // 再次从栈中弹出
                _inactive.Pop();
            }
            
            // 设置对象位置和旋转
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            
            // 激活对象
            obj.SetActive(true);
            
            // 添加到活跃列表
            _active.Add(obj);
            
            // 更新峰值
            if (_active.Count > _peakActive)
            {
                _peakActive = _active.Count;
            }
            
            // 获取池化组件
            PooledObject pooledObj = obj.GetComponent<PooledObject>();
            if (pooledObj != null)
            {
                // 调用对象的OnGet方法
                pooledObj.OnGet();
                
                // 设置自动回收
                if (_config.MaxLifetime > 0)
                {
                    pooledObj.StartAutoRelease(_config.MaxLifetime);
                }
            }
            
            return obj;
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
            if (pooledObj != null)
            {
                // 调用对象的OnRelease方法
                pooledObj.OnRelease();
                
                // 停止自动回收
                pooledObj.StopAutoRelease();
            }
            
            // 重置对象状态
            ResetObject(obj);
            
            // 从活跃列表中移除
            _active.Remove(obj);
            
            // 添加到空闲栈
            _inactive.Push(obj);
        }

        /// <summary>
        /// 扩容对象池
        /// </summary>
        public void Expand()
        {
            // 计算扩容数量
            int expandSize = Mathf.Max(1, Mathf.FloorToInt(Size * _config.ExpansionRate));
            
            // 限制最大大小
            expandSize = Mathf.Min(expandSize, _config.MaxSize - Size);
            
            if (expandSize <= 0)
                return;
                
            // 创建新对象
            for (int i = 0; i < expandSize; i++)
            {
                CreateObject();
            }
            
            _expansionCount++;
            
            Debug.Log($"[ObjectPool] 扩容对象池 '{PoolId}'：创建 {expandSize} 个对象，当前大小 {Size}/{_config.MaxSize}");
        }

        /// <summary>
        /// 碎片整理
        /// </summary>
        public void Defragment()
        {
            // 记录开始时间
            float startTime = Time.realtimeSinceStartup;
            
            // 整理空闲对象
            GameObject[] inactiveArray = _inactive.ToArray();
            _inactive.Clear();
            
            foreach (GameObject obj in inactiveArray)
            {
                if (obj != null)
                {
                    _inactive.Push(obj);
                }
            }
            
            // 整理活跃对象
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                if (_active[i] == null)
                {
                    _active.RemoveAt(i);
                }
            }
            
            _fragmentationCount++;
            
            // 计算耗时
            float elapsedTime = (Time.realtimeSinceStartup - startTime) * 1000f;
            
            Debug.Log($"[ObjectPool] 整理对象池 '{PoolId}'：耗时 {elapsedTime:F2}ms，当前大小 {Size}/{_config.MaxSize}");
        }

        /// <summary>
        /// 清空对象池
        /// </summary>
        public void Clear()
        {
            // 销毁所有空闲对象
            while (_inactive.Count > 0)
            {
                GameObject obj = _inactive.Pop();
                if (obj != null)
                {
                    GameObject.Destroy(obj);
                }
            }
            
            // 清空活跃列表（不销毁活跃对象）
            _active.Clear();
            
            // 重置统计数据
            _totalCreated = 0;
            _peakActive = 0;
            _expansionCount = 0;
            _fragmentationCount = 0;
            
            Debug.Log($"[ObjectPool] 清空对象池 '{PoolId}'");
        }

        /// <summary>
        /// 获取空闲率
        /// </summary>
        /// <returns>空闲率（0-1）</returns>
        public float GetIdleRate()
        {
            if (Size == 0)
                return 1f;
                
            return (float)_inactive.Count / Size;
        }

        /// <summary>
        /// 获取碎片率
        /// </summary>
        /// <returns>碎片率（0-1）</returns>
        public float GetFragmentationRate()
        {
            int nullCount = 0;
            
            // 检查空闲对象
            foreach (GameObject obj in _inactive)
            {
                if (obj == null)
                {
                    nullCount++;
                }
            }
            
            // 检查活跃对象
            foreach (GameObject obj in _active)
            {
                if (obj == null)
                {
                    nullCount++;
                }
            }
            
            if (Size == 0)
                return 0f;
                
            return (float)nullCount / Size;
        }

        #endregion

        #region 内部方法

        /// <summary>
        /// 创建新对象
        /// </summary>
        /// <returns>创建的对象</returns>
        private GameObject CreateObject()
        {
            // 检查是否达到最大大小
            if (Size >= _config.MaxSize)
            {
                Debug.LogWarning($"[ObjectPool] 对象池 '{PoolId}' 已达到最大大小 {_config.MaxSize}，无法创建更多对象");
                return null;
            }
            
            // 实例化对象
            GameObject obj = GameObject.Instantiate(_config.Prefab, _container);
            
            // 设置对象名称
            obj.name = $"{_config.Prefab.name}_{_totalCreated}";
            
            // 添加池化组件
            PooledObject pooledObj = obj.GetComponent<PooledObject>();
            if (pooledObj == null)
            {
                pooledObj = obj.AddComponent<PooledObject>();
            }
            
            // 设置池引用
            pooledObj.Pool = this;
            
            // 重置对象状态
            ResetObject(obj);
            
            // 添加到空闲栈
            _inactive.Push(obj);
            
            _totalCreated++;
            
            return obj;
        }

        /// <summary>
        /// 重置对象状态
        /// </summary>
        /// <param name="obj">要重置的对象</param>
        private void ResetObject(GameObject obj)
        {
            // 禁用对象
            obj.SetActive(false);
            
            // 重置变换
            obj.transform.SetParent(_container);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            
            // 重置刚体
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            // 重置刚体2D
            Rigidbody2D rb2d = obj.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                rb2d.velocity = Vector2.zero;
                rb2d.angularVelocity = 0f;
            }
        }

        #endregion
    }
} 