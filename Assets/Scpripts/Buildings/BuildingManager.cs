using UnityEngine;

// Управляет строительством зданий: показывает превью, проверяет валидность, размещает.
public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }
    public bool IsBuildingMode => isBuildingMode;

    [SerializeField] private HexGrid hexGrid; // Сетка для размещения зданий.
    [SerializeField] private Material previewValidMaterial; // Материал для валидного превью.
    [SerializeField] private Material previewInvalidMaterial; // Материал для невалидного.

    private GameObject currentPreview; // Текущее превью здания.
    private Renderer[] previewRenderers; // Кэш рендеров превью.
    private bool isBuildingMode; // Активен ли режим строительства.
    private GameObject buildingPrefab; // Префаб текущего здания.
    private int currentBuildingCost; // Стоимость текущего здания.

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
        if (prefab == null || hexGrid == null || previewValidMaterial == null || previewInvalidMaterial == null)
        {
            Debug.LogError("BuildingManager: Отсутствуют необходимые компоненты!");
            return;
        }

        EndBuildingMode(); // Очищаем предыдущее превью, если есть.

        buildingPrefab = prefab;
        currentBuildingCost = cost;
        isBuildingMode = true;

        currentPreview = Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity);
        currentPreview.SetActive(false);

        // Отключаем физику и скрипты для превью.
        if (currentPreview.TryGetComponent<BuildingBase>(out var buildingBase))
            buildingBase.enabled = false;

        foreach (var collider in currentPreview.GetComponentsInChildren<Collider>())
            collider.enabled = false;

        previewRenderers = currentPreview.GetComponentsInChildren<Renderer>();
        SetPreviewMaterial(previewValidMaterial);
    }

    // Обновляет положение превью и обрабатывает клики.
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

                // Проверяем валидность клетки и обновляем материал.
                bool isValid = hexGrid.IsCellFree(cell.Coord);
                SetPreviewMaterial(isValid ? previewValidMaterial : previewInvalidMaterial);

                // ЛКМ: строим, если клетка свободна и хватает валюты.
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
                currentPreview.SetActive(false); // Скрываем превью вне сетки.
            }
        }

        if (Input.GetMouseButtonDown(1)) EndBuildingMode(); // ПКМ: отмена.
    }

    // Устанавливает материал для превью.
    private void SetPreviewMaterial(Material mat)
    {
        foreach (var renderer in previewRenderers)
            renderer.material = mat;
    }

    // Завершает режим строительства.
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