using UnityEngine;
using System.Collections.Generic;
using TrianCatStudio;

namespace TrianCatStudio
{
    /// <summary>
    /// 敌人管理器 - 全局管理敌人，实现为非MonoBehaviour单例
    /// </summary>
    public class EnemyManager : BaseManager<EnemyManager>
    {
        // 敌人预制体字典
        private Dictionary<string, GameObject> enemyPrefabs = new Dictionary<string, GameObject>();
        
        // 敌人数据字典
        private Dictionary<string, EnemyData> enemyDataDict = new Dictionary<string, EnemyData>();
        
        // 当前活跃的敌人列表
        private List<Enemy> activeEnemies = new List<Enemy>();
        
        // 敌人ID计数器
        private int enemyCounter = 0;
        
        // 是否已初始化
        private bool isInitialized = false;
        
        /// <summary>
        /// 初始化敌人管理器
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;
            
            // 加载敌人预制体和数据
            LoadEnemyPrefabs();
            LoadEnemyData();
            
            isInitialized = true;
            Debug.Log("[EnemyManager] 敌人管理器初始化完成");
        }
        
        /// <summary>
        /// 加载敌人预制体
        /// </summary>
        private void LoadEnemyPrefabs()
        {
            // 这里可以从Resources文件夹加载预制体
            // 也可以使用Addressables或其他资源管理系统
            GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/Enemies");
            
            foreach (var prefab in prefabs)
            {
                if (prefab.GetComponent<Enemy>() != null)
                {
                    string enemyId = prefab.GetComponent<Enemy>().EnemyId;
                    if (!string.IsNullOrEmpty(enemyId))
                    {
                        enemyPrefabs[enemyId] = prefab;
                    }
                    else
                    {
                        Debug.LogWarning($"[EnemyManager] 敌人预制体 {prefab.name} 没有设置EnemyId");
                    }
                }
            }
            
            Debug.Log($"[EnemyManager] 已加载 {enemyPrefabs.Count} 个敌人预制体");
        }
        
        /// <summary>
        /// 加载敌人数据
        /// </summary>
        private void LoadEnemyData()
        {
            // 这里可以从JSON文件、ScriptableObject或其他数据源加载敌人数据
            // 这里仅作为示例，实际应根据项目需求实现
            // 例如：从Resources文件夹加载敌人数据文件
            TextAsset dataFile = Resources.Load<TextAsset>("Data/EnemyData");
            if (dataFile != null)
            {
                try
                {
                    // 解析JSON数据
                    EnemyDataCollection dataCollection = JsonUtility.FromJson<EnemyDataCollection>(dataFile.text);
                    
                    foreach (var data in dataCollection.enemies)
                    {
                        enemyDataDict[data.enemyId] = data;
                    }
                    
                    Debug.Log($"[EnemyManager] 已加载 {enemyDataDict.Count} 个敌人数据");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[EnemyManager] 加载敌人数据失败: {e.Message}");
                }
            }
            else
            {
                // 没有找到数据文件，创建一些默认数据
                CreateDefaultEnemyData();
            }
        }
        
        /// <summary>
        /// 创建默认敌人数据
        /// </summary>
        private void CreateDefaultEnemyData()
        {
            // 创建一些默认敌人数据
            // 这仅作为示例或测试使用
            
            // 普通僵尸
            var zombieData = new EnemyData
            {
                enemyId = "zombie_basic",
                displayName = "僵尸",
                description = "一个普通的僵尸，行动缓慢但很危险。",
                enemyType = EnemyType.Normal,
                maxHealth = 100,
                moveSpeed = 2.5f,
                attackRange = 1.5f,
                detectRange = 10f,
                attackDamage = 15f,
                attackCooldown = 2f,
                physicalDefense = 10,
                experienceValue = 10
            };
            
            // 快速僵尸
            var fastZombieData = new EnemyData
            {
                enemyId = "zombie_fast",
                displayName = "疾速僵尸",
                description = "一个移动迅速的僵尸，攻击力较弱但速度很快。",
                enemyType = EnemyType.Normal,
                maxHealth = 70,
                moveSpeed = 5f,
                attackRange = 1.5f,
                detectRange = 12f,
                attackDamage = 10f,
                attackCooldown = 1f,
                physicalDefense = 5,
                experienceValue = 15
            };
            
            // 坦克僵尸
            var tankZombieData = new EnemyData
            {
                enemyId = "zombie_tank",
                displayName = "坦克僵尸",
                description = "一个体型巨大的僵尸，生命值很高但移动缓慢。",
                enemyType = EnemyType.Elite,
                maxHealth = 300,
                moveSpeed = 1.5f,
                attackRange = 2f,
                detectRange = 8f,
                attackDamage = 25f,
                attackCooldown = 3f,
                physicalDefense = 30,
                experienceValue = 30
            };
            
            // 添加到字典
            enemyDataDict[zombieData.enemyId] = zombieData;
            enemyDataDict[fastZombieData.enemyId] = fastZombieData;
            enemyDataDict[tankZombieData.enemyId] = tankZombieData;
            
            Debug.Log("[EnemyManager] 已创建默认敌人数据");
        }
        
