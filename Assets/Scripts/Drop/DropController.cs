using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace TrianCatStudio
{
    /// <summary>
    /// 掉落控制器 - 管理物品掉落和掉落表
    /// </summary>
    public class DropController : BaseManager<DropController>
    {
        // 掉落表字典
        private Dictionary<string, DropTable> dropTables = new Dictionary<string, DropTable>();
        
        // 敌人掉落表映射 - 敌人ID到掉落表ID的映射
        private Dictionary<string, string> enemyDropTableMap = new Dictionary<string, string>();
        
        // 掉落物品预制体字典
        private Dictionary<string, GameObject> dropItemPrefabs = new Dictionary<string, GameObject>();
        
        // 掉落相关设置
        private float dropForce = 5f;  // 掉落物品的弹跳力
        private float spreadRadius = 1.5f;  // 掉落物品的分散半径
        private int playerLevel = 1;  // 当前玩家等级
        private float gameStartTime;  // 游戏开始时间
        
        // 已掉落的物品列表
        private List<GameObject> activeDropItems = new List<GameObject>();
        
        // 初始化
        private DropController()
        {
            gameStartTime = Time.time;
            Init();
        }
        
        // 初始化
        protected virtual void Init()
        {
            // 加载掉落表数据
            LoadDropTables();
            
            // 加载掉落物品预制体
            LoadDropItemPrefabs();
            
            Debug.Log("[DropController] 初始化完成");
        }
        
        /// <summary>
        /// 加载掉落表数据
        /// </summary>
        private void LoadDropTables()
        {
            // 从Resources文件夹加载掉落表JSON
            TextAsset jsonFile = Resources.Load<TextAsset>("Data/DropTables");
            
            if (jsonFile != null)
            {
                try
                {
                    // 解析JSON
                    DropTableCollection collection = JsonUtility.FromJson<DropTableCollection>(jsonFile.text);
                    
                    foreach (var table in collection.dropTables)
                    {
                        if (!string.IsNullOrEmpty(table.tableId))
                        {
                            dropTables[table.tableId] = table;
                        }
                    }
                    
                    Debug.Log($"[DropController] 已加载 {dropTables.Count} 个掉落表");
                    
                    // 加载敌人掉落表映射
                    LoadEnemyDropMappings();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[DropController] 加载掉落表失败: {e.Message}");
                    // 如果加载失败，创建一些默认掉落表
                    CreateDefaultDropTables();
                }
            }
            else
            {
                Debug.LogWarning("[DropController] 找不到掉落表JSON文件，使用默认掉落表");
                CreateDefaultDropTables();
            }
        }
        
        /// <summary>
        /// 加载敌人掉落表映射
        /// </summary>
        private void LoadEnemyDropMappings()
        {
            // 从Resources文件夹加载敌人掉落映射JSON
            TextAsset jsonFile = Resources.Load<TextAsset>("Data/EnemyDropMappings");
            
            if (jsonFile != null)
            {
                try
                {
                    // 解析JSON
                    EnemyDropMappingCollection collection = JsonUtility.FromJson<EnemyDropMappingCollection>(jsonFile.text);
                    
                    foreach (var mapping in collection.mappings)
                    {
                        if (!string.IsNullOrEmpty(mapping.enemyId) && !string.IsNullOrEmpty(mapping.dropTableId))
                        {
                            enemyDropTableMap[mapping.enemyId] = mapping.dropTableId;
                        }
                    }
                    
                    Debug.Log($"[DropController] 已加载 {enemyDropTableMap.Count} 个敌人掉落映射");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[DropController] 加载敌人掉落映射失败: {e.Message}");
                    CreateDefaultEnemyMappings();
                }
            }
            else
            {
                Debug.LogWarning("[DropController] 找不到敌人掉落映射JSON文件，使用默认映射");
                CreateDefaultEnemyMappings();
            }
        }
        
        /// <summary>
        /// 加载掉落物品预制体
        /// </summary>
        private void LoadDropItemPrefabs()
        {
            // 从Resources文件夹加载掉落物品预制体
            GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/DropItems");
            
            foreach (var prefab in prefabs)
            {
                DropItem dropItem = prefab.GetComponent<DropItem>();
                if (dropItem != null && !string.IsNullOrEmpty(dropItem.ItemId))
                {
                    dropItemPrefabs[dropItem.ItemId] = prefab;
                }
            }
            
            Debug.Log($"[DropController] 已加载 {dropItemPrefabs.Count} 个掉落物品预制体");
        }
        
        /// <summary>
        /// 创建默认掉落表（用于测试或无数据时）
        /// </summary>
        private void CreateDefaultDropTables()
        {
            // 创建一个简单的通用掉落表
            DropTable commonDropTable = new DropTable
            {
                tableId = "drop_common",
                tableName = "通用掉落",
                description = "常见敌人的掉落表",
                dropMethod = DropTable.DropMethod.RandomMultiple,
                minDropCount = 1,
                maxDropCount = 2,
                dropChance = 0.7f
            };
            
            // 创建掉落组 - 消耗品组
            DropTable.DropGroup consumableGroup = new DropTable.DropGroup
            {
                groupId = "group_consumable",
                groupName = "消耗品",
                groupWeight = 3.0f,
                groupDropChance = 0.8f
            };
            
            // 添加消耗品组的物品
            consumableGroup.entries.Add(new DropTable.DropEntry
            {
                itemId = "health_potion",
                weight = 5.0f,
                minCount = 1,
                maxCount = 2,
                dropChance = 0.5f
            });
            
            consumableGroup.entries.Add(new DropTable.DropEntry
            {
                itemId = "mana_potion",
                weight = 3.0f,
                minCount = 1,
                maxCount = 1,
                dropChance = 0.3f
            });
            
            // 创建掉落组 - 材料组
            DropTable.DropGroup materialGroup = new DropTable.DropGroup
            {
                groupId = "group_material",
                groupName = "材料",
                groupWeight = 2.0f,
                groupDropChance = 0.5f
            };
            
            // 添加材料组的物品
            materialGroup.entries.Add(new DropTable.DropEntry
            {
                itemId = "wood",
                weight = 3.0f,
                minCount = 1,
                maxCount = 3,
                dropChance = 0.7f
            });
            
            materialGroup.entries.Add(new DropTable.DropEntry
            {
                itemId = "stone",
                weight = 2.0f,
                minCount = 1,
                maxCount = 2,
                dropChance = 0.5f
            });
            
            // 添加组到掉落表
            commonDropTable.dropGroups.Add(consumableGroup);
            commonDropTable.dropGroups.Add(materialGroup);
            
            // 创建精英敌人的掉落表
            DropTable eliteDropTable = new DropTable
            {
                tableId = "drop_elite",
                tableName = "精英掉落",
                description = "精英敌人的掉落表",
                dropMethod = DropTable.DropMethod.RandomMultiple,
                minDropCount = 2,
                maxDropCount = 3,
                dropChance = 1.0f
            };
            
            // 创建掉落组 - 装备组
            DropTable.DropGroup equipmentGroup = new DropTable.DropGroup
            {
                groupId = "group_equipment",
                groupName = "装备",
                groupWeight = 1.0f,
                groupDropChance = 0.4f
            };
            
            // 添加装备组的物品
            equipmentGroup.entries.Add(new DropTable.DropEntry
            {
                itemId = "sword",
                weight = 1.0f,
                minCount = 1,
                maxCount = 1,
                dropChance = 0.2f
            });
            
            equipmentGroup.entries.Add(new DropTable.DropEntry
            {
                itemId = "shield",
                weight = 1.0f,
                minCount = 1,
                maxCount = 1,
                dropChance = 0.2f
            });
            
            // 添加消耗品组和装备组到精英掉落表
            eliteDropTable.dropGroups.Add(consumableGroup);
            eliteDropTable.dropGroups.Add(equipmentGroup);
            
            // 添加到掉落表字典
            dropTables[commonDropTable.tableId] = commonDropTable;
            dropTables[eliteDropTable.tableId] = eliteDropTable;
            
            Debug.Log("[DropController] 已创建默认掉落表");
        }
        
        /// <summary>
        /// 创建默认敌人掉落映射
        /// </summary>
        private void CreateDefaultEnemyMappings()
        {
            // 为一些默认敌人设置掉落表映射
            enemyDropTableMap["zombie_basic"] = "drop_common";
            enemyDropTableMap["zombie_fast"] = "drop_common";
            enemyDropTableMap["zombie_tank"] = "drop_elite";
            
            Debug.Log("[DropController] 已创建默认敌人掉落映射");
        }
        
        /// <summary>
        /// 为敌人生成掉落物品
        /// </summary>
        public void GenerateDropsForEnemy(string enemyId, Vector3 position, int enemyLevel = 1)
        {
            // 检查是否有该敌人的掉落表映射
            if (!enemyDropTableMap.ContainsKey(enemyId))
            {
                Debug.LogWarning($"[DropController] 敌人 {enemyId} 没有掉落表映射");
                return;
            }
            
            string dropTableId = enemyDropTableMap[enemyId];
            
            // 检查掉落表是否存在
            if (!dropTables.ContainsKey(dropTableId))
            {
                Debug.LogWarning($"[DropController] 掉落表 {dropTableId} 不存在");
                return;
            }
            
            // 获取掉落表并处理掉落
            DropTable dropTable = dropTables[dropTableId];
            
            // 计算游戏时间
            float gameTime = Time.time - gameStartTime;
            
            // 处理掉落，获取掉落结果列表
            List<DropResult> drops = dropTable.ProcessDrops(playerLevel, gameTime);
            
            // 生成掉落物品
            SpawnDropItems(drops, position, enemyLevel);
        }
        
        /// <summary>
        /// 生成掉落物品实体
        /// </summary>
        private void SpawnDropItems(List<DropResult> drops, Vector3 position, int enemyLevel)
        {
            if (drops == null || drops.Count == 0)
                return;
                
            foreach (var drop in drops)
            {
                if (string.IsNullOrEmpty(drop.itemId) || drop.count <= 0)
                    continue;
                    
                // 检查是否有该物品的预制体
                if (!dropItemPrefabs.ContainsKey(drop.itemId))
                {
                    Debug.LogWarning($"[DropController] 物品 {drop.itemId} 没有预制体");
                    continue;
                }
                
                // 计算随机掉落位置（在原点周围分散）
                Vector3 dropPosition = position + new Vector3(
                    Random.Range(-spreadRadius, spreadRadius),
                    0.5f, // 略微抬高，防止掉入地面
                    Random.Range(-spreadRadius, spreadRadius)
                );
                
                // 实例化掉落物品
                GameObject dropItemObj = Object.Instantiate(dropItemPrefabs[drop.itemId], dropPosition, Quaternion.identity);
                DropItem dropItem = dropItemObj.GetComponent<DropItem>();
                
                if (dropItem != null)
                {
                    // 设置物品属性
                    dropItem.SetItemData(drop.itemId, drop.count, enemyLevel);
                    
                    // 添加物理效果（如果有Rigidbody）
                    Rigidbody rb = dropItemObj.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        // 给物品一个随机的弹跳力
                        Vector3 force = new Vector3(
                            Random.Range(-0.5f, 0.5f),
                            Random.Range(0.5f, 1.0f),
                            Random.Range(-0.5f, 0.5f)
                        ).normalized * dropForce;
                        
                        rb.AddForce(force, ForceMode.Impulse);
                    }
                    
                    // 添加到活跃物品列表
                    activeDropItems.Add(dropItemObj);
                }
            }
        }
        
        /// <summary>
        /// 手动生成物品掉落
        /// </summary>
        public void GenerateItemDrop(string itemId, int count, Vector3 position, int level = 1)
        {
            if (string.IsNullOrEmpty(itemId) || count <= 0)
                return;
                
            // 检查是否有该物品的预制体
            if (!dropItemPrefabs.ContainsKey(itemId))
            {
                Debug.LogWarning($"[DropController] 物品 {itemId} 没有预制体");
                return;
            }
            
            // 创建掉落结果并生成物品
            DropResult drop = new DropResult(itemId, count, position);
            SpawnDropItems(new List<DropResult> { drop }, position, level);
        }
        
        /// <summary>
        /// 从指定的掉落表生成物品
        /// </summary>
        public void GenerateDropsFromTable(string dropTableId, Vector3 position, int level = 1)
        {
            if (string.IsNullOrEmpty(dropTableId))
                return;
                
            // 检查掉落表是否存在
            if (!dropTables.ContainsKey(dropTableId))
            {
                Debug.LogWarning($"[DropController] 掉落表 {dropTableId} 不存在");
                return;
            }
            
            // 获取掉落表并处理掉落
            DropTable dropTable = dropTables[dropTableId];
            
            // 计算游戏时间
            float gameTime = Time.time - gameStartTime;
            
            // 处理掉落，获取掉落结果列表
            List<DropResult> drops = dropTable.ProcessDrops(playerLevel, gameTime);
            
            // 生成掉落物品
            SpawnDropItems(drops, position, level);
        }
        
        /// <summary>
        /// 添加自定义掉落表
        /// </summary>
        public void AddDropTable(DropTable dropTable)
        {
            if (dropTable != null && !string.IsNullOrEmpty(dropTable.tableId))
            {
                dropTables[dropTable.tableId] = dropTable;
            }
        }
        
        /// <summary>
        /// 添加敌人掉落表映射
        /// </summary>
        public void AddEnemyDropMapping(string enemyId, string dropTableId)
        {
            if (!string.IsNullOrEmpty(enemyId) && !string.IsNullOrEmpty(dropTableId))
            {
                enemyDropTableMap[enemyId] = dropTableId;
            }
        }
        
        /// <summary>
        /// 设置玩家等级
        /// </summary>
        public void SetPlayerLevel(int level)
        {
            playerLevel = Mathf.Max(1, level);
        }
        
        /// <summary>
        /// 设置掉落物品的物理参数
        /// </summary>
        public void SetDropPhysicsParams(float force, float radius)
        {
            dropForce = force;
            spreadRadius = radius;
        }
        
        /// <summary>
        /// 清理长时间未拾取的掉落物品
        /// </summary>
        public void CleanupOldDrops(float maxAge = 300f)
        {
            List<GameObject> dropsToRemove = new List<GameObject>();
            
            foreach (var dropObj in activeDropItems)
            {
                if (dropObj == null)
                {
                    dropsToRemove.Add(dropObj);
                    continue;
                }
                
                DropItem dropItem = dropObj.GetComponent<DropItem>();
                if (dropItem != null && dropItem.GetDropAge() > maxAge)
                {
                    Object.Destroy(dropObj);
                    dropsToRemove.Add(dropObj);
                }
            }
            
            // 从列表中移除已清理的物品
            foreach (var dropObj in dropsToRemove)
            {
                activeDropItems.Remove(dropObj);
            }
        }
        
        /// <summary>
        /// 获取活跃的掉落物品数量
        /// </summary>
        public int GetActiveDropCount()
        {
            // 清理空引用
            activeDropItems.RemoveAll(item => item == null);
            return activeDropItems.Count;
        }
    }
    
    /// <summary>
    /// 敌人掉落映射
    /// </summary>
    [System.Serializable]
    public class EnemyDropMapping
    {
        public string enemyId;     // 敌人ID
        public string dropTableId; // 掉落表ID
    }
    
    /// <summary>
    /// 敌人掉落映射集合
    /// </summary>
    [System.Serializable]
    public class EnemyDropMappingCollection
    {
        public List<EnemyDropMapping> mappings = new List<EnemyDropMapping>();
    }
} 