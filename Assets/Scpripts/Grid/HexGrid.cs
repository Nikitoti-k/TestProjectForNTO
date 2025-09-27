using System.Collections.Generic;
using UnityEngine;

// Управляет гексагональной сеткой: генерация, размещение зданий, визуализация.
[ExecuteInEditMode]
public class HexGrid : MonoBehaviour, IGrid
{
    public static HexGrid Instance { get; private set; }
    public float CellSize => cellSize;
    public Dictionary<HexCoord, HexCell> Cells => cells;

    [SerializeField] private int gridRadius = 5; // Радиус сетки.
    [SerializeField] private float cellSize = 2f; // Размер клетки.
    [SerializeField] private bool drawGizmos = true; // Показывать гизмо в редакторе.
    [SerializeField] private bool showGridInGame = true; // Показывать сетку в игре.
    [SerializeField] private Camera playerCamera; // Камера игрока.
    [SerializeField] public GameObject headquartersPrefab; // Префаб штаба.

    private Dictionary<HexCoord, HexCell> cells = new Dictionary<HexCoord, HexCell>(); // Клетки сетки.
    private List<GameObject> placedBuildings = new List<GameObject>(); // Размещённые здания.
    private GameObject linesParent; // Родитель для линий сетки.
    private Headquarters headquarters; // Ссылка на штаб.

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

    // Генерирует сетку и загружает сохранение.
    void Start()
    {
        if (cells.Count == 0)
            RegenerateGrid();

        // Загружаем сохранение только после генерации сетки.
        if (Application.isPlaying && SaveManager.Instance != null)
            SaveManager.Instance.LoadGame();

        // Размещаем штаб, если он не создан.
        if (headquarters == null && headquartersPrefab != null && Application.isPlaying)
            PlaceHeadquarters();
    }

    // Очищает здания при уничтожении объекта.
    void OnDestroy()
    {
        foreach (var building in placedBuildings)
        {
            if (building != null)
                Destroy(Application.isPlaying ? building : building, 0f);
        }
        placedBuildings.Clear();
        headquarters = null;
    }

    // Генерирует гексагональную сетку.
    [ContextMenu("GenerateHexGrid")]
    public void RegenerateGrid()
    {
        ClearOldLines();
        cells.Clear();

        for (int q = -gridRadius; q <= gridRadius; q++)
        {
            int r1 = Mathf.Max(-gridRadius, -q - gridRadius);
            int r2 = Mathf.Min(gridRadius, -q + gridRadius);
            for (int r = r1; r <= r2; r++)
            {
                var coord = new HexCoord(q, r);
                cells.Add(coord, new HexCell(coord, CoordToWorldPos(coord)));
            }
        }

        if (showGridInGame || !Application.isPlaying)
            SetupLineRenderers();
    }

