using UnityEngine;
using UnityEngine.AI;
using System;

namespace TrianCatStudio
{
    /// <summary>
    /// 敌人类 - 挂载在敌人游戏对象上，用于管理组件和连接EnemyController
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(EnemyPerception))]
    [RequireComponent(typeof(EnemyPathfinding))]
    [RequireComponent(typeof(EnemyStateManager))]
    public class Enemy : Entity, IPoolable
    {
        [Header("基本设置")]
        [SerializeField] private string enemyId;
        
        [Header("组件引用")]
        [SerializeField] private Transform modelTransform;
        [SerializeField] private Animator animator;
        [SerializeField] private NavMeshAgent navAgent;
        [SerializeField] private PooledObject pooledObject;
        [SerializeField] private DamageModifier damageModifier;
        [SerializeField] private EnemyStateManager stateManager;
        
        // 内部状态
        private bool isInitialized = false;
        private bool isAttacking = false;
        private float lastAttackTime;
        
        // 目标引用
        private Transform targetTransform;
        
        // 敌人数据
        private EnemyData enemyData;
        
        // 唯一ID
        private string uniqueId;
        
        // 事件
        public new event Action<Enemy> OnDeath;
        public event Action<GameObject> OnAttack;
        
        // 公共属性
        public string EnemyId => enemyId;
        public string UniqueId => uniqueId;
        public EnemyData EnemyData => enemyData;
        public Transform ModelTransform => modelTransform;
        public Animator EnemyAnimator => animator;
        public NavMeshAgent NavAgent => navAgent;
        public Transform Target => targetTransform;
        public new bool IsDead => base.IsDead();
        public float CurrentHealth => base.GetCurrentHealth();
        public float MaxHealth => base.GetMaxHealth();
        public EnemyStateManager StateManager => stateManager;
        
        protected override void Awake()
        {
            base.Awake();
            
            // 获取或添加必需组件
            if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();
            if (pooledObject == null) pooledObject = GetComponent<PooledObject>() ?? gameObject.AddComponent<PooledObject>();
            if (damageModifier == null) damageModifier = GetComponent<DamageModifier>() ?? gameObject.AddComponent<DamageModifier>();
            if (stateManager == null) stateManager = GetComponent<EnemyStateManager>() ?? gameObject.AddComponent<EnemyStateManager>();
            
            // 如果未指定模型变换，使用自身
            if (modelTransform == null) modelTransform = transform;
            
            // 如果未指定动画控制器，尝试获取
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }
        
        private void Start()
        {
            if (!isInitialized)
            {
                // 使用默认配置初始化
                Initialize(System.Guid.NewGuid().ToString(), null);
            }
        }
        
        /// <summary>
        /// 初始化敌人
        /// </summary>
        public void Initialize(string uid, EnemyData data)
        {
            uniqueId = uid;
            
            // 设置敌人数据
            if (data != null)
            {
                enemyData = data;
                enemyId = data.enemyId;
                
                // 应用数据到组件
                ApplyDataToComponents();
            }
            
            // 订阅死亡事件
            base.OnDeath += HandleDeath;
            
            // 重置攻击状态
            isAttacking = false;
            lastAttackTime = -GetAttackCooldown();
            
            isInitialized = true;
            
            // 状态机在其Awake中已初始化
        }
        
        /// <summary>
        /// 将敌人数据应用到各组件
        /// </summary>
        private void ApplyDataToComponents()
        {
            if (enemyData != null)
            {
                // 设置生命值
                SetMaxHealth(enemyData.maxHealth);
                
                // 设置抗性
                if (damageModifier != null)
                {
                    damageModifier.SetPhysicalResistance(enemyData.physicalDefense);
                    damageModifier.SetFireResistance(enemyData.fireResistance);
                    damageModifier.SetIceResistance(enemyData.iceResistance);
                    damageModifier.SetLightningResistance(enemyData.lightningResistance);
                    damageModifier.SetPoisonResistance(enemyData.poisonResistance);
                }
                
                // 设置导航代理
                if (navAgent != null)
                {
                    navAgent.speed = enemyData.moveSpeed;
                    navAgent.stoppingDistance = enemyData.attackRange * 0.8f;
                    navAgent.updateRotation = true;
                }
            }
        }
        
        /// <summary>
        /// 处理死亡事件
        /// </summary>
        private void HandleDeath(GameObject killer)
        {
            // 触发自身的死亡事件
            OnDeath?.Invoke(this);
            
            // 禁用碰撞器和NavMeshAgent
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }
            
            if (navAgent != null)
            {
                navAgent.enabled = false;
            }
            
            // 延迟回收/销毁
            if (pooledObject != null)
            {
                pooledObject.StartAutoRelease(3f); // 3秒后回收
            }
            else
            {
                Destroy(gameObject, 3f); // 3秒后销毁
            }
        }
        
        /// <summary>
        /// 设置目标
        /// </summary>
        public void SetTarget(Transform target)
        {
            targetTransform = target;
        }
        
        /// <summary>
        /// 移动到位置
        /// </summary>
        public void MoveTo(Vector3 position)
        {
            if (IsDead || navAgent == null) return;
            
            navAgent.SetDestination(position);
        }
        
        /// <summary>
        /// 停止移动
        /// </summary>
        public void StopMoving()
        {
            if (navAgent == null) return;
            
            navAgent.ResetPath();
            navAgent.velocity = Vector3.zero;
        }
        
        /// <summary>
        /// 尝试攻击
        /// </summary>
        public bool TryAttack()
        {
            if (IsDead || isAttacking || Time.time - lastAttackTime < GetAttackCooldown())
                return false;
                
            // 开始攻击
            isAttacking = true;
            lastAttackTime = Time.time;
            
            // 播放攻击动画
            if (animator != null)
                animator.SetTrigger("Attack");
                
            // 执行伤害判定
            Invoke("DealDamage", 0.5f); // 延迟调用
            
            return true;
        }
        
        /// <summary>
        /// 造成伤害（由动画事件或延迟调用）
        /// </summary>
        private void DealDamage()
        {
            if (IsDead) return;
            
            if (targetTransform != null)
            {
                // 检查目标是否在攻击范围内
                float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);
                if (distanceToTarget <= GetAttackRange())
                {
                    // 触发攻击事件
                    OnAttack?.Invoke(targetTransform.gameObject);
                    
                    // 对目标造成伤害
                    var damageable = targetTransform.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        float damage = GetAttackDamage();
                        damageable.TakeDamage(damage, DamageType.Physical, gameObject);
                    }
                }
            }
            
            // 攻击完成
            isAttacking = false;
        }
        
        /// <summary>
        /// 检查目标是否在攻击范围内
        /// </summary>
        public bool IsTargetInAttackRange()
        {
            if (targetTransform == null) return false;
            return Vector3.Distance(transform.position, targetTransform.position) <= GetAttackRange();
        }
        
        /// <summary>
        /// 检查目标是否在检测范围内
        /// </summary>
        public bool IsTargetInDetectRange()
        {
            if (targetTransform == null) return false;
            return Vector3.Distance(transform.position, targetTransform.position) <= GetDetectRange();
        }
        
        #region 获取属性（优先使用数据，否则使用默认值）
        
        public float GetAttackDamage()
        {
            return enemyData != null ? enemyData.attackDamage : 10f;
        }
        
        public float GetAttackRange()
        {
            return enemyData != null ? enemyData.attackRange : 1.5f;
        }
        
        public float GetDetectRange()
        {
            return enemyData != null ? enemyData.detectRange : 10f;
        }
        
        public float GetAttackCooldown()
        {
            return enemyData != null ? enemyData.attackCooldown : 1.5f;
        }
        
        public float GetPatrolRange()
        {
            return enemyData != null ? enemyData.patrolRange : 10f;
        }
        
        public float GetChaseTimeout()
        {
            return enemyData != null ? enemyData.chaseTimeout : 10f;
        }
        
        #endregion
        
        #region 对象池接口实现
        
        public void OnPoolGet()
        {
            // 从对象池获取时
            gameObject.SetActive(true);
            
            // 重置状态（注意：不要在这里完成初始化，由调用者完成）
            isInitialized = false;
        }
        
        public void OnPoolRelease()
        {
            // 放回对象池时
            isInitialized = false;
            targetTransform = null;
            
            // 解除事件
            base.OnDeath -= HandleDeath;
            
            OnDeath = null;
            OnAttack = null;
            
            gameObject.SetActive(false);
        }
        
        #endregion
        
        #region MonoBehaviour生命周期
        
        private void OnDestroy()
        {
            // 解除事件
            base.OnDeath -= HandleDeath;
        }
        
        #endregion
    }
} 