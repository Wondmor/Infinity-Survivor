using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrianCatStudio;
public class StartGamePoint : MonoBehaviour
{
    public void OnStartButtonClick()
    {
        // 传递家园场景初始化参数
        var parameters = new Dictionary<string, object>
        {
            ["SpawnPosition"] = new Vector3(0, 0, 0),
            ["LayoutID"] = "default_home"
        };

        // 触发场景切换
        SceneController.Instance.LoadHomeScene();
    }
}
