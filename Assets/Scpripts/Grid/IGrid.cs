// Интерфейс для grid management - абстракция, чтоб mock или swap impl если нужно (SOLID: interface segregation).
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