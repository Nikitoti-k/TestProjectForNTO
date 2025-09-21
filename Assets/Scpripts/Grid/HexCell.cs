using UnityEngine;
// Одна клетка в нашей сетке, храним координаты (гексагональные и мировые), ссылаемся на здание которое стоит на этой клетке
public class HexCell
{
    public HexCoord Coord { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    public bool IsOccupied { get; private set; } = false; public BuildingBase Building { get; private set; }

    public HexCell(HexCoord coord, Vector3 worldPos)
    {
        Coord = coord;
        WorldPosition = worldPos;
    }

    public void Occupy(BuildingBase building)
    {
        if (IsOccupied) return;
        Building = building;
        IsOccupied = true;
    }

    public void Free()
    {
        Building = null;
        IsOccupied = false;
    }

}