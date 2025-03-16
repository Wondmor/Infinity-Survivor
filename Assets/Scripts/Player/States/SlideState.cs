using UnityEngine;

namespace TrianCatStudio
{
    public class SlideState : PlayerBaseState
    {
        private float slideSpeed = 8f; // 滑铲初始速度
        private float slideDuration = 0.8f; // 滑铲持续时间
        private float slideDeceleration = 5f; // 滑铲减速度
        private float minSpeedToSlide = 3f; // 最小滑铲速度
        private float slideTimer = 0f; // 滑铲计时器
        private Vector3 slideDirection; // 滑铲方向
        private float currentSlideSpeed; // 当前滑铲速度
        
        public override bool CanBeInterrupted => false;

        public SlideState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Action;
        }

        public override void OnEnter()
        {
            Debug.Log("SlideState.OnEnter: 进入滑铲状态");
            
            // 初始化滑铲参数
            slideTimer = 0f;
            
            // 获取当前移动方向作为滑铲方向
            slideDirection = manager.Player.transform.forward;
            
            // 设置初始滑铲速度
            currentSlideSpeed = slideSpeed;
            
            // 使用AnimController触发滑铲动画
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.TriggerSlide();
            }
            else
            {
                // 备用方案：直接设置Animator参数
                SetAnimatorTrigger("Slide");
            }
            
            // 可以在这里调整碰撞体高度
            // 例如：manager.Player.GetComponent<CapsuleCollider>().height = slideHeight;
        }
        
        public override void OnExit()
        {
            Debug.Log("SlideState.OnExit: 退出滑铲状态");
            
            // 恢复碰撞体高度
            // 例如：manager.Player.GetComponent<CapsuleCollider>().height = originalHeight;
            
            // 如果速度仍然足够，进入下蹲状态，否则恢复正常状态
            if (currentSlideSpeed > minSpeedToSlide / 2)
            {
                manager.TriggerCrouch(true);
            }
        }
        
        public override void Update(float deltaTime)
        {
            // 更新滑铲计时器
            slideTimer += deltaTime;
            
            // 计算当前滑铲速度（随时间减少）
            currentSlideSpeed = Mathf.Max(0, slideSpeed - slideDeceleration * slideTimer);
            
            // 如果滑铲时间结束或速度过低，退出滑铲状态
            if (slideTimer >= slideDuration || currentSlideSpeed < minSpeedToSlide)
            {
                manager.ChangeLayerState(StateLayer, null);
                return;
            }
        }
        
        public override void HandleInput()
        {
            // 滑铲过程中不处理额外输入
        }
        
        public override void PhysicsUpdate(float deltaTime)
        {
            // 应用滑铲速度
            Vector3 slideVelocity = slideDirection * currentSlideSpeed;
            
            // 保持垂直速度
            slideVelocity.y = manager.Player.Rb.velocity.y;
            
            // 设置刚体速度
            manager.Player.Rb.velocity = slideVelocity;
        }
    }
} 