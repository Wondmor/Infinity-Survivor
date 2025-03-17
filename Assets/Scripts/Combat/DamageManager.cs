using System.Collections.Generic;
using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 伤害管理器，处理所有伤害流程
    /// </summary>
    public class DamageManager : MonoBehaviour
    {
        // 单例实例
        private static DamageManager _instance;
        public static DamageManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("DamageManager");
                    _instance = go.AddComponent<DamageManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // 待处理的伤害队列
        private Queue<DamageInfo> damageQueue = new Queue<DamageInfo>();

        // 是否正在处理伤害
        private bool isProcessing = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            // 如果队列中有伤害且当前没有在处理，则开始处理
            if (damageQueue.Count > 0 && !isProcessing)
            {
                ProcessNextDamage();
            }
        }

        /// <summary>
        /// 添加伤害到处理队列
        /// </summary>
        public void AddDamage(DamageInfo damageInfo)
        {
            if (damageInfo == null || damageInfo.defender == null)
            {
                Debug.LogWarning("尝试添加无效的伤害信息");
                return;
            }

            damageQueue.Enqueue(damageInfo);
            
            // 如果当前没有在处理伤害，立即开始处理
            if (!isProcessing)
            {
                ProcessNextDamage();
            }
        }

        /// <summary>
        /// 处理队列中的下一个伤害
        /// </summary>
        private void ProcessNextDamage()
        {
            if (damageQueue.Count == 0)
            {
                isProcessing = false;
                return;
            }

            isProcessing = true;
            DamageInfo damageInfo = damageQueue.Dequeue();

            // 检查防御者是否还存在
            if (damageInfo.defender == null)
            {
                isProcessing = false;
                ProcessNextDamage();
                return;
            }

            // 处理伤害流程
            ProcessDamage(damageInfo);
        }

        /// <summary>
        /// 处理单个伤害信息
        /// </summary>
        private void ProcessDamage(DamageInfo damageInfo)
        {
            // 1. 触发攻击者的OnHit事件
            if (damageInfo.attacker != null)
            {
                TriggerAttackerOnHit(damageInfo);
            }

            // 2. 触发防御者的BeHurt事件
            TriggerDefenderBeHurt(damageInfo);

            // 3. 计算最终伤害
            DamageData finalDamage = damageInfo.CalculateFinalDamage();

            // 4. 应用伤害
            ApplyDamage(damageInfo.defender, finalDamage);

            // 5. 检查是否击杀
            bool isKilled = CheckIfKilled(damageInfo.defender);

            // 6. 触发击杀或被击杀事件
            if (isKilled)
            {
                if (damageInfo.attacker != null)
                {
                    TriggerAttackerOnKill(damageInfo);
                }
                TriggerDefenderBeKilled(damageInfo);
            }

            // 7. 应用所有待添加的Buff
            ApplyBuffs(damageInfo);

            // 8. 标记伤害已处理
            damageInfo.isProcessed = true;

            // 9. 处理下一个伤害
            isProcessing = false;
            ProcessNextDamage();
        }

        /// <summary>
        /// 触发攻击者的OnHit事件
        /// </summary>
        private void TriggerAttackerOnHit(DamageInfo damageInfo)
        {
            // 获取攻击者上的所有Buff组件
            IBuffOnHit[] buffs = damageInfo.attacker.GetComponents<IBuffOnHit>();
            foreach (var buff in buffs)
            {
                buff.OnHit(damageInfo);
            }
        }

        /// <summary>
        /// 触发防御者的BeHurt事件
        /// </summary>
        private void TriggerDefenderBeHurt(DamageInfo damageInfo)
        {
            // 获取防御者上的所有Buff组件
            IBuffBeHurt[] buffs = damageInfo.defender.GetComponents<IBuffBeHurt>();
            foreach (var buff in buffs)
            {
                buff.BeHurt(damageInfo);
            }
        }

        /// <summary>
        /// 应用伤害到防御者
        /// </summary>
        private void ApplyDamage(GameObject defender, DamageData damage)
        {
            // 获取防御者的生命值组件
            IDamageable damageable = defender.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage.GetTotalDamage());
                
                // 显示伤害数字（可选）
                ShowDamageNumber(defender, damage);
            }
        }

        /// <summary>
        /// 显示伤害数字
        /// </summary>
        private void ShowDamageNumber(GameObject target, DamageData damage)
        {
            // 这里可以实现伤害数字的显示逻辑
            // 例如实例化一个UI预制体，显示伤害数值
            Debug.Log($"对 {target.name} 造成 {damage.GetTotalDamage()} 点伤害");
        }

        /// <summary>
        /// 检查目标是否被击杀
        /// </summary>
        private bool CheckIfKilled(GameObject target)
        {
            // 获取目标的生命值组件
            IHealth health = target.GetComponent<IHealth>();
            if (health != null)
            {
                return health.IsDead();
            }
            return false;
        }

        /// <summary>
        /// 触发攻击者的OnKill事件
        /// </summary>
        private void TriggerAttackerOnKill(DamageInfo damageInfo)
        {
            // 获取攻击者上的所有Buff组件
            IBuffOnKill[] buffs = damageInfo.attacker.GetComponents<IBuffOnKill>();
            foreach (var buff in buffs)
            {
                buff.OnKill(damageInfo);
            }
        }

        /// <summary>
        /// 触发防御者的BeKilled事件
        /// </summary>
        private void TriggerDefenderBeKilled(DamageInfo damageInfo)
        {
            // 获取防御者上的所有Buff组件
            IBuffBeKilled[] buffs = damageInfo.defender.GetComponents<IBuffBeKilled>();
            foreach (var buff in buffs)
            {
                buff.BeKilled(damageInfo);
            }
        }

        /// <summary>
        /// 应用所有待添加的Buff
        /// </summary>
        private void ApplyBuffs(DamageInfo damageInfo)
        {
            foreach (var buffInfo in damageInfo.addBuffs)
            {
                if (buffInfo.target != null)
                {
                    // 这里应该调用Buff系统的AddBuff方法
                    // BuffSystem.Instance.AddBuff(buffInfo);
                    Debug.Log($"添加Buff {buffInfo.buffId} 到 {buffInfo.target.name}，持续 {buffInfo.duration} 秒，{buffInfo.stacks} 层");
                }
            }
        }
    }

    // Buff接口定义
    public interface IBuffOnHit
    {
        void OnHit(DamageInfo damageInfo);
    }

    public interface IBuffBeHurt
    {
        void BeHurt(DamageInfo damageInfo);
    }

    public interface IBuffOnKill
    {
        void OnKill(DamageInfo damageInfo);
    }

    public interface IBuffBeKilled
    {
        void BeKilled(DamageInfo damageInfo);
    }

    // 生命值接口
    public interface IHealth
    {
        float GetCurrentHealth();
        float GetMaxHealth();
        bool IsDead();
    }
} 