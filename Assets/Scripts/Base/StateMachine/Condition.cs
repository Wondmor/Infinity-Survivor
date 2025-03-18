namespace TrianCatStudio
{
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
} 