// Управляет гексагональной сеткой: генерация, размещение зданий, визуализация.
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HexGrid : MonoBehaviour, IGrid
{
    public static HexGrid Instance { get; private set; }
    public float CellSize => sceneSettings != null ? sceneSettings.CellSize : 2f;
    public Dictionary<HexCoord, HexCell> Cells => cells;

    [SerializeField] private GameSceneConfiguration sceneSettings;
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private bool showGridInGame = true;
    [SerializeField] private Camera playerCamera;

    private Dictionary<HexCoord, HexCell> cells = new Dictionary<HexCoord, HexCell>();
    private List<GameObject> placedBuildings = new List<GameObject>();
    private GameObject linesParent;
    private Headquarters headquarters;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (cells.Count == 0)
            RegenerateGrid();

        if (Application.isPlaying && SaveManager.Instance != null)
            SaveManager.Instance.LoadGame();

        if (headquarters == null && sceneSettings != null && sceneSettings.HeadquartersPrefab != null && Application.isPlaying)
            PlaceHeadquarters();
    }

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

    public void RegenerateGrid()
    {
        if (sceneSettings == null)
        {
            Debug.LogError("HexGrid: GameSceneConfiguration не назначен!");
            return;
        }
        RegenerateGrid(sceneSettings);
    }

    public void RegenerateGrid(GameSceneConfiguration settings)
    {
        ClearOldLines();
        cells.Clear();

        int gridRadius = settings.GridRadius;
        float cellSize = settings.CellSize;

        // Генерация гексагональных координат в пределах радиуса
        for (int q = -gridRadius; q <= gridRadius; q++)
        {
            int r1 = Mathf.Max(-gridRadius, -q - gridRadius);
            int r2 = Mathf.Min(gridRadius, -q + gridRadius);
            for (int r = r1; r <= r2; r++)
            {
                var coord = new HexCoord(q, r);
                cells.Add(coord, new HexCell(coord, CoordToWorldPos(coord, cellSize)));
            }
        }

        if (showGridInGame || !Application.isPlaying)
            SetupLineRenderers();
    }

    private void ClearOldLines()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
        linesParent = null;
    }

    private Vector3 CoordToWorldPos(HexCoord coord, float cellSize)
    {
        // Гексагональные в мировые координаты
        float x = cellSize * Mathf.Sqrt(3f) * (coord.q + coord.r * 0.5f);
        float z = cellSize * 1.5f * coord.r;
        return new Vector3(x, 0f, z) + transform.position;
    }

    private Vector3 CoordToWorldPos(HexCoord coord) => CoordToWorldPos(coord, CellSize);

    public HexCell GetCellFromWorldPos(Vector3 worldPos)
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("HexGrid: Камера игрока не назначена!");
            return null;
        }

        // Мировые в гексагональные координаты
        Vector3 localPos = worldPos - transform.position;
        float q = (Mathf.Sqrt(3f) / 3f * localPos.x - 1f / 3f * localPos.z) / CellSize;
        float r = (2f / 3f * localPos.z) / CellSize;
        return GetCell(RoundHexCoord(q, r));
    }

    public HexCell GetCell(HexCoord coord)
    {
        return cells.TryGetValue(coord, out var cell) ? cell : null;
    }

    private HexCoord RoundHexCoord(float q, float r)
    {
        // Округление дробных гексагональных координат с учетом кубической системы (q + r + s = 0)
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

    public bool IsCellFree(HexCoord coord) => cells.TryGetValue(coord, out var cell) && !cell.IsOccupied;

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
            float scaleFactor = sceneSettings != null ? sceneSettings.BuildingScaleFactor : 1f;
            building.transform.localScale *= CellSize * scaleFactor;

            Bounds bounds = CalculateBounds(building.gameObject);
            float yOffset = -bounds.min.y;
            building.transform.position = new Vector3(cell.WorldPosition.x, transform.position.y + yOffset, cell.WorldPosition.z);

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

    public void FreeCell(HexCoord coord)
    {
        if (cells.TryGetValue(coord, out var cell))
        {
            cell.Free();
            UpdateNeighborBridges(coord);
        }
    }

    private void PlaceHeadquarters()
    {
        if (headquarters != null || sceneSettings == null || sceneSettings.HeadquartersPrefab == null)
        {
            Debug.LogWarning("HexGrid: Штаб уже существует или префаб не назначен в GameSceneConfiguration!");
            return;
        }

        var center = new HexCoord(0, 0);
        if (!cells.ContainsKey(center) || !AreNeighborsFree(center))
        {
            Debug.LogWarning("HexGrid: Нельзя разместить штаб в центре!");
            return;
        }

        var hqObj = Instantiate(sceneSettings.HeadquartersPrefab, CoordToWorldPos(center), Quaternion.identity);
        if (hqObj == null) return;

        headquarters = hqObj.GetComponent<Headquarters>();
        if (headquarters != null)
        {
            float scaleFactor = sceneSettings != null ? sceneSettings.BuildingScaleFactor : 1f;
            headquarters.transform.localScale *= CellSize * scaleFactor;

            Bounds bounds = CalculateBounds(hqObj);
            float yOffset = -bounds.min.y;
            headquarters.transform.position = new Vector3(CoordToWorldPos(center).x, transform.position.y + yOffset, CoordToWorldPos(center).z);

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

    private bool AreNeighborsFree(HexCoord center)
    {
        foreach (var coord in GetNeighborCoords(center))
        {
            if (!cells.ContainsKey(coord) || !IsCellFree(coord)) return false;
        }
        return true;
    }

    public Headquarters GetHeadquarters() => headquarters;

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

    public Vector3 GetWorldPosFromCoord(HexCoord coord) => cells.TryGetValue(coord, out var cell) ? cell.WorldPosition : Vector3.zero;

    public BuildingBase GetBuildingAt(HexCoord coord) => cells.TryGetValue(coord, out var cell) ? cell.Building : null;

    private void UpdateNeighborBridges(HexCoord coord)
    {
        foreach (var nCoord in GetNeighborCoords(coord))
        {
            if (GetBuildingAt(nCoord) is Wall wall)
                wall.UpdateBridges();
        }
    }

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
            lr.material = sceneSettings.GridLineMaterial != null ? sceneSettings.GridLineMaterial : new Material(Shader.Find("Sprites/Default")); // берём обычный юнитевский, если не задали в SO
            lr.startColor = lr.endColor = Color.white;
            lr.startWidth = lr.endWidth = 0.05f;
            lr.useWorldSpace = true;
            lr.loop = true;

            // Создание шестиугольника для визуализации клетки в игре
            Vector3[] positions = new Vector3[6];
            for (int i = 0; i < 6; i++)
            {
                float angleDeg = 60f * i - 30f;
                float angleRad = Mathf.Deg2Rad * angleDeg;
                positions[i] = cell.WorldPosition + new Vector3(CellSize * Mathf.Cos(angleRad), 0f, CellSize * Mathf.Sin(angleRad));
            }
            lr.positionCount = 6;
            lr.SetPositions(positions);
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || cells.Count == 0) return;

        Gizmos.color = Color.white;
        foreach (var cell in cells.Values)
        {
            // Отрисовка шестиугольника для гизмо в редакторе
            Vector3[] corners = new Vector3[6];
            for (int i = 0; i < 6; i++)
            {
                float angleDeg = 60f * i - 30f;
                float angleRad = Mathf.Deg2Rad * angleDeg;
                corners[i] = cell.WorldPosition + new Vector3(CellSize * Mathf.Cos(angleRad), 0f, CellSize * Mathf.Sin(angleRad));
            }
            for (int i = 0; i < 6; i++)
                Gizmos.DrawLine(corners[i], corners[(i + 1) % 6]);
        }
    }
}