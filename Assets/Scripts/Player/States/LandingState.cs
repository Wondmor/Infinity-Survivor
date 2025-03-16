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
            // 着陆状态下不处理特殊输入
        }
        
        public override void PhysicsUpdate(float deltaTime)
        {
            // 着陆状态下不需要特殊的物理更新
        }
    }
} 