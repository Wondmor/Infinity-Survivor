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
} 