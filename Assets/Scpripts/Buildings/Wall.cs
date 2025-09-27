using UnityEngine;

public class Wall : BuildingBase
{
    [SerializeField] private GameObject bridgePrefab;
    private GameObject[] bridges = new GameObject[6]; // ������������� ������ ��� 6 �����������.

    public override void Initialize(HexCoord coord)
    {
        base.Initialize(coord);
        // ����������� �������� ������ �� ������ �������� ���� ������.
    }

    public override void Upgrade()
    {
        base.Upgrade();
        UpdateBridges(); // ��������� ����� ����� ��������, ���� ��� ������ �� �� ���.
    }

    protected override void DestroyBuilding()
    {
        NotifyNeighbors();
        base.DestroyBuilding();
    }

    public void UpdateBridges()
    {
        // ���������� ������� ����� � ���.
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
                // ����� ���� �� ����.
                bridges[i] = BridgePool.Instance.GetBridge();
                if (bridges[i] != null)
                {
                    bridges[i].transform.SetPositionAndRotation(Position + dir, Quaternion.LookRotation(dir));
                    bridges[i].transform.SetParent(transform);
                    foreach (var collider in bridges[i].GetComponentsInChildren<Collider>())
                    {
                        collider.enabled = false; // ����� - ������, ��� �����������.
                    }
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