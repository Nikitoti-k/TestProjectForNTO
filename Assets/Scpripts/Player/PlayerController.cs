using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Controls inputActions;
    public float moveSpeed = 5f; // Скорость движения
    public float rotationSpeed = 720f; // Скорость поворота (градусы/сек)
    public GameObject thirdPersonCameraObject; // Объект камеры для третьего лица
    public GameObject firstPersonCameraObject; // Объект камеры для первого лица
    public Key togglePerspectiveKey = Key.F; // Клавиша для переключения перспективы
    private Vector2 moveInput;
    private Rigidbody rb;
    private bool isFirstPerson = false;

    private void Awake()
    {
        // Инициализация Input System
        inputActions = new Controls();

        // Проверка Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("На объекте игрока отсутствует компонент Rigidbody! Добавьте Rigidbody и заморозьте вращение по X и Z.");
        }

        // Проверка объектов камер
        if (thirdPersonCameraObject == null || firstPersonCameraObject == null)
        {
            Debug.LogError("Один или оба объекта камер (ThirdPersonCameraObject или FirstPersonCameraObject) не назначены в инспекторе!");
        }

        // Начальная настройка камер
        UpdateCameraPerspective();
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
        // Проверка нажатия клавиши для переключения перспективы
        if (Keyboard.current != null && Keyboard.current[togglePerspectiveKey].wasPressedThisFrame)
        {
            isFirstPerson = !isFirstPerson;
            UpdateCameraPerspective();
        }
    }

    private void FixedUpdate()
    {
        // Получаем активную камеру
        GameObject activeCameraObject = isFirstPerson ? firstPersonCameraObject : thirdPersonCameraObject;
        if (activeCameraObject == null) return;

        Camera activeCamera = activeCameraObject.GetComponent<Camera>();
         if (activeCamera == null)
         {
             Debug.LogError($"Объект {activeCameraObject.name} не содержит компонент Camera!");
             return;
         }

         // Получаем направление камеры
         Vector3 cameraForward = activeCamera.transform.forward;
         Vector3 cameraRight = activeCamera.transform.right;
         cameraForward.y = 0f; // Движение только по XZ
         cameraRight.y = 0f;
         cameraForward = cameraForward.normalized;
         cameraRight = cameraRight.normalized;

         // Вычисляем направление движения
         Vector3 moveDirection = cameraForward * moveInput.y + cameraRight * moveInput.x;
        
        // Применяем движение через Rigidbody
        if (moveDirection != Vector3.zero)
        {
            // Движение
            Vector3 moveVelocity = moveDirection * moveSpeed;
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z); // Сохраняем Y для гравитации

            // Плавный поворот игрока в сторону движения
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // Остановка горизонтального движения
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    private void UpdateCameraPerspective()
    {
        if (thirdPersonCameraObject == null || firstPersonCameraObject == null) return;

        if (isFirstPerson)
        {
            // Активируем камеру от первого лица, деактивируем от третьего
            thirdPersonCameraObject.SetActive(false);
            firstPersonCameraObject.SetActive(true);
        }
        else
        {
            // Активируем камеру от третьего лица, деактивируем от первого
            firstPersonCameraObject.SetActive(false);
            thirdPersonCameraObject.SetActive(true);
        }
    }
}