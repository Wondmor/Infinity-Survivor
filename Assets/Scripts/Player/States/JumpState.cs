using UnityEngine;

namespace TrianCatStudio
{
    public class JumpState : PlayerBaseState
    {
        // ��Ծ���Ʋ���
        private float jumpTimer = 0f;
        private float jumpForce = 5f; // Ĭ����Ծ����
        private float fallMultiplier = 2.5f; // ������ٱ���
        
        public override bool CanBeInterrupted => false;

        public JumpState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Action;
        }

        public override void OnEnter()
        {
            Debug.Log("JumpState.OnEnter: ������Ծ״̬");
            
            // ʹ��AnimController���ö���״̬
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetAnimationState(PlayerAnimController.AnimationState.Jump);
                manager.Player.AnimController.TriggerJump();
            }
            else
            {
                // ���÷�����ֱ������Animator����
                SetAnimatorTrigger("Jump");
                SetAnimatorBool("IsGrounded", false);
            }
            
            // ��ʼ����Ծ
            jumpTimer = 0f;
            
            // �������״̬
            manager.Player.jumpCount = 1; // ������Ծ����Ϊ1
            manager.Player.HasDoubleJumped = false;
            
            // Ӧ����Ծ��
            PerformJump();
        }
        
        public override void OnExit()
        {
            Debug.Log("JumpState.OnExit: �˳���Ծ״̬");
            
            // ȷ�������ظ�������Ծ
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.ResetJumpTrigger();
            }
            else
            {
                ResetAnimatorTrigger("Jump");
            }
        }
        
        public override void Update(float deltaTime)
        {
            // ������Ծ��ʱ���������ڶ�����
            jumpTimer += deltaTime;
            
            // ���¶�������
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.UpdateJumpTime(deltaTime);
            }
            else
            {
                SetAnimatorFloat("JumpTime", jumpTimer); // ��Ծʱ��
            }
            
            // Ӧ�ø��õ�����о�
            if (manager.Player.Rb.velocity.y < 0)
            {
                manager.Player.Rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * deltaTime;
            }
        }
        
        public override void HandleInput()
        {
            // �������������
            if (manager.Player.InputManager.IsJumpPressed && CanDoubleJump())
            {
                manager.TriggerDoubleJump();
            }
        }
        
        public override void PhysicsUpdate(float deltaTime)
        {
            // ����Ƿ�ʼ����
            if (manager.Player.Rb.velocity.y < -0.1f)
            {
                manager.TriggerFalling();
            }
            
            // ����Ƿ��Ѿ���½
            if (manager.Player.IsGrounded && jumpTimer > 0.1f)
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
        
        // ִ����Ծ
        private void PerformJump()
        {
            // ��ȡ��Ծ����
            jumpForce = manager.Player.GetJumpForce();
            
            // ���ô�ֱ�ٶȲ�ֱ���������ϵ��ٶȣ�������ʹ��AddForce��
            manager.Player.Rb.velocity = new Vector3(
                manager.Player.Rb.velocity.x,
                jumpForce, // ֱ�����ô�ֱ�ٶ�
                manager.Player.Rb.velocity.z
            );
            
            // ��¼��Ծʱ��
            manager.Player.lastJumpTime = Time.time;
            
            Debug.Log($"ִ����Ծ: �ٶ�����Ϊ {jumpForce}");
        }
        
        // ����Ƿ���Զ�����
        private bool CanDoubleJump()
        {
            return !manager.Player.IsGrounded && manager.Player.jumpCount == 1 && !manager.Player.HasDoubleJumped;
        }
    }
} 