using UnityEngine;
using TrianCatStudio;

public class MainMenuState : IState
{
    public void OnEnter()
    {
        Debug.Log("进入主菜单场景");
        // 在这里初始化主菜单UI等
    }

    public void OnExit()
    {
        Debug.Log("退出主菜单场景");
        // 在这里清理主菜单资源等
    }

    public void Update(float deltaTime)
    {
        // 在这里处理主菜单场景的更新逻辑
    }
} 