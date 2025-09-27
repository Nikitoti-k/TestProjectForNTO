using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private float tiltAngle = 30f; // Наклон Canvas для изометрии (0 = горизонтально, 45 = под камеру)
    private Camera mainCamera;

    private void OnEnable()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Направление от Canvas к камере, игнорируя Y для горизонтальной ориентации
            Vector3 directionToCamera = mainCamera.transform.position - transform.position;
            directionToCamera.y = 0; // Игнорируем высоту камеры
            if (directionToCamera.sqrMagnitude < 0.01f) return; // Избегаем деления на 0

            // Поворачиваем Canvas по Y, чтобы он смотрел в сторону камеры
            Quaternion lookRotation = Quaternion.LookRotation(-directionToCamera.normalized, Vector3.up);
            // Применяем наклон по X для изометрии
            transform.rotation = Quaternion.Euler(tiltAngle, lookRotation.eulerAngles.y, 0);
        }
        else
        {
            Debug.Log("Пизедц");
        }
    }
}