using UnityEngine;

namespace TrianCatStudio
{
    public class HardLandingState : PlayerBaseState
    {
        private float recoveryTime = 0.5f; // 硬着陆恢复时间（比普通着陆长）
        private float timer = 0f;
        
        public HardLandingState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Action;
        }

        public override void OnEnter()
        {
            // 使用AnimController设置动画状态
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetAnimationState(PlayerAnimController.AnimationState.HardLand);
                manager.Player.AnimController.TriggerHardLand();
            }
            else
            {
                // 备用方案：直接设置Animator参数
                SetAnimatorTrigger("HardLand");
                SetAnimatorBool("IsGrounded", true);
            }
            
            // 重置状态
            timer = 0f;
            manager.Player.HasDoubleJumped = false;
            manager.Player.jumpCount = 0;
            
            // 硬着陆可能会有额外效果，如屏幕震动
            // 这里可以添加相关代码
        }

        public override void Update(float deltaTime)
        {
            // 更新恢复计时器
            timer += deltaTime;
            
            // 检查是否恢复完成
            if (timer >= recoveryTime)
            {
                // 触发恢复完成事件
                manager.StateMachine.SetTrigger("RecoveryComplete");
            }
        }
        
        public override void HandleInput()
        {
            // 允许在硬着陆状态下一定恢复时间后跳跃或翻滚
            if (timer > recoveryTime * 0.3f) // 硬着陆需要更长的恢复时间才能跳跃
            {
                // 检测跳跃输入
                if (manager.Player.InputManager.IsJumpPressed)
                {
                    Debug.Log("HardLandingState.HandleInput: 检测到跳跃输入，打断硬着陆恢复");
                    
                    // 重置跳跃状态
                    manager.Player.jumpCount = 0; // 确保从0开始计数
                    manager.Player.HasDoubleJumped = false;
                    
                    // 触发跳跃
                    manager.TriggerJump();
                }
                // 检测翻滚输入
                else if (manager.Player.InputManager.IsDashPressed)
                {
                    Debug.Log("HardLandingState.HandleInput: 检测到翻滚输入，打断硬着陆恢复");
                    manager.TriggerRoll();
                }
            }
        }
        
        public override void PhysicsUpdate(float deltaTime)
        {
            // 硬着陆状态下可能需要减缓移动速度
            if (timer < recoveryTime * 0.7f)
            {
                // 减缓移动速度
                Vector3 velocity = manager.Player.Rb.velocity;
                velocity.x *= 0.8f;
                velocity.z *= 0.8f;
                manager.Player.Rb.velocity = velocity;
            }
        }
    }
} 