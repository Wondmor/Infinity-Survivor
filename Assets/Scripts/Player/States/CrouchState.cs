using UnityEngine;

namespace TrianCatStudio
{
    public class CrouchState : PlayerBaseState
    {
        private float crouchSpeed = 1.5f; // 下蹲时的移动速度
        private float crouchHeight = 0.5f; // 下蹲时的高度缩放
        
        public override bool CanBeInterrupted => true;

        public CrouchState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Action;
        }

        public override void OnEnter()
        {
            Debug.Log("CrouchState.OnEnter: 进入下蹲状态");
            
            // 使用AnimController设置动画状态
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetCrouchState(true);
            }
            else
            {
                // 备用方案：直接设置Animator参数
                SetAnimatorBool("IsCrouching", true);
            }
            
            // 可以在这里调整碰撞体高度
            // 例如：manager.Player.GetComponent<CapsuleCollider>().height = crouchHeight;
        }
        
        public override void OnExit()
        {
            Debug.Log("CrouchState.OnExit: 退出下蹲状态");
            
            // 重置下蹲状态
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetCrouchState(false);
            }
            else
            {
                SetAnimatorBool("IsCrouching", false);
            }
            
            // 恢复碰撞体高度
            // 例如：manager.Player.GetComponent<CapsuleCollider>().height = originalHeight;
        }
        
        public override void Update(float deltaTime)
        {
            // 检查是否仍在下蹲
            if (!manager.Player.InputManager.IsCrouching)
            {
                // 如果不再下蹲，退出状态
                manager.ChangeLayerState(StateLayer, null);
                return;
            }
            
            // 更新动画参数
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetCrouchState(true);
            }
        }
        
        public override void HandleInput()
        {
            // 处理下蹲时的输入
        }
        
        public override void PhysicsUpdate(float deltaTime)
        {
            // 下蹲时的物理更新，例如限制移动速度
        }
    }
} 