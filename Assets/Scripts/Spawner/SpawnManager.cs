using UnityEngine;
using System.Collections;

namespace TrianCatStudio
{
    /// <summary>
    /// 刷怪管理器 - 整合刷怪控制器和游戏系统的桥梁
    /// </summary>
    public class SpawnManager : SingletonAutoMono<SpawnManager>
    {
        [Header("初始设置")]
        [SerializeField] private bool activateOnStart = true;  // 是否在开始时就激活刷怪系统
        [SerializeField] private float initialDelay = 2f;      // 初始延迟时间
        
        [Header("调试选项")]
        [SerializeField] private bool debugMode = false;       // 调试模式
        [SerializeField] private bool spawnStats = true;       // 显示刷怪统计信息
        
        [Header("性能设置")]
        [SerializeField] private float updateInterval = 0.5f;  // 更新间隔
        [SerializeField] private int maxConcurrentEnemies = 50; // 最大同时存在敌人数量
        
        // 内部状态
        private bool isActive = false;
        private float gameTime = 0f;
        private float lastUpdateTime = 0f;
        
        // 玩家参考
        private Player player;
        
        // 事件系统
        public delegate void SpawnEvent(int enemyCount);
        public event SpawnEvent OnEnemyCountChanged;
        
        private void Start()
        {
            // 初始化
            Initialize();
            
            // 自动激活
            if (activateOnStart)
            {
                StartCoroutine(ActivateWithDelay(initialDelay));
            }
        }
        
        private void Initialize()
        {
            // 确保刷怪控制器已初始化
            SpawnController controller = SpawnController.Instance;
            
            // 获取玩家引用
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<Player>();
                
                // 设置初始玩家等级
                if (player != null)
                {
                    // 这里我们假设Player没有LevelUp事件，改为直接获取Level并设置
                    SpawnController.Instance.SetPlayerLevel(1); // 设置默认等级为1
                }
            }
            
            // 设置最大敌人数量
            controller.SetMaxEnemyCount(maxConcurrentEnemies);
            
            // 订阅事件
            controller.OnEnemySpawned += HandleEnemySpawned;
            controller.OnEnemyKilled += HandleEnemyKilled;
            controller.OnWaveStarted += HandleWaveStarted;
            controller.OnWaveCompleted += HandleWaveCompleted;
            
            // 输出初始化日志
            Debug.Log("[SpawnManager] 初始化完成");
        }
        
        private void OnDestroy()
        {
            // 取消订阅事件，防止内存泄漏
            if (SpawnController.Instance != null)
            {
                SpawnController.Instance.OnEnemySpawned -= HandleEnemySpawned;
                SpawnController.Instance.OnEnemyKilled -= HandleEnemyKilled;
                SpawnController.Instance.OnWaveStarted -= HandleWaveStarted;
                SpawnController.Instance.OnWaveCompleted -= HandleWaveCompleted;
            }
        }
        
        private void Update()
        {
            if (!isActive)
                return;
                
            // 更新游戏时间
            gameTime += Time.deltaTime;
            
            // 按照指定间隔更新刷怪控制器
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                SpawnController.Instance.UpdateController();
                lastUpdateTime = Time.time;
                
                // 触发敌人数量变化事件
                OnEnemyCountChanged?.Invoke(SpawnController.Instance.ActiveEnemyCount);
                
                // 输出调试信息
                if (debugMode && spawnStats)
                {
                    Debug.Log($"[SpawnManager] 敌人统计: 活跃 {SpawnController.Instance.ActiveEnemyCount}, " +
                              $"总共已刷出 {SpawnController.Instance.TotalEnemiesSpawned}, " +
                              $"已击杀 {SpawnController.Instance.TotalEnemiesKilled}");
                }
            }
        }
        
        /// <summary>
        /// 延迟激活刷怪系统
        /// </summary>
        private IEnumerator ActivateWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ActivateSpawning();
        }
        
        /// <summary>
        /// 激活刷怪系统
        /// </summary>
        public void ActivateSpawning()
        {
            isActive = true;
            SpawnController.Instance.SetSpawningEnabled(true);
            lastUpdateTime = Time.time;
            
            // 开始自动波次
            SpawnController.Instance.StartAllActiveWaves();
            
            Debug.Log("[SpawnManager] 刷怪系统已激活");
        }
        
        /// <summary>
        /// 停止刷怪系统
        /// </summary>
        public void DeactivateSpawning()
        {
            isActive = false;
            SpawnController.Instance.SetSpawningEnabled(false);
            
            Debug.Log("[SpawnManager] 刷怪系统已停止");
        }
        
        /// <summary>
        /// 重新加载所有配置
        /// </summary>
        public void ReloadConfigurations()
        {
            bool wasActive = isActive;
            
            // 停止刷怪
            if (wasActive)
            {
                DeactivateSpawning();
            }
            
            // 重新加载配置
            SpawnController.Instance.ReloadAllConfigurations();
            
            // 如果之前是活跃的，重新激活
            if (wasActive)
            {
                ActivateSpawning();
            }
            
            Debug.Log("[SpawnManager] 刷怪配置已重新加载");
        }
        
        /// <summary>
        /// 清理所有敌人
        /// </summary>
        public void ClearAllEnemies()
        {
            EnemyManager.Instance.ClearAllEnemies();
            Debug.Log("[SpawnManager] 所有敌人已清除");
        }
        
        /// <summary>
        /// 手动触发指定波次
        /// </summary>
        public void TriggerWave(string waveId)
        {
            if (SpawnController.Instance.StartWaveById(waveId, Vector3.zero))
            {
                Debug.Log($"[SpawnManager] 波次 {waveId} 已手动触发");
            }
            else
            {
                Debug.LogWarning($"[SpawnManager] 波次 {waveId} 触发失败，可能不存在或已禁用");
            }
        }
        
        /// <summary>
        /// 设置玩家等级（可从外部调用）
        /// </summary>
        public void SetPlayerLevel(int level)
        {
            SpawnController.Instance.SetPlayerLevel(level);
            
            if (debugMode)
            {
                Debug.Log($"[SpawnManager] 玩家等级设置为 {level}");
            }
        }
        
        #region 事件处理
        
        private void HandleEnemySpawned(Enemy enemy)
        {
            // 可以在这里添加额外逻辑，例如播放音效或粒子效果
            if (debugMode)
            {
                Debug.Log($"[SpawnManager] 敌人已刷出: {enemy.name}");
            }
        }
        
        private void HandleEnemyKilled(Enemy enemy)
        {
            // 可以在这里添加额外逻辑，例如通知UI更新
            if (debugMode)
            {
                Debug.Log($"[SpawnManager] 敌人已被击杀: {enemy.name}");
            }
        }
        
        private void HandleWaveStarted(string waveId)
        {
            // 波次开始的处理
            Debug.Log($"[SpawnManager] 波次已开始: {waveId}");
            
            // 这里可以添加UI通知，音效等
        }
        
        private void HandleWaveCompleted(string waveId)
        {
            // 波次完成的处理
            Debug.Log($"[SpawnManager] 波次已完成: {waveId}");
            
            // 这里可以添加奖励，UI通知等
        }
        
        #endregion
    }
} 