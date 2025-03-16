using UnityEngine;

namespace TrianCatStudio
{
    public class JumpState : PlayerBaseState
    {
        // 跳跃控制参数
        private float jumpTimer = 0f;
        private float jumpForce = 5f; // 默认跳跃力度
        private float fallMultiplier = 2.5f; // 下落加速倍数
        
        public override bool CanBeInterrupted => false;

        public JumpState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Action;
        }

        public override void OnEnter()
        {
            Debug.Log("JumpState.OnEnter: 进入跳跃状态");
            
            // 使用AnimController设置动画状态
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetAnimationState(PlayerAnimController.AnimationState.Jump);
                manager.Player.AnimController.TriggerJump();
            }
            else
            {
                // 备用方案：直接设置Animator参数
                SetAnimatorTrigger("Jump");
                SetAnimatorBool("IsGrounded", false);
            }
            
            // 初始化跳跃
            jumpTimer = 0f;
            
            // 设置玩家状态
            manager.Player.jumpCount = 1; // 设置跳跃计数为1
            manager.Player.HasDoubleJumped = false;
            
            // 应用跳跃力
            PerformJump();
        }
        
        public override void OnExit()
        {
            Debug.Log("JumpState.OnExit: 退出跳跃状态");
            
            // 确保不会重复触发跳跃
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
            // 更新跳跃计时器（仅用于动画）
            jumpTimer += deltaTime;
            
            // 更新动画参数
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.UpdateJumpTime(deltaTime);
            }
            else
            {
                SetAnimatorFloat("JumpTime", jumpTimer); // 跳跃时间
            }
            
            // 应用更好的下落感觉
            if (manager.Player.Rb.velocity.y < 0)
            {
                manager.Player.Rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * deltaTime;
            }
        }
        
        public override void HandleInput()
        {
            // 处理二段跳输入
            if (manager.Player.InputManager.IsJumpPressed && CanDoubleJump())
            {
                manager.TriggerDoubleJump();
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
        
        // 执行跳跃
        private void PerformJump()
        {
            // 获取跳跃力度
            jumpForce = manager.Player.GetJumpForce();
            
            // 重置垂直速度并直接设置向上的速度（而不是使用AddForce）
            manager.Player.Rb.velocity = new Vector3(
                manager.Player.Rb.velocity.x,
                jumpForce, // 直接设置垂直速度
                manager.Player.Rb.velocity.z
            );
            
            // 记录跳跃时间
            manager.Player.lastJumpTime = Time.time;
            
            Debug.Log($"执行跳跃: 速度设置为 {jumpForce}");
        }
        
        // 检查是否可以二段跳
        private bool CanDoubleJump()
        {
            return !manager.Player.IsGrounded && manager.Player.jumpCount == 1 && !manager.Player.HasDoubleJumped;
        }
    }
} 