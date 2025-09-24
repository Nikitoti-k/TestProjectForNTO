// ѕредставл€ет клетку шестиугольной сетки, хранит координаты и состо€ние зан€тости.
using UnityEngine;

public class HexCell
{
    public HexCoord Coord { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    public bool IsOccupied { get; private set; }
    public BuildingBase Building { get; private set; }

    public HexCell(HexCoord coord, Vector3 worldPos)
    {
        Coord = coord;
        WorldPosition = worldPos;
    }

    public void Occupy(BuildingBase building)
    {
        if (IsOccupied)
        {
            return;
        }
        Building = building;
        IsOccupied = true;
    }

    public void Free()
    {
        Building = null;
        IsOccupied = false;
    }
}