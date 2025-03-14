using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrianCatStudio
{
    /// <summary>
    /// 底层状态接口
    /// </summary>
    public interface IState
    {
        void OnEnter();
        void OnExit();
        void Update(float deltaTime);
    }
    
    public enum ParameterType
    {
        Bool,
        Int,
        Float,
        Trigger
    }

    public enum ComparisonType
    {
        Equals,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterOrEqual,
        LessOrEqual
    }

    /// <summary>
    /// 状态转换条件
    /// </summary>
    public class Condition
    {
        public string ParameterName;
        public ParameterType Type;
        public ComparisonType Comparison;
        public object ExpectedValue;

        /// <summary>
        /// 构造函数，用于初始化 Condition 对象
        /// </summary>
        /// <param name="parameterName">参数名称</param>
        /// <param name="type">参数类型</param>
        /// <param name="comparison">比较类型</param>
        /// <param name="expectedValue">期望值</param>
        public Condition(string parameterName, ParameterType type, ComparisonType comparison, object expectedValue)
        {
            ParameterName = parameterName;
            Type = type;
            Comparison = comparison;
            ExpectedValue = expectedValue;
        }
    }

    /// <summary>
    /// 状态转换（类似Animator中的）
    /// </summary>
    public class Transition
    {
        public IState TargetState;
        public List<Condition> Conditions;

        public bool CheckConditions(Dictionary<string, object> parameters)
        {
            foreach (var condition in Conditions)
            {
                if (!parameters.TryGetValue(condition.ParameterName, out object currentValue))
                    return false;

                switch (condition.Type)
                {
                    case ParameterType.Bool:
                        if (!CheckBool((bool)currentValue, (bool)condition.ExpectedValue, condition.Comparison))
                            return false;
                        break;
                    case ParameterType.Int:
                        if (!CheckInt((int)currentValue, (int)condition.ExpectedValue, condition.Comparison))
                            return false;
                        break;
                    case ParameterType.Float:
                        if (!CheckFloat((float)currentValue, (float)condition.ExpectedValue, condition.Comparison))
                            return false;
                        break;
                    case ParameterType.Trigger:
                        if (!CheckTrigger((bool)currentValue))
                            return false;
                        break;
                }
            }

            return true;
        }

        private bool CheckBool(bool a, bool b, ComparisonType comp) => comp switch
        {
            ComparisonType.Equals => a == b,
            ComparisonType.NotEqual => a != b,
            _ => throw new ArgumentException("Invalid comparison for bool."),
        };

        private bool CheckInt(int a, int b, ComparisonType comp) => comp switch
        {
            ComparisonType.Equals => a == b,
            ComparisonType.NotEqual => a != b,
            ComparisonType.GreaterThan => a > b,
            ComparisonType.LessThan => a < b,
            ComparisonType.GreaterOrEqual => a >= b,
            ComparisonType.LessOrEqual => a <= b,
            _ => false,
        };

        private bool CheckFloat(float a, float b, ComparisonType comp) => comp switch
        {
            ComparisonType.Equals => Mathf.Approximately(a, b),
            ComparisonType.NotEqual => !Mathf.Approximately(a, b),
            ComparisonType.GreaterThan => a > b,
            ComparisonType.LessThan => a < b,
            ComparisonType.GreaterOrEqual => a >= b,
            ComparisonType.LessOrEqual => a <= b,
            _ => false,
        };

        private bool CheckTrigger(bool current) => current;
    }
    
    /// <summary>
    /// 状态机基类
    /// </summary>
    public class StateMachine
    {
        private IState currentState;
        public IState CurrentState { get => currentState;}
        private Dictionary<IState, List<Transition>> transitions = new Dictionary<IState, List<Transition>>();
        private Dictionary<string, object> parameters = new Dictionary<string, object>();
        //保存已注册的状态
        private HashSet<IState> allStates = new HashSet<IState>();

        //增加跳转
        public void AddTransition(IState from, Transition transition)
        {
            if (!transitions.ContainsKey(from))
                transitions[from] = new List<Transition>();
            transitions[from].Add(transition);
            allStates.Add(from); // 注册状态到集合
        }

        // 全局跳转方法
        public void AddGlobalTransition(IState to, params Condition[] conditions)
        {
            var transition = new Transition
            {
                TargetState = to,
                Conditions = new List<Condition>(conditions)
            };

            foreach (var state in allStates)
            {
                AddTransition(state, transition);
            }
        }

        public void SetCurrentState(IState state)
        {
            currentState?.OnExit();
            currentState = state;
            currentState.OnEnter();
            CheckTransitions(); // Immediate check after state change
        }

        public void Update(float deltaTime) => currentState?.Update(deltaTime);

        public void SetBool(string name, bool value) => SetParameter(name, value);
        public void SetInt(string name, int value) => SetParameter(name, value);
        public void SetFloat(string name, float value) => SetParameter(name, value);
        public void SetTrigger(string name)
        {
            parameters[name] = true;
            CheckTransitions();
            parameters[name] = false;
        }

        private void SetParameter<T>(string name, T value)
        {
            parameters[name] = value;
            CheckTransitions();
        }

        private void CheckTransitions()
        {
            if (currentState != null && transitions.TryGetValue(currentState, out var validTransitions))
            {
                foreach (var transition in validTransitions)
                {
                    if (transition.CheckConditions(parameters))
                    {
                        SetCurrentState(transition.TargetState);
                        break;
                    }
                }
            }
        }
    }
}