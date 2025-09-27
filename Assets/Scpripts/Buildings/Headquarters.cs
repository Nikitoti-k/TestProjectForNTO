using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Headquarters : BuildingBase
{
    private List<HexCoord> occupiedCoords = new List<HexCoord>();
    public List<HexCoord> OccupiedCoords => occupiedCoords;
    public UnityEvent OnDefeat = new UnityEvent();

    public override void Initialize(HexCoord centerCoord)
    {
        base.Initialize(centerCoord);
        occupiedCoords.Clear();
        occupiedCoords.Add(centerCoord);
        occupiedCoords.AddRange(HexGrid.Instance.GetNeighborCoords(centerCoord));
        foreach (var coord in occupiedCoords)
        {
            if (HexGrid.Instance.Cells.TryGetValue(coord, out var cell))
            {
                cell.Occupy(this);
            }
        }
        Debug.Log($"{name}: Initialized at {centerCoord}, HP: {CurrentHealth}, Occupied: {occupiedCoords.Count} cells");
    }

    public override void TakeDamage(int amount)
    {
        Debug.Log($"{name}: Taking {amount} damage, HP left: {CurrentHealth - amount}");
        CurrentHealth -= amount;
        if (CurrentHealth <= 0)
        {
            Debug.Log($"{name}: HP <= 0, destroying HQ");
            DestroyBuilding();
            OnDefeat.Invoke();
        }
    }

    protected override void DestroyBuilding()
    {
        if (HexGrid.Instance != null)
        {
            foreach (var coord in occupiedCoords)
            {
                HexGrid.Instance.FreeCell(coord);
            }
            Debug.Log($"{name}: Freed {occupiedCoords.Count} cells starting from {GridPosition}");
        }
        else
        {
            Debug.LogError($"{name}: No HexGrid!");
        }
        Debug.Log($"{name}: Destroying object");
        Destroy(gameObject);
    }

    public override void Upgrade()
    {
        // No upgrade for HQ.
    }

    public override void UpgradeToLevel(int level) // Изменено: public для сохранения.
    {
        currentLevel = level;
        if (data != null && data.Levels.Count > level)
        {
            CurrentHealth = data.Levels[level].MaxHealth;
            UpdateVisual(level);
            Debug.Log($"{name}: Set level {level}, HP: {CurrentHealth}");
        }
        else
        {
            Debug.LogWarning($"{name}: Invalid level {level} or missing data!");
            CurrentHealth = data?.Levels[0].MaxHealth ?? 1;
            Debug.Log($"{name}: Fallback HP: {CurrentHealth}");
        }
    }

    public override int GetUpgradeCost() => 0;

    public override int GetSellPrice() => 0;

    public override bool CanUpgrade() => false;

    public override void Sell()
    {
        // No sell for HQ.
    }

    public override List<string> GetUpgradeParameters() => new List<string>();

    public override string GetLevelDisplay() => "Макс. уровень";

    public override string GetBuildingName() => "Штаб";

    private void OnMouseDown()
    {
        // Block UI for HQ.
    }
}