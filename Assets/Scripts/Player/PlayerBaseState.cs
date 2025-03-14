using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TrianCatStudio
{
    public abstract class PlayerBaseState : IState
    {
        // ��������
        public int StateLayer { get; protected set; }
        public virtual bool CanBeInterrupted => StateLayer < (int)StateLayerType.Action;

        // �������
        protected readonly PlayerStateManager manager;

        protected PlayerBaseState(PlayerStateManager manager)
        {
            this.manager = manager;
        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnExit()
        {
        }

        public virtual void Update(float deltaTime)
        {

        }
    }
}