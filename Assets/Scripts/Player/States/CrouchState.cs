using UnityEngine;

namespace TrianCatStudio
{
    public class CrouchState : PlayerBaseState
    {
        private float crouchSpeed = 1.5f; // �¶�ʱ���ƶ��ٶ�
        private float crouchHeight = 0.5f; // �¶�ʱ�ĸ߶�����
        
        public override bool CanBeInterrupted => true;

        public CrouchState(PlayerStateManager manager) : base(manager)
        {
            StateLayer = (int)StateLayerType.Action;
        }

        public override void OnEnter()
        {
            Debug.Log("CrouchState.OnEnter: �����¶�״̬");
            
            // ʹ��AnimController���ö���״̬
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetCrouchState(true);
            }
            else
            {
                // ���÷�����ֱ������Animator����
                SetAnimatorBool("IsCrouching", true);
            }
            
            // ���������������ײ��߶�
            // ���磺manager.Player.GetComponent<CapsuleCollider>().height = crouchHeight;
        }
        
        public override void OnExit()
        {
            Debug.Log("CrouchState.OnExit: �˳��¶�״̬");
            
            // �����¶�״̬
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetCrouchState(false);
            }
            else
            {
                SetAnimatorBool("IsCrouching", false);
            }
            
            // �ָ���ײ��߶�
            // ���磺manager.Player.GetComponent<CapsuleCollider>().height = originalHeight;
        }
        
        public override void Update(float deltaTime)
        {
            // ����Ƿ������¶�
            if (!manager.Player.InputManager.IsCrouching)
            {
                // ��������¶ף��˳�״̬
                manager.ChangeLayerState(StateLayer, null);
                return;
            }
            
            // ���¶�������
            if (manager.Player.AnimController != null)
            {
                manager.Player.AnimController.SetCrouchState(true);
            }
        }
        
        public override void HandleInput()
        {
            // �����¶�ʱ������
        }
        
        public override void PhysicsUpdate(float deltaTime)
        {
            // �¶�ʱ��������£����������ƶ��ٶ�
        }
    }
} 