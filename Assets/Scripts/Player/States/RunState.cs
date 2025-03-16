using UnityEngine;

namespace TrianCatStudio
{
    public class RunState : PlayerBaseState
    {
        public RunState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Base;
        }

        public override void OnEnter()
        {
            // 使用AnimController设置动画状态
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetAnimationState(PlayerAnimController.AnimationState.Run);
            }
            else
            {
                // 备用方案：直接设置Animator参数
                SetAnimatorBool("IsMoving", true);
                SetAnimatorBool("IsRunning", true);
                SetAnimatorInteger("MotionState", 2); // Run动画状态
            }
        }

        public override void Update(float deltaTime)
        {
            // Run状态下的更新逻辑
        }
        
        public override void HandleInput()
        {
            // 处理Run状态下的输入
            if (manager.Player.InputManager.IsJumpPressed)
            {
                // 通过状态机触发跳跃
                manager.TriggerJump();
            }
        }
    }
} 