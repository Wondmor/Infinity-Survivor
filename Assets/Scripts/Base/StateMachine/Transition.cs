using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrianCatStudio
{
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
} 