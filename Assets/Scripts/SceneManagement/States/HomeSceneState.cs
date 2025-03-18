using UnityEngine;
using TrianCatStudio;

public class HomeSceneState : IState
{
    public void OnEnter()
    {
        Debug.Log("进入家园场景");
        // 在这里初始化家园场景的UI、NPC等
    }

    public void OnExit()
    {
        Debug.Log("退出家园场景");
        // 在这里清理家园场景的资源等
    }

    public void Update(float deltaTime)
    {
        // 在这里处理家园场景的更新逻辑
    }
} 