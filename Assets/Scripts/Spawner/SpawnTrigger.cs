using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace TrianCatStudio
{
    /// <summary>
    /// 区域触发器 - 检测玩家进入特定区域后触发刷怪
    /// </summary>
    public class SpawnTrigger : MonoBehaviour
    {
        [Header("触发器设置")]
        [SerializeField] private TriggerType triggerType = TriggerType.OnEnter;  // 触发类型
        [SerializeField] private LayerMask targetLayers;                          // 目标层（通常是玩家层）
        [SerializeField] private bool oneTimeOnly = true;                         // 是否只触发一次
        [SerializeField] private float cooldown = 0f;                             // 触发冷却时间
        [SerializeField] private bool activateOnStart = false;                    // 是否在开始时就激活
        [SerializeField] private string triggerId = "";                           // 触发器ID
        
        [Header("可视化设置")]
        [SerializeField] private Color gizmoColor = new Color(1f, 0.5f, 0, 0.3f); // Gizmo颜色
        [SerializeField] private bool showGizmo = true;                            // 是否显示Gizmo
        
        [Header("波次设置")]
        [SerializeField] private string[] waveIds;                                 // 要触发的波次ID
        [SerializeField] private bool triggerAllWaves = false;                     // 是否触发所有波次
        
        [Header("事件")]
        [SerializeField] private UnityEvent onTriggerActivated;                    // 触发激活事件
        [SerializeField] private UnityEvent onAllWavesCompleted;                   // 所有波次完成事件
        
        // 内部状态
        private bool isActivated = false;              // 是否已激活
        private bool isTriggered = false;              // 是否已触发
        private float lastTriggerTime = -99999f;       // 上次触发时间
        private int wavesCompleted = 0;                // 已完成波次数量
        private HashSet<Transform> entitiesInTrigger = new HashSet<Transform>();  // 在触发器中的实体
        
        // 组件引用
        private Collider triggerCollider;              // 触发器碰撞体
        
        /// <summary>
        /// 触发类型枚举
        /// </summary>
        public enum TriggerType
        {
            OnEnter,        // 进入时触发
            OnStay,         // 停留时触发
            OnExit,         // 退出时触发
            Manual          // 手动触发
        }
        
        private void Awake()
        {
            // 获取碰撞体组件
            triggerCollider = GetComponent<Collider>();
            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;  // 确保是触发器
            }
            
            // 如果未设置目标层，默认使用Player层
            if (targetLayers == 0)
            {
                targetLayers = LayerMask.GetMask("Player");
            }
            
            // 如果未设置触发器ID，使用GameObject名称
            if (string.IsNullOrEmpty(triggerId))
            {
                triggerId = gameObject.name;
            }
        }
        
        private void Start()
        {
            // 根据设置决定是否自动激活
            isActivated = activateOnStart;
            
            // 订阅波次完成事件
            if (waveIds != null && waveIds.Length > 0)
            {
                SpawnController.Instance.OnWaveCompleted += HandleWaveCompleted;
            }
        }
        
        private void OnDestroy()
        {
            // 取消订阅事件
            if (SpawnController.Instance != null)
            {
                SpawnController.Instance.OnWaveCompleted -= HandleWaveCompleted;
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!isActivated || (oneTimeOnly && isTriggered))
                return;
                
            // 检查是否是目标层
            if (((1 << other.gameObject.layer) & targetLayers) == 0)
                return;
                
            entitiesInTrigger.Add(other.transform);
            
            if (triggerType == TriggerType.OnEnter)
            {
                TryTrigger();
            }
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (!isActivated || (oneTimeOnly && isTriggered))
                return;
                
            // 检查是否是目标层
            if (((1 << other.gameObject.layer) & targetLayers) == 0)
                return;
                
            if (triggerType == TriggerType.OnStay)
            {
                TryTrigger();
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            // 检查是否是目标层
            if (((1 << other.gameObject.layer) & targetLayers) == 0)
                return;
                
            entitiesInTrigger.Remove(other.transform);
            
            if (isActivated && triggerType == TriggerType.OnExit && !oneTimeOnly && isTriggered)
            {
                TryTrigger();
            }
        }
        
        /// <summary>
        /// 尝试触发
        /// </summary>
        private void TryTrigger()
        {
            // 检查冷却时间
            if (Time.time - lastTriggerTime < cooldown)
                return;
                
            // 更新状态
            isTriggered = true;
            lastTriggerTime = Time.time;
            
            // 启动波次
            if (waveIds != null && waveIds.Length > 0)
            {
                foreach (var waveId in waveIds)
                {
                    if (!string.IsNullOrEmpty(waveId))
                    {
                        SpawnController.Instance.StartWaveById(waveId, transform.position);
                    }
                }
            }
            
            // 触发全局波次
            if (triggerAllWaves)
            {
                SpawnController.Instance.StartAllActiveWaves();
            }
            
            // 触发Unity事件
            onTriggerActivated?.Invoke();
            
            // 如果只触发一次，禁用碰撞器
            if (oneTimeOnly && triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }
        }
        
        /// <summary>
        /// 处理波次完成事件
        /// </summary>
        private void HandleWaveCompleted(string waveId)
        {
            // 检查是否是我们关心的波次
            if (waveIds != null && System.Array.IndexOf(waveIds, waveId) >= 0)
            {
                wavesCompleted++;
                
                // 检查是否所有波次都已完成
                if (wavesCompleted >= waveIds.Length)
                {
                    // 触发所有波次完成事件
                    onAllWavesCompleted?.Invoke();
                    
                    // 取消订阅，防止重复调用
                    SpawnController.Instance.OnWaveCompleted -= HandleWaveCompleted;
                }
            }
        }
        
        /// <summary>
        /// 手动激活触发器
        /// </summary>
        public void Activate()
        {
            isActivated = true;
            
            // 如果是手动触发类型且区域内有实体，立即触发
            if (triggerType == TriggerType.Manual && entitiesInTrigger.Count > 0)
            {
                TryTrigger();
            }
        }
        
        /// <summary>
        /// 手动停用触发器
        /// </summary>
        public void Deactivate()
        {
            isActivated = false;
        }
        
        /// <summary>
        /// 手动触发
        /// </summary>
        public void ManualTrigger()
        {
            if (!isActivated || (oneTimeOnly && isTriggered))
                return;
                
            TryTrigger();
        }
        
        /// <summary>
        /// 重置触发器
        /// </summary>
        public void Reset()
        {
            isTriggered = false;
            wavesCompleted = 0;
            
            // 重新启用碰撞器
            if (triggerCollider != null)
            {
                triggerCollider.enabled = true;
            }
            
            // 重新订阅事件
            SpawnController.Instance.OnWaveCompleted -= HandleWaveCompleted;
            SpawnController.Instance.OnWaveCompleted += HandleWaveCompleted;
        }
        
        private void OnDrawGizmos()
        {
            if (!showGizmo)
                return;
                
            Gizmos.color = gizmoColor;
            
            // 获取碰撞体
            Collider col = GetComponent<Collider>();
            
            if (col is BoxCollider)
            {
                BoxCollider box = (BoxCollider)col;
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.matrix = oldMatrix;
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphere = (SphereCollider)col;
                Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius * transform.lossyScale.x);
            }
            else if (col is CapsuleCollider)
            {
                // 简化为球体绘制
                CapsuleCollider capsule = (CapsuleCollider)col;
                Gizmos.DrawSphere(transform.position + capsule.center, capsule.radius * transform.lossyScale.x);
            }
            else
            {
                // 如果没有碰撞体或不是支持的类型，绘制一个默认球体
                Gizmos.DrawSphere(transform.position, 1f);
            }
        }
    }
} 