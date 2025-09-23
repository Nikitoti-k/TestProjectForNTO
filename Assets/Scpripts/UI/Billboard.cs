using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private float tiltAngle = 30f; // Ќаклон Canvas дл€ изометрии (0 = горизонтально, 45 = под камеру)
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Ќаправление от Canvas к камере, игнориру€ Y дл€ горизонтальной ориентации
            Vector3 directionToCamera = mainCamera.transform.position - transform.position;
            directionToCamera.y = 0; // »гнорируем высоту камеры
            if (directionToCamera.sqrMagnitude < 0.01f) return; // »збегаем делени€ на 0

            // ѕоворачиваем Canvas по Y, чтобы он смотрел в сторону камеры
            Quaternion lookRotation = Quaternion.LookRotation(-directionToCamera.normalized, Vector3.up);
            // ѕримен€ем наклон по X дл€ изометрии
            transform.rotation = Quaternion.Euler(tiltAngle, lookRotation.eulerAngles.y, 0);
        }
    }
}