using UnityEngine;
using TrianCatStudio;

public class LoadingState : IState
{
    public float Progress { get; set; }

    public void OnEnter() => Progress = 0;
    public void OnExit() => Progress = 1;
    public void Update(float deltaTime) { }
} 