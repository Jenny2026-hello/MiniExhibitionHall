using UnityEngine;

/// <summary>
/// 展品拾取查看 - 允许玩家拾起展品并在手中旋转查看
/// 在ExhibitBase基础上扩展功能
/// </summary>
public class ExhibitPickup : ExhibitBase
{
    [Header("拾取设置")]
    public float pickupDistance = 1.5f;      // 拾取后距相机的距离
    public float rotationSpeed = 100f;       // 手动旋转速度
    public Vector3 pickupOffset = Vector3.zero; // 拾取后的位置偏移
    public Vector3 pickupScale = Vector3.one;   // 拾取后的缩放
    public float minZoom = 0.5f;             // 最小缩放距离
    public float maxZoom = 3f;               // 最大缩放距离

    private bool isPickedUp = false;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private float originalAutoRotateSpeed;
    private Camera playerCamera;

    void Start()
    {
        // 保存原始Transform
        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
        originalAutoRotateSpeed = autoRotateSpeed;

        playerCamera = Camera.main;
    }

    /// <summary>
    /// 在基类Update之后执行拾取相关逻辑
    /// </summary>
    void LateUpdate()
    {
        // 未被拾取时，基类的Update会处理自动旋转
        if (!isPickedUp) return;

        // 鼠标左键拖动旋转
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, -mouseX, Space.World);
            transform.Rotate(Vector3.right, mouseY, Space.World);
        }

        // 鼠标滚轮缩放
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 pos = transform.localPosition;
            pos.z += scroll * 0.5f;
            pos.z = Mathf.Clamp(pos.z, minZoom, maxZoom);
            transform.localPosition = pos;
        }

        // 按E放下
        if (Input.GetKeyDown(KeyCode.E))
        {
            PutDown();
        }
    }

    public override void OnInteract()
    {
        if (!isPickedUp)
        {
            PickUp();
        }
        else
        {
            PutDown();
        }
    }

    /// <summary>
    /// 拾取展品
    /// </summary>
    void PickUp()
    {
        isPickedUp = true;

        // 修改提示
        hintText = "按 E 放下 | 鼠标拖动旋转 | 滚轮缩放";

        // 挂载到相机下
        if (playerCamera != null)
        {
            transform.SetParent(playerCamera.transform);
            transform.localPosition = Vector3.forward * pickupDistance + pickupOffset;
            transform.localRotation = Quaternion.identity;
            transform.localScale = pickupScale;
        }

        // 停止自动旋转
        autoRotateSpeed = 0;

        // 播放音效
        if (exhibitSound != null)
        {
            AudioManager.Instance?.PlaySFX(exhibitSound);
        }
    }

    /// <summary>
    /// 放下展品
    /// </summary>
    void PutDown()
    {
        isPickedUp = false;

        // 恢复提示
        hintText = "按 E 拾取查看";

        // 恢复原始Transform
        transform.SetParent(originalParent);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;

        // 恢复自动旋转
        autoRotateSpeed = originalAutoRotateSpeed;
    }
}
