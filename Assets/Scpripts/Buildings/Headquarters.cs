// ���������� ������ - ����, ���� ��� ������ + ��������� ��� ���������� (��������� � ������ � ��, ��� ����� ������������ ��� ��� ���� ��� ������)
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
        
    }

    public override void TakeDamage(int amount)
    {
        Debug.Log($"{name}: ������� {amount} �����, �� ��������: {CurrentHealth - amount}");
        CurrentHealth -= amount;
        if (CurrentHealth <= 0)
        {           
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
            Debug.LogError($"{name}: ��� HexGrid!");
        }
        Destroy(gameObject);
    }

    public override void Upgrade()
    {
        // �� ����������
    }

    public override void UpgradeToLevel(int level) 
    {
        currentLevel = level;
        if (data != null && data.Levels.Count > level)
        {
            CurrentHealth = data.Levels[level].MaxHealth;
            UpdateVisual(level);
          
        }
        else
        {
            CurrentHealth = data?.Levels[0].MaxHealth ?? 1;
           
        }
    }

    public override int GetUpgradeCost() => 0;

    public override int GetSellPrice() => 0;

    public override bool CanUpgrade() => false;

    public override void Sell()
    {
        // ���������� ������� ���� ��� ������)
    }

    public override List<string> GetUpgradeParameters() => new List<string>();

    public override string GetLevelDisplay() => "����. �������";

    public override string GetBuildingName() => "����";

    private void OnMouseDown()
    {
        // �� ������� UI
    }
}