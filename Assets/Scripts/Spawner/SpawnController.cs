using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.AI;

namespace TrianCatStudio
{
    /// <summary>
    /// 刷怪控制器 - 管理所有刷怪规则和波次
    /// </summary>
    public class SpawnController : BaseManager<SpawnController>
    {        
        // 私有构造函数，初始化时调用
        private SpawnController()
        {
            Init();
        }
        
        // 事件
        public delegate void WaveEventHandler(string waveId);
        public event WaveEventHandler OnWaveStarted;
        public event WaveEventHandler OnWaveCompleted;
        
        public delegate void EnemyEventHandler(Enemy enemy);
        public event EnemyEventHandler OnEnemySpawned;
        public event EnemyEventHandler OnEnemyKilled;
        
        // 刷怪规则集合
        private Dictionary<string, SpawnRule> spawnRules = new Dictionary<string, SpawnRule>();
        
        // 波次集合
        private Dictionary<string, SpawnWave> spawnWaves = new Dictionary<string, SpawnWave>();
        
        // 活跃的敌人
        private List<Enemy> activeEnemies = new List<Enemy>();
        
        // 已完成的规则和波次
        private HashSet<string> completedRules = new HashSet<string>();
        private HashSet<string> completedWaves = new HashSet<string>();
        
        // 游戏状态
        private float gameStartTime;
        private bool isSpawningEnabled = true;
        private int playerLevel = 1;
        private Transform playerTransform;
        
        // 统计数据
        private int totalEnemiesSpawned = 0;
        private int totalEnemiesKilled = 0;
        
        public int TotalEnemiesSpawned => totalEnemiesSpawned;
        public int TotalEnemiesKilled => totalEnemiesKilled;
        public int ActiveEnemyCount => activeEnemies.Count;
        
        // 初始化
        protected virtual void Init()
        {
            gameStartTime = Time.time;

            
            // 找到玩家
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            
            Debug.Log("[SpawnController] 初始化完成");
        }
        
        /// <summary>
        /// 更新，由外部调用
        /// </summary>
        public void UpdateController()
        {
            if (!isSpawningEnabled)
                return;
                
            float currentTime = Time.time;
            
            // 更新活跃的波次
            UpdateActiveWaves(currentTime);
            
            // 清理无效的敌人引用
            CleanupDeadEnemies();
        }
        
        /// <summary>
        /// 更新活跃的波次
        /// </summary>
        private void UpdateActiveWaves(float currentTime)
        {
            foreach (var wave in spawnWaves.Values)
            {
                if (wave.isActive && !wave.isCompleted)
                {
                    // 更新波次状态
                    wave.UpdateWave(currentTime, totalEnemiesKilled);
                    
                    // 如果波次活跃，检查当前阶段的规则并刷怪
                    var currentStage = wave.GetCurrentStage();
                    if (currentStage != null && currentStage.isActive && 
                        currentTime >= currentStage.stageStartTime)
                    {
                        SpawnFromStage(currentStage, currentTime);
                    }
                }
            }
        }
        
        /// <summary>
        /// 根据阶段刷怪
        /// </summary>
        private void SpawnFromStage(SpawnWave.WaveStage stage, float currentTime)
        {
            if (stage.ruleIds == null || stage.ruleIds.Count == 0)
                return;
                
            foreach (var ruleId in stage.ruleIds)
            {
                if (spawnRules.TryGetValue(ruleId, out SpawnRule rule))
                {
                    if (rule.ShouldSpawn(playerLevel, currentTime - gameStartTime, new List<string>(completedRules)))
                    {
                        SpawnFromRule(rule, Vector3.zero);
                    }
                }
            }
        }
        
        /// <summary>
        /// 根据规则刷怪
        /// </summary>
        public void SpawnFromRule(SpawnRule rule, Vector3 spawnPosition)
        {
            if (!isSpawningEnabled || rule == null || !rule.enabled)
                return;
                
            // 检查当前活跃敌人数量
            if (activeEnemies.Count >= rule.maxConcurrentEnemies)
                return;
                
            // 获取刷怪数量
            int spawnCount = rule.GetSpawnCount();
            
            // 检查是否有数量限制
            if (rule.hasLimit && rule.spawnLimit <= 0)
                return;
                
            // 调整刷怪数量
            if (rule.hasLimit)
            {
                spawnCount = Mathf.Min(spawnCount, rule.spawnLimit);
                rule.spawnLimit -= spawnCount;
            }
            
            // 执行刷怪
            for (int i = 0; i < spawnCount; i++)
            {
                // 获取敌人数据
                SpawnRule.EnemySpawnData enemyData = rule.GetRandomEnemy();
                if (enemyData == null)
                    continue;
                    
                // 获取刷怪位置
                Vector3 position = DetermineSpawnPosition(rule, spawnPosition);
                
                // 创建敌人
                SpawnEnemy(enemyData.enemyId, position, rule);
            }
        }
        
