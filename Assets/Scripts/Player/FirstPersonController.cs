using UnityEngine;

/// <summary>
/// 第一人称控制器 - WASD移动 + 鼠标视角 + 重力
/// 挂载到玩家GameObject上（包含CharacterController组件）
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float gravity = -9.81f;

    [Header("Look")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;        // 垂直视角上限（防止翻过头）

    private CharacterController controller;
    private Camera playerCamera;
    private Vector3 velocity;               // 垂直速度（重力）
    private float verticalRotation = 0f;    // 当前垂直旋转角

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // 尝试获取子物体上的Camera
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            Debug.LogError("FirstPersonController: 未找到Camera组件！请在玩家GameObject下放置一个Camera子物体。");
        }

        // 从GameManager读取设置
        if (GameManager.Instance != null)
        {
            mouseSensitivity = GameManager.Instance.mouseSensitivity;
            walkSpeed = GameManager.Instance.moveSpeed;
        }
    }

    void Update()
    {
        // 暂停时不响应
        if (Time.timeScale < 0.1f) return;
        if (Cursor.lockState != CursorLockMode.Locked) return;

        HandleLook();
        HandleMovement();
    }

    /// <summary>
    /// 鼠标视角控制
    /// </summary>
    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 水平旋转（玩家整体旋转）
        transform.Rotate(Vector3.up * mouseX);

        // 垂直旋转（仅旋转Camera，限制角度）
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    /// <summary>
    /// WASD移动 + 重力
    /// </summary>
    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");  // A/D
        float vertical = Input.GetAxis("Vertical");      // W/S

        // 基于玩家朝向计算移动方向
        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        // 按住Shift奔跑
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        controller.Move(move * speed * Time.deltaTime);

        // 重力处理
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 小负值确保贴地
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
