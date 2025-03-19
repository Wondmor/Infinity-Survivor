using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace TrianCatStudio
{
    /// <summary>
    /// 敌人寻路控制系统 - 负责处理敌人的移动和路径计算
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyPathfinding : MonoBehaviour
    {
        [Header("寻路设置")]
        [SerializeField] private float defaultSpeed = 3.5f;
        [SerializeField] private float chaseSpeed = 5f;
        [SerializeField] private float stoppingDistance = 1.2f;
        [SerializeField] private float obstacleAvoidanceRadius = 0.5f;
        [SerializeField] private float pathUpdateInterval = 0.25f;
        [SerializeField] private float smoothRotationSpeed = 5f;
        
        [Header("高级寻路")]
        [SerializeField] private bool useSmartPathfinding = true;
        [SerializeField] private float maxPathfindingTime = 1.0f; // 最长计算路径时间
        [SerializeField] private int wallDetectionRays = 8; // 墙体检测射线数量
        [SerializeField] private float wallAvoidanceDistance = 1.5f; // 墙体避让距离
        
        // 内部引用
        private NavMeshAgent navAgent;
        private Enemy enemy;
        private Transform target;
        private Vector3 currentDestination;
        private bool isPathValid = false;
        private bool isCalculatingPath = false;
        private bool isMovementPaused = false;
        
        // 路径状态
        private NavMeshPath currentPath;
        private Coroutine pathUpdateCoroutine;
        
        // 已知危险区域
        private System.Collections.Generic.List<DangerZone> dangerZones = new System.Collections.Generic.List<DangerZone>();
        
        /// <summary>
        /// 危险区域定义
        /// </summary>
        private class DangerZone
        {
            public Vector3 position;
            public float radius;
            public float dangerLevel; // 0-1，1最危险
            public float expirationTime; // 过期时间
            
            public DangerZone(Vector3 position, float radius, float dangerLevel, float duration)
            {
                this.position = position;
                this.radius = radius;
                this.dangerLevel = dangerLevel;
                this.expirationTime = Time.time + duration;
            }
            
            public bool IsExpired()
            {
                return Time.time > expirationTime;
            }
            
            public float GetDangerValueAt(Vector3 point)
            {
                float distance = Vector3.Distance(position, point);
                if (distance > radius) return 0;
                
                // 线性衰减危险值
                return dangerLevel * (1f - distance / radius);
            }
        }
        
        private void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            enemy = GetComponent<Enemy>();
            currentPath = new NavMeshPath();
            
            // 配置导航代理
            ConfigureNavAgent();
        }
        
        private void OnEnable()
        {
            // 启动寻路更新协程
            if (pathUpdateCoroutine == null)
            {
                pathUpdateCoroutine = StartCoroutine(UpdatePathCoroutine());
            }
        }
        
        private void OnDisable()
        {
            // 停止寻路更新协程
            if (pathUpdateCoroutine != null)
            {
                StopCoroutine(pathUpdateCoroutine);
                pathUpdateCoroutine = null;
            }
        }
        
        private void ConfigureNavAgent()
        {
            if (navAgent == null) return;
            
            navAgent.speed = defaultSpeed;
            navAgent.stoppingDistance = stoppingDistance;
            navAgent.radius = obstacleAvoidanceRadius;
            navAgent.updateRotation = false; // 我们使用自定义旋转
            navAgent.avoidancePriority = Random.Range(30, 60); // 随机避让优先级以避免卡住
            
            // 其他配置
            navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            navAgent.autoTraverseOffMeshLink = true;
        }
        
        private void Update()
        {
            if (isMovementPaused || navAgent == null) return;
            
            // 更新目标位置
            if (target != null && Vector3.Distance(currentDestination, target.position) > stoppingDistance)
            {
                SetDestination(target.position);
            }
            
            // 平滑旋转朝向移动方向
            if (navAgent.velocity.sqrMagnitude > 0.1f)
            {
                Vector3 lookDirection = navAgent.velocity.normalized;
                lookDirection.y = 0;
                
                if (lookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation, 
                        targetRotation, 
                        smoothRotationSpeed * Time.deltaTime
                    );
                }
            }
            
            // 更新危险区域
            UpdateDangerZones();
        }
        
        /// <summary>
        /// 设置移动目标位置
        /// </summary>
        public void SetDestination(Vector3 destination)
        {
            if (navAgent == null || !navAgent.isActiveAndEnabled || isMovementPaused)
                return;
            
            currentDestination = destination;
            
            if (useSmartPathfinding)
            {
                // 使用智能寻路，考虑危险区域
                CalculateSmartPath(destination);
            }
            else
            {
                // 直接使用NavMesh寻路
                navAgent.SetDestination(destination);
                isPathValid = navAgent.hasPath;
            }
        }
        
        /// <summary>
        /// 设置移动目标
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                SetDestination(target.position);
            }
        }
        
        /// <summary>
        /// 停止移动
        /// </summary>
        public void StopMoving()
        {
            if (navAgent != null && navAgent.isActiveAndEnabled)
            {
                navAgent.ResetPath();
                isPathValid = false;
            }
        }
        
        /// <summary>
        /// 暂停移动
        /// </summary>
        public void PauseMovement(bool pause)
        {
            isMovementPaused = pause;
            
            if (navAgent != null && navAgent.isActiveAndEnabled)
            {
                navAgent.isStopped = pause;
                if (pause)
                {
                    navAgent.velocity = Vector3.zero;
                }
            }
        }
        
        /// <summary>
        /// 设置移动速度
        /// </summary>
        public void SetSpeed(float speed)
        {
            if (navAgent != null)
            {
                navAgent.speed = speed;
            }
        }
        
        /// <summary>
        /// 设置为追击速度
        /// </summary>
        public void SetChaseSpeed()
        {
            SetSpeed(chaseSpeed);
        }
        
        /// <summary>
        /// 设置为默认速度
        /// </summary>
        public void SetDefaultSpeed()
        {
            SetSpeed(defaultSpeed);
        }
        
        /// <summary>
        /// 是否有有效路径
        /// </summary>
        public bool HasPath()
        {
            return navAgent != null && navAgent.hasPath && isPathValid;
        }
        
        /// <summary>
        /// 获取到目标的距离
        /// </summary>
        public float GetDistanceToDestination()
        {
            if (!HasPath()) return float.MaxValue;
            
            // 计算路径距离
            float distance = 0f;
            
            if (navAgent.path.corners.Length < 2)
            {
                return Vector3.Distance(transform.position, currentDestination);
            }
            
            // 计算路径各段距离之和
            for (int i = 0; i < navAgent.path.corners.Length - 1; i++)
            {
                distance += Vector3.Distance(navAgent.path.corners[i], navAgent.path.corners[i + 1]);
            }
            
            return distance;
        }
        
        /// <summary>
        /// 是否可以到达目标
        /// </summary>
        public bool CanReachDestination(Vector3 destination)
        {
            NavMeshPath tempPath = new NavMeshPath();
            bool canReach = NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, tempPath);
            
            return canReach && tempPath.status == NavMeshPathStatus.PathComplete;
        }
        
        /// <summary>
        /// 获取下一个路径点
        /// </summary>
        public Vector3 GetNextCorner()
        {
            if (!HasPath() || navAgent.path.corners.Length < 2)
                return transform.position;
                
            return navAgent.path.corners[1];
        }
        
        /// <summary>
        /// 获取最终路径点
        /// </summary>
        public Vector3 GetFinalDestination()
        {
            if (!HasPath()) return transform.position;
            
            int lastIdx = navAgent.path.corners.Length - 1;
            return navAgent.path.corners[lastIdx];
        }
        
        /// <summary>
        /// 添加一个危险区域
        /// </summary>
        public void AddDangerZone(Vector3 position, float radius, float dangerLevel, float duration)
        {
            dangerZones.Add(new DangerZone(position, radius, dangerLevel, duration));
            
            // 如果当前在这个危险区域内，可以立即重新计算路径
            if (useSmartPathfinding && 
                Vector3.Distance(transform.position, position) < radius * 1.5f && 
                HasPath())
            {
                CalculateSmartPath(currentDestination);
            }
        }
        
        /// <summary>
        /// 检查点是否安全
        /// </summary>
        public float GetPointDangerLevel(Vector3 point)
        {
            float maxDanger = 0f;
            
            foreach (var zone in dangerZones)
            {
                float dangerValue = zone.GetDangerValueAt(point);
                maxDanger = Mathf.Max(maxDanger, dangerValue);
            }
            
            return maxDanger;
        }
        
        #region 内部方法
        
        /// <summary>
        /// 更新危险区域
        /// </summary>
        private void UpdateDangerZones()
        {
            // 移除过期的危险区域
            dangerZones.RemoveAll(zone => zone.IsExpired());
        }
        
        /// <summary>
        /// 计算智能路径，避开危险区域
        /// </summary>
        private void CalculateSmartPath(Vector3 destination)
        {
            if (isCalculatingPath) return;
            
            isCalculatingPath = true;
            StartCoroutine(CalculateSmartPathCoroutine(destination));
        }
        
        /// <summary>
        /// 智能寻路协程
        /// </summary>
        private IEnumerator CalculateSmartPathCoroutine(Vector3 destination)
        {
            // 检查常规路径是否可行
            NavMeshPath directPath = new NavMeshPath();
            bool hasDirectPath = NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, directPath);
            
            // 检查直接路径是否穿过危险区域
            bool pathThroughDanger = PathGoesThroughDanger(directPath);
            
            if (hasDirectPath && !pathThroughDanger)
            {
                // 如果直接路径安全，使用它
                navAgent.SetPath(directPath);
                isPathValid = true;
                isCalculatingPath = false;
                yield break;
            }
            
            // 尝试找到绕过危险区域的路径
            Vector3 bestDestination = destination;
            float lowestDanger = CalculatePathDanger(directPath);
            float startTime = Time.time;
            
            // 如果直接路径不安全，尝试找替代路径
            for (int i = 0; i < 8; i++)
            {
                // 超时检查
                if (Time.time - startTime > maxPathfindingTime)
                {
                    break;
                }
                
                // 在目标周围找点
                float angle = i * (360f / 8);
                float radius = Random.Range(3f, 10f);
                Vector3 offset = new Vector3(
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
                    0,
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius
                );
                
                Vector3 alternativeDestination = destination + offset;
                
                // 确保点在NavMesh上
                if (NavMesh.SamplePosition(alternativeDestination, out NavMeshHit hit, radius, NavMesh.AllAreas))
                {
                    alternativeDestination = hit.position;
                    
                    // 检查到这个点的路径
                    NavMeshPath alternatePath = new NavMeshPath();
                    if (NavMesh.CalculatePath(transform.position, alternativeDestination, NavMesh.AllAreas, alternatePath))
                    {
                        float pathDanger = CalculatePathDanger(alternatePath);
                        
                        if (pathDanger < lowestDanger)
                        {
                            lowestDanger = pathDanger;
                            bestDestination = alternativeDestination;
                        }
                    }
                }
                
                yield return null; // 分帧计算
            }
            
            // 使用找到的最佳路径
            navAgent.SetDestination(bestDestination);
            isPathValid = navAgent.hasPath;
            isCalculatingPath = false;
        }
        
        /// <summary>
        /// 检查路径是否穿过危险区域
        /// </summary>
        private bool PathGoesThroughDanger(NavMeshPath path)
        {
            if (path.corners.Length < 2 || dangerZones.Count == 0)
                return false;
            
            // 检查路径各段
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Vector3 start = path.corners[i];
                Vector3 end = path.corners[i + 1];
                float segmentLength = Vector3.Distance(start, end);
                
                // 在路径段上取样点检查
                int samples = Mathf.CeilToInt(segmentLength / 0.5f) + 1;
                for (int j = 0; j < samples; j++)
                {
                    float t = j / (float)(samples - 1);
                    Vector3 samplePoint = Vector3.Lerp(start, end, t);
                    
                    // 检查点是否在危险区域
                    foreach (var zone in dangerZones)
                    {
                        if (zone.GetDangerValueAt(samplePoint) > 0.5f) // 超过50%危险度
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 计算路径的总危险度
        /// </summary>
        private float CalculatePathDanger(NavMeshPath path)
        {
            if (path.corners.Length < 2 || dangerZones.Count == 0)
                return 0f;
            
            float totalDanger = 0f;
            float totalDistance = 0f;
            
            // 计算路径各段的危险度加权和
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Vector3 start = path.corners[i];
                Vector3 end = path.corners[i + 1];
                float segmentLength = Vector3.Distance(start, end);
                totalDistance += segmentLength;
                
                // 在路径段上取样点检查
                int samples = Mathf.CeilToInt(segmentLength / 0.5f) + 1;
                for (int j = 0; j < samples; j++)
                {
                    float t = j / (float)(samples - 1);
                    Vector3 samplePoint = Vector3.Lerp(start, end, t);
                    
                    float pointDanger = 0f;
                    
                    // 累加所有危险区域在该点的危险度
                    foreach (var zone in dangerZones)
                    {
                        pointDanger += zone.GetDangerValueAt(samplePoint);
                    }
                    
                    totalDanger += pointDanger * (segmentLength / samples);
                }
            }
            
            // 归一化为路径平均危险度
            return totalDistance > 0 ? totalDanger / totalDistance : 0f;
        }
        
        /// <summary>
        /// 更新寻路路径的协程
        /// </summary>
        private IEnumerator UpdatePathCoroutine()
        {
            WaitForSeconds wait = new WaitForSeconds(pathUpdateInterval);
            
            while (true)
            {
                yield return wait;
                
                // 如果有目标且不在暂停状态，更新路径
                if (target != null && !isMovementPaused && !isCalculatingPath)
                {
                    SetDestination(target.position);
                }
            }
        }
        
        /// <summary>
        /// 检测墙壁并避开
        /// </summary>
        private void AvoidWalls()
        {
            float rayLength = wallAvoidanceDistance;
            bool wallDetected = false;
            Vector3 avoidDirection = Vector3.zero;
            
            // 发射多个射线检测墙壁
            for (int i = 0; i < wallDetectionRays; i++)
            {
                float angle = i * (360f / wallDetectionRays);
                Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * transform.forward;
                
                Debug.DrawRay(transform.position + Vector3.up * 0.5f, rayDirection * rayLength, Color.yellow);
                
                if (Physics.Raycast(transform.position + Vector3.up * 0.5f, rayDirection, out RaycastHit hit, rayLength))
                {
                    if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Obstacle"))
                    {
                        wallDetected = true;
                        // 计算避开方向（远离墙）
                        avoidDirection += -rayDirection.normalized * (rayLength - hit.distance) / rayLength;
                    }
                }
            }
            
            // 如果检测到墙壁，调整目标位置
            if (wallDetected && avoidDirection != Vector3.zero)
            {
                avoidDirection.Normalize();
                Vector3 targetPosition = transform.position + avoidDirection * 2f;
                
                // 向前移动一点，以防卡住
                targetPosition += transform.forward;
                
                if (NavMesh.SamplePosition(targetPosition, out NavMeshHit navHit, 3f, NavMesh.AllAreas))
                {
                    navAgent.SetDestination(navHit.position);
                }
            }
        }
        
        #endregion
        
        #region 调试
        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            // 绘制当前路径
            if (navAgent != null && navAgent.hasPath)
            {
                Gizmos.color = isPathValid ? Color.green : Color.red;
                
                Vector3 previousCorner = transform.position;
                
                foreach (Vector3 corner in navAgent.path.corners)
                {
                    Gizmos.DrawLine(previousCorner, corner);
                    Gizmos.DrawSphere(corner, 0.2f);
                    previousCorner = corner;
                }
            }
            
            // 绘制危险区域
            foreach (var zone in dangerZones)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                Gizmos.DrawSphere(zone.position, zone.radius);
            }
        }
        
        #endregion
    }
} 