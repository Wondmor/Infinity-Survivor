using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrianCatStudio
{
    public abstract class SceneState : IState
    {
        public string SceneName { get; protected set; }
        public float Progress { get; protected set; }

        public virtual void OnEnter()
        {
            SceneController.Instance.StartCoroutine(LoadSceneRoutine());
        }

        private IEnumerator LoadSceneRoutine()
        {
            // 显示加载界面
            //UIManager.Instance.Show<LoadingUI>();

            // 异步加载场景
            var operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(SceneName);
            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                Progress = Mathf.Clamp01(operation.progress / 0.9f);

                if (operation.progress >= 0.9f)
                {
                    operation.allowSceneActivation = true;
                }

                yield return null;
            }

            // 场景加载完成后的初始化
            OnSceneLoaded();

            // 隐藏加载界面
            //UIManager.Instance.Hide<LoadingUI>();
        }

        protected virtual void OnSceneLoaded()
        {
            // 子类实现具体逻辑
        }

        public virtual void OnExit()
        {
            // 清理场景特定资源
            //Addressables.ReleaseScene(SceneName);
        }

        public virtual void Update(float deltaTime)
        {
        }
    }
}