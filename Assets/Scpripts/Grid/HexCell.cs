// Класс клетки в hex-гриде. Хранит coord, позицию в мире, флаг занятости и ссылку на здание.
using UnityEngine;

public class HexCell
{
    public HexCoord Coord { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    public bool IsOccupied { get; private set; }
    public BuildingBase Building { get; private set; } // BuildingBase - базовый класс для построек, assume defined elsewhere.

    public HexCell(HexCoord coord, Vector3 worldPos)
    {
        Coord = coord;
        WorldPosition = worldPos;
    }

    public void Occupy(BuildingBase building)
    {
        if (IsOccupied) return; // Не overwrite, если уже занято - avoid bugs с дубликатами.
        Building = building;
        IsOccupied = true;
    }

    public void Free()
    {
        Building = null;
        IsOccupied = false;
    }
}