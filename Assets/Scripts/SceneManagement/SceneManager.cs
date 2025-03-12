using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace TrianCatStudio
{
    /// <summary>
    /// 场景管理器Mono单例，也是整个游戏的入口
    /// </summary>
    public class SceneManager : SingletonAutoMono<SceneManager>
    {
        // 状态机实例
        public StateMachine StateMachine { get; private set; }

        // 场景参数配置
        private Dictionary<string, object> sceneParams = new Dictionary<string, object>();

        // 加载进度（0-1）
        public float LoadingProgress { get; private set; }

        protected void Awake()
        {
            InitializeStateMachine();
        }

        private void InitializeStateMachine()
        {
            StateMachine = new StateMachine();

            // 注册所有场景状态
            //StateMachine.AddTransition(new MainMenuState(), 
            //    new Transition{ TargetState = new LoadingState(), Conditions = ... });

            // 设置初始状态
            StateMachine.SetCurrentState(new MainMenuState());
        }

        /// <summary>
        /// 切换场景的通用接口
        /// </summary>
        /// <param name="parameters">参数字典（字符串-Object）</param>
        /// <typeparam name="T">目标状态</typeparam>
        public void SwitchScene<T>(Dictionary<string, object> parameters = null) where T : IState
        {
            sceneParams = parameters ?? new Dictionary<string, object>();
            StateMachine.SetTrigger($"To{typeof(T).Name}");
        }

        // 获取场景参数
        public T GetSceneParam<T>(string key)
        {
            if (sceneParams.TryGetValue(key, out object value))
                return (T)value;
            return default;
        }

        private void Update()
        {
            StateMachine.Update(Time.deltaTime);
            UpdateLoadingProgress();
        }

        private void UpdateLoadingProgress()
        {
            if (StateMachine.CurrentState is LoadingState loadingState)
                LoadingProgress = loadingState.Progress;
        }
    }
}