        /// <summary>
        /// 生成敌人
        /// </summary>
        public Enemy SpawnEnemy(string enemyId, Vector3 position, Quaternion rotation, int level = 1, Transform target = null)
        {
            if (!isInitialized)
            {
                Initialize();
            }
            
            // 检查是否有此敌人的预制体
            if (!enemyPrefabs.ContainsKey(enemyId))
            {
                Debug.LogError($"[EnemyManager] 找不到敌人预制体: {enemyId}");
                return null;
            }
            
            // 获取敌人数据
            EnemyData enemyData = null;
            if (enemyDataDict.ContainsKey(enemyId))
            {
                enemyData = enemyDataDict[enemyId].GetScaledData(level);
            }
            else
            {
                Debug.LogWarning($"[EnemyManager] 找不到敌人数据: {enemyId}，将使用默认值");
            }
            
            // 实例化敌人
            GameObject enemyInstance = Object.Instantiate(enemyPrefabs[enemyId], position, rotation);
            Enemy enemy = enemyInstance.GetComponent<Enemy>();
            
            if (enemy != null)
            {
                // 生成唯一ID
                string uniqueId = $"{enemyId}_{enemyCounter++}";
                
                // 初始化敌人
                enemy.Initialize(uniqueId, enemyData);
                
                // 设置目标
                if (target != null)
                {
                    enemy.SetTarget(target);
                }
                
                // 添加到活跃敌人列表
                activeEnemies.Add(enemy);
                
                // 订阅敌人死亡事件
                enemy.OnDeath += HandleEnemyDeath;
                
                return enemy;
            }
            
            Debug.LogError($"[EnemyManager] 敌人预制体上没有Enemy组件: {enemyId}");
            Object.Destroy(enemyInstance);
            return null;
        }
        
        /// <summary>
        /// 从对象池获取敌人
        /// </summary>
        public Enemy GetEnemyFromPool(string enemyId, Vector3 position, Quaternion rotation, int level = 1, Transform target = null)
        {
            if (!isInitialized)
            {
                Initialize();
            }
            
            // 检查是否有此敌人的预制体
            if (!enemyPrefabs.ContainsKey(enemyId))
            {
                Debug.LogError($"[EnemyManager] 找不到敌人预制体: {enemyId}");
                return null;
            }
            
            // 从对象池获取敌人
            GameObject enemyInstance = ObjectPoolManager.Instance.Get(enemyPrefabs[enemyId], position, rotation);
            Enemy enemy = enemyInstance.GetComponent<Enemy>();
            
            if (enemy != null)
            {
                // 获取敌人数据
                EnemyData enemyData = null;
                if (enemyDataDict.ContainsKey(enemyId))
                {
                    enemyData = enemyDataDict[enemyId].GetScaledData(level);
                }
                
                // 生成唯一ID
                string uniqueId = $"{enemyId}_{enemyCounter++}";
                
                // 初始化敌人
                enemy.Initialize(uniqueId, enemyData);
                
                // 设置目标
                if (target != null)
                {
                    enemy.SetTarget(target);
                }
                
                // 添加到活跃敌人列表
                activeEnemies.Add(enemy);
                
                // 订阅敌人死亡事件
                enemy.OnDeath += HandleEnemyDeath;
                
                return enemy;
            }
            
            Debug.LogError($"[EnemyManager] 敌人预制体上没有Enemy组件: {enemyId}");
            enemyInstance.SetActive(false); // 放回对象池
            return null;
        }
        
        /// <summary>
        /// 处理敌人死亡
        /// </summary>
        private void HandleEnemyDeath(Enemy enemy)
        {
            // 解除事件订阅
            enemy.OnDeath -= HandleEnemyDeath;
            
            // 从活跃敌人列表中移除
            activeEnemies.Remove(enemy);
            
            // 生成掉落物
            SpawnDrops(enemy);
        }
        
        /// <summary>
        /// 生成掉落物
        /// </summary>
        private void SpawnDrops(Enemy enemy)
        {
            if (enemy == null)
                return;
                
            // 使用DropController生成掉落物品
            DropController.Instance.GenerateDropsForEnemy(
                enemy.EnemyId, 
                enemy.transform.position, 
                1  // 使用默认等级1，因为Enemy类没有Level属性
            );
        }
        
        /// <summary>
        /// 获取最近的敌人
        /// </summary>
        public Enemy GetNearestEnemy(Vector3 position, float maxDistance = float.MaxValue)
        {
            Enemy nearest = null;
            float minDistance = maxDistance;
            
            foreach (var enemy in activeEnemies)
            {
                if (enemy == null || enemy.IsDead)
                    continue;
                    
                float distance = Vector3.Distance(position, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = enemy;
                }
            }
            
            return nearest;
        }
        
        /// <summary>
        /// 获取范围内的敌人
        /// </summary>
        public List<Enemy> GetEnemiesInRange(Vector3 position, float range)
        {
            List<Enemy> result = new List<Enemy>();
            
            foreach (var enemy in activeEnemies)
            {
                if (enemy == null || enemy.IsDead)
                    continue;
                    
                float distance = Vector3.Distance(position, enemy.transform.position);
                if (distance <= range)
                {
                    result.Add(enemy);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 清理所有敌人
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (var enemy in activeEnemies.ToArray())
            {
                if (enemy != null)
                {
                    // 解除事件订阅
                    enemy.OnDeath -= HandleEnemyDeath;
                    
                    // 销毁敌人
                    Object.Destroy(enemy.gameObject);
                }
            }
            
            activeEnemies.Clear();
            Debug.Log("[EnemyManager] 已清理所有敌人");
        }
        
        /// <summary>
        /// 获取敌人数据
        /// </summary>
        public EnemyData GetEnemyData(string enemyId)
        {
            if (enemyDataDict.ContainsKey(enemyId))
            {
                return enemyDataDict[enemyId];
            }
            
            Debug.LogWarning($"[EnemyManager] 找不到敌人数据: {enemyId}");
            return null;
        }
        
        /// <summary>
        /// 获取活跃敌人数量
        /// </summary>
        public int GetActiveEnemyCount()
        {
            return activeEnemies.Count;
        }
    }
} 