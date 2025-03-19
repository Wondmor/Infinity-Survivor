using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TrianCatStudio
{
    /// <summary>
    /// 敌人感知系统 - 负责处理敌人的感知功能，包括视觉、听觉等
    /// </summary>
    public class EnemyPerception : MonoBehaviour
    {
        #region 事件定义
        
        // 定义事件委托
        public delegate void TargetDetectedHandler(Transform target);
        public delegate void NoiseHeardHandler(Vector3 position, float volume);
        
        // 定义事件
        public event TargetDetectedHandler OnTargetDetected;
        public event TargetDetectedHandler OnTargetLost;
        public event NoiseHeardHandler OnNoiseHeard;
        
        #endregion
        
        #region 视觉设置
        
        [Header("视觉设置")]
        [SerializeField] private bool useVision = true;
        [SerializeField] private float viewDistance = 10f; // 视野距离
        [SerializeField] private float viewAngle = 90f; // 视野角度
        [SerializeField] private float eyeHeight = 1.6f; // 视线高度
        [SerializeField] private LayerMask targetLayers; // 目标层
        [SerializeField] private LayerMask obstacleLayers; // 视线阻挡层
        [SerializeField] private float visionCheckInterval = 0.2f; // 视觉检查间隔
        [SerializeField] private float targetLostDelay = 0.5f; // 目标丢失延迟
        
        #endregion
        
        #region 听觉设置
        
        [Header("听觉设置")]
        [SerializeField] private bool useHearing = true;
        [SerializeField] private float hearingRange = 15f; // 听觉范围
        [SerializeField] private float noiseThreshold = 0.1f; // 噪音阈值
        [SerializeField] private float alertNoiseMultiplier = 2f; // 警戒状态下听觉范围倍率
        
        #endregion
        
        #region 触觉设置
        
        [Header("触觉设置")]
        [SerializeField] private bool useTouching = true;
        [SerializeField] private float touchRange = 1.5f; // 触摸范围
        
        #endregion
        
        #region 记忆设置
        
        [Header("记忆设置")]
        [SerializeField] private float shortTermMemoryTime = 10f; // 短期记忆持续时间
        [SerializeField] private int maxRememberedPoints = 5; // 最大记忆点数量
        
        #endregion
        
        #region 调试设置
        
        [Header("调试设置")]
        [SerializeField] private bool showVisionGizmos = true;
        [SerializeField] private bool showHearingGizmos = true;
        [SerializeField] private bool logDetection = false;
        
        #endregion
        
        // 内部状态
        private Enemy enemy;
        private Coroutine visionCheckCoroutine;
        private Coroutine targetLostCoroutine;
        private List<MemoryPoint> memoryPoints = new List<MemoryPoint>();
        private bool isAlert = false;
        
        // 当前感知的目标
        private Transform currentTarget;
        public Transform CurrentTarget => currentTarget;
        
        // 目标最后已知位置
        private Vector3 lastKnownTargetPosition;
        public Vector3 LastKnownTargetPosition => lastKnownTargetPosition;
        
        // 是否能看到目标
        private bool isTargetVisible = false;
        public bool IsTargetVisible => isTargetVisible;
        
        // 记忆点结构
        private class MemoryPoint
        {
            public Vector3 position;
            public float importance; // 0-1，1最重要
            public float expirationTime;
            public MemoryPointType type;
            
            public enum MemoryPointType
            {
                Visual,
                Audio,
                Damage,
                Custom
            }
            
            public MemoryPoint(Vector3 position, float importance, float duration, MemoryPointType type)
            {
                this.position = position;
                this.importance = importance;
                this.expirationTime = Time.time + duration;
                this.type = type;
            }
            
            public bool IsExpired()
            {
                return Time.time > expirationTime;
            }
        }
        
        private void Awake()
        {
            enemy = GetComponent<Enemy>();
        }
        
        private void OnEnable()
        {
            StartPerception();
        }
        
        private void OnDisable()
        {
            StopPerception();
        }
        
        private void Start()
        {
            // 初始化感知组件
            InitializePerception();
        }
        
        private void Update()
        {
            // 更新记忆点
            UpdateMemoryPoints();
        }
        
        #region 公共接口
        
        /// <summary>
        /// 启动感知系统
        /// </summary>
        public void StartPerception()
        {
            if (useVision && visionCheckCoroutine == null)
            {
                visionCheckCoroutine = StartCoroutine(VisionCheckCoroutine());
            }
        }
        
        /// <summary>
        /// 停止感知系统
        /// </summary>
        public void StopPerception()
        {
            if (visionCheckCoroutine != null)
            {
                StopCoroutine(visionCheckCoroutine);
                visionCheckCoroutine = null;
            }
            
            if (targetLostCoroutine != null)
            {
                StopCoroutine(targetLostCoroutine);
                targetLostCoroutine = null;
            }
        }
        
        /// <summary>
        /// 设置警戒状态
        /// </summary>
        public void SetAlertState(bool alert)
        {
            isAlert = alert;
        }
        
        /// <summary>
        /// 强制感知目标
        /// </summary>
        public void ForcePerceiveTarget(Transform target)
        {
            if (target == null) return;
            
            // 强制设置当前目标
            if (currentTarget != target)
            {
                currentTarget = target;
                lastKnownTargetPosition = target.position;
                isTargetVisible = true;
                
                // 触发检测到目标事件
                OnTargetDetected?.Invoke(target);
            }
        }
        
        /// <summary>
        /// 处理听到的声音
        /// </summary>
        public void HearNoise(Vector3 position, float volume)
        {
            if (!useHearing) return;
            
            // 如果声音太小，忽略
            if (volume < noiseThreshold) return;
            
            // 计算听觉范围（警戒状态下增强）
            float currentHearingRange = isAlert ? hearingRange * alertNoiseMultiplier : hearingRange;
            
            // 检查声音是否在听觉范围内
            float distance = Vector3.Distance(transform.position, position);
            if (distance <= currentHearingRange)
            {
                // 声音音量随距离衰减
                float adjustedVolume = volume * (1 - distance / currentHearingRange);
                
                // 添加到记忆点
                AddMemoryPoint(position, adjustedVolume, shortTermMemoryTime, MemoryPoint.MemoryPointType.Audio);
                
                // 触发听到声音事件
                OnNoiseHeard?.Invoke(position, adjustedVolume);
                
                if (logDetection)
                {
                    Debug.Log($"{gameObject.name} 听到声音: 位置 {position}, 音量 {adjustedVolume}");
                }
            }
        }
        
        /// <summary>
        /// 处理受到的伤害（可以作为感知线索）
        /// </summary>
        public void PerceiveDamage(Vector3 sourcePosition)
        {
            // 伤害触觉始终处理
            
            // 添加到记忆点（很高的重要性）
            AddMemoryPoint(sourcePosition, 1f, shortTermMemoryTime, MemoryPoint.MemoryPointType.Damage);
            
            // 可能的扩展：尝试搜索伤害来源方向
            StartCoroutine(LookTowardsDamageSource(sourcePosition));
        }
        
        /// <summary>
        /// 获取最重要的记忆点
        /// </summary>
        public bool GetMostImportantMemoryPoint(out Vector3 position)
        {
            position = transform.position;
            
            if (memoryPoints.Count == 0)
                return false;
            
            // 按重要性排序
            memoryPoints.Sort((a, b) => b.importance.CompareTo(a.importance));
            
            position = memoryPoints[0].position;
            return true;
        }
        
        /// <summary>
        /// 手动添加一个记忆点
        /// </summary>
        public void AddCustomMemoryPoint(Vector3 position, float importance, float duration)
        {
            AddMemoryPoint(position, importance, duration, MemoryPoint.MemoryPointType.Custom);
        }
        
        #endregion
        
        #region 内部方法
        
        private void InitializePerception()
        {
            // 初始化时可能需要的设置
            if (targetLayers == 0)
            {
                // 默认目标层是Player层
                targetLayers = LayerMask.GetMask("Player");
            }
            
            if (obstacleLayers == 0)
            {
                // 默认障碍物层是Default和Obstacle层
                obstacleLayers = LayerMask.GetMask("Default", "Obstacle");
            }
        }
        
        private void UpdateMemoryPoints()
        {
            // 移除过期的记忆点
            memoryPoints.RemoveAll(point => point.IsExpired());
            
            // 如果记忆点超过最大数量，移除最不重要的
            if (memoryPoints.Count > maxRememberedPoints)
            {
                memoryPoints.Sort((a, b) => a.importance.CompareTo(b.importance));
                memoryPoints.RemoveRange(0, memoryPoints.Count - maxRememberedPoints);
            }
        }
        
        private void AddMemoryPoint(Vector3 position, float importance, float duration, MemoryPoint.MemoryPointType type)
        {
            memoryPoints.Add(new MemoryPoint(position, importance, duration, type));
        }
        
        #endregion
        
        #region 视觉感知
        
        private IEnumerator VisionCheckCoroutine()
        {
            WaitForSeconds wait = new WaitForSeconds(visionCheckInterval);
            
            while (true)
            {
                yield return wait;
                
                CheckVision();
            }
        }
        
        private void CheckVision()
        {
            if (!useVision) return;
            
            // 仅在非战斗状态或未发现目标时搜索新目标
            if (currentTarget == null)
            {
                SearchForTargets();
            }
            else
            {
                // 检查当前目标是否仍然可见
                isTargetVisible = CheckTargetVisibility(currentTarget);
                
                if (isTargetVisible)
                {
                    // 更新最后已知位置
                    lastKnownTargetPosition = currentTarget.position;
                    
                    // 如果之前目标不可见，现在可见了，触发事件
                    if (targetLostCoroutine != null)
                    {
                        StopCoroutine(targetLostCoroutine);
                        targetLostCoroutine = null;
                        OnTargetDetected?.Invoke(currentTarget);
                    }
                }
                else if (targetLostCoroutine == null)
                {
                    // 目标丢失，启动延迟处理
                    targetLostCoroutine = StartCoroutine(TargetLostDelayCoroutine());
                }
            }
        }
        
        private void SearchForTargets()
        {
            // 首先检查Player标签的对象
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && CheckTargetVisibility(player.transform))
            {
                DetectTarget(player.transform);
                return;
            }
            
            // 检查指定层的碰撞体
            Collider[] colliders = Physics.OverlapSphere(transform.position, viewDistance, targetLayers);
            
            foreach (var collider in colliders)
            {
                // 检查是否是有效目标
                if (IsValidTarget(collider.transform) && CheckTargetVisibility(collider.transform))
                {
                    DetectTarget(collider.transform);
                    break;
                }
            }
        }
        
        private bool IsValidTarget(Transform target)
        {
            // 检查目标是否有效（例如，是否是Player或具有指定组件）
            return target.CompareTag("Player") || target.GetComponent<Player>() != null;
        }
        
        // 检查目标是否在视野范围内
        private bool CheckTargetVisibility(Transform target)
        {
            if (target == null) return false;
            
            Vector3 directionToTarget = target.position - transform.position;
            float distanceToTarget = directionToTarget.magnitude;
            
            // 检查距离
            if (distanceToTarget > viewDistance) return false;
            
            // 检查角度
            Vector3 forward = transform.forward;
            directionToTarget.y = 0; // 忽略垂直角度差
            forward.y = 0;
            float angle = Vector3.Angle(forward, directionToTarget);
            if (angle > viewAngle * 0.5f) return false;
            
            // 检查障碍物
            Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;
            Vector3 targetCenter = target.position + Vector3.up * (target.GetComponent<CharacterController>()?.height * 0.5f ?? 1f);
            
            // 从眼睛位置到目标中心发射射线
            Ray ray = new Ray(eyePosition, targetCenter - eyePosition);
            
            // 可视化射线
            Debug.DrawRay(eyePosition, targetCenter - eyePosition, Color.blue, visionCheckInterval);
            
            if (Physics.Raycast(ray, out RaycastHit hit, distanceToTarget, obstacleLayers))
            {
                // 射线碰到了其他东西，而不是目标
                if (hit.transform != target && !hit.transform.IsChildOf(target))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private void DetectTarget(Transform target)
        {
            // 如果这是新目标，触发事件
            if (currentTarget != target)
            {
                currentTarget = target;
                lastKnownTargetPosition = target.position;
                isTargetVisible = true;
                
                // 添加视觉记忆点
                AddMemoryPoint(target.position, 1f, shortTermMemoryTime, MemoryPoint.MemoryPointType.Visual);
                
                // 触发检测到目标事件
                OnTargetDetected?.Invoke(target);
                
                if (logDetection)
                {
                    Debug.Log($"{gameObject.name} 检测到目标: {target.name}");
                }
            }
        }
        
        private IEnumerator TargetLostDelayCoroutine()
        {
            // 延迟处理目标丢失
            yield return new WaitForSeconds(targetLostDelay);
            
            // 添加最后已知位置作为记忆点
            AddMemoryPoint(lastKnownTargetPosition, 0.8f, shortTermMemoryTime, MemoryPoint.MemoryPointType.Visual);
            
            // 触发目标丢失事件
            if (currentTarget != null)
            {
                Transform lostTarget = currentTarget;
                OnTargetLost?.Invoke(lostTarget);
                
                if (logDetection)
                {
                    Debug.Log($"{gameObject.name} 丢失目标: {lostTarget.name}");
                }
            }
            
            isTargetVisible = false;
            targetLostCoroutine = null;
        }
        
        private IEnumerator LookTowardsDamageSource(Vector3 sourcePosition)
        {
            // 获取伤害来源方向
            Vector3 direction = (sourcePosition - transform.position).normalized;
            direction.y = 0;
            
            // 计算目标旋转
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                float rotationTime = 0.5f;
                float startTime = Time.time;
                Quaternion startRotation = transform.rotation;
                
                // 平滑旋转
                while (Time.time < startTime + rotationTime)
                {
                    float t = (Time.time - startTime) / rotationTime;
                    transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                    yield return null;
                }
                
                transform.rotation = targetRotation;
            }
        }
        
        #endregion
        
        #region 听觉感知
        
        // 全局静态方法，供其他对象调用来发出声音
        public static void MakeNoise(Vector3 position, float volume, float radius)
        {
            // 查找范围内的所有敌人感知组件
            Collider[] colliders = Physics.OverlapSphere(position, radius);
            
            foreach (var collider in colliders)
            {
                EnemyPerception perception = collider.GetComponentInParent<EnemyPerception>();
                if (perception != null)
                {
                    // 让敌人感知到声音
                    perception.HearNoise(position, volume);
                }
            }
        }
        
        #endregion
        
        #region Gizmos
        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            // 绘制视野
            if (showVisionGizmos && useVision)
            {
                DrawVisionGizmos();
            }
            
            // 绘制听觉范围
            if (showHearingGizmos && useHearing)
            {
                DrawHearingGizmos();
            }
            
            // 绘制记忆点
            DrawMemoryPoints();
        }
        
        private void DrawVisionGizmos()
        {
            float halfFOV = viewAngle * 0.5f;
            Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;
            
            // 绘制视野扇形
            Gizmos.color = isTargetVisible ? Color.red : Color.yellow;
            Vector3 leftRayDir = Quaternion.Euler(0, -halfFOV, 0) * transform.forward;
            Vector3 rightRayDir = Quaternion.Euler(0, halfFOV, 0) * transform.forward;
            
            Gizmos.DrawRay(eyePosition, leftRayDir * viewDistance);
            Gizmos.DrawRay(eyePosition, rightRayDir * viewDistance);
            
            // 绘制视野弧线
            Vector3 prevPos = eyePosition + leftRayDir * viewDistance;
            float step = 5f;
            for (float angle = -halfFOV + step; angle <= halfFOV; angle += step)
            {
                Vector3 nextDir = Quaternion.Euler(0, angle, 0) * transform.forward;
                Vector3 nextPos = eyePosition + nextDir * viewDistance;
                Gizmos.DrawLine(prevPos, nextPos);
                prevPos = nextPos;
            }
            
            // 如果有目标，绘制到目标的线
            if (currentTarget != null && isTargetVisible)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(eyePosition, currentTarget.position);
            }
        }
        
        private void DrawHearingGizmos()
        {
            float currentHearingRange = isAlert ? hearingRange * alertNoiseMultiplier : hearingRange;
            
            // 绘制听觉范围
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawWireSphere(transform.position, currentHearingRange);
        }
        
        private void DrawMemoryPoints()
        {
            foreach (var point in memoryPoints)
            {
                // 根据记忆点类型选择颜色
                switch (point.type)
                {
                    case MemoryPoint.MemoryPointType.Visual:
                        Gizmos.color = Color.green;
                        break;
                    case MemoryPoint.MemoryPointType.Audio:
                        Gizmos.color = Color.cyan;
                        break;
                    case MemoryPoint.MemoryPointType.Damage:
                        Gizmos.color = Color.red;
                        break;
                    case MemoryPoint.MemoryPointType.Custom:
                        Gizmos.color = Color.magenta;
                        break;
                }
                
                // 绘制记忆点
                Gizmos.DrawSphere(point.position, 0.3f + point.importance * 0.3f);
                
                // 绘制到记忆点的线
                Gizmos.DrawLine(transform.position, point.position);
            }
        }
        
        #endregion
    }
} 