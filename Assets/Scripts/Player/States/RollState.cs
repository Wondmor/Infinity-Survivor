using UnityEngine;

namespace TrianCatStudio
{
    public class RollState : PlayerBaseState
    {
        private float rollTimer = 0f;        // ������ʱ��
        private float rollDuration = 0.5f;   // ��������ʱ��
        private float rollSpeed = 5f;        // �����ٶ�
        private Vector3 rollDirection;       // ��������
        
        public RollState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Action;
        }

        public override bool CanBeInterrupted => false;

        public override void OnEnter()
        {
            // ʹ��AnimController���ö���״̬
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetAnimationState(PlayerAnimController.AnimationState.Roll);
                manager.Player.AnimController.TriggerRoll();
            }
            else
            {
                // ���÷�����ֱ������Animator����
                SetAnimatorTrigger("Roll");
            }
            
            rollTimer = 0f;
            
            // ��ȡ��������
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
            
            // Ӧ�÷����ƶ�
            if (rollTimer < rollDuration)
            {
                // ���㷭���ٶ����ߣ���ʼ�죬��������
                float speedMultiplier = 1f - (rollTimer / rollDuration);
                Vector3 rollVelocity = rollDirection * rollSpeed * speedMultiplier;
                rollVelocity.y = manager.Player.Rb.velocity.y; // ������ֱ�ٶ�
                
                manager.Player.Rb.velocity = rollVelocity;
            }
            
            // ��������
            if (rollTimer >= rollDuration)
            {
                manager.StateMachine.SetTrigger("RollComplete");
            }
        }
    }
} 