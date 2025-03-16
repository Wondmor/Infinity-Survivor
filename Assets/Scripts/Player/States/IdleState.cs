using UnityEngine;

namespace TrianCatStudio
{
    public class IdleState : PlayerBaseState
    {
        public IdleState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Base;
        }

        public override void OnEnter()
        {
            // 使用AnimController设置动画状态
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetAnimationState(PlayerAnimController.AnimationState.Idle);
            }
            else
            {
                // 备用方案：直接设置Animator参数
                SetAnimatorBool("IsMoving", false);
                SetAnimatorInteger("MotionState", 0); // Idle动画状态
            }
        }

        public override void Update(float deltaTime)
        {
            // Idle状态下的更新逻辑
        }
        
        public override void HandleInput()
        {
            // 处理Idle状态下的输入
            if (manager.Player.InputManager.IsJumpPressed)
            {
                // 通过状态机触发跳跃
                manager.TriggerJump();
            }
        }
    }
} 