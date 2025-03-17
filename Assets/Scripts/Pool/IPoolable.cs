namespace TrianCatStudio
{
    /// <summary>
    /// 可池化接口 - 实现此接口的对象可以自定义池化行为
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 对象从池中获取时调用
        /// </summary>
        void OnPoolGet();
        
        /// <summary>
        /// 对象释放回池时调用
        /// </summary>
        void OnPoolRelease();
    }
} 