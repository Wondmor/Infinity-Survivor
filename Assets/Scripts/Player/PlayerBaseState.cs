using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TrianCatStudio
{
    /// <summary>
    /// ״̬������ö��
    /// </summary>
    public enum StateLayerType
    {
        Base = 0,       // �����㣺��������ƶ����ߡ��ܡ�վ����
        Action = 1,     // �����㣺������Ծ�������������䡢��½��
        UpperBody = 2,  // �ϰ���㣺������׼������������
        Additive = 3    // ���Ӳ㣺������б��ͷ����ת��
    }

    public abstract class PlayerBaseState : IState
    {
        // ��������
        public int StateLayer { get; protected set; } = (int)StateLayerType.Base;
        public virtual bool CanBeInterrupted => true;

        // �������
        protected readonly PlayerStateManager manager;
        
        // ��������
        private static Dictionary<string, bool> animationExistsCache = new Dictionary<string, bool>();

        protected PlayerBaseState(PlayerStateManager manager)
        {
            this.manager = manager;
        }

        public virtual void OnEnter()
        {
            // ����ʵ�־����߼�
        }

        public virtual void OnExit()
        {
            // ����ʵ�־����߼�
        }

        public virtual void Update(float deltaTime)
        {
            // ����ʵ�־����߼�
        }

        /// <summary>
        /// �������룬������ʵ��
        /// </summary>
        public virtual void HandleInput()
        {
            // ����ʵ�־����߼�
        }

        /// <summary>
        /// ����������£�������ʵ��
        /// </summary>
        public virtual void PhysicsUpdate(float deltaTime)
        {
            // ����ʵ�־����߼�
        }

        /// <summary>
        /// ���ö���״̬��ͨ������Animator���������ƶ���״̬��
        /// </summary>
        protected void SetAnimationState(string stateName)
        {
            if (manager.Player?.Animator == null) return;
            manager.Player.Animator.SetTrigger(stateName);
        }
        
        /// <summary>
        /// ���ö������������п����ñ���
        /// </summary>
        protected void SetAnimatorFloat(string paramName, float value)
        {
            if (manager.Player?.Animator == null) return;
            manager.Player.Animator.SetFloat(paramName, value);
        }
        
        /// <summary>
        /// ���ö������������п����ñ���
        /// </summary>
        protected void SetAnimatorBool(string paramName, bool value)
        {
            if (manager.Player?.Animator == null) return;
            manager.Player.Animator.SetBool(paramName, value);
        }
        
        /// <summary>
        /// ���ö������������п����ñ���
        /// </summary>
        protected void SetAnimatorTrigger(string name)
        {
            if (manager.Player.Animator != null)
            {
                manager.Player.Animator.ResetTrigger(name);
                manager.Player.Animator.SetTrigger(name);
            }
        }
        
        /// <summary>
        /// ���ö������������п����ñ���
        /// </summary>
        protected void SetAnimatorInteger(string paramName, int value)
        {
            if (manager.Player?.Animator == null) return;
            manager.Player.Animator.SetInteger(paramName, value);
        }

        // ����Animator������
        protected void ResetAnimatorTrigger(string name)
        {
            if (manager.Player.Animator != null)
            {
                manager.Player.Animator.ResetTrigger(name);
            }
        }
    }
}