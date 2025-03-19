using UnityEngine;
using System;
using System.Collections.Generic;

namespace TrianCatStudio
{
    /// <summary>
    /// 刷怪波次管理 - 用于管理一系列按顺序执行的刷怪规则
    /// </summary>
    [Serializable]
    public class SpawnWave
    {
        // 基本信息
        public string waveId;                   // 波次ID
        public string waveName;                 // 波次名称
        public bool enabled = true;             // 是否启用
        
        // 波次设置
        public List<WaveStage> stages = new List<WaveStage>();  // 波次阶段列表
        public bool loopWave = false;                           // 是否循环波次
        public int loopCount = 0;                               // 循环次数 (0表示无限循环)
        public float initialDelay = 0f;                         // 初始延迟时间
        
        // 波次触发条件
        public TriggerType triggerType = TriggerType.Automatic;  // 触发类型
        public float autoTriggerTime = 0f;                       // 自动触发时间
        public string prerequisiteWaveId = "";                   // 前置波次ID
        
        // 波次特殊属性
        public bool isBossWave = false;          // 是否为Boss波次
        public bool isEventWave = false;         // 是否为事件波次
        public float difficultyModifier = 1.0f;  // 难度修正系数
        
        // 波次状态
        [NonSerialized] public bool isActive = false;           // 是否激活
        [NonSerialized] public bool isCompleted = false;        // 是否完成
        [NonSerialized] public int currentStageIndex = -1;      // 当前阶段索引
        [NonSerialized] public float waveStartTime = 0f;        // 波次开始时间
        [NonSerialized] public int completedLoops = 0;          // 已完成循环次数
        
        // 波次事件
        public delegate void WaveEventHandler(SpawnWave wave);
        public event WaveEventHandler OnWaveStarted;
        public event WaveEventHandler OnWaveCompleted;
        public event WaveEventHandler OnStageChanged;
        
        /// <summary>
        /// 波次阶段 - 每个波次可以包含多个阶段
        /// </summary>
        [Serializable]
        public class WaveStage
        {
            public string stageId;                // 阶段ID
            public string stageName;              // 阶段名称
            public List<string> ruleIds = new List<string>();  // 该阶段使用的规则ID列表
            public float stageDuration = 30f;     // 阶段持续时间
            public float stageDelay = 0f;         // 阶段开始前的延迟时间
            public CompletionCondition completionCondition = CompletionCondition.TimeElapsed;  // 完成条件
            public int requiredKillCount = 0;     // 需要击杀的敌人数量
            
            // 阶段状态
            [NonSerialized] public bool isActive = false;     // 是否激活
            [NonSerialized] public bool isCompleted = false;  // 是否完成
            [NonSerialized] public float stageStartTime = 0f; // 阶段开始时间
            [NonSerialized] public int enemiesKilled = 0;     // 已击杀敌人数量
            
            /// <summary>
            /// 阶段完成条件枚举
            /// </summary>
            public enum CompletionCondition
            {
                TimeElapsed,      // 时间结束
                AllEnemiesKilled, // 所有敌人被击杀
                KillCount,        // 击杀特定数量的敌人
                Manual            // 手动触发
            }
            
            /// <summary>
            /// 检查阶段是否应该结束
            /// </summary>
            public bool ShouldComplete(float currentTime, int currentKillCount)
            {
                switch (completionCondition)
                {
                    case CompletionCondition.TimeElapsed:
                        return currentTime >= stageStartTime + stageDuration;
                        
                    case CompletionCondition.AllEnemiesKilled:
                        // 这需要外部输入当前活跃敌人数
                        // 通常由SpawnController调用并提供
                        return currentKillCount > 0 && enemiesKilled >= currentKillCount;
                        
                    case CompletionCondition.KillCount:
                        return enemiesKilled >= requiredKillCount;
                        
                    case CompletionCondition.Manual:
                        return false; // 需要手动触发
                        
                    default:
                        return false;
                }
            }
        }
        
