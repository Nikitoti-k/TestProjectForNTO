// Класс клетки в hex-гриде. Хранит coord, позицию в мире, флаг занятости и ссылку на здание.
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
        if (IsOccupied) return; //проверка занятости
        Building = building;
        IsOccupied = true;
    }

    public void Free()
    {
        Building = null;
        IsOccupied = false;
    }
}