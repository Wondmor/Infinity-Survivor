# 基于JSON的刷怪系统使用指南

## 概述

刷怪系统是一个灵活、强大的敌人生成系统，使用JSON配置文件来定义刷怪规则、波次和触发器，无需修改代码即可调整游戏的刷怪逻辑。该系统由以下几个核心组件组成：

1. **SpawnRule**：定义敌人刷新的规则，包括敌人类型、数量、位置等。
2. **SpawnWave**：定义刷怪波次，包含多个阶段，每个阶段使用不同的刷怪规则。
3. **SpawnTrigger**：定义触发区域，当玩家进入该区域时触发特定波次。
4. **SpawnController**：非Mono单例，管理所有刷怪逻辑的核心控制器。
5. **SpawnManager**：Mono单例，是游戏世界与刷怪系统的桥梁，挂载在场景中。

## 快速上手

1. 确保以下文件夹存在：
   - `Assets/Resources/Data`（用于存放JSON配置文件）

2. 创建以下JSON配置文件：
   - `Assets/Resources/Data/SpawnRules.json`
   - `Assets/Resources/Data/SpawnWaves.json`
   - `Assets/Resources/Data/SpawnTriggers.json`

3. 在场景中添加SpawnManager组件：
   - 创建一个空游戏对象
   - 添加SpawnManager组件
   - 配置SpawnManager的参数（如初始延迟时间、最大敌人数量等）

4. 运行游戏，系统将自动加载JSON配置并按照定义的规则生成敌人。

## 配置文件详解

### SpawnRules.json

```json
{
  "spawnRules": [
    {
      "ruleId": "rule_zombie_basic",
      "ruleName": "基础僵尸刷怪",
      "enabled": true,
      "enemies": [
        {
          "enemyId": "zombie_basic",
          "weight": 1.0,
          "minLevel": 1,
          "maxLevel": 3,
          "isBoss": false,
          "healthModifier": 1.0,
          "damageModifier": 1.0,
          "speedModifier": 1.0
        }
      ],
      "positionType": 2,
      "spawnAreaSize": {
        "x": 15,
        "y": 0,
        "z": 15
      },
      "minDistanceFromPlayer": 8,
      "maxDistanceFromPlayer": 15,
      "useNavMesh": true,
      "minSpawnCount": 3,
      "maxSpawnCount": 5,
      "maxConcurrentEnemies": 10,
      "spawnInterval": 3,
      "conditionType": 0,
      "spawnProbability": 1.0,
      "playerLevelRequirement": 0,
      "timeRequirement": 0,
      "prerequisiteRuleId": "",
      "hasLimit": false,
      "spawnLimit": 0,
      "progressModifier": 1.0
    }
  ]
}
```

**关键参数说明：**
- `ruleId`：规则唯一标识符
- `enemies`：可生成的敌人列表
- `positionType`：刷怪位置类型（0=随机, 2=围绕玩家, 3=屏幕外, 4=导航网格边缘）
- `conditionType`：刷怪条件（0=总是, 1=概率, 2=玩家等级, 3=时间, 5=规则完成后）

### SpawnWaves.json

```json
{
  "spawnWaves": [
    {
      "waveId": "wave_tutorial",
      "waveName": "教程阶段",
      "enabled": true,
      "loopWave": false,
      "loopCount": 0,
      "initialDelay": 5.0,
      "triggerType": 0,
      "autoTriggerTime": 0,
      "prerequisiteWaveId": "",
      "isBossWave": false,
      "isEventWave": false,
      "difficultyModifier": 0.8,
      "stages": [
        {
          "stageId": "stage_tutorial_1",
          "stageName": "教程第一波",
          "stageDelay": 0,
          "stageDuration": 60,
          "completionCondition": 0,
          "requiredKillCount": 0,
          "ruleIds": ["rule_zombie_basic"]
        }
      ]
    }
  ]
}
```

**关键参数说明：**
- `waveId`：波次唯一标识符
- `triggerType`：触发类型（0=自动, 1=基于时间, 2=前一波次完成后, 3=玩家进入区域）
- `stages`：波次包含的阶段列表
- `ruleIds`：阶段使用的刷怪规则ID列表

### SpawnTriggers.json