        /// <summary>
        /// 波次触发类型枚举
        /// </summary>
        public enum TriggerType
        {
            Automatic,      // 自动触发
            TimeBased,      // 基于时间触发
            PlayerEnteredArea, // 玩家进入区域触发
            PreviousWaveCompleted, // 前一波次完成触发
            Manual          // 手动触发
        }
        
        /// <summary>
        /// 启动波次
        /// </summary>
        public void StartWave(float currentTime)
        {
            if (isActive || isCompleted)
                return;
                
            isActive = true;
            isCompleted = false;
            currentStageIndex = -1;
            waveStartTime = currentTime;
            completedLoops = 0;
            
            // 触发开始事件
            OnWaveStarted?.Invoke(this);
            
            // 启动第一个阶段
            AdvanceToNextStage(currentTime);
        }
        
        /// <summary>
        /// 推进到下一个阶段
        /// </summary>
        public void AdvanceToNextStage(float currentTime)
        {
            // 结束当前阶段
            if (currentStageIndex >= 0 && currentStageIndex < stages.Count)
            {
                stages[currentStageIndex].isActive = false;
                stages[currentStageIndex].isCompleted = true;
            }
            
            currentStageIndex++;
            
            // 检查是否完成所有阶段
            if (currentStageIndex >= stages.Count)
            {
                // 检查是否需要循环
                if (loopWave && (loopCount == 0 || completedLoops < loopCount - 1))
                {
                    completedLoops++;
                    currentStageIndex = 0;
                }
                else
                {
                    CompleteWave();
                    return;
                }
            }
            
            // 启动新阶段
            var stage = stages[currentStageIndex];
            stage.isActive = true;
            stage.isCompleted = false;
            stage.stageStartTime = currentTime + stage.stageDelay;
            stage.enemiesKilled = 0;
            
            // 触发阶段变更事件
            OnStageChanged?.Invoke(this);
        }
        
        /// <summary>
        /// 完成波次
        /// </summary>
        public void CompleteWave()
        {
            isActive = false;
            isCompleted = true;
            
            // 触发完成事件
            OnWaveCompleted?.Invoke(this);
        }
        
        /// <summary>
        /// 获取当前活跃的阶段
        /// </summary>
        public WaveStage GetCurrentStage()
        {
            if (currentStageIndex >= 0 && currentStageIndex < stages.Count)
                return stages[currentStageIndex];
            return null;
        }
        
        /// <summary>
        /// 更新波次状态
        /// </summary>
        public void UpdateWave(float currentTime, int activeEnemiesKilled)
        {
            if (!isActive || isCompleted)
                return;
                
            var currentStage = GetCurrentStage();
            if (currentStage == null)
                return;
                
            // 检查是否需要等待延迟
            if (currentTime < currentStage.stageStartTime)
                return;
                
            // 记录击杀数
            currentStage.enemiesKilled = activeEnemiesKilled;
            
            // 检查阶段是否应该结束
            if (currentStage.ShouldComplete(currentTime, activeEnemiesKilled))
            {
                AdvanceToNextStage(currentTime);
            }
        }
        
        /// <summary>
        /// 手动触发完成当前阶段
        /// </summary>
        public void ManualCompleteCurrentStage(float currentTime)
        {
            if (!isActive || isCompleted)
                return;
                
            var currentStage = GetCurrentStage();
            if (currentStage != null && currentStage.completionCondition == WaveStage.CompletionCondition.Manual)
            {
                AdvanceToNextStage(currentTime);
            }
        }
        
        /// <summary>
        /// 创建一个简单的波次
        /// </summary>
        public static SpawnWave CreateSimpleWave(string id, List<string> ruleIds, float duration = 60f)
        {
            SpawnWave wave = new SpawnWave
            {
                waveId = id,
                waveName = "Wave " + id
            };
            
            WaveStage stage = new WaveStage
            {
                stageId = "stage_" + id + "_1",
                stageName = "Stage 1",
                stageDuration = duration,
                completionCondition = WaveStage.CompletionCondition.TimeElapsed
            };
            
            stage.ruleIds.AddRange(ruleIds);
            wave.stages.Add(stage);
            
            return wave;
        }
    }
} 