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
        Base = 0,       // 基础层：处理基本移动（走、跑、站立）
        Action = 1,     // 动作层：处理跳跃、二段跳、下落、着陆等
        UpperBody = 2,  // 上半身层：处理瞄准、武器操作等
        Additive = 3    // 附加层：处理倾斜、头部旋转等
    }

    public abstract class PlayerBaseState : IState
    {
        // 公共属性
        public int StateLayer { get; protected set; } = (int)StateLayerType.Base;
        public virtual bool CanBeInterrupted => true;

        // 组件引用
        protected readonly PlayerStateManager manager;
        
        // 动画缓存
        private static Dictionary<string, bool> animationExistsCache = new Dictionary<string, bool>();

        protected PlayerBaseState(PlayerStateManager manager)
        {
            this.manager = manager;
        }

        public virtual void OnEnter()
        {
            // 子类实现具体逻辑
        }

        public virtual void OnExit()
        {
            // 子类实现具体逻辑
        }

        public virtual void Update(float deltaTime)
        {
            // 子类实现具体逻辑
        }

        /// <summary>
        /// 处理输入，由子类实现
        /// </summary>
        public virtual void HandleInput()
        {
            // 子类实现具体逻辑
        }

        /// <summary>
        /// 处理物理更新，由子类实现
        /// </summary>
        public virtual void PhysicsUpdate(float deltaTime)
        {
            // 子类实现具体逻辑
        }

        /// <summary>
        /// 设置动画状态，通过设置Animator参数来控制动画状态机
        /// </summary>
        protected void SetAnimationState(string stateName)
        {
            if (manager.Player?.Animator == null) return;
            manager.Player.Animator.SetTrigger(stateName);
        }
        
        /// <summary>
        /// 设置动画参数，带有空引用保护
        /// </summary>
        protected void SetAnimatorFloat(string paramName, float value)
        {
            if (manager.Player?.Animator == null) return;
            manager.Player.Animator.SetFloat(paramName, value);
        }
        
        /// <summary>
        /// 设置动画参数，带有空引用保护
        /// </summary>
        protected void SetAnimatorBool(string paramName, bool value)
        {
            if (manager.Player?.Animator == null) return;
            manager.Player.Animator.SetBool(paramName, value);
        }
        
        /// <summary>
        /// 设置动画参数，带有空引用保护
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
        /// 设置动画参数，带有空引用保护
        /// </summary>
        protected void SetAnimatorInteger(string paramName, int value)
        {
            if (manager.Player?.Animator == null) return;
            manager.Player.Animator.SetInteger(paramName, value);
        }

        // 重置Animator触发器
        protected void ResetAnimatorTrigger(string name)
        {
            if (manager.Player.Animator != null)
            {
                manager.Player.Animator.ResetTrigger(name);
            }
        }
    }
}