using UnityEngine;

public class Wall : BuildingBase
{
    [SerializeField] private GameObject bridgePrefab;
    private GameObject[] bridges = new GameObject[6]; // массив для возможный перемычек-мостов

    public override void Initialize(HexCoord coord)
    {
        base.Initialize(coord);
        // Откладываем создание мостов до полной загрузки всех зданий
    }

    public override void Upgrade()
    {
        base.Upgrade();
        UpdateBridges(); 
    }

    protected override void DestroyBuilding()
    {
        NotifyNeighbors();
        base.DestroyBuilding();
    }

    public void UpdateBridges()
    {
      
        for (int i = 0; i < bridges.Length; i++)
        {
            if (bridges[i] != null)
            {
                BridgePool.Instance.ReturnBridge(bridges[i]);
                bridges[i] = null;
            }
        }

        if (HexGrid.Instance == null) return;

        var directions = HexGrid.Instance.GetNeighborCoords(GridPosition);
        for (int i = 0; i < directions.Count; i++)
        {
            var neighborCoord = directions[i];
            if (HexGrid.Instance.GetBuildingAt(neighborCoord) is Wall)
            {
                Vector3 neighborPos = HexGrid.Instance.GetWorldPosFromCoord(neighborCoord);
                Vector3 dir = (neighborPos - Position).normalized * (HexGrid.Instance.CellSize * 0.5f);
              
                bridges[i] = BridgePool.Instance.GetBridge();
                if (bridges[i] != null)
                {
                    
                    bridges[i].transform.position = Position + dir;
                    bridges[i].transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                    
                    Vector3 euler = bridges[i].transform.eulerAngles;
                    bridges[i].transform.eulerAngles = new Vector3(0f, euler.y, euler.z);
                    bridges[i].transform.SetParent(transform);
                  /*  foreach (var collider in bridges[i].GetComponentsInChildren<Collider>())
                    {
                        collider.enabled = true;
                    }*/
                }
            }
        }
    }

    private void NotifyNeighbors()
    {
        if (HexGrid.Instance == null) return;
        var directions = HexGrid.Instance.GetNeighborCoords(GridPosition);
        foreach (var neighborCoord in directions)
        {
            if (HexGrid.Instance.GetBuildingAt(neighborCoord) is Wall wall && wall != null)
            {
                wall.UpdateBridges();
            }
        }
    }
}