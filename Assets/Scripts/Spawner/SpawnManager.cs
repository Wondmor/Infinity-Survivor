using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        
        [Header("分帧加载设置")]
        [SerializeField] private bool useFrameLoading = true;  // 是否使用分帧加载
        [SerializeField] private int objectsPerFrame = 10;     // 每帧创建对象数量
        [SerializeField] private float frameLoadingInterval = 0.02f; // 分帧加载间隔
        
        // 内部状态
        private bool isActive = false;
        private float gameTime = 0f;
        private float lastUpdateTime = 0f;
        private bool isInitializing = false;
        private bool configurationLoaded = false;
        
        // 加载进度
        private float loadingProgress = 0f;
        
        // 玩家参考
        private Player player;
        
        // 事件系统
        public delegate void SpawnEvent(int enemyCount);
        public event SpawnEvent OnEnemyCountChanged;
        
        public delegate void LoadingEvent(float progress);
        public event LoadingEvent OnLoadingProgressChanged;
        
        // 加载状态属性
        public bool IsLoading => isInitializing;
        public float LoadingProgress => loadingProgress;
        
        private void Start()
        {
            // 初始化
            StartCoroutine(InitializeAsync());
            
            // 自动激活
            if (activateOnStart)
            {
                StartCoroutine(ActivateWithDelay(initialDelay));
            }
        }
        
        /// <summary>
        /// 异步初始化
        /// </summary>
        private IEnumerator InitializeAsync()
        {
            isInitializing = true;
            loadingProgress = 0f;
            
            // 确保刷怪控制器已创建
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
                    controller.SetPlayerLevel(1); // 设置默认等级为1
                }
            }
            
            // 设置最大敌人数量
            controller.SetMaxEnemyCount(maxConcurrentEnemies);
            
            // 订阅事件
            controller.OnEnemySpawned += HandleEnemySpawned;
            controller.OnEnemyKilled += HandleEnemyKilled;
            controller.OnWaveStarted += HandleWaveStarted;
            controller.OnWaveCompleted += HandleWaveCompleted;
            
            // 分帧加载配置和预热对象
            yield return StartCoroutine(LoadConfigurationsAsync());
            
            // 标记初始化完成
            isInitializing = false;
            loadingProgress = 1f;
            OnLoadingProgressChanged?.Invoke(loadingProgress);
            
            Debug.Log("[SpawnManager] 初始化完成");
        }
        
        /// <summary>
        /// 异步加载配置
        /// </summary>
        private IEnumerator LoadConfigurationsAsync()
        {
            // 如果不使用分帧加载，直接同步加载
            if (!useFrameLoading)
            {
                SpawnController.Instance.LoadAllConfigurations();
                configurationLoaded = true;
                loadingProgress = 1f;
                OnLoadingProgressChanged?.Invoke(loadingProgress);
                yield break;
            }
            
            // 分步加载
            // 1. 加载刷怪规则
            yield return StartCoroutine(LoadSpawnRulesAsync());
            loadingProgress = 0.33f;
            OnLoadingProgressChanged?.Invoke(loadingProgress);
            
            // 2. 加载波次配置
            yield return StartCoroutine(LoadSpawnWavesAsync());
            loadingProgress = 0.66f;
            OnLoadingProgressChanged?.Invoke(loadingProgress);
            
            // 3. 加载触发器配置并创建触发器
            yield return StartCoroutine(LoadSpawnTriggersAsync());
            loadingProgress = 1.0f;
            OnLoadingProgressChanged?.Invoke(loadingProgress);
            
            configurationLoaded = true;
            Debug.Log("[SpawnManager] 配置异步加载完成");
        }
        
        /// <summary>
        /// 异步加载刷怪规则
        /// </summary>
        private IEnumerator LoadSpawnRulesAsync()
        {
            TextAsset rulesJson = Resources.Load<TextAsset>(SpawnController.SPAWN_RULES_PATH);
            if (rulesJson != null)
            {
                bool loadingSuccess = true;
                SpawnController.SpawnRulesContainer container = null;
                
                try
                {
                    container = JsonUtility.FromJson<SpawnController.SpawnRulesContainer>(rulesJson.text);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[SpawnManager] 加载刷怪规则失败: {e.Message}");
                    loadingSuccess = false;
                }
                
                if (loadingSuccess && container != null)
                {
                    // 分帧添加规则
                    int totalRules = container.spawnRules.Count;
                    int processedRules = 0;
                    
                    while (processedRules < totalRules)
                    {
                        int rulesThisFrame = Mathf.Min(objectsPerFrame, totalRules - processedRules);
                        
                        for (int i = 0; i < rulesThisFrame; i++)
                        {
                            SpawnController.Instance.AddSpawnRule(container.spawnRules[processedRules + i]);
                        }
                        
                        processedRules += rulesThisFrame;
                        
                        // 更新进度
                        float localProgress = (float)processedRules / totalRules;
                        
                        // 仅在调试模式下显示详细进度
                        if (debugMode)
                        {
                            Debug.Log($"[SpawnManager] 加载刷怪规则进度: {localProgress:P}");
                        }
                        
                        yield return new WaitForSeconds(frameLoadingInterval);
                    }
                    
                    Debug.Log($"[SpawnManager] 成功加载 {container.spawnRules.Count} 个刷怪规则");
                }
            }
            else
            {
                Debug.LogWarning($"[SpawnManager] 无法找到刷怪规则配置文件: {SpawnController.SPAWN_RULES_PATH}");
            }
        }
        
        /// <summary>
        /// 异步加载波次配置
        /// </summary>
        private IEnumerator LoadSpawnWavesAsync()
        {
            TextAsset wavesJson = Resources.Load<TextAsset>(SpawnController.SPAWN_WAVES_PATH);
            if (wavesJson != null)
            {
                bool loadingSuccess = true;
                SpawnController.SpawnWavesContainer container = null;
                
                try
                {
                    container = JsonUtility.FromJson<SpawnController.SpawnWavesContainer>(wavesJson.text);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[SpawnManager] 加载波次配置失败: {e.Message}");
                    loadingSuccess = false;
                }
                
                if (loadingSuccess && container != null)
                {
                    // 分帧添加波次
                    int totalWaves = container.spawnWaves.Count;
                    int processedWaves = 0;
                    
                    while (processedWaves < totalWaves)
                    {
                        int wavesThisFrame = Mathf.Min(objectsPerFrame, totalWaves - processedWaves);
                        
                        for (int i = 0; i < wavesThisFrame; i++)
                        {
                            SpawnController.Instance.AddSpawnWave(container.spawnWaves[processedWaves + i]);
                        }
                        
                        processedWaves += wavesThisFrame;
                        
                        // 更新进度
                        float localProgress = (float)processedWaves / totalWaves;
                        
                        // 仅在调试模式下显示详细进度
                        if (debugMode)
                        {
                            Debug.Log($"[SpawnManager] 加载波次进度: {localProgress:P}");
                        }
                        
                        yield return new WaitForSeconds(frameLoadingInterval);
                    }
                    
                    Debug.Log($"[SpawnManager] 成功加载 {container.spawnWaves.Count} 个波次");
                }
            }
            else
            {
                Debug.LogWarning($"[SpawnManager] 无法找到波次配置文件: {SpawnController.SPAWN_WAVES_PATH}");
            }
        }
        
        /// <summary>
        /// 异步加载触发器配置
        /// </summary>
        private IEnumerator LoadSpawnTriggersAsync()
        {
            TextAsset triggersJson = Resources.Load<TextAsset>(SpawnController.SPAWN_TRIGGERS_PATH);
            if (triggersJson != null)
            {
                bool loadingSuccess = true;
                SpawnController.SpawnTriggersContainer container = null;
                
                try
                {
                    container = JsonUtility.FromJson<SpawnController.SpawnTriggersContainer>(triggersJson.text);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[SpawnManager] 加载触发器配置失败: {e.Message}");
                    loadingSuccess = false;
                }
                
                if (loadingSuccess && container != null)
                {
                    // 分帧创建触发器
                    int totalTriggers = container.spawnTriggers.Count;
                    int processedTriggers = 0;
                    
                    // 创建容器
                    GameObject triggerContainer = new GameObject("SpawnTriggers");
                    
                    while (processedTriggers < totalTriggers)
                    {
                        int triggersThisFrame = Mathf.Min(objectsPerFrame, totalTriggers - processedTriggers);
                        
                        for (int i = 0; i < triggersThisFrame; i++)
                        {
                            var triggerData = container.spawnTriggers[processedTriggers + i];
                            
                            // 缓存触发器数据
                            SpawnController.Instance.AddTriggerData(triggerData);
                            
                            // 创建触发器游戏对象
                            if (triggerData.enabled)
                            {
                                yield return CreateTriggerObject(triggerData, triggerContainer.transform);
                            }
                        }
                        
                        processedTriggers += triggersThisFrame;
                        
                        // 更新进度
                        float localProgress = (float)processedTriggers / totalTriggers;
                        
                        // 仅在调试模式下显示详细进度
                        if (debugMode)
                        {
                            Debug.Log($"[SpawnManager] 加载触发器进度: {localProgress:P}");
                        }
                        
                        yield return new WaitForSeconds(frameLoadingInterval);
                    }
                    
                    Debug.Log($"[SpawnManager] 成功加载 {container.spawnTriggers.Count} 个触发器");
                }
            }
            else
            {
                Debug.LogWarning($"[SpawnManager] 无法找到触发器配置文件: {SpawnController.SPAWN_TRIGGERS_PATH}");
            }
        }
        
        /// <summary>
        /// 创建单个触发器对象
        /// </summary>
        private IEnumerator CreateTriggerObject(SpawnController.SpawnTriggerData data, Transform parent)
        {
            GameObject triggerObj = new GameObject(data.triggerName);
            triggerObj.transform.parent = parent;
            triggerObj.transform.position = data.position;
            triggerObj.transform.eulerAngles = data.rotation;
            
            // 添加碰撞器
            BoxCollider collider = triggerObj.AddComponent<BoxCollider>();
            collider.size = data.size;
            collider.isTrigger = true;
            
            // 添加SpawnTrigger组件
            SpawnTrigger trigger = triggerObj.AddComponent<SpawnTrigger>();
            
            // 设置目标层
            if (data.targetLayers != null && data.targetLayers.Count > 0)
            {
                LayerMask mask = 0;
                foreach (var layerName in data.targetLayers)
                {
                    mask |= 1 << LayerMask.NameToLayer(layerName);
                }
                typeof(SpawnTrigger).GetField("targetLayers", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(trigger, mask);
            }
            
            // 设置触发器属性
            typeof(SpawnTrigger).GetField("triggerType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(trigger, (SpawnTrigger.TriggerType)data.triggerType);
            typeof(SpawnTrigger).GetField("oneTimeOnly", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(trigger, data.oneTimeOnly);
            typeof(SpawnTrigger).GetField("cooldown", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(trigger, data.cooldown);
            typeof(SpawnTrigger).GetField("activateOnStart", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(trigger, data.activateOnStart);
            typeof(SpawnTrigger).GetField("triggerId", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(trigger, data.triggerId);
            typeof(SpawnTrigger).GetField("waveIds", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(trigger, data.waveIds.ToArray());
            typeof(SpawnTrigger).GetField("triggerAllWaves", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(trigger, data.triggerAllWaves);
            
            yield return null;
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
            if (!isActive || !configurationLoaded)
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
            // 等待初始化完成
            while (isInitializing)
            {
                yield return null;
            }
            
            // 额外延迟
            yield return new WaitForSeconds(delay);
            
            ActivateSpawning();
        }
        
        /// <summary>
        /// 激活刷怪系统
        /// </summary>
        public void ActivateSpawning()
        {
            // 如果仍在初始化，等待初始化完成后再激活
            if (isInitializing)
            {
                StartCoroutine(ActivateWithDelay(0.1f));
                return;
            }
            
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
            
            // 设置正在初始化状态
            isInitializing = true;
            configurationLoaded = false;
            loadingProgress = 0f;
            
            // 重新异步加载配置
            StartCoroutine(LoadConfigurationsAsync());
            
            // 等待加载完成后重新激活
            if (wasActive)
            {
                StartCoroutine(ReactivateAfterLoad());
            }
            
            Debug.Log("[SpawnManager] 刷怪配置正在重新加载...");
        }
        
        /// <summary>
        /// 等待加载完成后重新激活
        /// </summary>
        private IEnumerator ReactivateAfterLoad()
        {
            while (isInitializing)
            {
                yield return null;
            }
            
            ActivateSpawning();
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