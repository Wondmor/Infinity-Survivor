using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 掉落表数据类 - 定义掉落物品的规则和权重
    /// </summary>
    [Serializable]
    public class DropTable
    {
        // 掉落表基本信息
        public string tableId;             // 掉落表ID
        public string tableName;           // 掉落表名称
        public string description;         // 描述
        
        // 掉落控制
        public bool isEnabled = true;                  // 是否启用
        public DropMethod dropMethod = DropMethod.RandomOneOrNone;  // 掉落方式
        public int minDropCount = 0;                   // 最小掉落数量
        public int maxDropCount = 1;                   // 最大掉落数量
        public float dropChance = 1.0f;                // 掉落概率 (0-1)
        
        // 分组掉落列表
        public List<DropGroup> dropGroups = new List<DropGroup>();  // 掉落组列表
        
        // 特殊条件
        public string conditionExpression;             // 条件表达式（预留功能）
        
        /// <summary>
        /// 掉落方式枚举
        /// </summary>
        public enum DropMethod
        {
            RandomOneOrNone,    // 随机掉落一个或不掉落
            RandomOne,          // 随机掉落一个
            RandomMultiple,     // 随机掉落多个
            DropAll,            // 掉落所有物品
            WeightedRandom      // 按权重随机掉落
        }
        
        /// <summary>
        /// 掉落物品组 - 为了实现分层随机
        /// </summary>
        [Serializable]
        public class DropGroup
        {
            public string groupId;                 // 组ID
            public string groupName;               // 组名称
            public float groupWeight = 1.0f;       // 组权重
            public float groupDropChance = 1.0f;   // 组掉落概率
            public DropMethod groupDropMethod = DropMethod.RandomOne;  // 组内掉落方式
            public int minGroupDrops = 1;          // 组内最小掉落数量
            public int maxGroupDrops = 1;          // 组内最大掉落数量
            
            // 组内物品
            public List<DropEntry> entries = new List<DropEntry>();  // 掉落物品列表
        }
        
        /// <summary>
        /// 掉落物品条目
        /// </summary>
        [Serializable]
        public class DropEntry
        {
            public string itemId;           // 物品ID
            public float weight = 1.0f;     // 权重
            public int minCount = 1;        // 最小数量
            public int maxCount = 1;        // 最大数量
            public float dropChance = 1.0f; // 物品掉落概率

            // 特殊属性
            public bool isFixed = false;    // 是否为固定掉落（必定掉落）
            public bool isUnique = false;   // 是否为唯一物品（只掉落一次）
            public bool hasBeenDropped = false; // 是否已经掉落过（用于唯一物品）
            
            // 条件
            public int minPlayerLevel = 0;  // 最低玩家等级要求
            public float minGameTime = 0f;  // 最低游戏时间要求（秒）
            
            // 获取一个随机的掉落数量
            public int GetRandomCount()
            {
                return UnityEngine.Random.Range(minCount, maxCount + 1);
            }
        }
        
        /// <summary>
        /// 处理掉落表，返回应掉落的物品列表
        /// </summary>
        public List<DropResult> ProcessDrops(int playerLevel = 1, float gameTime = 0f)
        {
            List<DropResult> results = new List<DropResult>();
            
            if (!isEnabled || dropGroups.Count == 0 || UnityEngine.Random.value > dropChance)
                return results;
                
            int numToDrop = 0;
            
            // 根据掉落方式确定掉落数量
            switch (dropMethod)
            {
                case DropMethod.RandomOneOrNone:
                    numToDrop = UnityEngine.Random.value <= dropChance ? 1 : 0;
                    break;
                    
                case DropMethod.RandomOne:
                    numToDrop = 1;
                    break;
                    
                case DropMethod.RandomMultiple:
                    numToDrop = UnityEngine.Random.Range(minDropCount, maxDropCount + 1);
                    break;
                    
                case DropMethod.DropAll:
                    // 会处理所有组，这里不需要设置numToDrop
                    break;
                    
                case DropMethod.WeightedRandom:
                    numToDrop = UnityEngine.Random.Range(minDropCount, maxDropCount + 1);
                    break;
            }
            
            // 处理所有掉落组
            if (dropMethod == DropMethod.DropAll)
            {
                // 掉落所有组的物品
                foreach (var group in dropGroups)
                {
                    results.AddRange(ProcessGroup(group, playerLevel, gameTime));
                }
            }
            else
            {
                // 按权重随机选择掉落组
                List<DropGroup> availableGroups = new List<DropGroup>(dropGroups);
                
                for (int i = 0; i < numToDrop && availableGroups.Count > 0; i++)
                {
                    DropGroup selectedGroup = GetRandomWeightedGroup(availableGroups);
                    if (selectedGroup != null)
                    {
                        results.AddRange(ProcessGroup(selectedGroup, playerLevel, gameTime));
                        
                        // 如果是随机一个，处理完后就移除该组防止重复选择
                        if (dropMethod == DropMethod.RandomOne || dropMethod == DropMethod.RandomOneOrNone)
                        {
                            availableGroups.Remove(selectedGroup);
                        }
                    }
                }
            }
            
            return results;
        }
        
        /// <summary>
        /// 处理单个掉落组
        /// </summary>
        private List<DropResult> ProcessGroup(DropGroup group, int playerLevel, float gameTime)
        {
            List<DropResult> results = new List<DropResult>();
            
            // 检查组的掉落概率
            if (UnityEngine.Random.value > group.groupDropChance)
                return results;
                
            // 过滤出满足条件的物品
            List<DropEntry> validEntries = new List<DropEntry>();
            foreach (var entry in group.entries)
            {
                // 检查物品是否满足条件
                if (entry.minPlayerLevel <= playerLevel && 
                    entry.minGameTime <= gameTime &&
                    (!entry.isUnique || !entry.hasBeenDropped))
                {
                    validEntries.Add(entry);
                }
            }
            
            if (validEntries.Count == 0)
                return results;
                
            // 先处理固定掉落的物品
            foreach (var entry in validEntries)
            {
                if (entry.isFixed)
                {
                    if (UnityEngine.Random.value <= entry.dropChance)
                    {
                        int count = entry.GetRandomCount();
                        if (count > 0)
                        {
                            results.Add(new DropResult(entry.itemId, count));
                            
                            // 标记为已掉落
                            if (entry.isUnique)
                                entry.hasBeenDropped = true;
                        }
                    }
                }
            }
            
            // 移除固定掉落的物品，处理随机掉落
            validEntries.RemoveAll(e => e.isFixed);
            
            if (validEntries.Count == 0)
                return results;
                
            // 根据组内掉落方式进行处理
            int numToDrop = 0;
            switch (group.groupDropMethod)
            {
                case DropMethod.RandomOneOrNone:
                    numToDrop = UnityEngine.Random.value <= group.groupDropChance ? 1 : 0;
                    break;
                    
                case DropMethod.RandomOne:
                    numToDrop = 1;
                    break;
                    
                case DropMethod.RandomMultiple:
                    numToDrop = UnityEngine.Random.Range(group.minGroupDrops, group.maxGroupDrops + 1);
                    break;
                    
                case DropMethod.DropAll:
                    numToDrop = validEntries.Count;
                    break;
                    
                case DropMethod.WeightedRandom:
                    numToDrop = UnityEngine.Random.Range(group.minGroupDrops, group.maxGroupDrops + 1);
                    break;
            }
            
            // 限制掉落数量不超过有效物品数量
            numToDrop = Mathf.Min(numToDrop, validEntries.Count);
            
            // 随机选择指定数量的物品
            for (int i = 0; i < numToDrop; i++)
            {
                if (validEntries.Count == 0)
                    break;
                    
                DropEntry selectedEntry = GetRandomWeightedEntry(validEntries);
                if (selectedEntry != null)
                {
                    if (UnityEngine.Random.value <= selectedEntry.dropChance)
                    {
                        int count = selectedEntry.GetRandomCount();
                        if (count > 0)
                        {
                            results.Add(new DropResult(selectedEntry.itemId, count));
                            
                            // 标记为已掉落
                            if (selectedEntry.isUnique)
                                selectedEntry.hasBeenDropped = true;
                        }
                    }
                    
                    // 该物品已处理，从列表中移除防止重复选择
                    validEntries.Remove(selectedEntry);
                }
            }
            
            return results;
        }
        
        /// <summary>
        /// 根据权重随机选择一个掉落组
        /// </summary>
        private DropGroup GetRandomWeightedGroup(List<DropGroup> groups)
        {
            if (groups == null || groups.Count == 0)
                return null;
                
            // 计算总权重
            float totalWeight = 0;
            foreach (var group in groups)
            {
                totalWeight += group.groupWeight;
            }
            
            if (totalWeight <= 0)
                return groups[UnityEngine.Random.Range(0, groups.Count)];
                
            // 根据权重随机选择
            float randomValue = UnityEngine.Random.Range(0, totalWeight);
            float currentWeight = 0;
            
            foreach (var group in groups)
            {
                currentWeight += group.groupWeight;
                if (randomValue <= currentWeight)
                    return group;
            }
            
            return groups[groups.Count - 1]; // 防止浮点精度问题导致没有选中
        }
        
        /// <summary>
        /// 根据权重随机选择一个掉落物品
        /// </summary>
        private DropEntry GetRandomWeightedEntry(List<DropEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return null;
                
            // 计算总权重
            float totalWeight = 0;
            foreach (var entry in entries)
            {
                totalWeight += entry.weight;
            }
            
            if (totalWeight <= 0)
                return entries[UnityEngine.Random.Range(0, entries.Count)];
                
            // 根据权重随机选择
            float randomValue = UnityEngine.Random.Range(0, totalWeight);
            float currentWeight = 0;
            
            foreach (var entry in entries)
            {
                currentWeight += entry.weight;
                if (randomValue <= currentWeight)
                    return entry;
            }
            
            return entries[entries.Count - 1]; // 防止浮点精度问题导致没有选中
        }
    }
    
    /// <summary>
    /// 掉落结果类 - 表示掉落的物品和数量
    /// </summary>
    [Serializable]
    public class DropResult
    {
        public string itemId;   // 物品ID
        public int count;       // 物品数量
        public Vector3 position; // 掉落位置
        
        public DropResult(string id, int itemCount)
        {
            itemId = id;
            count = itemCount;
        }
        
        public DropResult(string id, int itemCount, Vector3 dropPosition)
        {
            itemId = id;
            count = itemCount;
            position = dropPosition;
        }
    }
    
    /// <summary>
    /// 掉落表集合 - 用于从JSON加载多个掉落表
    /// </summary>
    [Serializable]
    public class DropTableCollection
    {
        public List<DropTable> dropTables = new List<DropTable>();
    }
} 