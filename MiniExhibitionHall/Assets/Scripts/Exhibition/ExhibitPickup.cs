using UnityEngine;

public class ExhibitPickup : ExhibitBase
{
    [Header("Pickup")]
    public float pickupDistance = 1.8f;
    public float rotationSpeed = 120f;
    public Vector3 pickupOffset = Vector3.zero;
    public Vector3 pickupScale = Vector3.one;
    public float minZoom = 0.9f;
    public float maxZoom = 3f;

    private bool isPickedUp;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private float originalAutoRotateSpeed;
    private Camera playerCamera;

    void Start()
    {
        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
        originalAutoRotateSpeed = autoRotateSpeed;
        playerCamera = Camera.main;
    }

    protected override void Update()
    {
        if (!isPickedUp)
        {
            base.Update();
            return;
        }

        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, -mouseX, Space.World);
            Vector3 pitchAxis = Camera.main != null ? Camera.main.transform.right : Vector3.right;
            transform.Rotate(pitchAxis, mouseY, Space.World);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 pos = transform.localPosition;
            pos.z = Mathf.Clamp(pos.z + scroll * 0.6f, minZoom, maxZoom);
            transform.localPosition = pos;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            PutDown();
        }
    }

    public override void OnInteract()
    {
        if (isPickedUp)
        {
            PutDown();
        }
        else
        {
            PickUp();
        }
    }

    void PickUp()
    {
        isPickedUp = true;
        hintText = "\u6309 E \u653e\u56de | \u9f20\u6807\u5de6\u952e\u62d6\u52a8\u65cb\u8f6c | \u6eda\u8f6e\u7f29\u653e";

        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera != null)
        {
            transform.SetParent(playerCamera.transform, true);
            transform.localPosition = Vector3.forward * pickupDistance + pickupOffset;
            transform.localRotation = Quaternion.identity;
            transform.localScale = pickupScale;
        }

        autoRotateSpeed = 0f;
        GameManager.Instance?.ShowHint(hintText);

        if (exhibitSound != null)
        {
            AudioManager.Instance?.PlaySFX(exhibitSound);
        }
    }

    void PutDown()
    {
        isPickedUp = false;
        hintText = "\u6309 E \u62ff\u8d77\u89c2\u5bdf";

        transform.SetParent(originalParent, true);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;
        autoRotateSpeed = originalAutoRotateSpeed;

        if (isPlayerNearby)
        {
            GameManager.Instance?.ShowHint(hintText);
        }
        else
        {
            GameManager.Instance?.HideHint();
        }
    }
}
