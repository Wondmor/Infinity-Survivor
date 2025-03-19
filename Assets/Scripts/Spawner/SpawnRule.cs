using UnityEngine;
using System;
using System.Collections.Generic;

namespace TrianCatStudio
{
    /// <summary>
    /// 刷怪规则数据类 - 定义刷怪的规则和数据
    /// </summary>
    [Serializable]
    public class SpawnRule
    {
        // 基本设置
        public string ruleId;                     // 规则ID
        public string ruleName;                   // 规则名称
        public bool enabled = true;               // 是否启用
        
        // 敌人设置
        public List<EnemySpawnData> enemies = new List<EnemySpawnData>();  // 可刷出的敌人列表
        
        // 刷怪位置设置
        public SpawnPositionType positionType = SpawnPositionType.Random;  // 刷怪位置类型
        public Vector3 spawnAreaSize = new Vector3(10f, 0f, 10f);          // 随机区域大小
        public float minDistanceFromPlayer = 5f;                           // 与玩家的最小距离
        public float maxDistanceFromPlayer = 15f;                          // 与玩家的最大距离
        public bool useNavMesh = true;                                     // 是否使用导航网格点
        
        // 刷怪数量设置
        public int minSpawnCount = 1;             // 最小刷怪数量
        public int maxSpawnCount = 3;             // 最大刷怪数量
        public int maxConcurrentEnemies = 5;      // 最大同时存在敌人数量
        public float spawnInterval = 2f;          // 刷怪间隔时间
        
        // 刷怪条件
        public SpawnConditionType conditionType = SpawnConditionType.Always;  // 刷怪条件类型
        public float spawnProbability = 1.0f;                                 // 刷怪概率 (0-1)
        public int playerLevelRequirement = 0;                                // 玩家等级要求
        public float timeRequirement = 0f;                                    // 时间要求（游戏开始后的秒数）
        public string prerequisiteRuleId = "";                                // 前置规则ID
        
        // 刷怪限制
        public bool hasLimit = false;             // 是否有数量限制
        public int spawnLimit = 10;               // 刷怪总数限制
        public float durationLimit = 0f;          // 持续时间限制 (0表示无限制)
        
        // 关卡进度设置
        public float progressModifier = 1.0f;     // 进度修正系数
        
        /// <summary>
        /// 单个敌人的刷怪数据
        /// </summary>
        [Serializable]
        public class EnemySpawnData
        {
            public string enemyId;                // 敌人ID
            public float weight = 1.0f;           // 权重 (用于随机选择)
            public int minLevel = 1;              // 最小等级
            public int maxLevel = 1;              // 最大等级
            public bool isBoss = false;           // 是否为Boss
            
            // 特殊调整
            public float healthModifier = 1.0f;   // 生命值修正系数
            public float damageModifier = 1.0f;   // 伤害修正系数
            public float speedModifier = 1.0f;    // 速度修正系数
            
            public EnemySpawnData(string id, float spawnWeight = 1.0f)
            {
                enemyId = id;
                weight = spawnWeight;
            }
        }
        
        /// <summary>
        /// 刷怪位置类型枚举
        /// </summary>
        public enum SpawnPositionType
        {
            Random,             // 区域内随机位置
            FixedPoints,        // 固定点位置
            AroundPlayer,       // 玩家周围
            OffScreen,          // 屏幕外
            NavMeshEdge,        // 导航网格边缘
            FromPoint           // 从指定点位置
        }
        
        /// <summary>
        /// 刷怪条件类型枚举
        /// </summary>
        public enum SpawnConditionType
        {
            Always,             // 总是刷怪
            Probability,        // 按概率刷怪
            PlayerLevel,        // 玩家等级达到要求时刷怪
            TimeElapsed,        // 时间达到要求时刷怪
            EnemiesCleared,     // 敌人清理完成后刷怪
            RuleCompleted,      // 特定规则完成后刷怪
            Custom              // 自定义条件
        }
        
        /// <summary>
        /// 根据权重随机选择一个敌人
        /// </summary>
        public EnemySpawnData GetRandomEnemy()
        {
            if (enemies == null || enemies.Count == 0)
                return null;
                
            // 计算总权重
            float totalWeight = 0;
            foreach (var enemy in enemies)
            {
                totalWeight += enemy.weight;
            }
            
            // 随机选择
            float randomValue = UnityEngine.Random.Range(0, totalWeight);
            float cumulativeWeight = 0;
            
            foreach (var enemy in enemies)
            {
                cumulativeWeight += enemy.weight;
                if (randomValue <= cumulativeWeight)
                    return enemy;
            }
            
            // 默认返回第一个
            return enemies[0];
        }
        
        /// <summary>
        /// 获取应该刷出的敌人数量
        /// </summary>
        public int GetSpawnCount()
        {
            return UnityEngine.Random.Range(minSpawnCount, maxSpawnCount + 1);
        }
        
        /// <summary>
        /// 判断当前是否应该刷怪
        /// </summary>
        public bool ShouldSpawn(int playerLevel, float gameTime, List<string> completedRules)
        {
            if (!enabled)
                return false;
                
            // 检查条件
            switch (conditionType)
            {
                case SpawnConditionType.Always:
                    return true;
                    
                case SpawnConditionType.Probability:
                    return UnityEngine.Random.value <= spawnProbability;
                    
                case SpawnConditionType.PlayerLevel:
                    return playerLevel >= playerLevelRequirement;
                    
                case SpawnConditionType.TimeElapsed:
                    return gameTime >= timeRequirement;
                    
                case SpawnConditionType.RuleCompleted:
                    if (string.IsNullOrEmpty(prerequisiteRuleId))
                        return true;
                    return completedRules != null && completedRules.Contains(prerequisiteRuleId);
                    
                default:
                    return true;
            }
        }
        
        /// <summary>
        /// 创建一个简单的刷怪规则
        /// </summary>
        public static SpawnRule CreateSimpleRule(string id, string enemyId, int count = 5)
        {
            SpawnRule rule = new SpawnRule
            {
                ruleId = id,
                ruleName = "Simple Rule " + id,
                minSpawnCount = count,
                maxSpawnCount = count
            };
            
            rule.enemies.Add(new EnemySpawnData(enemyId, 1.0f));
            
            return rule;
        }
    }
} 