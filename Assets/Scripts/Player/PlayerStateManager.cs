using UnityEngine;

namespace TrianCatStudio
{
    public class PlayerStateManager : BaseManager<PlayerStateManager>
    {
        // 动画参数常量
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        private static readonly int IsAiming = Animator.StringToHash("IsAiming");
        private static readonly int JumpTrigger = Animator.StringToHash("Jump");

        private Animator animator;
        private Transform playerTransform;

        protected void Init()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            animator = player.GetComponent<Animator>();
            playerTransform = player.transform;
        }

        // 动画控制接口
        public void SetMoveSpeed(float speed) => animator.SetFloat(MoveSpeed, speed);
        public void TriggerJump() => animator.SetTrigger(JumpTrigger);
        public void SetAimingState(bool isAiming) => animator.SetBool(IsAiming, isAiming);
        public void CrossFade(string stateName, float transitionTime) => animator.CrossFade(stateName, transitionTime);

        // 物理控制接口
        public Vector3 GetMovementDirection() => playerTransform.forward;
        public void RotateTowards(Vector3 direction, float turnSpeed)
        {
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                playerTransform.rotation = Quaternion.Slerp(
                    playerTransform.rotation,
                    targetRotation,
                    turnSpeed * Time.deltaTime
                );
            }
        }
    }
}
