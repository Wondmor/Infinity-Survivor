using UnityEngine;

namespace TrianCatStudio
{
    public class RollState : PlayerBaseState
    {
        private float rollTimer = 0f;        // 翻滚计时器
        private float rollDuration = 0.5f;   // 翻滚持续时间
        private float rollSpeed = 5f;        // 翻滚速度
        private Vector3 rollDirection;       // 翻滚方向
        
        public RollState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Action;
        }

        public override bool CanBeInterrupted => false;

        public override void OnEnter()
        {
            // 使用AnimController设置动画状态
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetAnimationState(PlayerAnimController.AnimationState.Roll);
                manager.Player.AnimController.TriggerRoll();
            }
            else
            {
                // 备用方案：直接设置Animator参数
                SetAnimatorTrigger("Roll");
            }
            
            rollTimer = 0f;
            
            // 获取翻滚方向
            rollDirection = manager.Player.MoveDirection;
            if (rollDirection.magnitude < 0.1f)
            {
                rollDirection = manager.Player.transform.forward;
            }
            rollDirection.Normalize();
        }

        public override void Update(float deltaTime)
        {
            rollTimer += deltaTime;
            
            // 应用翻滚移动
            if (rollTimer < rollDuration)
            {
                // 计算翻滚速度曲线（开始快，结束慢）
                float speedMultiplier = 1f - (rollTimer / rollDuration);
                Vector3 rollVelocity = rollDirection * rollSpeed * speedMultiplier;
                rollVelocity.y = manager.Player.Rb.velocity.y; // 保留垂直速度
                
                manager.Player.Rb.velocity = rollVelocity;
            }
            
            // 翻滚结束
            if (rollTimer >= rollDuration)
            {
                manager.StateMachine.SetTrigger("RollComplete");
            }
        }
    }
} 