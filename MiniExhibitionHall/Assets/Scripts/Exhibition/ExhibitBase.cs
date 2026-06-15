using UnityEngine;

public class ExhibitBase : MonoBehaviour
{
    [Header("Exhibit Info")]
    public string exhibitName = "Unnamed Exhibit";

    [TextArea(3, 10)]
    public string exhibitDescription = "Exhibit description.";

    [Header("Display")]
    public float interactionDistance = 3.4f;
    public string hintText = "\u6309 E \u67e5\u770b\u8be6\u60c5";
    public float autoRotateSpeed = 10f;

    [Header("Audio")]
    public AudioClip exhibitSound;

    [Header("3I Notes")]
    public string immersionNote = "";
    public string interactionNote = "";
    public string imaginationNote = "";

    protected bool isPlayerNearby;
    private Transform player;

    protected virtual void Update()
    {
        if (autoRotateSpeed > 0f)
        {
            transform.Rotate(Vector3.up, autoRotateSpeed * Time.deltaTime, Space.World);
        }

        UpdateDistanceInteraction();
    }

    protected virtual void UpdateDistanceInteraction()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null) return;
            player = playerObject.transform;
        }

        bool nowNearby = Vector3.Distance(transform.position, player.position) <= interactionDistance;
        if (nowNearby && !isPlayerNearby)
        {
            isPlayerNearby = true;
            GameManager.Instance?.ShowHint(hintText);
        }
        else if (!nowNearby && isPlayerNearby)
        {
            isPlayerNearby = false;
            GameManager.Instance?.HideHint();
        }

        if (nowNearby && Input.GetKeyDown(KeyCode.E) && Cursor.lockState == CursorLockMode.Locked)
        {
            OnInteract();
        }
    }

    public virtual void OnInteract()
    {
        GameManager.Instance?.ShowExhibitInfo(
            exhibitName,
            exhibitDescription,
            immersionNote,
            interactionNote,
            imaginationNote
        );

        if (exhibitSound != null)
        {
            AudioManager.Instance?.PlaySFX(exhibitSound);
        }
    }

    public virtual void OnCloseInteract()
    {
        GameManager.Instance?.HideExhibitInfo();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
