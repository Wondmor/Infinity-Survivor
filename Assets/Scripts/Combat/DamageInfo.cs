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
            
            // 设置伤害来源
            if(damage != null)
            {
                damage.source = attacker;
            }
            
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

        // 静态创建方法
        
        /// <summary>
        /// 创建物理伤害信息
        /// </summary>
        public static DamageInfo CreatePhysical(GameObject attacker, GameObject defender, float damage)
        {
            return new DamageInfo(attacker, defender, DamageData.CreatePhysical(damage));
        }
        
        /// <summary>
        /// 创建火焰伤害信息
        /// </summary>
        public static DamageInfo CreateFire(GameObject attacker, GameObject defender, float damage)
        {
            return new DamageInfo(attacker, defender, DamageData.CreateFire(damage));
        }
        
        /// <summary>
        /// 创建冰冻伤害信息
        /// </summary>
        public static DamageInfo CreateIce(GameObject attacker, GameObject defender, float damage)
        {
            return new DamageInfo(attacker, defender, DamageData.CreateIce(damage));
        }
        
        /// <summary>
        /// 创建雷电伤害信息
        /// </summary>
        public static DamageInfo CreateLightning(GameObject attacker, GameObject defender, float damage)
        {
            return new DamageInfo(attacker, defender, DamageData.CreateLightning(damage));
        }
        
        /// <summary>
        /// 创建毒素伤害信息
        /// </summary>
        public static DamageInfo CreatePoison(GameObject attacker, GameObject defender, float damage)
        {
            return new DamageInfo(attacker, defender, DamageData.CreatePoison(damage));
        }
        
        /// <summary>
        /// 创建纯粹伤害信息（无视防御）
        /// </summary>
        public static DamageInfo CreatePure(GameObject attacker, GameObject defender, float damage)
        {
            return new DamageInfo(attacker, defender, DamageData.CreatePure(damage));
        }
        
        /// <summary>
        /// 创建混合伤害信息
        /// </summary>
        public static DamageInfo CreateMixed(GameObject attacker, GameObject defender, 
                                          float physical = 0, float fire = 0, float ice = 0, 
                                          float lightning = 0, float poison = 0, float pure = 0)
        {
            return new DamageInfo(attacker, defender, DamageData.CreateMixed(physical, fire, ice, lightning, poison, pure));
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
        public float water = 0;     // 水系伤害（冰冻）
        public float earth = 0;     // 土系伤害
        public float wind = 0;      // 风系伤害
        public float lightning = 0; // 雷电伤害
        public float poison = 0;    // 毒素伤害
        
        // 特殊伤害
        public float pure = 0;      // 纯粹伤害（无视防御）
        
        // 伤害来源
        public GameObject source;   // 伤害来源对象
        
        // 暴击倍率
        public float criticalMultiplier = 2.0f;

        /// <summary>
        /// 获取总伤害值
        /// </summary>
        public float GetTotalDamage()
        {
            return physical + fire + water + earth + wind + lightning + poison + pure;
        }
        
        /// <summary>
        /// 创建物理伤害
        /// </summary>
        public static DamageData CreatePhysical(float damage)
        {
            return new DamageData { physical = damage };
        }
        
        /// <summary>
        /// 创建火焰伤害
        /// </summary>
        public static DamageData CreateFire(float damage)
        {
            return new DamageData { fire = damage };
        }
        
        /// <summary>
        /// 创建冰冻伤害
        /// </summary>
        public static DamageData CreateIce(float damage)
        {
            return new DamageData { water = damage };
        }
        
        /// <summary>
        /// 创建雷电伤害
        /// </summary>
        public static DamageData CreateLightning(float damage)
        {
            return new DamageData { lightning = damage };
        }
        
        /// <summary>
        /// 创建毒素伤害
        /// </summary>
        public static DamageData CreatePoison(float damage)
        {
            return new DamageData { poison = damage };
        }
        
        /// <summary>
        /// 创建纯粹伤害（无视防御）
        /// </summary>
        public static DamageData CreatePure(float damage)
        {
            return new DamageData { pure = damage };
        }
        
        /// <summary>
        /// 创建混合伤害
        /// </summary>
        public static DamageData CreateMixed(float physical = 0, float fire = 0, float ice = 0, 
                                            float lightning = 0, float poison = 0, float pure = 0)
        {
            return new DamageData 
            { 
                physical = physical,
                fire = fire,
                water = ice, // 水系等同于冰冻
                lightning = lightning,
                poison = poison,
                pure = pure
            };
        }
        
        /// <summary>
        /// 获取主要伤害类型
        /// </summary>
        public DamageType GetMainDamageType()
        {
            // 创建伤害类型和对应伤害值的字典
            var damageValues = new System.Collections.Generic.Dictionary<DamageType, float>
            {
                { DamageType.Physical, physical },
                { DamageType.Fire, fire },
                { DamageType.Ice, water },      // 水系伤害对应冰冻伤害类型
                { DamageType.Lightning, lightning },
                { DamageType.Poison, poison }
            };
            
            // 默认为物理伤害
            DamageType mainType = DamageType.Physical;
            float highestDamage = physical;
            
            // 找出伤害值最高的类型
            foreach(var pair in damageValues)
            {
                if(pair.Value > highestDamage)
                {
                    highestDamage = pair.Value;
                    mainType = pair.Key;
                }
            }
            
            // 如果纯粹伤害很高，考虑使用真实伤害类型
            if(pure > highestDamage)
            {
                return DamageType.True;
            }
            
            return mainType;
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
            poison *= criticalMultiplier;
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
                poison = this.poison,
                pure = this.pure,
                source = this.source,
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