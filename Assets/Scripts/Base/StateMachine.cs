using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public IState To { get; private set; }
        public List<Condition> Conditions { get; private set; }

        public Transition(IState to, Condition[] conditions)
        {
            To = to; // 允许to为null，表示退出当前状态
            Conditions = new List<Condition>(conditions);
        }

        public bool ConditionsMet(Dictionary<string, object> parameters)
        {
            foreach (var condition in Conditions)
            {
                if (!parameters.TryGetValue(condition.ParameterName, out object currentValue))
                {
                    // 只在调试模式下打印日志
                    #if UNITY_EDITOR
                    // Debug.Log($"条件不满足: 参数 {condition.ParameterName} 不存在");
                    #endif
                    return false;
                }

                bool conditionMet = false;
                switch (condition.Type)
                {
                    case ParameterType.Bool:
                        conditionMet = CheckBool((bool)currentValue, (bool)condition.ExpectedValue, condition.Comparison);
                        break;
                    case ParameterType.Int:
                        conditionMet = CheckInt((int)currentValue, (int)condition.ExpectedValue, condition.Comparison);
                        break;
                    case ParameterType.Float:
                        conditionMet = CheckFloat((float)currentValue, (float)condition.ExpectedValue, condition.Comparison);
                        break;
                    case ParameterType.Trigger:
                        conditionMet = CheckTrigger((bool)currentValue);
                        break;
                }
                
                if (!conditionMet)
                {
                    // 只在调试模式下打印日志，且只对触发器类型打印
                    #if UNITY_EDITOR
                    if (condition.Type == ParameterType.Trigger)
                    {
                        Debug.Log($"触发器条件不满足: {condition.ParameterName} = {currentValue}");
                    }
                    #endif
                    return false;
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

        private bool CheckTrigger(bool current)
        {
            // 只检查触发器是否为true，不重置它
            return current;
        }
    }
    
    /// <summary>
    /// 状态机基类
    /// </summary>
    public class StateMachine
    {
        private readonly object stateLock = new object();
        
        // 当前状态
        private IState currentState;
        public IState CurrentState => currentState;

        // 状态层级
        private Dictionary<int, IState> layerStates = new Dictionary<int, IState>();
        public Dictionary<int, IState> LayerStates => layerStates;

        // 状态转换规则
        private Dictionary<IState, List<Transition>> transitions = new Dictionary<IState, List<Transition>>();
        private Dictionary<int, List<Transition>> globalTransitions = new Dictionary<int, List<Transition>>();
        
        // 参数字典
        private Dictionary<string, object> parameters = new Dictionary<string, object>();
        
        // 进入初始状态
        public void Initialize(IState startState)
        {
            currentState = startState;
            startState.OnEnter();
        }

        // 初始化分层状态机
        public void InitializeLayers(Dictionary<int, IState> initialLayerStates)
        {
            if (initialLayerStates == null)
            {
                Debug.LogError("InitializeLayers: initialLayerStates为空");
                return;
            }
            
            layerStates.Clear();
            
            foreach (var layerState in initialLayerStates)
            {
                layerStates[layerState.Key] = layerState.Value;
                
                // 只有当状态不为空时才调用OnEnter
                if (layerState.Value != null)
                {
                    layerState.Value.OnEnter();
                }
            }
        }

        // 更新状态
        public void Update(float deltaTime)
        {
            try
            {
                lock (stateLock)
                {
                    // 检查全局转换
                    var layersToCheck = globalTransitions.Keys.ToList();
                    foreach (var layer in layersToCheck)
                    {
                        CheckGlobalTransitions(layer);
                    }
                    
                    // 检查当前状态的转换
                    CheckTransitions();
                    
                    // 更新当前状态
                    if (currentState != null)
                    {
                        currentState.Update(deltaTime);
                    }
                    
                    // 更新各层状态
                    var statesToUpdate = layerStates.ToList();
                    foreach (var layerState in statesToUpdate)
                    {
                        if (layerState.Value != null)
                        {
                            layerState.Value.Update(deltaTime);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"StateMachine.Update发生异常: {e.Message}\n{e.StackTrace}");
            }
        }

        // 添加转换规则
        public void AddTransition(IState from, IState to, params Condition[] conditions)
        {
            // 如果from为空，不添加转换
            if (from == null)
            {
                Debug.LogWarning("尝试为空状态添加转换");
                return;
            }
            
            if (!transitions.ContainsKey(from))
            {
                transitions[from] = new List<Transition>();
            }
            
            transitions[from].Add(new Transition(to, conditions));
        }

        // 添加全局转换规则（对特定层）
        public void AddGlobalTransition(int layer, IState to, params Condition[] conditions)
        {
            // 如果to为空，不添加转换
            if (to == null)
            {
                Debug.LogWarning($"尝试为层 {layer} 添加到空状态的全局转换");
                return;
            }
            
            if (!globalTransitions.ContainsKey(layer))
            {
                globalTransitions[layer] = new List<Transition>();
            }
            
            globalTransitions[layer].Add(new Transition(to, conditions));
        }

        // 添加全局转换规则（对主状态）
        public void AddGlobalTransition(IState to, params Condition[] conditions)
        {
            // 如果to为空，不添加转换
            if (to == null)
            {
                Debug.LogWarning("尝试添加到空状态的全局转换");
                return;
            }
            
            AddGlobalTransition(-1, to, conditions);
        }

        // 设置参数
        public void SetBool(string name, bool value) => SetParameter(name, value);
        public void SetInt(string name, int value) => SetParameter(name, value);
        public void SetFloat(string name, float value) => SetParameter(name, value);
        
        // 获取参数方法（用于调试）
        public bool GetBool(string name)
        {
            if (parameters.TryGetValue(name, out var value) && value is bool boolValue)
            {
                return boolValue;
            }
            return false;
        }
        
        public int GetInt(string name)
        {
            if (parameters.TryGetValue(name, out var value) && value is int intValue)
            {
                return intValue;
            }
            return 0;
        }
        
        public float GetFloat(string name)
        {
            if (parameters.TryGetValue(name, out var value) && value is float floatValue)
            {
                return floatValue;
            }
            return 0f;
        }

        // 设置触发器
        public void SetTrigger(string name)
        {
            #if UNITY_EDITOR
            // 只在编辑器模式下，且只对跳跃相关的触发器输出日志
            if (name.Contains("Jump") || name.Contains("Fall"))
            {
                string currentValue = parameters.ContainsKey(name) ? parameters[name].ToString() : "未设置";
                Debug.Log($"StateMachine.SetTrigger: 设置触发器 {name}，当前值={currentValue}");
            }
            #endif
            
            parameters[name] = true;
        }

        // 重置特定触发器
        public void ResetTrigger(string name)
        {
            if (parameters.ContainsKey(name) && parameters[name] is bool)
            {
                #if UNITY_EDITOR
                // 只在编辑器模式下，且只对跳跃相关的触发器输出日志
                if (name.Contains("Jump") || name.Contains("Fall"))
                {
                    Debug.Log($"StateMachine.ResetTrigger: 重置触发器 {name}");
                }
                #endif
                
                parameters[name] = false;
            }
        }

        // 切换状态
        public void ChangeState(IState newState)
        {
            lock (stateLock)
            {
                if (currentState != null)
                {
                    currentState.OnExit();
                }
                
                currentState = newState;
                
                if (currentState != null)
                {
                    currentState.OnEnter();
                }
            }
        }
        
        // 切换层状态
        public void ChangeState(int layer, IState newState)
        {
            lock (stateLock)
            {
                if (layerStates.TryGetValue(layer, out IState oldState))
                {
                    if (oldState != null)
                    {
                        oldState.OnExit();
                    }
                }
                
                layerStates[layer] = newState;
                
                if (newState != null)
                {
                    newState.OnEnter();
                }
            }
        }

        private void SetParameter<T>(string name, T value)
        {
            parameters[name] = value;
            // 不要立即检查转换，让Update循环来处理
            // CheckTransitions();
        }

        private void CheckTransitions()
        {
            if (currentState != null && transitions.TryGetValue(currentState, out var validTransitions))
            {
                foreach (var transition in validTransitions)
                {
                    if (transition.ConditionsMet(parameters))
                    {
                        // 只输出跳跃相关状态的转换日志
                        string fromState = currentState.GetType().Name;
                        string toState = transition.To != null ? transition.To.GetType().Name : "null";
                        if (fromState.Contains("Jump") || fromState.Contains("Fall") ||
                            (toState != "null" && (toState.Contains("Jump") || toState.Contains("Fall"))))
                        {
                            Debug.Log($"CheckTransitions: 主状态转换 - 从 {fromState} 到 {toState}");
                        }
                        
                        // 重置所有触发器类型的条件
                        foreach (var condition in transition.Conditions)
                        {
                            if (condition.Type == ParameterType.Trigger)
                            {
                                ResetTrigger(condition.ParameterName);
                            }
                        }
                        
                        // 如果目标状态为空，表示退出当前状态
                        if (transition.To == null)
                        {
                            currentState.OnExit();
                            currentState = null;
                            return;
                        }
                        else
                        {
                            ChangeState(transition.To);
                            return;
                        }
                    }
                }
            }
            
            // 检查各层状态的转换
            foreach (var layer in layerStates.Keys.ToList())
            {
                var layerState = layerStates[layer];
                if (layerState != null && transitions.TryGetValue(layerState, out var layerTransitions))
                {
                    foreach (var transition in layerTransitions)
                    {
                        if (transition.ConditionsMet(parameters))
                        {
                            // 只输出跳跃相关状态的转换日志
                            string fromState = layerState.GetType().Name;
                            string toState = transition.To != null ? transition.To.GetType().Name : "null";
                            if (fromState.Contains("Jump") || fromState.Contains("Fall") ||
                                (toState != "null" && (toState.Contains("Jump") || toState.Contains("Fall"))))
                            {
                                Debug.Log($"CheckTransitions: 层 {layer} 状态转换 - 从 {fromState} 到 {toState}");
                            }
                            
                            // 重置所有触发器类型的条件
                            foreach (var condition in transition.Conditions)
                            {
                                if (condition.Type == ParameterType.Trigger)
                                {
                                    ResetTrigger(condition.ParameterName);
                                }
                            }
                            
                            // 如果目标状态为空，表示退出当前层状态
                            if (transition.To == null)
                            {
                                layerState.OnExit();
                                layerStates.Remove(layer);
                                break;
                            }
                            else
                            {
                                ChangeState(layer, transition.To);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void CheckGlobalTransitions(int layer)
        {
            // 无全局转换规则，直接返回
            if (!globalTransitions.ContainsKey(layer))
            {
                return;
            }
            
            List<Transition> validTransitions = globalTransitions[layer];
            IState currentLayerState = layer == -1 ? currentState : (layerStates.ContainsKey(layer) ? layerStates[layer] : null);
            
            if (validTransitions == null || validTransitions.Count == 0)
            {
                return;
            }
            
            foreach (var transition in validTransitions)
            {
                if (transition.ConditionsMet(parameters))
                {
                    // 避免状态自我转换
                    if (transition.To != null && currentLayerState != null && 
                        transition.To.GetType() == currentLayerState.GetType())
                    {
                        // 只输出跳跃相关状态的日志
                        string stateName = currentLayerState.GetType().Name;
                        if (stateName.Contains("Jump") || stateName.Contains("Fall"))
                        {
                            Debug.Log($"CheckGlobalTransitions: 跳过到相同状态的转换 {stateName}");
                        }
                        
                        // 重置触发器
                        foreach (var condition in transition.Conditions)
                        {
                            if (condition.Type == ParameterType.Trigger)
                            {
                                ResetTrigger(condition.ParameterName);
                            }
                        }
                        
                        continue;
                    }
                    
                    // 只输出跳跃相关状态的转换日志
                    if (transition.To != null)
                    {
                        string toState = transition.To.GetType().Name;
                        if (toState.Contains("Jump") || toState.Contains("Fall"))
                        {
                            Debug.Log($"CheckGlobalTransitions: 层 {layer} 全局转换到 {toState}");
                        }
                    }
                    
                    // 重置所有触发器类型的条件
                    foreach (var condition in transition.Conditions)
                    {
                        if (condition.Type == ParameterType.Trigger)
                        {
                            ResetTrigger(condition.ParameterName);
                        }
                    }
                    
                    if (transition.To != null)
                    {
                        if (layer == -1)
                        {
                            ChangeState(transition.To);
                        }
                        else
                        {
                            ChangeState(layer, transition.To);
                        }
                    }
                    return;
                }
            }
        }

        // 获取特定层的当前状态
        public IState GetCurrentStateInLayer(int layer)
        {
            if (layer == -1)
            {
                return currentState;
            }
            
            if (layerStates.TryGetValue(layer, out var state))
            {
                return state;
            }
            
            return null;
        }
    }
}