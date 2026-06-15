using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float gravity = -9.81f;

    [Header("Look")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 82f;

    private CharacterController controller;
    private Camera playerCamera;
    private Vector3 velocity;
    private float verticalRotation;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera == null)
        {
            Debug.LogError("FirstPersonController: Player is missing a child Camera.");
        }

        if (GameManager.Instance != null)
        {
            mouseSensitivity = GameManager.Instance.mouseSensitivity;
            walkSpeed = GameManager.Instance.moveSpeed;
        }
    }

    void Update()
    {
        if (Time.timeScale < 0.1f) return;
        if (Cursor.lockState != CursorLockMode.Locked) return;

        HandleLook();
        HandleMovement();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        if (GameManager.Instance != null && GameManager.Instance.invertY)
        {
            mouseY = -mouseY;
        }

        transform.Rotate(Vector3.up * mouseX);
        verticalRotation = Mathf.Clamp(verticalRotation - mouseY, -maxLookAngle, maxLookAngle);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        if (move.sqrMagnitude > 1f) move.Normalize();

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        controller.Move(move * speed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
