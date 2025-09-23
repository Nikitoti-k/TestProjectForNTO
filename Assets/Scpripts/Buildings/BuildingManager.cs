using UnityEngine;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private Material previewMaterialTemplate; // �������� ��� ������, ������� � ����������

    private GameObject currentPreview;
    private bool isBuildingMode = false;
    private GameObject buildingPrefab;
    private Material previewMaterial; // ���� ��������� ��� runtime ���������
    private int currentBuildingCost;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartBuilding(GameObject prefab, int cost)
    {
        if (prefab == null) return;

        buildingPrefab = prefab;
        currentBuildingCost = cost;
        isBuildingMode = true;

        // ��������� BuildingBase
        BuildingBase buildingBase = prefab.GetComponent<BuildingBase>();
        if (buildingBase == null)
        {
            Debug.LogError("Building prefab does not have BuildingBase component!");
            return;
        }

        // �������� BuildingData (��� Turret, Wall, Factory, Mixing)
        BuildingData buildingData = GetBuildingData(buildingBase);
        GameObject previewModel = null;

        if (buildingData != null)
        {
            // ��������� ������ ��� ModelPrefab (��������, TurretModule)
            TurretModule turretModule = GetModule<TurretModule>(buildingData);
            if (turretModule != null && turretModule.LevelData.Count > 0 && turretModule.LevelData[0].ModelPrefab != null)
            {
                previewModel = turretModule.LevelData[0].ModelPrefab;
            }
            // ����� �������� ������ ������ (WallModule, FactoryModule), ���� ��� ����� ModelPrefab
        }

        // Fallback: ���� ��� ModelPrefab, ���������� ��� ������
        if (previewModel == null)
        {
            previewModel = prefab;
        }

        // ������������ ������
        currentPreview = Instantiate(previewModel, Vector3.zero, Quaternion.identity);
        currentPreview.SetActive(false);

        // ��������� BuildingBase (��� ��� ����������, ��������, Turret)
        BuildingBase previewBuildingBase = currentPreview.GetComponent<BuildingBase>();
        if (previewBuildingBase != null)
        {
            previewBuildingBase.enabled = false;
        }

        // ��������� �������� �� ����������
        Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            // ��������� �������� �� �������
            if (previewMaterialTemplate != null)
            {
                previewMaterial = new Material(previewMaterialTemplate);
            }
            else
            {
                // Fallback: ���������� �������� ������� �������
                previewMaterial = new Material(renderers[0].sharedMaterial);
                Debug.LogWarning("PreviewMaterialTemplate not assigned in BuildingManager. Using default material.");
            }

            // ������������� ����������������
            Color color = previewMaterial.color;
            color.a = 0.5f;
            previewMaterial.color = color;

            // ��������� �������� �� ���� ��������
            foreach (var renderer in renderers)
            {
                renderer.material = previewMaterial;
            }
        }
        else
        {
            Debug.LogWarning("No renderers found in preview model!");
        }
    }

    private void Update()
    {
        if (!isBuildingMode) return;

        HandlePreview();

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, hexGrid.transform.position.y);
            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                HexCell cell = hexGrid.GetCellFromWorldPos(hitPoint);
                if (cell != null && hexGrid.IsCellFree(cell.Coord))
                {
                    GameObject buildingObj = Instantiate(buildingPrefab, cell.WorldPosition, Quaternion.identity);
                    BuildingBase building = buildingObj.GetComponent<BuildingBase>();
                    if (building != null)
                    {
                        hexGrid.PlaceBuilding(cell.Coord, building);
                        CurrencyManager.Instance.SpendCurrency(currentBuildingCost);
                    }
                    EndBuildingMode();
                }
            }
        }
    }

    private void HandlePreview()
    {
        if (currentPreview == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, hexGrid.transform.position.y);
        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            HexCell cell = hexGrid.GetCellFromWorldPos(hitPoint);
            if (cell != null)
            {
                currentPreview.transform.position = cell.WorldPosition;
                currentPreview.transform.localScale = Vector3.one * hexGrid.CellSize * 0.8f;
                currentPreview.SetActive(true);

                // �����������: ������ ���� � ����������� �� ���������� ������
                /*
                if (previewMaterial != null)
                {
                    Color color = previewMaterial.color;
                    color = hexGrid.IsCellFree(cell.Coord) ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
                    previewMaterial.color = color;
                }
                */
                return;
            }
        }
        currentPreview.SetActive(false);
    }

    private void EndBuildingMode()
    {
        isBuildingMode = false;
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
        if (previewMaterial != null)
        {
            Destroy(previewMaterial);
            previewMaterial = null;
        }
        currentBuildingCost = 0;
    }

    // ��������������� ������ ��� ������ � ��������
    private BuildingData GetBuildingData(BuildingBase building)
    {
        var field = typeof(BuildingBase).GetField("data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(building) as BuildingData;
    }

    private T GetModule<T>(BuildingData data) where T : BuildingModule
    {
        return data?.Modules.Find(m => m is T) as T;
    }
}