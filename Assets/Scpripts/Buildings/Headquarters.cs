using System.Collections.Generic;
using UnityEngine;

public class Headquarters : BuildingBase
{
    [SerializeField] private int maxHealth = 1000; // Здоровье в инспекторе
    private List<HexCoord> occupiedCoords = new List<HexCoord>();

    public override int CurrentHealth { get; protected set; }
    public List<HexCoord> OccupiedCoords => occupiedCoords;

    public override void Initialize(HexCoord centerCoord)
    {
        base.Initialize(centerCoord);
        CurrentHealth = maxHealth;
        occupiedCoords.Add(centerCoord);
        occupiedCoords.AddRange(GetNeighborCoords(centerCoord));
    }

    public override void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth <= 0)
        {
            DestroyBuilding();
        }
        Debug.Log($"Headquarters health: {CurrentHealth}");
    }

    protected override void DestroyBuilding()
    {
        base.DestroyBuilding();
        Debug.Log("Headquarters destroyed! Game Over!");
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

    // Переопределяем Upgrade, так как Headquarters не использует BuildingData
    public override void Upgrade()
    {
        // Можно оставить пустым или добавить логику, если апгрейды нужны
        Debug.Log("Headquarters upgrade not implemented.");
    }

    protected override void UpgradeToLevel(int level)
    {
        currentLevel = level;
        CurrentHealth = maxHealth; // Здоровье не меняется, или можно добавить логику
    }
}