        /// <summary>
        /// 确定刷怪位置
        /// </summary>
        private Vector3 DetermineSpawnPosition(SpawnRule rule, Vector3 defaultPosition)
        {
            Vector3 result = defaultPosition;
            
            // 如果默认位置是零向量，使用规则类型决定位置
            if (defaultPosition == Vector3.zero)
            {
                switch (rule.positionType)
                {
                    case SpawnRule.SpawnPositionType.Random:
                        result = GetRandomPositionInArea(Vector3.zero, rule.spawnAreaSize);
                        break;
                        
                    case SpawnRule.SpawnPositionType.AroundPlayer:
                        if (playerTransform != null)
                        {
                            result = GetRandomPositionAroundTarget(
                                playerTransform.position, 
                                rule.minDistanceFromPlayer, 
                                rule.maxDistanceFromPlayer
                            );
                        }
                        break;
                        
                    case SpawnRule.SpawnPositionType.NavMeshEdge:
                        result = GetRandomNavMeshEdgePosition(Vector3.zero, 20f);
                        break;
                        
                    case SpawnRule.SpawnPositionType.OffScreen:
                        result = GetOffScreenPosition(20f);
                        break;
                        
                    default:
                        result = Vector3.zero;
                        break;
                }
            }
            
            // 如果需要使用导航网格，确保位置在导航网格上
            if (rule.useNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(result, out hit, 10f, NavMesh.AllAreas))
                {
                    result = hit.position;
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 创建敌人
        /// </summary>
        private void SpawnEnemy(string enemyId, Vector3 position, SpawnRule rule)
        {
            Enemy enemy = EnemyManager.Instance.SpawnEnemy(enemyId, position, Quaternion.identity);
            
            if (enemy != null)
            {
                // 添加到活跃敌人列表
                activeEnemies.Add(enemy);
                
                // 更新统计
                totalEnemiesSpawned++;
                
                // 订阅敌人死亡事件
                enemy.OnDeath += HandleEnemyDeath;
                
                // 触发事件
                OnEnemySpawned?.Invoke(enemy);
            }
        }
        
        /// <summary>
        /// 处理敌人死亡
        /// </summary>
        private void HandleEnemyDeath(Enemy enemy)
        {
            if (enemy != null)
            {
                // 更新统计
                totalEnemiesKilled++;
                
                // 从活跃列表移除
                activeEnemies.Remove(enemy);
                
                // 取消订阅事件
                enemy.OnDeath -= HandleEnemyDeath;
                
                // 触发事件
                OnEnemyKilled?.Invoke(enemy);
            }
        }
        
        /// <summary>
        /// 清理无效的敌人引用
        /// </summary>
        private void CleanupDeadEnemies()
        {
            activeEnemies.RemoveAll(e => e == null || e.IsDead);
        }
        
        #region 波次管理
        
        /// <summary>
        /// 添加刷怪规则
        /// </summary>
        public void AddSpawnRule(SpawnRule rule)
        {
            if (rule != null && !string.IsNullOrEmpty(rule.ruleId))
            {
                spawnRules[rule.ruleId] = rule;
            }
        }
        
        /// <summary>
        /// 添加刷怪波次
        /// </summary>
        public void AddSpawnWave(SpawnWave wave)
        {
            if (wave != null && !string.IsNullOrEmpty(wave.waveId))
            {
                spawnWaves[wave.waveId] = wave;
                
                // 订阅波次事件
                wave.OnWaveCompleted += (completedWave) => {
                    completedWaves.Add(completedWave.waveId);
                    OnWaveCompleted?.Invoke(completedWave.waveId);
                };
                
                wave.OnWaveStarted += (startedWave) => {
                    OnWaveStarted?.Invoke(startedWave.waveId);
                };
            }
        }
        
        /// <summary>
        /// 通过ID启动波次
        /// </summary>
        public bool StartWaveById(string waveId, Vector3 position)
        {
            if (spawnWaves.TryGetValue(waveId, out SpawnWave wave) && wave.enabled)
            {
                // 设置波次位置
                // (这里可以添加逻辑来根据position调整波次的刷怪位置)
                
                // 启动波次
                wave.StartWave(Time.time);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 启动所有活跃波次
        /// </summary>
        public void StartAllActiveWaves()
        {
            float currentTime = Time.time;
            foreach (var wave in spawnWaves.Values)
            {
                if (wave.enabled && !wave.isActive && !wave.isCompleted)
                {
                    // 检查是否满足自动触发条件
                    if (wave.triggerType == SpawnWave.TriggerType.Automatic)
                    {
                        wave.StartWave(currentTime);
                    }
                    else if (wave.triggerType == SpawnWave.TriggerType.TimeBased && 
                             currentTime - gameStartTime >= wave.autoTriggerTime)
                    {
                        wave.StartWave(currentTime);
                    }
                    else if (wave.triggerType == SpawnWave.TriggerType.PreviousWaveCompleted && 
                             !string.IsNullOrEmpty(wave.prerequisiteWaveId) &&
                             completedWaves.Contains(wave.prerequisiteWaveId))
                    {
                        wave.StartWave(currentTime);
                    }
                }
            }
        }
        
        /// <summary>
        /// 停止所有波次
        /// </summary>
        public void StopAllWaves()
        {
            foreach (var wave in spawnWaves.Values)
            {
                if (wave.isActive && !wave.isCompleted)
                {
                    wave.CompleteWave();
                }
            }
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 获取区域内的随机位置
        /// </summary>
        private Vector3 GetRandomPositionInArea(Vector3 center, Vector3 size)
        {
            return center + new Vector3(
                UnityEngine.Random.Range(-size.x/2, size.x/2),
                UnityEngine.Random.Range(-size.y/2, size.y/2),
                UnityEngine.Random.Range(-size.z/2, size.z/2)
            );
        }
        
        /// <summary>
        /// 获取目标周围的随机位置
        /// </summary>
        private Vector3 GetRandomPositionAroundTarget(Vector3 targetPosition, float minDistance, float maxDistance)
        {
            float distance = UnityEngine.Random.Range(minDistance, maxDistance);
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized * distance;
            
            return targetPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
        }
        
        /// <summary>
        /// 获取导航网格边缘的随机位置
        /// </summary>
        private Vector3 GetRandomNavMeshEdgePosition(Vector3 center, float maxDistance)
        {
            // 随机方向
            Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
            Vector3 direction = new Vector3(randomDirection.x, 0, randomDirection.y);
            
            // 从中心点向随机方向发射射线
            NavMeshHit hit;
            if (NavMesh.Raycast(center, center + direction * maxDistance, out hit, NavMesh.AllAreas))
            {
                return hit.position;
            }
            
            // 如果射线未命中，使用目标位置附近的随机点
            return GetRandomPositionAroundTarget(center, maxDistance/2, maxDistance);
        }
        
        /// <summary>
        /// 获取屏幕外的位置
        /// </summary>
        private Vector3 GetOffScreenPosition(float distance)
        {
            // 获取主摄像机
            Camera mainCamera = Camera.main;
            if (mainCamera == null || playerTransform == null)
                return Vector3.zero;
                
            // 屏幕边缘的四个方向
            Vector3[] directions = new Vector3[4] {
                Vector3.forward,
                Vector3.back,
                Vector3.right,
                Vector3.left
            };
            
            // 随机选择一个方向
            Vector3 direction = directions[UnityEngine.Random.Range(0, directions.Length)];
            
            // 计算屏幕外的位置
            return playerTransform.position + direction * distance;
        }
        
        /// <summary>
        /// 设置玩家等级
        /// </summary>
        public void SetPlayerLevel(int level)
        {
            playerLevel = level;
        }
        
        /// <summary>
        /// 启用或禁用刷怪
        /// </summary>
        public void SetSpawningEnabled(bool enabled)
        {
            isSpawningEnabled = enabled;
        }
        
        /// <summary>
        /// 处理敌人被击杀（旧方法，保留为向后兼容）
        /// </summary>
        private void HandleEnemyKilled(Enemy enemy, GameObject killer)
        {
            // 调用新的处理方法
            HandleEnemyDeath(enemy);
        }
        
        #endregion
        
        protected virtual void OnDestroy()
        {
            // 清理敌人事件订阅
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    enemy.OnDeath -= HandleEnemyDeath;
                }
            }
            
            // 清理波次事件订阅
            foreach (var wave in spawnWaves.Values)
            {
                // 由于波次完成事件是委托，我们不能直接取消订阅
                // 但我们可以在这里记录波次已销毁，避免回调时出错
            }
            
            // 清理控制器事件
            OnWaveStarted = null;
            OnWaveCompleted = null;
            OnEnemySpawned = null;
            OnEnemyKilled = null;
        }
    }
} 