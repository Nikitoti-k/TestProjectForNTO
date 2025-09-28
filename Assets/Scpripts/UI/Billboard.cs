using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private float tiltAngle = 30f; // Ќаклон Canvas дл€ изометрии (0 = горизонтально, 45 = под камеру)
    private Camera mainCamera;

    private void OnEnable()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
           
            Vector3 directionToCamera = mainCamera.transform.position - transform.position;
            directionToCamera.y = 0;
            if (directionToCamera.sqrMagnitude < 0.01f) return;

           
            Quaternion lookRotation = Quaternion.LookRotation(-directionToCamera.normalized, Vector3.up);
           
            transform.rotation = Quaternion.Euler(tiltAngle, lookRotation.eulerAngles.y, 0);
        }
       
    }
}