using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TrianCatStudio
{
    /// <summary>
    /// 状态层类型枚举
    /// </summary>
    public enum StateLayerType
    {
        Base = 0,       // 基础层：控制基本移动（走、跑、站立）
        Action = 1,     // 动作层：控制跳跃、下落、翻滚、着陆等
        UpperBody = 2,  // 上半身层：控制瞄准、攻击、交互等
        Additive = 3    // 附加层：控制倾斜、头部旋转等
    }

    public abstract class PlayerBaseState : IState
    {
        // 基本属性
        public int StateLayer { get; protected set; } = (int)StateLayerType.Base;
        public virtual bool CanBeInterrupted => true;

        // 管理器引用
        protected readonly PlayerStateManager manager;
        
        // 动画缓存
        private static Dictionary<string, bool> animationExistsCache = new Dictionary<string, bool>();

        protected PlayerBaseState(PlayerStateManager manager)
        {
            this.manager = manager;
        }

        public virtual void OnEnter()
        {
            // 由子类实际实现逻辑
        }

        public virtual void OnExit()
        {
            // 由子类实际实现逻辑
        }

        public virtual void Update(float deltaTime)
        {
            // 由子类实际实现逻辑
        }

        /// <summary>
        /// 处理输入，由子类实现
        /// </summary>
        public virtual void HandleInput()
        {
            // 由子类实际实现逻辑
        }

        /// <summary>
        /// 处理物理更新，由子类实现
        /// </summary>
        public virtual void PhysicsUpdate(float deltaTime)
        {
            // 由子类实际实现逻辑
        }

        /// <summary>
        /// 设置动画状态，通过调用Animator控制器来控制动画状态机
        /// </summary>
        protected void SetAnimationState(string stateName)
        {
            if (manager.Player?.Animator == null) return;
            manager.Player.Animator.SetTrigger(stateName);
        }
        
        /// <summary>
        /// 设置动画控制器中可控制的变量
        /// </summary>
        protected void SetAnimatorFloat(string paramName, float value)
        {
            if (manager.Player?.Animator == null) return;
            manager.Player.Animator.SetFloat(paramName, value);
        }
        
        /// <summary>
        /// 设置动画控制器中可控制的变量
        /// </summary>
        protected void SetAnimatorBool(string paramName, bool value)
        {
            if (manager.Player?.Animator == null) return;
            manager.Player.Animator.SetBool(paramName, value);
        }
        
        /// <summary>
        /// 设置动画控制器中可控制的变量
        /// </summary>
        protected void SetAnimatorTrigger(string name)
        {
            if (manager.Player.Animator != null)
            {
                manager.Player.Animator.SetTrigger(name);
            }
        }
        
        /// <summary>
        /// 设置动画控制器中可控制的变量
        /// </summary>
        protected void SetAnimatorInteger(string paramName, int value)
        {
            if (manager.Player?.Animator == null) return;
            manager.Player.Animator.SetInteger(paramName, value);
        }
    }
}