// Базовый класс для всех построек, управляет здоровьем, улучшениями и продажей.
using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingBase : MonoBehaviour, IBuildingInteractable
{
    [SerializeField] protected BuildingData data;
    public Vector3 Position => transform.position;
    public HexCoord GridPosition { get; protected set; }
    public virtual int CurrentHealth { get; protected set; }
    public bool IsPlaced { get; protected set; }
    protected int currentLevel = 0;

    [SerializeField] private float sellPriceMultiplier = 0.8f;

    public virtual void Initialize(HexCoord coord)
    {
        if (data == null)
        {
            throw new System.NullReferenceException("Не задан: data");
        }

        GridPosition = coord;
        IsPlaced = true;
        if (data.Levels.Count > 0)
        {
            UpgradeToLevel(0);
        }
    }

    public virtual void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth <= 0)
        {
            DestroyBuilding();
        }
    }

    protected virtual void DestroyBuilding()
    {
        HexGrid.Instance.FreeCell(GridPosition);
        Destroy(gameObject);
    }

    public virtual void Upgrade()
    {
        if (data == null || currentLevel >= data.Levels.Count - 1)
        {
            return;
        }
        UpgradeToLevel(currentLevel + 1);
    }

    protected virtual void UpgradeToLevel(int level)
    {
        currentLevel = level;
        if (data.Levels.Count > level)
        {
            CurrentHealth = data.Levels[level].MaxHealth;
        }
    }

    protected T GetModule<T>() where T : BuildingModule
    {
        if (data == null || data.Modules == null)
        {
            return null;
        }
        return data.Modules.Find(m => m is T) as T;
    }

    public virtual int GetUpgradeCost()
    {
        if (data == null || currentLevel >= data.Levels.Count - 1)
        {
            return 0;
        }
        return data.Levels[currentLevel + 1].Cost;
    }

    public virtual int GetSellPrice()
    {
        if (data == null)
        {
            return 0;
        }

        int totalCost = 0;
        for (int i = 0; i <= currentLevel && i < data.Levels.Count; i++)
        {
            totalCost += data.Levels[i].Cost;
        }
        return Mathf.FloorToInt(totalCost * sellPriceMultiplier);
    }

    public virtual bool CanUpgrade()
    {
        if (data == null || currentLevel >= data.Levels.Count - 1)
        {
            return false;
        }
        return CurrencyManager.Instance.CanAfford(GetUpgradeCost());
    }

    public virtual void Sell()
    {
        if (CurrencyManager.Instance == null)
        {
            return;
        }
        int sellPrice = GetSellPrice();
        CurrencyManager.Instance.AddCurrency(sellPrice);
        DestroyBuilding();
    }

    public virtual Vector3 GetUIPosition()
    {
        return transform.position + Vector3.up * 2f;
    }

    public virtual List<string> GetUpgradeParameters()
    {
        if (data == null)
        {
            return new List<string>();
        }

        List<string> parameters = new List<string>();
        bool isMaxLevel = currentLevel >= data.Levels.Count - 1;
        if (!isMaxLevel)
        {
            int currentHealth = data.Levels[currentLevel].MaxHealth;
            int nextHealth = data.Levels[currentLevel + 1].MaxHealth;
            parameters.Add($"{data.MaxHPDisplayName}: {currentHealth} -> {nextHealth}");
            parameters.AddRange(FormatModuleParameters());
        }
        else
        {
            int currentHealth = data.Levels[currentLevel].MaxHealth;
            parameters.Add($"{data.MaxHPDisplayName}: {currentHealth}");
            parameters.AddRange(FormatModuleCurrentParameters());
        }
        return parameters;
    }

    protected virtual List<string> FormatModuleParameters()
    {
        if (data == null || data.Modules == null)
        {
            return new List<string>();
        }

        List<string> parameters = new List<string>();
        foreach (var module in data.Modules)
        {
            if (module is IUpgradeParameterProvider provider)
            {
                parameters.AddRange(provider.GetUpgradeParameters(currentLevel));
            }
        }
        return parameters;
    }

    protected virtual List<string> FormatModuleCurrentParameters()
    {
        if (data == null || data.Modules == null)
        {
            return new List<string>();
        }

        List<string> parameters = new List<string>();
        foreach (var module in data.Modules)
        {
            if (module is IUpgradeParameterProvider provider)
            {
                parameters.AddRange(provider.GetCurrentParameters(currentLevel));
            }
        }
        return parameters;
    }

    public virtual string GetLevelDisplay()
    {
        if (data == null || data.Levels.Count == 0)
        {
            return "Макс. уровень";
        }
        int maxLevel = data.Levels.Count - 1;
        return currentLevel >= maxLevel ? "Макс. уровень" : $"Уровень: {currentLevel + 1}/{maxLevel + 1}";
    }

    public virtual string GetBuildingName()
    {
        return data != null ? data.Name : "Безымянное здание";
    }

    private void OnMouseDown()
    {
        if (!IsPlaced || BuildingUpgradeUIManager.Instance == null || BuildingManager.Instance != null && BuildingManager.Instance.IsBuildingMode)
        {
            return;
        }
        BuildingUpgradeUIManager.Instance.ShowUI(this);
    }
}