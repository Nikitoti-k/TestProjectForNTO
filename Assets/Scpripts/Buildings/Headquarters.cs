using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Headquarters : BuildingBase
{
    private List<HexCoord> occupiedCoords = new List<HexCoord>();
    public List<HexCoord> OccupiedCoords => occupiedCoords;
    public UnityEvent OnDefeat = new UnityEvent(); // Событие поражения

    public override void Initialize(HexCoord centerCoord)
    {
        base.Initialize(centerCoord);
        UpgradeToLevel(0); // Инициализируем уровень и здоровье
        occupiedCoords.Add(centerCoord);
        occupiedCoords.AddRange(GetNeighborCoords(centerCoord));
        Debug.Log($"{name}: Инициализирован на координатах {centerCoord}, здоровье: {CurrentHealth}");
    }

    public override void TakeDamage(int amount)
    {
        Debug.Log($"{name}: Получен урон {amount}, текущее здоровье: {CurrentHealth}");
        CurrentHealth -= amount;
        if (CurrentHealth <= 0)
        {
            Debug.Log($"{name}: Здоровье <= 0, уничтожаем штаб");
            DestroyBuilding();
            OnDefeat.Invoke(); // Вызываем событие поражения
        }
    }

    protected override void DestroyBuilding()
    {
        if (HexGrid.Instance == null)
        {
            Debug.LogError($"{name}: HexGrid.Instance не найден!");
        }
        else
        {
            Debug.Log($"{name}: Освобождаем клетку {GridPosition}");
            HexGrid.Instance.FreeCell(GridPosition);
        }
        Debug.Log($"{name}: Уничтожаем объект");
        Destroy(gameObject);
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
        // Штаб не поддерживает улучшения
    }

    protected override void UpgradeToLevel(int level)
    {
        currentLevel = level;
        if (data != null && data.Levels.Count > level)
        {
            CurrentHealth = data.Levels[level].MaxHealth;
            Debug.Log($"{name}: Инициализировано здоровье: {CurrentHealth}");
        }
        else
        {
            Debug.LogWarning($"{name}: Некорректный уровень или данные для штаба!");
            CurrentHealth = data != null && data.Levels.Count > 0 ? data.Levels[0].MaxHealth : 1;
            Debug.Log($"{name}: Установлено запасное здоровье: {CurrentHealth}");
        }
    }

    public override int GetUpgradeCost()
    {
        return 0; // Штаб не улучшается
    }

    public override int GetSellPrice()
    {
        return 0; // Штаб нельзя продать
    }

    public override bool CanUpgrade()
    {
        return false; // Штаб не улучшается
    }

    public override void Sell()
    {
        // Штаб нельзя продать
    }

    public override List<string> GetUpgradeParameters()
    {
        return new List<string>(); // Штаб не имеет параметров улучшения
    }

    public override string GetLevelDisplay()
    {
        return "Макс. уровень"; // Штаб не имеет уровней
    }

    public override string GetBuildingName()
    {
        return "Штаб";
    }

    // Блокирует открытие меню улучшений для штаба
    private void OnMouseDown()
    {
        // Пустой метод, чтобы предотвратить вызов UI улучшений
    }
}