using UnityEngine;
using System;
using System.Collections.Generic;

namespace TrianCatStudio
{
    /// <summary>
    /// 敌人数据类 - 存储敌人的基本属性和配置信息
    /// </summary>
    [Serializable]
    public class EnemyData
    {
        // 基本信息
        public string enemyId;
        public string displayName;
        public string description;
        public EnemyType enemyType;
        
        // 基础属性
        public int level = 1;
        public float maxHealth = 100f;
        public float moveSpeed = 3.5f;
        public float attackRange = 1.5f;
        public float detectRange = 10f;
        public float attackDamage = 10f;
        public float attackCooldown = 1.5f;
        
        // 防御属性
        public float physicalDefense = 0f;
        public float fireResistance = 0f;
        public float iceResistance = 0f;
        public float lightningResistance = 0f;
        public float poisonResistance = 0f;
        
        // 行为配置
        public float idleTime = 2f;
        public float patrolRange = 10f;
        public float chaseTimeout = 10f;
        
        // 掉落配置
        public List<DropItem> drops = new List<DropItem>();
        public float experienceValue = 10f;
        
        /// <summary>
        /// 根据等级计算实际属性
        /// </summary>
        public EnemyData GetScaledData(int targetLevel)
        {
            if (targetLevel <= 1) return this;
            
            // 创建新的数据实例以避免修改原始数据
            EnemyData scaledData = new EnemyData
            {
                enemyId = this.enemyId,
                displayName = this.displayName,
                description = this.description,
                enemyType = this.enemyType,
                level = targetLevel,
                patrolRange = this.patrolRange,
                idleTime = this.idleTime,
                chaseTimeout = this.chaseTimeout,
                drops = this.drops,
            };
            
            // 计算等级缩放系数
            float levelFactor = 1f + (targetLevel - 1) * 0.2f; // 每级提升20%
            
            // 缩放基础属性
            scaledData.maxHealth = this.maxHealth * levelFactor;
            scaledData.moveSpeed = this.moveSpeed * Mathf.Sqrt(levelFactor); // 速度增长较慢
            scaledData.attackDamage = this.attackDamage * levelFactor;
            scaledData.attackRange = this.attackRange; // 不随等级改变
            scaledData.detectRange = this.detectRange; // 不随等级改变
            scaledData.attackCooldown = this.attackCooldown * Mathf.Pow(0.95f, targetLevel - 1); // 攻击速度小幅提升
            
            // 缩放防御属性
            scaledData.physicalDefense = this.physicalDefense * levelFactor;
            scaledData.fireResistance = this.fireResistance * levelFactor;
            scaledData.iceResistance = this.iceResistance * levelFactor;
            scaledData.lightningResistance = this.lightningResistance * levelFactor;
            scaledData.poisonResistance = this.poisonResistance * levelFactor;
            
            // 缩放经验值
            scaledData.experienceValue = this.experienceValue * levelFactor;
            
            return scaledData;
        }
    }
    
    /// <summary>
    /// 敌人类型枚举
    /// </summary>
    public enum EnemyType
    {
        Normal,     // 普通敌人
        Elite,      // 精英敌人
        Boss,       // Boss敌人
        Minion      // 小兵（通常是Boss召唤的）
    }
    
    /// <summary>
    /// 敌人数据集合（用于JSON反序列化）
    /// </summary>
    [Serializable]
    public class EnemyDataCollection
    {
        public List<EnemyData> enemies = new List<EnemyData>();
    }
} 