```json
{
  "spawnTriggers": [
    {
      "triggerId": "trigger_tutorial",
      "triggerName": "教程区域",
      "enabled": true,
      "triggerType": 0,
      "position": {
        "x": 0,
        "y": 0,
        "z": 0
      },
      "size": {
        "x": 20,
        "y": 5,
        "z": 20
      },
      "rotation": {
        "x": 0,
        "y": 0,
        "z": 0
      },
      "targetLayers": ["Player"],
      "oneTimeOnly": false,
      "cooldown": 0,
      "activateOnStart": true,
      "waveIds": ["wave_tutorial"],
      "triggerAllWaves": false
    }
  ]
}
```

**关键参数说明：**
- `triggerId`：触发器唯一标识符
- `position`、`size`、`rotation`：触发器的位置、大小和旋转
- `triggerType`：触发类型（0=进入时, 1=停留时, 2=离开时, 3=手动）
- `waveIds`：触发的波次ID列表

## 运行时使用

### 通过SpawnManager控制刷怪

```csharp
// 开始刷怪
SpawnManager.Instance.ActivateSpawning();

// 停止刷怪
SpawnManager.Instance.DeactivateSpawning();

// 手动触发特定波次
SpawnManager.Instance.TriggerWave("wave_boss");

// 清除所有敌人
SpawnManager.Instance.ClearAllEnemies();

// 重新加载配置文件
SpawnManager.Instance.ReloadConfigurations();
```

### 监听刷怪事件

```csharp
// 订阅敌人数量变化事件
SpawnManager.Instance.OnEnemyCountChanged += HandleEnemyCountChanged;

// 处理敌人数量变化
private void HandleEnemyCountChanged(int count)
{
    // 更新UI显示等
    enemyCountText.text = $"敌人数量: {count}";
}
```

## 进阶功能

### 动态修改刷怪规则

```csharp
// 获取刷怪规则
SpawnRule zombieRule = SpawnController.Instance.GetRuleById("rule_zombie_basic");
if (zombieRule != null)
{
    // 修改规则参数
    zombieRule.minSpawnCount = 5;
    zombieRule.maxSpawnCount = 10;
    zombieRule.difficultyModifier = 1.5f;
}
```

### 创建自定义波次

```csharp
// 创建新的波次
SpawnWave customWave = SpawnWave.CreateSimpleWave(
    "wave_custom", 
    new List<string>{"rule_zombie_basic", "rule_skeleton_basic"}, 
    120f
);

// 添加到控制器
SpawnController.Instance.AddSpawnWave(customWave);

// 启动波次
SpawnController.Instance.StartWaveById("wave_custom", Vector3.zero);
```

### 手动创建触发区域

```csharp
// 创建触发区域游戏对象
GameObject triggerObj = new GameObject("自定义触发区");
BoxCollider collider = triggerObj.AddComponent<BoxCollider>();
collider.size = new Vector3(30, 5, 30);
collider.isTrigger = true;

// 添加SpawnTrigger组件
SpawnTrigger trigger = triggerObj.AddComponent<SpawnTrigger>();

// 设置触发器参数
// ... (设置各种参数)

// 激活触发器
trigger.Activate();
```

## 故障排除

1. **敌人不生成**
   - 确认SpawnManager已正确挂载并激活
   - 检查JSON配置文件格式是否正确
   - 确认敌人ID与EnemyManager中定义的一致

2. **波次不触发**
   - 检查波次的triggerType是否正确设置
   - 确认prerequisiteWaveId（如果有）指向的波次已完成

3. **触发器不工作**
   - 确认targetLayers包含正确的层（通常是"Player"）
   - 检查碰撞体大小是否合适
   - 确认玩家对象有碰撞体并位于正确的层

4. **性能问题**
   - 调整SpawnManager中的maxConcurrentEnemies参数
   - 调整每个刷怪规则的maxConcurrentEnemies
   - 增加SpawnManager的updateInterval值

## 最佳实践

1. 为不同难度创建不同的配置文件集（例如，easy_rules.json, hard_rules.json）
2. 使用有描述性的ID，便于识别和管理
3. 利用波次的阶段特性创建节奏变化
4. 结合声音和视觉效果增强刷怪系统的反馈
5. 定期清理长时间未被击杀的敌人
6. 使用波次完成事件触发奖励和解锁新内容 