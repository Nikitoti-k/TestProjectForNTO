// Управляет размещением построек: отображает превью и подтверждает установку.
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }
    public bool IsBuildingMode => isBuildingMode;

    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private Material previewValidMaterial;
    [SerializeField] private Material previewInvalidMaterial;

    private GameObject currentPreview;
    private bool isBuildingMode = false;
    private GameObject buildingPrefab;
    private int currentBuildingCost;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartBuilding(GameObject prefab, int cost)
    {
        if (prefab == null || hexGrid == null || previewValidMaterial == null || previewInvalidMaterial == null)
        {
            return;
        }

        if (isBuildingMode && currentPreview != null)
        {
            Destroy(currentPreview);
        }

        buildingPrefab = prefab;
        currentBuildingCost = cost;
        isBuildingMode = true;

        currentPreview = Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity);
        currentPreview.SetActive(false);
        BuildingBase buildingBase = currentPreview.GetComponent<BuildingBase>();
        if (buildingBase != null)
        {
            buildingBase.enabled = false;
        }

        foreach (var collider in currentPreview.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }

        foreach (var renderer in currentPreview.GetComponentsInChildren<Renderer>())
        {
            renderer.material = previewValidMaterial;
        }
    }

    private void Update()
    {
        if (!isBuildingMode || currentPreview == null)
        {
            return;
        }

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

                bool isValid = hexGrid.IsCellFree(cell.Coord);
                Material currentMaterial = isValid ? previewValidMaterial : previewInvalidMaterial;
                foreach (var renderer in currentPreview.GetComponentsInChildren<Renderer>())
                {
                    renderer.material = currentMaterial;
                }

                if (Input.GetMouseButtonDown(0) && isValid)
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
            else
            {
                currentPreview.SetActive(false);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            EndBuildingMode();
        }
    }

    private void EndBuildingMode()
    {
        isBuildingMode = false;
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
        currentBuildingCost = 0;
    }
}