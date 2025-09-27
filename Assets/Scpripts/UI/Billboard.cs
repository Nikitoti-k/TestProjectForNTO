using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private float tiltAngle = 30f; // ������ Canvas ��� ��������� (0 = �������������, 45 = ��� ������)
    private Camera mainCamera;

    private void OnEnable()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            // ����������� �� Canvas � ������, ��������� Y ��� �������������� ����������
            Vector3 directionToCamera = mainCamera.transform.position - transform.position;
            directionToCamera.y = 0; // ���������� ������ ������
            if (directionToCamera.sqrMagnitude < 0.01f) return; // �������� ������� �� 0

            // ������������ Canvas �� Y, ����� �� ������� � ������� ������
            Quaternion lookRotation = Quaternion.LookRotation(-directionToCamera.normalized, Vector3.up);
            // ��������� ������ �� X ��� ���������
            transform.rotation = Quaternion.Euler(tiltAngle, lookRotation.eulerAngles.y, 0);
        }
        else
        {
            Debug.Log("������");
        }
    }
}