using UnityEngine;

namespace TrianCatStudio
{
    public class AimState : PlayerBaseState
    {
        public AimState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.UpperBody;
        }

        public override void OnEnter()
        {
            // 设置瞄准动画状态
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetAnimationState(PlayerAnimController.AnimationState.Aim);
            }
            else
            {
                // 备用方案：直接设置Animator参数
                SetAnimatorBool("IsAiming", true);
            }
        }

        public override void OnExit()
        {
            // 退出瞄准状态
            if (manager.Player.AnimController != null)
            {
                // 重置瞄准状态
            }
            else
            {
                // 备用方案：直接设置Animator参数
                SetAnimatorBool("IsAiming", false);
            }
        }

        public override void Update(float deltaTime)
        {
            // 瞄准状态的更新逻辑
        }

        public override void HandleInput()
        {
            // 处理瞄准状态下的输入
        }

        public override void PhysicsUpdate(float deltaTime)
        {
            // 瞄准状态的物理更新
        }
    }
} 