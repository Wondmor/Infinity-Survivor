using UnityEngine;

namespace TrianCatStudio
{
    public class FallingState : PlayerBaseState
    {
        private float airTime = 0f;
        private float fallMultiplier = 2.5f; // ������ٱ���
        private bool isFloating = false;
        private float floatThreshold = 0.8f; // ����Ư��״̬��ʱ����ֵ
        
        public FallingState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Action;
        }

        public override void OnEnter()
        {
            // ʹ��AnimController���ö���״̬
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetAnimationState(PlayerAnimController.AnimationState.Fall);
                manager.Player.AnimController.TriggerFall();
            }
            else
            {
                // ���÷�����ֱ������Animator����
                SetAnimatorTrigger("Fall");
                SetAnimatorBool("IsGrounded", false);
            }
            
            airTime = 0f;
            isFloating = false;
        }

        public override void Update(float deltaTime)
        {
            // ���¿���ʱ��
            airTime += deltaTime;
            
            // ����Ƿ�Ӧ�ý���Ư��״̬
            if (airTime > floatThreshold && !isFloating)
            {
                isFloating = true;
                
                // ���¶�������
                if (manager.Player.AnimController != null)
                {
                    manager.Player.AnimController.SetFloatingState(true);
                }
                else
                {
                    SetAnimatorBool("IsFloating", true);
                }
            }
            
            // ���¶�������
            if (manager.Player.AnimController != null)
            {
                // ����ʱ���Ѿ���AnimController�и���
            }
            else
            {
                SetAnimatorFloat("AirTime", airTime);
            }
            
            // Ӧ�ø��õ�����о�
            if (manager.Player.Rb.velocity.y < 0)
            {
                manager.Player.Rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * deltaTime;
            }
        }
        
        public override void HandleInput()
        {
            // ����������룬�������
            if (manager.Player.InputManager.IsJumpPressed && !manager.Player.HasDoubleJumped && manager.Player.jumpCount == 1)
            {
                manager.TriggerDoubleJump();
            }
        }
        
        public override void PhysicsUpdate(float deltaTime)
        {
            // ����Ƿ��Ѿ���½
            if (manager.Player.IsGrounded)
            {
                // ����Ѿ���½��������½״̬
                if (manager.Player.Rb.velocity.y < -5f)
                {
                    manager.TriggerHardLanding();
                }
                else
                {
                    manager.TriggerLanding();
                }
            }
        }
        
        public override void OnExit()
        {
            // ����Ư��״̬
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetFloatingState(false);
            }
            else
            {
                SetAnimatorBool("IsFloating", false);
            }
        }
    }
} 
