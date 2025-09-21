using System.Collections.Generic;
using UnityEngine;

public interface IGrid
{
    HexCell GetCellFromWorldPos(Vector3 worldPos);
    bool IsCellFree(HexCoord coord);
    void PlaceBuilding(HexCoord coord, BuildingBase building);
    Vector3 GetWorldPosFromCoord(HexCoord coord);
}

[ExecuteInEditMode]
public class HexGrid : MonoBehaviour, IGrid
{
    [SerializeField] private int gridRadius = 5;
    [SerializeField] private float cellSize = 2f;
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private bool showGridInGame = true;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject previewPrefab; 

    private Dictionary<HexCoord, HexCell> cells = new Dictionary<HexCoord, HexCell>();
    private GameObject linesParent;
    private GameObject currentPreview;
    private Material previewMaterial; 
    private List<GameObject> placedBuildings = new List<GameObject>(); 

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

        // Создай preview и копию материала
        if (previewPrefab != null && currentPreview == null)
        {
            currentPreview = Instantiate(previewPrefab, Vector3.zero, Quaternion.identity);
            currentPreview.SetActive(false); 
            Renderer renderer = currentPreview.GetComponent<Renderer>();
            if (renderer != null)
            {
                previewMaterial = new Material(renderer.sharedMaterial);
                renderer.material = previewMaterial;
                Color color = previewMaterial.color;
                color.a = 0.5f;
                previewMaterial.color = color;
            }
        }
    }

    private void OnDestroy()
    {
        // Очищаем материал
        if (previewMaterial != null)
        {
            DestroyImmediate(previewMaterial);
        }

        // Очищаем все здания
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
    }

    private void ClearOldLines()
    {
        if (Application.isPlaying)
        {
            int childCount = transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            Debug.Log($"Cleared {childCount} old child objects in runtime");
        }
        else
        {
            int childCount = transform.childCount;
            if (childCount > 0)
            {
                for (int i = childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }
                Debug.Log($"Cleared {childCount} old child objects in editor");
            }
        }
        linesParent = null;
    }

    private void GenerateGrid()
    {
        cells.Clear();
        Debug.Log($"Generating grid with radius {gridRadius}, cellSize {cellSize}");
        for (int q = -gridRadius; q <= gridRadius; q++)
        {
            int r1 = Mathf.Max(-gridRadius, -q - gridRadius);
            int r2 = Mathf.Min(gridRadius, -q + gridRadius);
            for (int r = r1; r <= r2; r++)
            {
                HexCoord coord = new HexCoord(q, r);
                Vector3 worldPos = CoordToWorldPos(coord);
                cells.Add(coord, new HexCell(coord, worldPos));
                Debug.Log($"Cell at q={q}, r={r}, pos={worldPos}");
            }
        }
        Debug.Log($"Generated {cells.Count} cells");
    }

    private Vector3 CoordToWorldPos(HexCoord coord)
    {
        float xOffset = cellSize * Mathf.Sqrt(3f) * (coord.q + coord.r * 0.5f);
        float zOffset = cellSize * 1.5f * coord.r;
        Vector3 pos = new Vector3(xOffset, 0f, zOffset) + transform.position;
        return pos;
    }

    public HexCell GetCellFromWorldPos(Vector3 worldPos)
    {
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
        else sInt = -qInt - rInt;

        return new HexCoord(qInt, rInt);
    }

    public bool IsCellFree(HexCoord coord)
    {
        return cells.TryGetValue(coord, out HexCell cell) && !cell.IsOccupied;
    }

    public void PlaceBuilding(HexCoord coord, BuildingBase building)
    {
        if (cells.TryGetValue(coord, out HexCell cell))
        {
            cell.Occupy(building);
            if (building != null)
            {
                building.transform.position = cell.WorldPosition;
                building.transform.localScale = Vector3.one * cellSize * 0.8f;
                placedBuildings.Add(building.gameObject);
            }
        }
    }

    public Vector3 GetWorldPosFromCoord(HexCoord coord)
    {
        return cells.TryGetValue(coord, out HexCell cell) ? cell.WorldPosition : Vector3.zero;
    }

    private void SetupLineRenderers()
    {
        if (!showGridInGame && Application.isPlaying) return;

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
            {
                Gizmos.DrawLine(corners[i], corners[(i + 1) % 6]);
            }
        }
    }

    private void Update()
    {
        if (playerCamera == null) playerCamera = Camera.main;

       
        HandlePreview();

       
        if (Input.GetMouseButtonDown(0) && Application.isPlaying) 
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, transform.position.y);
            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                HexCell cell = GetCellFromWorldPos(hitPoint);
                if (cell != null && IsCellFree(cell.Coord))
                {
                    GameObject buildingObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    buildingObj.AddComponent<PlaceholderBuilding>();
                    PlaceBuilding(cell.Coord, buildingObj.GetComponent<BuildingBase>());
                    if (currentPreview != null) currentPreview.SetActive(false);
                    Debug.Log($"Placed at Coord: {cell.Coord.q},{cell.Coord.r} | Pos: {cell.WorldPosition}");
                }
            }
        }
    }

    private void HandlePreview()
    {
        if (previewPrefab == null || currentPreview == null) return;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position.y);
        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            HexCell cell = GetCellFromWorldPos(hitPoint);
            if (cell != null && IsCellFree(cell.Coord))
            {
                currentPreview.transform.position = cell.WorldPosition;
                currentPreview.transform.localScale = Vector3.one * cellSize * 0.8f;
                currentPreview.SetActive(true);
                return;
            }
        }
        currentPreview.SetActive(false);
    }
}