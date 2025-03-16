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
        
        // 需要在下一帧重置的触发器列表
        private HashSet<string> _triggersToReset = new HashSet<string>();

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
                // 检查全局转换
                foreach (var layer in globalTransitions.Keys.ToList())
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
                foreach (var layerState in layerStates.ToList())
                {
                    if (layerState.Value != null)
                    {
                        layerState.Value.Update(deltaTime);
                    }
                }
                
                // 在每帧结束时清理触发器
                ClearTriggers();
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

        // 修改触发器逻辑，避免立即检查转换
        public void SetTrigger(string name)
        {
            Debug.Log($"StateMachine.SetTrigger: 设置触发器 {name}");
            
            // 先重置触发器，确保不会重复触发
            ResetTrigger(name);
            
            // 设置触发器
            parameters[name] = true;
            
            // 记录触发器，以便在下一帧自动重置
            _triggersToReset.Add(name);
        }

        // 添加清理触发器的方法
        public void ClearTriggers()
        {
            // 重置所有需要重置的触发器
            foreach (var triggerName in _triggersToReset)
            {
                if (parameters.ContainsKey(triggerName))
                {
                    parameters[triggerName] = false;
                }
            }
            
            // 清空触发器列表
            _triggersToReset.Clear();
        }

        // 添加重置特定触发器的方法
        public void ResetTrigger(string name)
        {
            if (parameters.ContainsKey(name) && parameters[name] is bool)
            {
                parameters[name] = false;
            }
        }

        // 切换状态（指定层）
        public void ChangeState(int layer, IState newState)
        {
            // 检查新状态是否为空
            if (newState == null)
            {
                Debug.LogWarning($"尝试将层 {layer} 切换到空状态");
                
                // 如果新状态为空，只退出当前状态，不进入新状态
                if (layerStates.TryGetValue(layer, out var layerCurrentState) && layerCurrentState != null)
                {
                    layerCurrentState.OnExit();
                    layerStates.Remove(layer); // 从层状态字典中移除该层
                }
                return;
            }
            
            if (layerStates.TryGetValue(layer, out var currentLayerState))
            {
                if (currentLayerState != null)
                {
                    currentLayerState.OnExit();
                }
            }
            
            layerStates[layer] = newState;
            newState.OnEnter();
        }

        // 切换主状态
        public void ChangeState(IState newState)
        {
            // 检查新状态是否为空
            if (newState == null)
            {
                Debug.LogWarning("尝试将主状态切换到空状态");
                return;
            }
            
            if (currentState != null)
            {
                currentState.OnExit();
            }
            
            currentState = newState;
            newState.OnEnter();
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
            if (globalTransitions.TryGetValue(layer, out var validTransitions))
            {
                foreach (var transition in validTransitions)
                {
                    if (transition.ConditionsMet(parameters))
                    {
                        // 全局转换的目标状态不应该为空，但为了安全起见，仍然进行检查
                        if (transition.To != null)
                        {
                            if (layer == -1)
                            {
                                // 主状态的全局转换
                                ChangeState(transition.To);
                            }
                            else
                            {
                                // 特定层的全局转换
                                ChangeState(layer, transition.To);
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"全局转换的目标状态为空，层：{layer}");
                        }
                        return;
                    }
                }
            }
        }
    }
}