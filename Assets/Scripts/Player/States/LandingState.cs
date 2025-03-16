using UnityEngine;

namespace TrianCatStudio
{
    public class LandingState : PlayerBaseState
    {
        private float recoveryTime = 0.2f; // ��½�ָ�ʱ��
        private float timer = 0f;
        
        public LandingState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Action;
        }

        public override void OnEnter()
        {
            // ʹ��AnimController���ö���״̬
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetAnimationState(PlayerAnimController.AnimationState.Land);
                manager.Player.AnimController.TriggerLand();
            }
            else
            {
                // ���÷�����ֱ������Animator����
                SetAnimatorTrigger("Land");
                SetAnimatorBool("IsGrounded", true);
            }
            
            // ����״̬
            timer = 0f;
            manager.Player.HasDoubleJumped = false;
            manager.Player.jumpCount = 0;
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
            // ��½״̬�²�������������
        }
        
        public override void PhysicsUpdate(float deltaTime)
        {
            // ��½״̬�²���Ҫ������������
        }
    }
} 