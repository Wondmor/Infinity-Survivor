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
            // ʹ��AnimController���ö���״̬
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetAnimationState(PlayerAnimController.AnimationState.Idle);
            }
            else
            {
                // ���÷�����ֱ������Animator����
                SetAnimatorBool("IsMoving", false);
                SetAnimatorInteger("MotionState", 0); // Idle����״̬
            }
        }

        public override void Update(float deltaTime)
        {
            // Idle״̬�µĸ����߼�
        }
        
        public override void HandleInput()
        {
            // ����Idle״̬�µ�����
            if (manager.Player.InputManager.IsJumpPressed)
            {
                // ͨ��״̬��������Ծ
                manager.TriggerJump();
            }
        }
    }
} 