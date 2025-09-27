// ��������� ��� grid management - ����������, ���� mock ��� swap impl ���� ����� (SOLID: interface segregation).
using UnityEngine;

public interface IGrid
{
    HexCell GetCellFromWorldPos(Vector3 worldPos);
    bool IsCellFree(HexCoord coord);
    void PlaceBuilding(HexCoord coord, BuildingBase building);
    Vector3 GetWorldPosFromCoord(HexCoord coord);
    Headquarters GetHeadquarters();
    BuildingBase GetBuildingAt(HexCoord coord);
    void FreeCell(HexCoord coord);
}