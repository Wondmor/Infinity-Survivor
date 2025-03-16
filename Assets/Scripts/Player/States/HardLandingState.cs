using UnityEngine;

namespace TrianCatStudio
{
    public class HardLandingState : PlayerBaseState
    {
        private float recoveryTime = 0.5f; // Ӳ��½�ָ�ʱ�䣨����ͨ��½����
        private float timer = 0f;
        
        public HardLandingState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Action;
        }

        public override void OnEnter()
        {
            // ʹ��AnimController���ö���״̬
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetAnimationState(PlayerAnimController.AnimationState.HardLand);
                manager.Player.AnimController.TriggerHardLand();
            }
            else
            {
                // ���÷�����ֱ������Animator����
                SetAnimatorTrigger("HardLand");
                SetAnimatorBool("IsGrounded", true);
            }
            
            // ����״̬
            timer = 0f;
            manager.Player.HasDoubleJumped = false;
            manager.Player.jumpCount = 0;
            
            // Ӳ��½���ܻ��ж���Ч��������Ļ��
            // ������������ش���
        }

        public override void Update(float deltaTime)
        {
            // ���»ָ���ʱ��
            timer += deltaTime;
            
            // ����Ƿ�ָ����
            if (timer >= recoveryTime)
            {
                // �����ָ�����¼�
                manager.StateMachine.SetTrigger("RecoveryComplete");
            }
        }
        
        public override void HandleInput()
        {
            // Ӳ��½״̬�²�������������
            // ���߿��������ض���������ϻָ����緭��
            if (manager.Player.InputManager.IsDashPressed && timer > recoveryTime * 0.5f)
            {
                manager.TriggerRoll();
            }
        }
        
        public override void PhysicsUpdate(float deltaTime)
        {
            // Ӳ��½״̬�¿�����Ҫ�����ƶ��ٶ�
            if (timer < recoveryTime * 0.7f)
            {
                // �����ƶ��ٶ�
                Vector3 velocity = manager.Player.Rb.velocity;
                velocity.x *= 0.8f;
                velocity.z *= 0.8f;
                manager.Player.Rb.velocity = velocity;
            }
        }
    }
} 