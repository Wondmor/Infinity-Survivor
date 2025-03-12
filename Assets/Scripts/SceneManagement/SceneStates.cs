using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrianCatStudio;
// 主菜单状态
public class MainMenuState : SceneState
{
    public MainMenuState() => SceneName = "MainMenu";

    protected override void OnSceneLoaded()
    {

    }
}

// 家园场景状态
public class HomeSceneState : SceneState
{
    public HomeSceneState() => SceneName = "HomeScene";

    protected override void OnSceneLoaded()
    {

    }
}

// 加载状态（通用）
public class LoadingState : IState
{
    public float Progress { get; private set; }

    public void OnEnter() => Progress = 0;
    public void OnExit() => Progress = 1;
    public void Update(float deltaTime) { }
}