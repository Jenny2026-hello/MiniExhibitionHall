using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    [Header("Zone Info")]
    public string zoneName = "Unnamed Zone";

    [TextArea(2, 5)]
    public string zoneDescription = "";

    [Header("Trigger")]
    public bool triggerOnce = false;
    public AudioClip zoneIntroAudio;

    private bool hasTriggered;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && hasTriggered) return;

        hasTriggered = true;
        GameManager.Instance?.ShowHint("\u8fdb\u5165\u5c55\u533a: " + zoneName + " | \u9760\u8fd1\u5c55\u54c1\u540e\u6309 E \u4ea4\u4e92");

        if (!string.IsNullOrEmpty(zoneDescription))
        {
            Debug.Log("[ZoneTrigger] " + zoneName + ": " + zoneDescription);
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
