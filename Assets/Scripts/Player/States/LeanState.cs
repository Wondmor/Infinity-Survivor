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
            // ��ʼ����б״̬
            previousRotation = manager.Player.transform.forward;
        }

        public override void Update(float deltaTime)
        {
            // ������б��ͷ����ת
            CalculateRotationalAdditives(deltaTime);
        }

        public override void HandleInput()
        {
            // �������Ӱ����б������
        }

        public override void PhysicsUpdate(float deltaTime)
        {
            // ��б״̬����Ҫ�������
        }

        /// <summary>
        /// ������ת��صĶ�������
        /// </summary>
        private void CalculateRotationalAdditives(float deltaTime)
        {
            // ��ȡ��ǰ��ת
            currentRotation = manager.Player.transform.forward;
            
            // ������ת����
            float rotationRate = 0f;
            if (previousRotation != Vector3.zero)
            {
                rotationRate = Vector3.SignedAngle(currentRotation, previousRotation, Vector3.up) / deltaTime * -1f;
            }
             
            // ������б��
            float targetLeanAmount = rotationRate * 0.01f;
            leanAmount = Mathf.Lerp(leanAmount, targetLeanAmount, rotationSmoothTime * deltaTime);
            
            // ����ͷ����ת
            float targetHeadLookX = rotationRate * 0.005f;
            headLookX = Mathf.Lerp(headLookX, targetHeadLookX, rotationSmoothTime * deltaTime);
            
            // Ӧ����б��ͷ����ת����
            float leanMultiplier = leanCurve.Evaluate(Mathf.Abs(leanAmount));
            float headLookMultiplier = headLookCurve.Evaluate(Mathf.Abs(headLookX));
            
            // ���ö�������
            SetAnimatorFloat("LeanAmount", leanAmount * leanMultiplier);
            SetAnimatorFloat("HeadLookX", headLookX * headLookMultiplier);
            SetAnimatorFloat("HeadLookY", headLookY);
            
            // ���浱ǰ��תΪ��һ֡��ǰһ֡��ת
            previousRotation = currentRotation;
        }
    }
} 