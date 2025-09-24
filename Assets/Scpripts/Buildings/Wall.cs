// Управляет стеной: создаёт перемычки с соседними стенами, поддерживает улучшения.
using UnityEngine;

public class Wall : BuildingBase
{
    [SerializeField] private GameObject bridgePrefab;
    [SerializeField] private Transform modelSlot;

    private GameObject[] bridges = new GameObject[6];
    private GameObject currentModel;

    public override void Initialize(HexCoord coord)
    {
        if (HexGrid.Instance == null || bridgePrefab == null || data == null)
        {
            return;
        }

        base.Initialize(coord);
        UpgradeToLevel(0);
    }

    public override void Upgrade()
    {
        if (HexGrid.Instance == null || bridgePrefab == null || data == null)
        {
            return;
        }

        base.Upgrade();
    }

    protected override void UpgradeToLevel(int level)
    {
        base.UpgradeToLevel(level);
        UpdateVisual(level);
        UpdateBridges();
    }

    protected override void DestroyBuilding()
    {
        if (HexGrid.Instance == null || bridgePrefab == null || data == null)
        {
            return;
        }

        NotifyNeighbors();
        base.DestroyBuilding();
    }

    // Создаёт или удаляет перемычки с соседними стенами
    public void UpdateBridges()
    {
        for (int i = 0; i < bridges.Length; i++)
        {
            if (bridges[i] != null)
            {
                Destroy(bridges[i]);
                bridges[i] = null;
            }
        }

        var directions = HexGrid.Instance.GetNeighborCoords(GridPosition);
        for (int i = 0; i < directions.Count; i++)
        {
            HexCoord neighborCoord = directions[i];
            BuildingBase neighbor = HexGrid.Instance.GetBuildingAt(neighborCoord);
            if (neighbor is Wall)
            {
                Vector3 neighborPos = HexGrid.Instance.GetWorldPosFromCoord(neighborCoord);
                Vector3 direction = (neighborPos - Position).normalized * (HexGrid.Instance.CellSize * 0.5f);
                bridges[i] = Instantiate(bridgePrefab, Position + direction, Quaternion.LookRotation(direction), transform);
                foreach (var collider in bridges[i].GetComponentsInChildren<Collider>())
                {
                    collider.enabled = false;
                }
            }
        }
    }

    // Уведомляет соседние стены для обновления их перемычек
    private void NotifyNeighbors()
    {
        var directions = HexGrid.Instance.GetNeighborCoords(GridPosition);
        for (int i = 0; i < directions.Count; i++)
        {
            HexCoord neighborCoord = directions[i];
            BuildingBase neighbor = HexGrid.Instance.GetBuildingAt(neighborCoord);
            if (neighbor is Wall wall)
            {
                wall.UpdateBridges();
            }
        }
    }

    // Обновляет модель стены для текущего уровня
    private void UpdateVisual(int level)
    {
        if (currentModel != null)
        {
            Destroy(currentModel);
        }

        if (data == null || data.Levels.Count <= level)
        {
            return;
        }

        var levelData = data.Levels[level];
        if (levelData.ModelPrefab == null)
        {
            return;
        }

        Transform parent = modelSlot != null ? modelSlot : transform;
        currentModel = Instantiate(levelData.ModelPrefab, parent.position, parent.rotation, parent);
    }
}