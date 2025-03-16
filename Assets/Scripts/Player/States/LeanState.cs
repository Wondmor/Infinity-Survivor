using UnityEngine;

namespace TrianCatStudio
{
    public class LeanState : PlayerBaseState
    {
        private float leanAmount = 0f;
        private float headLookX = 0f;
        private float headLookY = 0f;
        private Vector3 previousRotation;
        private Vector3 currentRotation;
        
        private float rotationSmoothTime = 0.1f;
        private AnimationCurve leanCurve = AnimationCurve.Linear(0, 0, 1, 1);
        private AnimationCurve headLookCurve = AnimationCurve.Linear(0, 0, 1, 1);

        public LeanState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Additive;
        }

        public override void OnEnter()
        {
            // 初始化倾斜状态
            previousRotation = manager.Player.transform.forward;
        }

        public override void Update(float deltaTime)
        {
            // 计算倾斜和头部旋转
            CalculateRotationalAdditives(deltaTime);
        }

        public override void HandleInput()
        {
            // 处理可能影响倾斜的输入
        }

        public override void PhysicsUpdate(float deltaTime)
        {
            // 倾斜状态不需要物理更新
        }

        /// <summary>
        /// 计算旋转相关的动画参数
        /// </summary>
        private void CalculateRotationalAdditives(float deltaTime)
        {
            // 获取当前旋转
            currentRotation = manager.Player.transform.forward;
            
            // 计算旋转速率
            float rotationRate = 0f;
            if (previousRotation != Vector3.zero)
            {
                rotationRate = Vector3.SignedAngle(currentRotation, previousRotation, Vector3.up) / deltaTime * -1f;
            }
             
            // 计算倾斜量
            float targetLeanAmount = rotationRate * 0.01f;
            leanAmount = Mathf.Lerp(leanAmount, targetLeanAmount, rotationSmoothTime * deltaTime);
            
            // 计算头部旋转
            float targetHeadLookX = rotationRate * 0.005f;
            headLookX = Mathf.Lerp(headLookX, targetHeadLookX, rotationSmoothTime * deltaTime);
            
            // 应用倾斜和头部旋转曲线
            float leanMultiplier = leanCurve.Evaluate(Mathf.Abs(leanAmount));
            float headLookMultiplier = headLookCurve.Evaluate(Mathf.Abs(headLookX));
            
            // 设置动画参数
            SetAnimatorFloat("LeanAmount", leanAmount * leanMultiplier);
            SetAnimatorFloat("HeadLookX", headLookX * headLookMultiplier);
            SetAnimatorFloat("HeadLookY", headLookY);
            
            // 保存当前旋转为下一帧的前一帧旋转
            previousRotation = currentRotation;
        }
    }
} 