using UnityEngine;

namespace TrianCatStudio
{
    public enum StateLayerType
    {
        Base = 0,
        Action = 1,
        Ultimate = 2
    }

    public class IdleState : PlayerBaseState
    {
        public IdleState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Base;
        }

        public override void OnEnter()
        {
            //manager.Player.Animator.SetFloat("Speed", 0);
        }

        public override void Update(float deltaTime)
        {

        }
    }

    public class WalkState : PlayerBaseState
    {
        public WalkState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Base;
        }

        public override void OnEnter()
        {
            manager.Player.Animator.SetFloat("Speed", 0.5f);
        }

        public override void Update(float deltaTime)
        {

        }
    }


    public class RunState : PlayerBaseState
    {
        public RunState(PlayerStateManager manager) : base(manager) { }

        public override void OnEnter()
        {
            base.OnEnter();
            manager.Player.Animator.CrossFade("Run", 0.1f);
        }

        public override void Update(float deltaTime)
        {

            base.Update(deltaTime);
        }
    }

    public class JumpState : PlayerBaseState
    {
        public JumpState(PlayerStateManager manager) : base(manager)
        {
        }
        private float jumpForce = 5f;

        public override bool CanBeInterrupted => false;

        public override void OnEnter()
        {
            manager.Player.Animator.Play("Jump");
            manager.Player.Rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            // �������CharacterController��Move������Ծ�켣[1,3](@ref)
        }
    }

    // RollState.cs������ײ�������
    public class RollState : PlayerBaseState
    {
        public RollState(PlayerStateManager manager) : base(manager)
        {
        }
        private float rollDuration = 0.8f;
        public override void OnEnter()
        {
            manager.Player.Animator.applyRootMotion = true;
            manager.Player.Animator.CrossFade("Roll", 0.1f);
            manager.Player.GetComponent<CapsuleCollider>().height *= 0.5f; // ����ʱ��С��ײ��[3](@ref)
        }

        public override void OnExit()
        {
            manager.Player.Animator.applyRootMotion = false;
            manager.Player.GetComponent<CapsuleCollider>().height *= 2f;
        }
    }
}