using UnityEngine;

namespace TrianCatStudio
{
    public class CameraController : MonoBehaviour
    {
        [Header("目标设置")]
        [SerializeField] private Transform target;              // 跟随目标（玩家）
        [SerializeField] private Vector3 offsetPosition = new Vector3(0, 2, -5); // 相对目标的偏移位置

        [Header("跟随设置")]
        [SerializeField] private float followSpeed = 10f;       // 相机跟随速度
        [SerializeField] private float rotationSpeed = 2f;      // 视角旋转速度
        [SerializeField] private float minVerticalAngle = -30f; // 最小垂直角度
        [SerializeField] private float maxVerticalAngle = 60f;  // 最大垂直角度

        // 运行时变量
        private float currentRotationX = 0f;
        private float currentRotationY = 0f;
        private Vector3 currentVelocity;

        private void Start()
        {
            if (target == null)
            {
                // 尝试找到玩家
                var player = FindObjectOfType<Player>();
                if (player != null)
                {
                    target = player.transform;
                }
                else
                {
                    Debug.LogError("CameraController: 未找到目标对象！");
                    enabled = false;
                    return;
                }
            }

            // 初始化旋转角度
            Vector3 angles = transform.eulerAngles;
            currentRotationX = angles.y;
            currentRotationY = angles.x;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // 获取鼠标输入
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // 更新旋转角度
            currentRotationX += mouseX * rotationSpeed;
            currentRotationY -= mouseY * rotationSpeed; // 注意这里是减号，因为Unity的坐标系统
            currentRotationY = Mathf.Clamp(currentRotationY, minVerticalAngle, maxVerticalAngle);

            // 计算旋转
            Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);

            // 计算目标位置
            Vector3 targetPosition = target.position + rotation * offsetPosition;

            // 平滑移动相机
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, 1f / followSpeed);
            transform.rotation = rotation;
        }

        // 设置跟随目标
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        // 重置相机位置
        public void ResetCamera()
        {
            if (target != null)
            {
                transform.position = target.position + offsetPosition;
                currentRotationX = 0f;
                currentRotationY = 0f;
                transform.rotation = Quaternion.identity;
            }
        }
    }
} 