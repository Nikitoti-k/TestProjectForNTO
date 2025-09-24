// Генерирует и отображает шестиугольную сетку, управляет размещением построек.
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HexGrid : MonoBehaviour, IGrid
{
    public static HexGrid Instance { get; private set; }

    [SerializeField] private int gridRadius = 5;
    [SerializeField] private float cellSize = 2f;
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private bool showGridInGame = true;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject headquartersPrefab;

    private Dictionary<HexCoord, HexCell> cells = new Dictionary<HexCoord, HexCell>();
    private GameObject linesParent;
    private List<GameObject> placedBuildings = new List<GameObject>();
    private Headquarters headquarters;
    public float CellSize => cellSize;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [ContextMenu("GenerateHexGrid")]
    public void RegenerateGrid()
    {
        ClearOldLines();
        GenerateGrid();
        SetupLineRenderers();
    }

    private void Start()
    {
        if (cells.Count == 0)
        {
            RegenerateGrid();
        }

        if (headquarters == null && headquartersPrefab != null && Application.isPlaying)
        {
            PlaceHeadquarters();
        }
    }

    private void OnDestroy()
    {
        foreach (var building in placedBuildings)
        {
            if (building != null)
            {
                if (Application.isPlaying)
                    Destroy(building);
                else
                    DestroyImmediate(building);
            }
        }
        placedBuildings.Clear();
        headquarters = null;
    }

    // Удаляет старые линии сетки
    private void ClearOldLines()
    {
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
        linesParent = null;
    }

    // Генерирует клетки сетки по радиусу
    private void GenerateGrid()
    {
        cells.Clear();
        for (int q = -gridRadius; q <= gridRadius; q++)
        {
            int r1 = Mathf.Max(-gridRadius, -q - gridRadius);
            int r2 = Mathf.Min(gridRadius, -q + gridRadius);
            for (int r = r1; r <= r2; r++)
            {
                HexCoord coord = new HexCoord(q, r);
                cells.Add(coord, new HexCell(coord, CoordToWorldPos(coord)));
            }
        }
    }

    // Преобразует hex-координаты в мировые
    private Vector3 CoordToWorldPos(HexCoord coord)
    {
        float xOffset = cellSize * Mathf.Sqrt(3f) * (coord.q + coord.r * 0.5f);
        float zOffset = cellSize * 1.5f * coord.r;
        return new Vector3(xOffset, 0f, zOffset) + transform.position;
    }

    public HexCell GetCellFromWorldPos(Vector3 worldPos)
    {
        if (playerCamera == null)
        {
            return null;
        }

        Vector3 localPos = worldPos - transform.position;
        float q = (Mathf.Sqrt(3f) / 3f * localPos.x - 1f / 3f * localPos.z) / cellSize;
        float r = (2f / 3f * localPos.z) / cellSize;
        HexCoord coord = RoundHexCoord(q, r);
        return GetCell(coord);
    }

    private HexCell GetCell(HexCoord coord)
    {
        return cells.TryGetValue(coord, out HexCell cell) ? cell : null;
    }

    // Округляет дробные hex-координаты до целых
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

    public bool IsCellFree(HexCoord coord)
    {
        return cells.TryGetValue(coord, out HexCell cell) && !cell.IsOccupied;
    }

    public void PlaceBuilding(HexCoord coord, BuildingBase building)
    {
        if (!cells.TryGetValue(coord, out HexCell cell) || cell.IsOccupied)
        {
            return;
        }

        cell.Occupy(building);
        if (building != null)
        {
            building.transform.position = cell.WorldPosition;
            building.transform.localScale = Vector3.one * cellSize * 0.8f;
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

    public void FreeCell(HexCoord coord)
    {
        if (cells.TryGetValue(coord, out HexCell cell))
        {
            cell.Free();
            UpdateNeighborBridges(coord);
        }
    }

    // Размещает штаб в центре сетки
    private void PlaceHeadquarters()
    {
        if (headquarters != null || headquartersPrefab == null)
        {
            return;
        }

        HexCoord centerCoord = new HexCoord(0, 0);
        if (!cells.ContainsKey(centerCoord))
        {
            return;
        }

        var neighborCoords = GetNeighborCoords(centerCoord);
        foreach (var coord in neighborCoords)
        {
            if (!cells.ContainsKey(coord) || !IsCellFree(coord))
            {
                return;
            }
        }

        GameObject hqObj = Instantiate(headquartersPrefab, CoordToWorldPos(centerCoord), Quaternion.identity);
        headquarters = hqObj.GetComponent<Headquarters>();
        if (headquarters != null)
        {
            headquarters.Initialize(centerCoord);
            foreach (var coord in headquarters.OccupiedCoords)
            {
                if (cells.TryGetValue(coord, out HexCell cell))
                {
                    cell.Occupy(headquarters);
                }
            }
            placedBuildings.Add(hqObj);
        }
    }

    public Headquarters GetHeadquarters()
    {
        return headquarters;
    }

    // Возвращает соседние координаты для hex-клетки
    public List<HexCoord> GetNeighborCoords(HexCoord center)
    {
        var directions = new HexCoord[]
        {
            new HexCoord(1, 0), new HexCoord(1, -1), new HexCoord(0, -1),
            new HexCoord(-1, 0), new HexCoord(-1, 1), new HexCoord(0, 1)
        };
        var neighbors = new List<HexCoord>();
        foreach (var dir in directions)
        {
            neighbors.Add(center + dir);
        }
        return neighbors;
    }

    public Vector3 GetWorldPosFromCoord(HexCoord coord)
    {
        return cells.TryGetValue(coord, out HexCell cell) ? cell.WorldPosition : Vector3.zero;
    }

    public BuildingBase GetBuildingAt(HexCoord coord)
    {
        return cells.TryGetValue(coord, out HexCell cell) ? cell.Building : null;
    }

    // Обновляет перемычки соседних стен
    private void UpdateNeighborBridges(HexCoord coord)
    {
        var directions = GetNeighborCoords(coord);
        foreach (var neighborCoord in directions)
        {
            BuildingBase neighbor = GetBuildingAt(neighborCoord);
            if (neighbor is Wall wall)
            {
                wall.UpdateBridges();
            }
        }
    }

    // Рисует сетку с помощью LineRenderer
    private void SetupLineRenderers()
    {
        if (!showGridInGame && Application.isPlaying)
        {
            return;
        }

        linesParent = new GameObject("GridLines");
        linesParent.transform.SetParent(transform);
        linesParent.transform.localPosition = Vector3.zero;

        foreach (var cell in cells.Values)
        {
            GameObject lineObj = new GameObject("HexLine_" + cell.Coord.q + "_" + cell.Coord.r);
            lineObj.transform.SetParent(linesParent.transform);
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
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

    private void OnDrawGizmos()
    {
        if (!drawGizmos || cells.Count == 0)
        {
            return;
        }

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
            {
                Gizmos.DrawLine(corners[i], corners[(i + 1) % 6]);
            }
        }
    }
}