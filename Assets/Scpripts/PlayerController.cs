using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Controls inputActions;
    public float moveSpeed = 5f;
    public Camera mainCamera; 
    private Vector2 moveInput;

    private void Awake()
    {
        inputActions = new Controls();
    
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Update()
    {
      
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0f; 
        cameraRight.y = 0f;
        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

       
        Vector3 moveDirection = cameraForward * moveInput.y + cameraRight * moveInput.x;

       
        if (moveDirection != Vector3.zero)
        {
          
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }
}