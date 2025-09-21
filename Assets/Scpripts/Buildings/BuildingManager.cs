using UnityEngine;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private GameObject turretPrefab;
    [SerializeField] private Button buildTurretButton;

    private GameObject currentPreview;
    private bool isBuildingMode = false;
    private GameObject buildingPrefab;
    private Material previewMaterial;

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

    private void Start()
    {
        if (buildTurretButton != null)
        {
            buildTurretButton.onClick.AddListener(StartBuildingTurret);
        }
    }

    public void StartBuildingTurret()
    {
        if (turretPrefab == null) return;

        buildingPrefab = turretPrefab;
        isBuildingMode = true;

        currentPreview = Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity);
        currentPreview.SetActive(false);
        Renderer renderer = currentPreview.GetComponent<Renderer>();
        if (renderer != null)
        {
            previewMaterial = new Material(renderer.sharedMaterial);
            Color color = previewMaterial.color;
            color.a = 0.5f;
            previewMaterial.color = color;
            renderer.material = previewMaterial;
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
            if (cell != null && hexGrid.IsCellFree(cell.Coord))
            {
                currentPreview.transform.position = cell.WorldPosition;
                currentPreview.transform.localScale = Vector3.one * hexGrid.CellSize * 0.8f;
                currentPreview.SetActive(true);
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
        }
    }
}