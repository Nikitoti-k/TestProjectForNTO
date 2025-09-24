// Управляет штабом: занимает гексагональную сетку, не поддерживает улучшения.
using System.Collections.Generic;
using UnityEngine;

public class Headquarters : BuildingBase
{
    [SerializeField] private int maxHealth = 1000;
    [SerializeField] private int baseCost = 1000;
    [SerializeField] private float sellPriceMultiplier = 0.8f;
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
    }

    protected override void DestroyBuilding()
    {
        base.DestroyBuilding();
    }

    // Возвращает соседние гексы для занятия штабом
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
            neighbors.Add(new HexCoord(center.q + dir.q, center.r + dir.r));
        }
        return neighbors;
    }

    public override void Upgrade()
    {
    }

    protected override void UpgradeToLevel(int level)
    {
        currentLevel = level;
        CurrentHealth = maxHealth;
    }

    public override int GetUpgradeCost()
    {
        return 0;
    }

    public override int GetSellPrice()
    {
        return Mathf.FloorToInt(baseCost * sellPriceMultiplier);
    }

    public override bool CanUpgrade()
    {
        return false;
    }

    public override void Sell()
    {
        int sellPrice = GetSellPrice();
        CurrencyManager.Instance.AddCurrency(sellPrice);
        DestroyBuilding();
    }

    public override List<string> GetUpgradeParameters()
    {
        return new List<string>();
    }

    public override string GetLevelDisplay()
    {
        return "Макс. уровень";
    }

    public override string GetBuildingName()
    {
        return "Штаб";
    }
}