using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Headquarters : BuildingBase
{
    private List<HexCoord> occupiedCoords = new List<HexCoord>();
    public List<HexCoord> OccupiedCoords => occupiedCoords;
    public UnityEvent OnDefeat = new UnityEvent(); // ������� ���������

    public override void Initialize(HexCoord centerCoord)
    {
        base.Initialize(centerCoord);
        UpgradeToLevel(0); // �������������� ������� � ��������
        occupiedCoords.Add(centerCoord);
        occupiedCoords.AddRange(GetNeighborCoords(centerCoord));
        Debug.Log($"{name}: ��������������� �� ����������� {centerCoord}, ��������: {CurrentHealth}");
    }

    public override void TakeDamage(int amount)
    {
        Debug.Log($"{name}: ������� ���� {amount}, ������� ��������: {CurrentHealth}");
        CurrentHealth -= amount;
        if (CurrentHealth <= 0)
        {
            Debug.Log($"{name}: �������� <= 0, ���������� ����");
            DestroyBuilding();
            OnDefeat.Invoke(); // �������� ������� ���������
        }
    }

    protected override void DestroyBuilding()
    {
        if (HexGrid.Instance == null)
        {
            Debug.LogError($"{name}: HexGrid.Instance �� ������!");
        }
        else
        {
            Debug.Log($"{name}: ����������� ������ {GridPosition}");
            HexGrid.Instance.FreeCell(GridPosition);
        }
        Debug.Log($"{name}: ���������� ������");
        Destroy(gameObject);
    }

    // ���������� �������� ����� ��� ������� ������
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
        // ���� �� ������������ ���������
    }

    protected override void UpgradeToLevel(int level)
    {
        currentLevel = level;
        if (data != null && data.Levels.Count > level)
        {
            CurrentHealth = data.Levels[level].MaxHealth;
            Debug.Log($"{name}: ���������������� ��������: {CurrentHealth}");
        }
        else
        {
            Debug.LogWarning($"{name}: ������������ ������� ��� ������ ��� �����!");
            CurrentHealth = data != null && data.Levels.Count > 0 ? data.Levels[0].MaxHealth : 1;
            Debug.Log($"{name}: ����������� �������� ��������: {CurrentHealth}");
        }
    }

    public override int GetUpgradeCost()
    {
        return 0; // ���� �� ����������
    }

    public override int GetSellPrice()
    {
        return 0; // ���� ������ �������
    }

    public override bool CanUpgrade()
    {
        return false; // ���� �� ����������
    }

    public override void Sell()
    {
        // ���� ������ �������
    }

    public override List<string> GetUpgradeParameters()
    {
        return new List<string>(); // ���� �� ����� ���������� ���������
    }

    public override string GetLevelDisplay()
    {
        return "����. �������"; // ���� �� ����� �������
    }

    public override string GetBuildingName()
    {
        return "����";
    }

    // ��������� �������� ���� ��������� ��� �����
    private void OnMouseDown()
    {
        // ������ �����, ����� ������������� ����� UI ���������
    }
}