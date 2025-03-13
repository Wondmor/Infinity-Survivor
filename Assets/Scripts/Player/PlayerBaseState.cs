using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrianCatStudio
{
    public abstract class PlayerBaseState : IState
    {
        protected readonly StateMachine stateMachine;
        protected PlayerStateManager stateManager;

        protected PlayerBaseState(StateMachine machine, InputManager input)
        {
            stateMachine = machine;
            stateManager = PlayerStateManager.Instance;
        }

        public virtual void OnEnter() { }
        public virtual void OnExit() { }

        public virtual void Update(float deltaTime)
        {
            HandleMovement(deltaTime);
            HandleRotation(deltaTime);
            CheckStateTransitions();
        }

        protected abstract void HandleMovement(float deltaTime);
        protected abstract void HandleRotation(float deltaTime);
        protected abstract void CheckStateTransitions();

    }
}
