using UnityEngine;

/// <summary>
/// 区域触发器 - 当玩家进入特定区域时触发事件
/// 例如：进入"古代文物区"时自动播放介绍语音
/// </summary>
public class ZoneTrigger : MonoBehaviour
{
    [Header("区域信息")]
    public string zoneName = "未命名区域";
    [TextArea(2, 5)]
    public string zoneDescription = "";

    [Header("触发设置")]
    public bool triggerOnce = false;          // 是否只触发一次
    public AudioClip zoneIntroAudio;          // 区域介绍音效

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && hasTriggered) return;

        hasTriggered = true;

        // 显示区域信息
        GameManager.Instance?.ShowHint("进入区域: " + zoneName);

        // 播放区域介绍的展品信息
        if (!string.IsNullOrEmpty(zoneDescription))
        {
            // 可以在这里触发特定的UI或音频
            Debug.Log("[ZoneTrigger] 进入区域: " + zoneName);
        }

        if (zoneIntroAudio != null)
        {
            AudioManager.Instance?.PlaySFX(zoneIntroAudio);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        GameManager.Instance?.HideHint();
    }
}
