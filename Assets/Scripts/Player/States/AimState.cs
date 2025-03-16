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
            // ������׼����״̬
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetAnimationState(PlayerAnimController.AnimationState.Aim);
            }
            else
            {
                // ���÷�����ֱ������Animator����
                SetAnimatorBool("IsAiming", true);
            }
        }

        public override void OnExit()
        {
            // �˳���׼״̬
            if (manager.Player.AnimController != null)
            {
                // ������׼״̬
            }
            else
            {
                // ���÷�����ֱ������Animator����
                SetAnimatorBool("IsAiming", false);
            }
        }

        public override void Update(float deltaTime)
        {
            // ��׼״̬�ĸ����߼�
        }

        public override void HandleInput()
        {
            // ������׼״̬�µ�����
        }

        public override void PhysicsUpdate(float deltaTime)
        {
            // ��׼״̬���������
        }
    }
} 