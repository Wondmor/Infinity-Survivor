using UnityEngine;

namespace TrianCatStudio
{
    public class LandingState : PlayerBaseState
    {
        private float recoveryTime = 0.2f; // 着陆恢复时间
        private float timer = 0f;
        
        public LandingState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Action;
        }

        public override void OnEnter()
        {
            // 使用AnimController设置动画状态
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetAnimationState(PlayerAnimController.AnimationState.Land);
                manager.Player.AnimController.TriggerLand();
            }
            else
            {
                // 备用方案：直接设置Animator参数
                SetAnimatorTrigger("Land");
                SetAnimatorBool("IsGrounded", true);
            }
            
            // 重置状态
            timer = 0f;
            manager.Player.HasDoubleJumped = false;
            manager.Player.jumpCount = 0;
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
            // 允许在着陆状态直接跳跃，打断着陆恢复
            if (manager.Player.InputManager.IsJumpPressed && timer > 0.05f) // 添加少量延迟，避免连续跳跃
            {
                Debug.Log("LandingState.HandleInput: 检测到跳跃输入，打断着陆恢复");
                
                // 重置跳跃状态
                manager.Player.jumpCount = 0; // 确保从0开始计数
                manager.Player.HasDoubleJumped = false;
                
                // 触发跳跃
                manager.TriggerJump();
            }
        }
        
        public override void PhysicsUpdate(float deltaTime)
        {
            // 着陆状态下不需要特殊的物理更新
        }
    }
} 