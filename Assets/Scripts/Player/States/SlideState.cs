using UnityEngine;

namespace TrianCatStudio
{
    public class SlideState : PlayerBaseState
    {
        private float slideSpeed = 8f; // ������ʼ�ٶ�
        private float slideDuration = 0.8f; // ��������ʱ��
        private float slideDeceleration = 5f; // �������ٶ�
        private float minSpeedToSlide = 3f; // ��С�����ٶ�
        private float slideTimer = 0f; // ������ʱ��
        private Vector3 slideDirection; // ��������
        private float currentSlideSpeed; // ��ǰ�����ٶ�
        
        public override bool CanBeInterrupted => false;

        public SlideState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Action;
        }

        public override void OnEnter()
        {
            Debug.Log("SlideState.OnEnter: ���뻬��״̬");
            
            // ��ʼ����������
            slideTimer = 0f;
            
            // ��ȡ��ǰ�ƶ�������Ϊ��������
            slideDirection = manager.Player.transform.forward;
            
            // ���ó�ʼ�����ٶ�
            currentSlideSpeed = slideSpeed;
            
            // ʹ��AnimController������������
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.TriggerSlide();
            }
            else
            {
                // ���÷�����ֱ������Animator����
                SetAnimatorTrigger("Slide");
            }
            
            // ���������������ײ��߶�
            // ���磺manager.Player.GetComponent<CapsuleCollider>().height = slideHeight;
        }
        
        public override void OnExit()
        {
            Debug.Log("SlideState.OnExit: �˳�����״̬");
            
            // �ָ���ײ��߶�
            // ���磺manager.Player.GetComponent<CapsuleCollider>().height = originalHeight;
            
            // ����ٶ���Ȼ�㹻�������¶�״̬������ָ�����״̬
            if (currentSlideSpeed > minSpeedToSlide / 2)
            {
                manager.TriggerCrouch(true);
            }
        }
        
        public override void Update(float deltaTime)
        {
            // ���»�����ʱ��
            slideTimer += deltaTime;
            
            // ���㵱ǰ�����ٶȣ���ʱ����٣�
            currentSlideSpeed = Mathf.Max(0, slideSpeed - slideDeceleration * slideTimer);
            
            // �������ʱ��������ٶȹ��ͣ��˳�����״̬
            if (slideTimer >= slideDuration || currentSlideSpeed < minSpeedToSlide)
            {
                manager.ChangeLayerState(StateLayer, null);
                return;
            }
        }
        
        public override void HandleInput()
        {
            // ���������в������������
        }
        
        public override void PhysicsUpdate(float deltaTime)
        {
            // Ӧ�û����ٶ�
            Vector3 slideVelocity = slideDirection * currentSlideSpeed;
            
            // ���ִ�ֱ�ٶ�
            slideVelocity.y = manager.Player.Rb.velocity.y;
            
            // ���ø����ٶ�
            manager.Player.Rb.velocity = slideVelocity;
        }
    }
} 