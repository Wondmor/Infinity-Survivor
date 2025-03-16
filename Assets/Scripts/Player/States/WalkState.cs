using UnityEngine;

namespace TrianCatStudio
{
    public class WalkState : PlayerBaseState
    {
        public WalkState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Base;
        }

        public override void OnEnter()
        {
            // ʹ��AnimController���ö���״̬
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetAnimationState(PlayerAnimController.AnimationState.Walk);
            }
            else
            {
                // ���÷�����ֱ������Animator����
                SetAnimatorBool("IsMoving", true);
                SetAnimatorBool("IsRunning", false);
                SetAnimatorInteger("MotionState", 1); // Walk����״̬
            }
        }

        public override void Update(float deltaTime)
        {
            // Walk״̬�µĸ����߼�
        }
        
        public override void HandleInput()
        {
            // ����Walk״̬�µ�����
            if (manager.Player.InputManager.IsJumpPressed)
            {
                // ͨ��״̬��������Ծ
                manager.TriggerJump();
            }
        }
    }
} 