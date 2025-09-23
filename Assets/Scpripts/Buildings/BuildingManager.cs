using UnityEngine;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private Material previewMaterialTemplate; // Материал для превью, задаётся в инспекторе

    private GameObject currentPreview;
    private bool isBuildingMode = false;
    private GameObject buildingPrefab;
    private Material previewMaterial; // Клон материала для runtime изменений
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

        // Проверяем BuildingBase
        BuildingBase buildingBase = prefab.GetComponent<BuildingBase>();
        if (buildingBase == null)
        {
            Debug.LogError("Building prefab does not have BuildingBase component!");
            return;
        }

        // Получаем BuildingData (для Turret, Wall, Factory, Mixing)
        BuildingData buildingData = GetBuildingData(buildingBase);
        GameObject previewModel = null;

        if (buildingData != null)
        {
            // Проверяем модули для ModelPrefab (например, TurretModule)
            TurretModule turretModule = GetModule<TurretModule>(buildingData);
            if (turretModule != null && turretModule.LevelData.Count > 0 && turretModule.LevelData[0].ModelPrefab != null)
            {
                previewModel = turretModule.LevelData[0].ModelPrefab;
            }
            // Можно добавить другие модули (WallModule, FactoryModule), если они имеют ModelPrefab
        }

        // Fallback: если нет ModelPrefab, используем сам префаб
        if (previewModel == null)
        {
            previewModel = prefab;
        }

        // Инстанцируем превью
        currentPreview = Instantiate(previewModel, Vector3.zero, Quaternion.identity);
        currentPreview.SetActive(false);

        // Отключаем BuildingBase (или его наследника, например, Turret)
        BuildingBase previewBuildingBase = currentPreview.GetComponent<BuildingBase>();
        if (previewBuildingBase != null)
        {
            previewBuildingBase.enabled = false;
        }

        // Применяем материал из инспектора
        Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            // Клонируем материал из шаблона
            if (previewMaterialTemplate != null)
            {
                previewMaterial = new Material(previewMaterialTemplate);
            }
            else
            {
                // Fallback: используем материал первого рендера
                previewMaterial = new Material(renderers[0].sharedMaterial);
                Debug.LogWarning("PreviewMaterialTemplate not assigned in BuildingManager. Using default material.");
            }

            // Устанавливаем полупрозрачность
            Color color = previewMaterial.color;
            color.a = 0.5f;
            previewMaterial.color = color;

            // Применяем материал ко всем рендерам
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

                // Опционально: меняем цвет в зависимости от валидности клетки
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

    // Вспомогательные методы для работы с модулями
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