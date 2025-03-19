using System;
using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 地面检测组件 - 负责处理所有与地面检测相关的逻辑
    /// </summary>
    public class GroundDetector : MonoBehaviour
    {
        // 地面检测参数
        [Header("检测设置")]
        [SerializeField] private GroundDetectionMethod detectionMethod = GroundDetectionMethod.MultiRaycast; // 地面检测方法
        [SerializeField] private float groundCheckDistance = 0.3f; // 地面检测距离
        [SerializeField] private float groundCheckRadius = 0.4f;   // 地面检测半径
        [SerializeField] private int groundRayCount = 5;           // 地面检测射线数量
        [SerializeField] private float maxSlopeAngle = 45f;        // 最大可行走斜坡角度
        [SerializeField] private LayerMask groundLayer = -1;       // 地面层级
        
        [Header("楼梯处理")]
        [SerializeField] private bool enableStairsHandling = true; // 启用楼梯处理
        [SerializeField] private float stepHeight = 0.3f;          // 最大台阶高度
        [SerializeField] private float stepSmoothing = 0.1f;       // 台阶平滑过渡时间
        [SerializeField] private float forwardCheckDistance = 0.5f; // 前方检测距离
        
        [Header("调试选项")]
        [SerializeField] private bool showDebugRays = true;        // 是否显示调试射线
        
        // 运行时状态
        public bool IsGrounded { get; private set; }
        public float LastGroundedTime { get; private set; }
        private bool wasGrounded;
        private Rigidbody rb;
        
        // 事件系统
        public delegate void GroundStateChangedHandler(bool isGrounded);
        public event GroundStateChangedHandler OnGroundStateChanged;
        
        // 着陆事件
        public delegate void LandingHandler(float fallSpeed);
        public event LandingHandler OnLanding;
        public event LandingHandler OnHardLanding;
        
        // 地面检测方法枚举
        public enum GroundDetectionMethod
        {
            SingleRaycast,  // 单射线检测
            MultiRaycast,   // 多射线检测
            SphereCast      // 球形检测
        }
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("GroundDetector需要Rigidbody组件");
                enabled = false;
            }
        }
        
        private void Update()
        {
            UpdateGroundDetection();
        }
        
        /// <summary>
        /// 更新地面检测
        /// </summary>
        public void UpdateGroundDetection()
        {
            wasGrounded = IsGrounded;
            
            // 改进的地面检测方法
            bool groundDetected = CheckGrounded();
            
            // 如果刚刚跳跃，强制设置为非地面状态
            if (rb != null && rb.velocity.y > 0.5f)
            {
                groundDetected = false;
            }
            
            IsGrounded = groundDetected;
            
            // 更新着地时间
            if (IsGrounded)
            {
                // 检测是否刚刚落地
                if (!wasGrounded && rb != null)
                {
                    float fallSpeed = -rb.velocity.y;
                    // 如果从高处落下，触发硬着陆事件
                    if (fallSpeed > 5f)
                    {
                        OnHardLanding?.Invoke(fallSpeed);
                    }
                    else if (fallSpeed > 0.5f)
                    {
                        OnLanding?.Invoke(fallSpeed);
                    }
                }
                
                LastGroundedTime = Time.time;
            }
            
            // 触发地面状态变化事件
            if (IsGrounded != wasGrounded)
            {
                OnGroundStateChanged?.Invoke(IsGrounded);
                
                // 调试输出
                Debug.Log($"地面状态变化: {wasGrounded} -> {IsGrounded}, 垂直速度: {(rb != null ? rb.velocity.y : 0)}");
            }
        }
        
        /// <summary>
        /// 地面检测方法选择器
        /// </summary>
        private bool CheckGrounded()
        {
            switch (detectionMethod)
            {
                case GroundDetectionMethod.SingleRaycast:
                    return CheckGroundedSingleRay();
                case GroundDetectionMethod.MultiRaycast:
                    return CheckGroundedMultiRay();
                case GroundDetectionMethod.SphereCast:
                    return CheckGroundedSphere();
                default:
                    return CheckGroundedMultiRay();
            }
        }
        
        /// <summary>
        /// 单射线检测地面
        /// </summary>
        private bool CheckGroundedSingleRay()
        {
            float rayOriginHeight = 0.1f;
            Vector3 rayOrigin = transform.position + Vector3.up * rayOriginHeight;
            
            if (showDebugRays)
            {
                Debug.DrawRay(rayOrigin, Vector3.down * groundCheckDistance, Color.yellow);
            }
            
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
            {
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                return slopeAngle < maxSlopeAngle;
            }
            
            return false;
        }
        
        /// <summary>
        /// 多射线检测地面
        /// </summary>
        private bool CheckGroundedMultiRay()
        {
            // 检测起点高度 - 增加高度以避免误判
            float rayOriginHeight = 0.15f;
            Vector3 rayOrigin = transform.position + Vector3.up * rayOriginHeight;
            
            // 减小地面检测距离，使检测更精确
            float effectiveGroundCheckDistance = groundCheckDistance * 0.8f;
            
            // 中心射线检测
            if (showDebugRays)
            {
                Debug.DrawRay(rayOrigin, Vector3.down * effectiveGroundCheckDistance, Color.green);
            }
            
            // 如果垂直速度明显向上，直接认为不在地面上
            if (rb != null && rb.velocity.y > 0.5f)
            {
                return false;
            }
            
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, effectiveGroundCheckDistance, groundLayer))
            {
                // 计算角度，处理斜坡
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (slopeAngle < maxSlopeAngle) // 可配置的最大斜坡角度
                {
                    return true;
                }
            }
            
            // 周围多点射线检测，处理楼梯等不规则地形
            for (int i = 0; i < groundRayCount; i++)
            {
                // 计算射线方向（围绕角色的圆形分布）
                float angle = i * (360f / groundRayCount);
                Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
                Vector3 rayStart = rayOrigin + direction * groundCheckRadius;
                
                // 绘制调试射线
                if (showDebugRays)
                {
                    Debug.DrawRay(rayStart, Vector3.down * effectiveGroundCheckDistance, Color.red);
                }
                
                // 射线检测
                if (Physics.Raycast(rayStart, Vector3.down, out hit, effectiveGroundCheckDistance, groundLayer))
                {
                    float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                    if (slopeAngle < maxSlopeAngle) // 可配置的最大斜坡角度
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 球形检测地面
        /// </summary>
        private bool CheckGroundedSphere()
        {
            // 检测起点高度
            float rayOriginHeight = 0.1f;
            Vector3 sphereCenter = transform.position + Vector3.up * rayOriginHeight;
            
            // 绘制调试球体
            if (showDebugRays)
            {
                Debug.DrawLine(sphereCenter, sphereCenter + Vector3.down * groundCheckDistance, Color.blue);
                // 在Scene视图中绘制球体
                #if UNITY_EDITOR
                UnityEditor.Handles.color = Color.blue;
                UnityEditor.Handles.DrawWireDisc(sphereCenter, Vector3.up, groundCheckRadius);
                UnityEditor.Handles.DrawWireDisc(sphereCenter + Vector3.down * groundCheckDistance, Vector3.up, groundCheckRadius);
                #endif
            }
            
            // 球形检测
            if (Physics.SphereCast(sphereCenter, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
            {
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                return slopeAngle < maxSlopeAngle;
            }
            
            return false;
        }
        
        /// <summary>
        /// 处理楼梯
        /// </summary>
        public void HandleStairs(Vector3 moveDirection)
        {
            if (!enableStairsHandling || !IsGrounded || moveDirection.magnitude < 0.1f)
                return;
            
            // 前方检测点
            Vector3 forwardPoint = transform.position + moveDirection.normalized * forwardCheckDistance;
            
            // 检测前方是否有障碍物
            if (Physics.Raycast(forwardPoint, Vector3.down, out RaycastHit downHit, stepHeight * 2f, groundLayer))
            {
                // 从障碍物上方发射射线向下
                Vector3 stepCheckStart = new Vector3(forwardPoint.x, transform.position.y + stepHeight, forwardPoint.z);
                
                if (showDebugRays)
                {
                    Debug.DrawLine(forwardPoint, forwardPoint + Vector3.down * stepHeight * 2f, Color.yellow);
                    Debug.DrawLine(stepCheckStart, stepCheckStart + Vector3.down * stepHeight * 2f, Color.cyan);
                }
                
                // 检测是否是台阶
                if (Physics.Raycast(stepCheckStart, Vector3.down, out RaycastHit stepHit, stepHeight * 2f, groundLayer))
                {
                    // 计算高度差
                    float heightDifference = Mathf.Abs(downHit.point.y - stepHit.point.y);
                    
                    // 如果高度差在台阶范围内
                    if (heightDifference > 0.01f && heightDifference < stepHeight)
                    {
                        // 判断是上楼梯还是下楼梯
                        bool isAscending = stepHit.point.y > downHit.point.y;
                        
                        if (isAscending)
                        {
                            // 上楼梯：稍微提升角色位置
                            Vector3 targetPosition = new Vector3(
                                transform.position.x,
                                stepHit.point.y + 0.05f, // 稍微抬高一点，避免卡住
                                transform.position.z
                            );
                            
                            // 平滑过渡
                            transform.position = Vector3.Lerp(
                                transform.position,
                                targetPosition,
                                stepSmoothing
                            );
                        }
                        else
                        {
                            // 下楼梯：保持接地状态，避免下楼时出现跳跃
                            IsGrounded = true;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 在编辑器中可视化地面检测范围
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // 检测起点高度
            float rayOriginHeight = 0.1f;
            Vector3 rayOrigin = transform.position + Vector3.up * rayOriginHeight;
            
            // 设置Gizmos颜色
            Gizmos.color = Color.green;
            
            // 根据不同的检测方法绘制不同的Gizmos
            switch (detectionMethod)
            {
                case GroundDetectionMethod.SingleRaycast:
                    // 单射线检测
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundCheckDistance);
                    break;
                    
                case GroundDetectionMethod.MultiRaycast:
                    // 中心射线
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundCheckDistance);
                    
                    // 周围射线
                    Gizmos.color = Color.red;
                    for (int i = 0; i < groundRayCount; i++)
                    {
                        float angle = i * (360f / groundRayCount);
                        Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                        Vector3 rayStart = rayOrigin + direction * groundCheckRadius;
                        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * groundCheckDistance);
                    }
                    
                    // 绘制检测半径
                    Gizmos.color = new Color(1, 0, 0, 0.2f);
                    DrawCircle(rayOrigin, groundCheckRadius, 32);
                    break;
                    
                case GroundDetectionMethod.SphereCast:
                    // 球形检测
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundCheckDistance);
                    
                    // 绘制球体
                    Gizmos.DrawWireSphere(rayOrigin, groundCheckRadius);
                    Gizmos.DrawWireSphere(rayOrigin + Vector3.down * groundCheckDistance, groundCheckRadius);
                    
                    // 绘制球体路径
                    Gizmos.color = new Color(0, 0, 1, 0.2f);
                    DrawCylinder(
                        rayOrigin, 
                        rayOrigin + Vector3.down * groundCheckDistance,
                        groundCheckRadius
                    );
                    break;
            }
            
            // 如果启用了楼梯处理，绘制楼梯检测
            if (enableStairsHandling)
            {
                Gizmos.color = Color.cyan;
                Vector3 forwardPoint = transform.position + transform.forward * forwardCheckDistance;
                Gizmos.DrawLine(forwardPoint, forwardPoint + Vector3.down * stepHeight * 2f);
                
                Vector3 stepCheckStart = new Vector3(forwardPoint.x, transform.position.y + stepHeight, forwardPoint.z);
                Gizmos.DrawLine(stepCheckStart, stepCheckStart + Vector3.down * stepHeight * 2f);
            }
        }
        
        // 辅助方法：绘制圆形
        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            float angle = 0f;
            float angleStep = 360f / segments;
            Vector3 previousPoint = center + new Vector3(Mathf.Sin(0) * radius, 0, Mathf.Cos(0) * radius);
            
            for (int i = 0; i <= segments; i++)
            {
                angle += angleStep;
                float radian = angle * Mathf.Deg2Rad;
                Vector3 currentPoint = center + new Vector3(Mathf.Sin(radian) * radius, 0, Mathf.Cos(radian) * radius);
                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }
        }
        
        // 辅助方法：绘制圆柱体
        private static void DrawCylinder(Vector3 start, Vector3 end, float radius)
        {
            Vector3 up = (end - start).normalized * radius;
            Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
            Vector3 right = Vector3.Cross(up, forward).normalized * radius;
            
            // 绘制圆柱体侧面
            for (float i = 0; i < 360; i += 30)
            {
                float radian = i * Mathf.Deg2Rad;
                float radian2 = (i + 30) * Mathf.Deg2Rad;
                
                Vector3 point1 = start + Mathf.Sin(radian) * right + Mathf.Cos(radian) * forward;
                Vector3 point2 = end + Mathf.Sin(radian) * right + Mathf.Cos(radian) * forward;
                Vector3 point3 = end + Mathf.Sin(radian2) * right + Mathf.Cos(radian2) * forward;
                Vector3 point4 = start + Mathf.Sin(radian2) * right + Mathf.Cos(radian2) * forward;
                
                Gizmos.DrawLine(point1, point2);
                Gizmos.DrawLine(point2, point3);
                Gizmos.DrawLine(point3, point4);
                Gizmos.DrawLine(point4, point1);
            }
        }
    }
} 