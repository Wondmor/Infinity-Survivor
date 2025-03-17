using System.Collections.Generic;
using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 伤害信息类，包含一次伤害的所有相关数据
    /// </summary>
    public class DamageInfo
    {
        // 攻击者和防御者
        public GameObject attacker;  // 攻击者，可以为空（如环境伤害）
        public GameObject defender;  // 防御者，必须存在

        // 伤害标签，用于标识伤害类型
        public List<string> tags = new List<string>();

        // 伤害数据
        public DamageData damage;

        // 伤害角度（攻击方向）
        public Vector3 damageDirection;

        // 命中和暴击相关
        public float hitRate = 1.0f;      // 命中率
        public float criticalRate = 0.0f;  // 暴击率

        // 需要添加的Buff列表
        public List<AddBuffInfo> addBuffs = new List<AddBuffInfo>();

        // 是否已经处理过
        public bool isProcessed = false;

        /// <summary>
        /// 创建一个基本的伤害信息
        /// </summary>
        public DamageInfo(GameObject attacker, GameObject defender, DamageData damage)
        {
            this.attacker = attacker;
            this.defender = defender;
            this.damage = damage;
            
            // 如果攻击者和防御者之间有方向，记录方向
            if (attacker != null && defender != null)
            {
                this.damageDirection = (defender.transform.position - attacker.transform.position).normalized;
            }
            else
            {
                this.damageDirection = Vector3.forward;
            }
        }

        /// <summary>
        /// 添加伤害标签
        /// </summary>
        public void AddTag(string tag)
        {
            if (!tags.Contains(tag))
            {
                tags.Add(tag);
            }
        }

        /// <summary>
        /// 检查是否包含特定标签
        /// </summary>
        public bool HasTag(string tag)
        {
            return tags.Contains(tag);
        }

        /// <summary>
        /// 添加Buff信息
        /// </summary>
        public void AddBuffToCha(AddBuffInfo buffInfo)
        {
            addBuffs.Add(buffInfo);
        }

        /// <summary>
        /// 计算最终伤害（考虑命中和暴击）
        /// </summary>
        public DamageData CalculateFinalDamage()
        {
            // 检查是否命中
            if (Random.value > hitRate)
            {
                // 未命中，返回零伤害
                return new DamageData();
            }

            // 检查是否暴击
            bool isCritical = Random.value < criticalRate;
            
            // 复制伤害数据
            DamageData finalDamage = damage.Clone();
            
            // 如果暴击，增加伤害
            if (isCritical)
            {
                finalDamage.ApplyCritical();
            }
            
            return finalDamage;
        }
    }

    /// <summary>
    /// 伤害数据结构，包含不同类型的伤害值
    /// </summary>
    [System.Serializable]
    public class DamageData
    {
        // 基础伤害
        public float physical = 0;  // 物理伤害
        
        // 元素伤害
        public float fire = 0;      // 火焰伤害
        public float water = 0;     // 水系伤害
        public float earth = 0;     // 土系伤害
        public float wind = 0;      // 风系伤害
        public float lightning = 0; // 雷电伤害
        
        // 特殊伤害
        public float pure = 0;      // 纯粹伤害（无视防御）
        
        // 暴击倍率
        public float criticalMultiplier = 2.0f;

        /// <summary>
        /// 获取总伤害值
        /// </summary>
        public float GetTotalDamage()
        {
            return physical + fire + water + earth + wind + lightning + pure;
        }

        /// <summary>
        /// 应用暴击效果
        /// </summary>
        public void ApplyCritical()
        {
            physical *= criticalMultiplier;
            fire *= criticalMultiplier;
            water *= criticalMultiplier;
            earth *= criticalMultiplier;
            wind *= criticalMultiplier;
            lightning *= criticalMultiplier;
            // 纯粹伤害通常不受暴击影响
        }

        /// <summary>
        /// 创建伤害数据的副本
        /// </summary>
        public DamageData Clone()
        {
            return new DamageData
            {
                physical = this.physical,
                fire = this.fire,
                water = this.water,
                earth = this.earth,
                wind = this.wind,
                lightning = this.lightning,
                pure = this.pure,
                criticalMultiplier = this.criticalMultiplier
            };
        }
    }

    /// <summary>
    /// Buff添加信息
    /// </summary>
    public class AddBuffInfo
    {
        public string buffId;           // Buff的唯一标识符
        public GameObject target;        // 目标对象
        public float duration = -1;      // 持续时间，-1表示永久
        public int stacks = 1;           // 层数
        
        public AddBuffInfo(string buffId, GameObject target, float duration = -1, int stacks = 1)
        {
            this.buffId = buffId;
            this.target = target;
            this.duration = duration;
            this.stacks = stacks;
        }
    }
} 