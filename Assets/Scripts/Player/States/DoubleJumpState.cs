using UnityEngine;

namespace TrianCatStudio
{
    public class DoubleJumpState : PlayerBaseState
    {
        private float jumpTimer = 0f;
        private float maxJumpTime = 0.5f; // 二段跳动画持续时间
        private float doubleJumpForce = 4f; // 二段跳力度
        private float fallMultiplier = 2.5f; // 下落加速倍数
        
        public DoubleJumpState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Action;
        }

        public override void OnEnter()
        {
            // 使用AnimController设置动画状态
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetAnimationState(PlayerAnimController.AnimationState.DoubleJump);
                manager.Player.AnimController.TriggerDoubleJump();
            }
            else
            {
                // 备用方案：直接设置Animator参数
                SetAnimatorTrigger("DoubleJump");
                SetAnimatorBool("IsGrounded", false);
            }
            
            jumpTimer = 0f;
            
            // 执行二段跳
            PerformDoubleJump();
        }

        public override void Update(float deltaTime)
        {
            jumpTimer += deltaTime;
            
            // 更新动画参数
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.UpdateJumpTime(deltaTime);
            }
            else
            {
                SetAnimatorFloat("JumpTime", jumpTimer / maxJumpTime);
            }
            
            // 应用更好的下落感觉
            if (manager.Player.Rb.velocity.y < 0)
            {
                manager.Player.Rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * deltaTime;
            }
        }
        
        public override void PhysicsUpdate(float deltaTime)
        {
            // 检测是否开始下落
            if (manager.Player.Rb.velocity.y < -0.1f)
            {
                manager.TriggerFalling();
            }
            
            // 检测是否已经着陆
            if (manager.Player.IsGrounded && jumpTimer > 0.1f)
            {
                // 如果已经着陆，触发着陆状态
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
        
        // 执行二段跳
        private void PerformDoubleJump()
        {
            // 重置垂直速度
            manager.Player.Rb.velocity = new Vector3(
                manager.Player.Rb.velocity.x,
                0f, // 重置垂直速度
                manager.Player.Rb.velocity.z
            );
            
            // 应用二段跳力
            doubleJumpForce = manager.Player.GetJumpForce(); // 获取配置的二段跳力度
            manager.Player.Rb.AddForce(Vector3.up * doubleJumpForce, ForceMode.Impulse);
            
            // 更新状态
            manager.Player.HasDoubleJumped = true;
            manager.Player.jumpCount = 2;
            manager.Player.lastJumpTime = Time.time;
        }
    }
} 