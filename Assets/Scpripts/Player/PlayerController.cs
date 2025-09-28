using UnityEngine;
using UnityEngine.InputSystem;
// Управление камерой на правую кнопку мыши для удобства пользования UI + изначально контролер писался под 3 лицо
public class PlayerController : MonoBehaviour
{
    private Controls inputActions;
    public float moveSpeed = 5f; 
    public float mouseSensitivity = 2f; 
    public GameObject firstPersonCameraObject; 
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Rigidbody rb;
    private Camera fpsCamera;
    private float xRotation = 0f;
    private bool isRightMouseButtonHeld = false;

    private void Awake()
    {
        inputActions = new Controls();
        rb = GetComponent<Rigidbody>();
        fpsCamera = firstPersonCameraObject.GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnEnable()
    {
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;
        inputActions.Player.Look.performed += ctx => isRightMouseButtonHeld = true;
        inputActions.Player.Look.canceled += ctx => isRightMouseButtonHeld = false;
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Update()
    {
        
        if (fpsCamera != null && isRightMouseButtonHeld)
        {
            float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
            float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

            
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            fpsCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

           
            transform.Rotate(Vector3.up * mouseX);
        }
    }

    private void FixedUpdate()
    {
        if (fpsCamera == null) return;

        
        Vector3 cameraForward = fpsCamera.transform.forward;
        Vector3 cameraRight = fpsCamera.transform.right;
        cameraForward.y = 0f; 
        cameraRight.y = 0f;
        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        
        Vector3 moveDirection = cameraForward * moveInput.y + cameraRight * moveInput.x;

        
        if (moveDirection != Vector3.zero)
        {
            Vector3 moveVelocity = moveDirection * moveSpeed;
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z); 
        }
        else
        {
           
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }
}