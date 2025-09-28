using UnityEngine;

// Управляет строительством зданий: показывает превью, проверяет валидность, размещает.
public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }
    public bool IsBuildingMode => isBuildingMode;

    [SerializeField] private HexGrid hexGrid; 
    [SerializeField] private GameSceneConfiguration sceneSettings; 

    private GameObject currentPreview; // Текущее превью здания.
    private Renderer[] previewRenderers; // Кэш рендеров превью.
    private Bounds cachedPreviewBounds; // Кэш bounding box превью.
    private bool isBuildingMode;
    private GameObject buildingPrefab; 
    private int currentBuildingCost;

    // Инициализация singleton.
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Запускает режим строительства с указанным префабом и стоимостью.
    public void StartBuilding(GameObject prefab, int cost)
    {
        if (prefab == null || hexGrid == null || sceneSettings == null ||
            sceneSettings.PreviewValidMaterial == null || sceneSettings.PreviewInvalidMaterial == null)
        {
            Debug.LogError("BuildingManager: отсутствуют компоненты!");
            return;
        }

        EndBuildingMode(); 

        buildingPrefab = prefab;
        currentBuildingCost = cost;
        isBuildingMode = true;

        currentPreview = Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity);
        currentPreview.SetActive(false);

        // Отключаем скрипты для превью
        if (currentPreview.TryGetComponent<BuildingBase>(out var buildingBase))
            buildingBase.enabled = false;

        foreach (var collider in currentPreview.GetComponentsInChildren<Collider>())
            collider.enabled = false;

        previewRenderers = currentPreview.GetComponentsInChildren<Renderer>();
        SetPreviewMaterial(sceneSettings.PreviewValidMaterial);

        // Применяем масштабирование и кэшируем bounding box.
        float scaleFactor = sceneSettings != null ? sceneSettings.BuildingScaleFactor : 1f;
        currentPreview.transform.localScale *= hexGrid.CellSize * scaleFactor;
        cachedPreviewBounds = CalculateBounds(currentPreview);
    }

   
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
                // Используем bounding box для выравнивания по плоскости
                float yOffset = -cachedPreviewBounds.min.y;
                currentPreview.transform.position = new Vector3(cell.WorldPosition.x, hexGrid.transform.position.y + yOffset, cell.WorldPosition.z);

                currentPreview.SetActive(true);

                // Проверяем валидность клетки
                bool isValid = hexGrid.IsCellFree(cell.Coord);
                SetPreviewMaterial(isValid ? sceneSettings.PreviewValidMaterial : sceneSettings.PreviewInvalidMaterial);

                
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
                currentPreview.SetActive(false); 
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape)) EndBuildingMode(); 
    }

   
    private void SetPreviewMaterial(Material mat)
    {
        foreach (var renderer in previewRenderers)
            renderer.material = mat;
    }

    // Вычисляет объединённый bounding box для объекта и его дочерних элементов
    private Bounds CalculateBounds(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(obj.transform.position, Vector3.one);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    
    private void EndBuildingMode()
    {
        isBuildingMode = false;
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
        previewRenderers = null;
        cachedPreviewBounds = default; 
        currentBuildingCost = 0;
    }
}