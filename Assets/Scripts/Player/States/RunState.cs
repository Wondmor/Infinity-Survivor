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
            // ʹ��AnimController���ö���״̬
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetAnimationState(PlayerAnimController.AnimationState.Run);
            }
            else
            {
                // ���÷�����ֱ������Animator����
                SetAnimatorBool("IsMoving", true);
                SetAnimatorBool("IsRunning", true);
                SetAnimatorInteger("MotionState", 2); // Run����״̬
            }
        }

        public override void Update(float deltaTime)
        {
            // Run״̬�µĸ����߼�
        }
        
        public override void HandleInput()
        {
            // ����Run״̬�µ�����
            if (manager.Player.InputManager.IsJumpPressed)
            {
                // ͨ��״̬��������Ծ
                manager.TriggerJump();
            }
        }
    }
} 