    // Очищает старые линии сетки.
    private void ClearOldLines()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
        linesParent = null;
    }

    // Преобразует координаты в мировую позицию.
    private Vector3 CoordToWorldPos(HexCoord coord)
    {
        float x = cellSize * Mathf.Sqrt(3f) * (coord.q + coord.r * 0.5f);
        float z = cellSize * 1.5f * coord.r;
        return new Vector3(x, 0f, z) + transform.position;
    }

    // Находит клетку по мировой позиции.
    public HexCell GetCellFromWorldPos(Vector3 worldPos)
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("HexGrid: Камера игрока не назначена!");
            return null;
        }

        Vector3 localPos = worldPos - transform.position;
        float q = (Mathf.Sqrt(3f) / 3f * localPos.x - 1f / 3f * localPos.z) / cellSize;
        float r = (2f / 3f * localPos.z) / cellSize;
        return GetCell(RoundHexCoord(q, r));
    }

    // Возвращает клетку по координатам.
    public HexCell GetCell(HexCoord coord)
    {
        return cells.TryGetValue(coord, out var cell) ? cell : null;
    }

    // Округляет дробные координаты до целых.
    private HexCoord RoundHexCoord(float q, float r)
    {
        float s = -q - r;
        int qInt = Mathf.RoundToInt(q);
        int rInt = Mathf.RoundToInt(r);
        int sInt = Mathf.RoundToInt(s);

        float qDelta = Mathf.Abs(qInt - q);
        float rDelta = Mathf.Abs(rInt - r);
        float sDelta = Mathf.Abs(sInt - s);

        if (qDelta > rDelta && qDelta > sDelta) qInt = -rInt - sInt;
        else if (rDelta > sDelta) rInt = -qInt - sInt;

        return new HexCoord(qInt, rInt);
    }

    // Проверяет, свободна ли клетка.
    public bool IsCellFree(HexCoord coord) => cells.TryGetValue(coord, out var cell) && !cell.IsOccupied;

    // Размещает здание в клетке.
    public void PlaceBuilding(HexCoord coord, BuildingBase building)
    {
        if (!cells.TryGetValue(coord, out var cell) || cell.IsOccupied)
        {
            Debug.LogWarning($"HexGrid: Клетка {coord} занята или не существует!");
            return;
        }

        cell.Occupy(building);
        if (building != null)
        {
            building.transform.position = cell.WorldPosition;
            building.transform.localScale *= cellSize;
            building.enabled = true;
            building.Initialize(coord);
            placedBuildings.Add(building.gameObject);

            if (building is Wall wall)
            {
                wall.UpdateBridges();
                UpdateNeighborBridges(coord);
            }
        }
    }

    // Освобождает клетку.
    public void FreeCell(HexCoord coord)
    {
        if (cells.TryGetValue(coord, out var cell))
        {
            cell.Free();
            UpdateNeighborBridges(coord);
        }
    }

    // Размещает штаб в центре сетки.
    private void PlaceHeadquarters()
    {
        if (headquarters != null || headquartersPrefab == null)
        {
            Debug.LogWarning("HexGrid: Штаб уже существует или префаб не назначен!");
            return;
        }

        var center = new HexCoord(0, 0);
        if (!cells.ContainsKey(center) || !AreNeighborsFree(center))
        {
            Debug.LogWarning("HexGrid: Нельзя разместить штаб в центре!");
            return;
        }

        var hqObj = Instantiate(headquartersPrefab, CoordToWorldPos(center), Quaternion.identity);
        if (hqObj == null) return;

        headquarters = hqObj.GetComponent<Headquarters>();
        if (headquarters != null)
        {
            headquarters.transform.localScale *= cellSize; // Масштабируем штаб.
            headquarters.Initialize(center);
            placedBuildings.Add(hqObj);
            cells[center].Occupy(headquarters);
        }
        else
        {
            Debug.LogError("HexGrid: Штаб не имеет компонента Headquarters!");
            Destroy(hqObj);
        }
    }

    // Проверяет, свободны ли соседние клетки.
    private bool AreNeighborsFree(HexCoord center)
    {
        foreach (var coord in GetNeighborCoords(center))
        {
            if (!cells.ContainsKey(coord) || !IsCellFree(coord)) return false;
        }
        return true;
    }

    // Возвращает штаб.
    public Headquarters GetHeadquarters() => headquarters;

    // Возвращает координаты соседних клеток.
    public List<HexCoord> GetNeighborCoords(HexCoord center)
    {
        var directions = new HexCoord[]
        {
            new(1, 0), new(1, -1), new(0, -1),
            new(-1, 0), new(-1, 1), new(0, 1)
        };
        var neighbors = new List<HexCoord>();
        foreach (var dir in directions)
            neighbors.Add(center + dir);
        return neighbors;
    }

    // Возвращает мировую позицию клетки.
    public Vector3 GetWorldPosFromCoord(HexCoord coord) => cells.TryGetValue(coord, out var cell) ? cell.WorldPosition : Vector3.zero;

    // Возвращает здание в клетке.
    public BuildingBase GetBuildingAt(HexCoord coord) => cells.TryGetValue(coord, out var cell) ? cell.Building : null;

    // Обновляет мосты соседних стен.
    private void UpdateNeighborBridges(HexCoord coord)
    {
        foreach (var nCoord in GetNeighborCoords(coord))
        {
            if (GetBuildingAt(nCoord) is Wall wall)
                wall.UpdateBridges();
        }
    }

    // Создаёт линии сетки для визуализации.
    private void SetupLineRenderers()
    {
        linesParent = new GameObject("GridLines");
        linesParent.transform.SetParent(transform);
        linesParent.transform.localPosition = Vector3.zero;

        foreach (var cell in cells.Values)
        {
            var lineObj = new GameObject($"HexLine_{cell.Coord.q}_{cell.Coord.r}");
            lineObj.transform.SetParent(linesParent.transform);
            var lr = lineObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = lr.endColor = Color.white;
            lr.startWidth = lr.endWidth = 0.05f;
            lr.useWorldSpace = true;
            lr.loop = true;

            Vector3[] positions = new Vector3[6];
            for (int i = 0; i < 6; i++)
            {
                float angleDeg = 60f * i - 30f;
                float angleRad = Mathf.Deg2Rad * angleDeg;
                positions[i] = cell.WorldPosition + new Vector3(cellSize * Mathf.Cos(angleRad), 0f, cellSize * Mathf.Sin(angleRad));
            }
            lr.positionCount = 6;
            lr.SetPositions(positions);
        }
    }

    // Рисует гизмо сетки в редакторе.
    private void OnDrawGizmos()
    {
        if (!drawGizmos || cells.Count == 0) return;

        Gizmos.color = Color.white;
        foreach (var cell in cells.Values)
        {
            Vector3[] corners = new Vector3[6];
            for (int i = 0; i < 6; i++)
            {
                float angleDeg = 60f * i - 30f;
                float angleRad = Mathf.Deg2Rad * angleDeg;
                corners[i] = cell.WorldPosition + new Vector3(cellSize * Mathf.Cos(angleRad), 0f, cellSize * Mathf.Sin(angleRad));
            }
            for (int i = 0; i < 6; i++)
                Gizmos.DrawLine(corners[i], corners[(i + 1) % 6]);
        }
    }
}