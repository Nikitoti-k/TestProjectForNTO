using System.Collections.Generic;
using UnityEngine;

public class Headquarters : BuildingBase
{
    private List<HexCoord> occupiedCoords = new List<HexCoord>(); 

    public List<HexCoord> OccupiedCoords => occupiedCoords;

    public override void Initialize(HexCoord centerCoord)
    {
        base.Initialize(centerCoord);
        // Занимаем центр и 6 соседних клеток
        occupiedCoords.Add(centerCoord);
        var neighbors = GetNeighborCoords(centerCoord);
        occupiedCoords.AddRange(neighbors);
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
       
    }

    protected override void DestroyBuilding()
    {
        base.DestroyBuilding();
       
       
    }

    private List<HexCoord> GetNeighborCoords(HexCoord center)
    {
        
        var directions = new HexCoord[]
        {
            new HexCoord(1, 0), new HexCoord(1, -1), new HexCoord(0, -1),
            new HexCoord(-1, 0), new HexCoord(-1, 1), new HexCoord(0, 1)
        };
        var neighbors = new List<HexCoord>();
        foreach (var dir in directions)
        {
            var neighbor = new HexCoord(center.q + dir.q, center.r + dir.r);
            neighbors.Add(neighbor);
        }
        return neighbors;
    }
}