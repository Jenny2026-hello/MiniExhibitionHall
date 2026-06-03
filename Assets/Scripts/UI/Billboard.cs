using UnityEngine;

/// <summary>
/// 始终面向相机 - UI文字/Cavas始终朝向玩家视线
/// 用于展品标签、信息指示牌等
/// 挂载到需要面向玩家的Canvas或GameObject上
/// </summary>
public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Billboard: 未找到Main Camera");
        }
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        // 让物体始终面向相机
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                         mainCamera.transform.rotation * Vector3.up);
    }
}
