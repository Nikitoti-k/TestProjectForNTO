using UnityEngine;

// ��������� �������������� ������: ���������� ������, ��������� ����������, ���������.
public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }
    public bool IsBuildingMode => isBuildingMode;

    [SerializeField] private HexGrid hexGrid; // ����� ��� ���������� ������.
    [SerializeField] private Material previewValidMaterial; // �������� ��� ��������� ������.
    [SerializeField] private Material previewInvalidMaterial; // �������� ��� �����������.

    private GameObject currentPreview; // ������� ������ ������.
    private Renderer[] previewRenderers; // ��� �������� ������.
    private bool isBuildingMode; // ������� �� ����� �������������.
    private GameObject buildingPrefab; // ������ �������� ������.
    private int currentBuildingCost; // ��������� �������� ������.

    // ������������� singleton.
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ��������� ����� ������������� � ��������� �������� � ����������.
    public void StartBuilding(GameObject prefab, int cost)
    {
        if (prefab == null || hexGrid == null || previewValidMaterial == null || previewInvalidMaterial == null)
        {
            Debug.LogError("BuildingManager: ����������� ����������� ����������!");
            return;
        }

        EndBuildingMode(); // ������� ���������� ������, ���� ����.

        buildingPrefab = prefab;
        currentBuildingCost = cost;
        isBuildingMode = true;

        currentPreview = Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity);
        currentPreview.SetActive(false);

        // ��������� ������ � ������� ��� ������.
        if (currentPreview.TryGetComponent<BuildingBase>(out var buildingBase))
            buildingBase.enabled = false;

        foreach (var collider in currentPreview.GetComponentsInChildren<Collider>())
            collider.enabled = false;

        previewRenderers = currentPreview.GetComponentsInChildren<Renderer>();
        SetPreviewMaterial(previewValidMaterial);
    }

    // ��������� ��������� ������ � ������������ �����.
    void Update()
    {
        if (!isBuildingMode || currentPreview == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, hexGrid.transform.position.y);
        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            var cell = hexGrid.GetCellFromWorldPos(hitPoint);
            if (cell != null)
            {
                currentPreview.transform.position = cell.WorldPosition;
                currentPreview.transform.localScale = Vector3.one * hexGrid.CellSize * 0.8f;
                currentPreview.SetActive(true);

                // ��������� ���������� ������ � ��������� ��������.
                bool isValid = hexGrid.IsCellFree(cell.Coord);
                SetPreviewMaterial(isValid ? previewValidMaterial : previewInvalidMaterial);

                // ���: ������, ���� ������ �������� � ������� ������.
                if (Input.GetMouseButtonDown(0) && isValid && CurrencyManager.Instance.CanAfford(currentBuildingCost))
                {
                    var buildingObj = Instantiate(buildingPrefab, cell.WorldPosition, Quaternion.identity);
                    if (buildingObj.TryGetComponent<BuildingBase>(out var building))
                    {
                        hexGrid.PlaceBuilding(cell.Coord, building);
                        CurrencyManager.Instance.SpendCurrency(currentBuildingCost);
                    }
                    EndBuildingMode();
                }
            }
            else
            {
                currentPreview.SetActive(false); // �������� ������ ��� �����.
            }
        }

        if (Input.GetMouseButtonDown(1)) EndBuildingMode(); // ���: ������.
    }

    // ������������� �������� ��� ������.
    private void SetPreviewMaterial(Material mat)
    {
        foreach (var renderer in previewRenderers)
            renderer.material = mat;
    }

    // ��������� ����� �������������.
    private void EndBuildingMode()
    {
        isBuildingMode = false;
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
        previewRenderers = null;
        currentBuildingCost = 0;
    }
}