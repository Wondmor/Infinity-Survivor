using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TrianCatStudio;

namespace TrianCatStudio
{
    /// <summary>
    /// 场景控制器Mono单例，也是整个游戏的入口
    /// </summary>
    public class SceneController : SingletonAutoMono<SceneController>
    {
        private StateMachine stateMachine;
        private LoadingState loadingState;
        private bool isLoading = false;

        private void Awake()
        {
            stateMachine = new StateMachine();
            loadingState = new LoadingState();
            
            // 初始化状态机
            InitializeStateMachine();
        }

        private void InitializeStateMachine()
        {
            // 创建场景状态
            var mainMenuState = new MainMenuState();
            var homeSceneState = new HomeSceneState();

            // 添加状态转换
            stateMachine.AddTransition(loadingState, mainMenuState, 
                new Condition("LoadingComplete", ParameterType.Trigger, ComparisonType.Equals, true));
            
            stateMachine.AddTransition(loadingState, homeSceneState, 
                new Condition("LoadingComplete", ParameterType.Trigger, ComparisonType.Equals, true));

            // 设置初始状态
            stateMachine.Initialize(loadingState);
        }

        private void Update()
        {
            stateMachine.Update(Time.deltaTime);
        }

        // 加载主菜单
        public void LoadMainMenu()
        {
            if (isLoading) return;
            StartCoroutine(LoadSceneAsync("MainMenu"));
        }

        // 加载家园场景
        public void LoadHomeScene()
        {
            if (isLoading) return;
            StartCoroutine(LoadSceneAsync("HomeScene"));
        }

        // 异步加载场景
        private IEnumerator LoadSceneAsync(string sceneName)
        {
            isLoading = true;
            loadingState.Progress = 0f;

            // 开始加载场景
            AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            asyncOperation.allowSceneActivation = false;

            // 等待加载完成
            while (!asyncOperation.isDone)
            {
                loadingState.Progress = asyncOperation.progress;

                // 当加载进度达到90%时
                if (asyncOperation.progress >= 0.9f)
                {
                    // 等待一帧，确保场景完全加载
                    yield return new WaitForEndOfFrame();
                    asyncOperation.allowSceneActivation = true;
                }

                yield return null;
            }

            // 设置加载完成触发器
            stateMachine.SetTrigger("LoadingComplete");
            isLoading = false;
        }

        // 获取当前加载进度
        public float GetLoadingProgress()
        {
            return loadingState.Progress;
        }
    }
} 