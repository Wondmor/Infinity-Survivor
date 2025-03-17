using UnityEngine;

namespace TrianCatStudio
{
    public class DoubleJumpState : PlayerBaseState
    {
        private float jumpTimer = 0f;
        private float maxJumpTime = 0.5f; // ��������������ʱ��
        private float doubleJumpForce = 4f; // ����������
        private float fallMultiplier = 2.5f; // ������ٱ���
        
        public DoubleJumpState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Action;
        }

        public override void OnEnter()
        {
            Debug.Log("DoubleJumpState.OnEnter: ���������״̬");
            
            // ʹ��AnimController���ö���״̬
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetAnimationState(PlayerAnimController.AnimationState.DoubleJump);
                manager.Player.AnimController.TriggerDoubleJump();
            }
            else
            {
                // ���÷�����ֱ������Animator����
                SetAnimatorTrigger("DoubleJump");
                SetAnimatorBool("IsGrounded", false);
            }
            
            jumpTimer = 0f;
            
            // �������״̬
            manager.Player.HasDoubleJumped = true;
            manager.Player.jumpCount = 2;
            
            // ִ�ж�����
            PerformDoubleJump();
        }

        public override void Update(float deltaTime)
        {
            jumpTimer += deltaTime;
            
            // ���¶�������
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.UpdateJumpTime(deltaTime);
            }
            else
            {
                SetAnimatorFloat("JumpTime", jumpTimer / maxJumpTime);
            }
            
            // Ӧ�ø��õ�����о�
            if (manager.Player.Rb.velocity.y < 0)
            {
                manager.Player.Rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * deltaTime;
            }
        }
        
        public override void PhysicsUpdate(float deltaTime)
        {
            // ����Ƿ�ʼ����
            if (manager.Player.Rb.velocity.y < -0.1f)
            {
                // ���ٵ��� TriggerFalling����Ϊ�����Ѿ�ɾ���� FallingState
                // ֱ���ڶ����������д������䶯��
                if (manager.Player.AnimController != null)
                {
                    manager.Player.AnimController.TriggerFall();
                }
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
        
        // ִ�ж�����
        private void PerformDoubleJump()
        {
            Debug.Log($"DoubleJumpState.PerformDoubleJump: ��ʼִ�ж����� - HasDoubleJumped={manager.Player.HasDoubleJumped}, jumpCount={manager.Player.jumpCount}");
            
            // ���ô�ֱ�ٶ�
            manager.Player.Rb.velocity = new Vector3(
                manager.Player.Rb.velocity.x,
                0f, // ���ô�ֱ�ٶ�
                manager.Player.Rb.velocity.z
            );
            
            // Ӧ�ö�������
            doubleJumpForce = manager.Player.GetJumpForce(); // ��ȡ���õĶ���������
            manager.Player.Rb.AddForce(Vector3.up * doubleJumpForce, ForceMode.Impulse);
            
            // ����״̬ - ��Щ״̬Ӧ���Ѿ��ڴ���������ʱ���ã�������ȷ��
            if (!manager.Player.HasDoubleJumped)
            {
                Debug.Log("DoubleJumpState.PerformDoubleJump: ����HasDoubleJumped=true");
                manager.Player.HasDoubleJumped = true;
            }
            
            if (manager.Player.jumpCount != 2)
            {
                Debug.Log($"DoubleJumpState.PerformDoubleJump: ����jumpCount��{manager.Player.jumpCount}��2");
                manager.Player.jumpCount = 2;
            }
            
            manager.Player.lastJumpTime = Time.time;
            
            Debug.Log($"DoubleJumpState.PerformDoubleJump: ������ִ����� - �ٶ�����Ϊ {doubleJumpForce}");
        }
    }
} 