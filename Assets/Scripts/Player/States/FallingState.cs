using UnityEngine;

namespace TrianCatStudio
{
    public class FallingState : PlayerBaseState
    {
        private float airTime = 0f;
        private float fallMultiplier = 2.5f; // 下落加速倍数
        private bool isFloating = false;
        private float floatThreshold = 0.8f; // 进入漂浮状态的时间阈值
        
        public FallingState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Action;
        }

        public override void OnEnter()
        {
            // 使用AnimController设置动画状态
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetAnimationState(PlayerAnimController.AnimationState.Fall);
                manager.Player.AnimController.TriggerFall();
            }
            else
            {
                // 备用方案：直接设置Animator参数
                SetAnimatorTrigger("Fall");
                SetAnimatorBool("IsGrounded", false);
            }
            
            airTime = 0f;
            isFloating = false;
        }

        public override void Update(float deltaTime)
        {
            // 更新空中时间
            airTime += deltaTime;
            
            // 检测是否应该进入漂浮状态
            if (airTime > floatThreshold && !isFloating)
            {
                isFloating = true;
                
                // 更新动画参数
                if (manager.Player.AnimController != null)
                {
                    manager.Player.AnimController.SetFloatingState(true);
                }
                else
                {
                    SetAnimatorBool("IsFloating", true);
                }
            }
            
            // 更新动画参数
            if (manager.Player.AnimController != null)
            {
                // 空中时间已经在AnimController中更新
            }
            else
            {
                SetAnimatorFloat("AirTime", airTime);
            }
            
            // 应用更好的下落感觉
            if (manager.Player.Rb.velocity.y < 0)
            {
                manager.Player.Rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * deltaTime;
            }
        }
        
        public override void HandleInput()
        {
            // 处理空中输入，如二段跳
            if (manager.Player.InputManager.IsJumpPressed && !manager.Player.HasDoubleJumped && manager.Player.jumpCount == 1)
            {
                manager.TriggerDoubleJump();
            }
        }
        
        public override void PhysicsUpdate(float deltaTime)
        {
            // 检测是否已经着陆
            if (manager.Player.IsGrounded)
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
        
        public override void OnExit()
        {
            // 重置漂浮状态
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetFloatingState(false);
            }
            else
            {
                SetAnimatorBool("IsFloating", false);
            }
        }
    }
} 
