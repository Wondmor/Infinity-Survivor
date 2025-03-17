using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 对象池更新器 - 在Unity生命周期中更新对象池管理器
    /// </summary>
    public class ObjectPoolUpdater : MonoBehaviour
    {
        // 单例实例
        private static ObjectPoolUpdater _instance;
        
        private void Awake()
        {
            // 确保只有一个实例
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 初始化对象池管理器
            var poolManager = ObjectPoolManager.Instance;
            Debug.Log("[ObjectPoolUpdater] 对象池更新器已初始化");
        }
        
        private void FixedUpdate()
        {
            // 在物理更新周期中更新对象池管理器
            ObjectPoolManager.Instance.UpdateStats();
        }
    }
} 