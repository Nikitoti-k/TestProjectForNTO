using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class BuildingBase : MonoBehaviour, IBuildingInteractable
{
    [SerializeField] protected BuildingData data;
    [SerializeField] private Transform modelSlot;
    [SerializeField] protected GameSceneConfiguration sceneSettings;
    private GameObject currentModel;
    public int CurrentLevel => currentLevel;
    public UnityEvent OnBuildingDestroyed = new UnityEvent();

    public Vector3 Position => transform.position;
    public HexCoord GridPosition { get; protected set; }
    public virtual int CurrentHealth { get; protected set; }
    public bool IsPlaced { get; protected set; }
    protected int currentLevel;

    public virtual void Initialize(HexCoord coord)
    {
        if (data == null) throw new System.NullReferenceException("BuildingData null - assign in prefab.");
        if (sceneSettings == null) Debug.LogWarning($"{name}: GameSceneConfiguration не назначен!");

        GridPosition = coord;
        IsPlaced = true;
        ClearModelSlot();
        if (data.Levels.Count > 0) UpgradeToLevel(0);
    }

    public virtual void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth <= 0) DestroyBuilding();
    }

    protected virtual void DestroyBuilding()
    {
        OnBuildingDestroyed.Invoke();
        HexGrid.Instance.FreeCell(GridPosition);
        Destroy(gameObject);
    }

    public virtual void Upgrade()
    {
        if (data == null || currentLevel >= data.Levels.Count - 1) return;
        if (CanUpgrade())
        {
            CurrencyManager.Instance.SpendCurrency(GetUpgradeCost());
            UpgradeToLevel(currentLevel + 1);
        }
    }

    public virtual void UpgradeToLevel(int level)
    {
        currentLevel = level;
        if (data.Levels.Count > level)
        {
            CurrentHealth = data.Levels[level].MaxHealth;
            UpdateVisual(level);
        }
    }

    protected void UpdateVisual(int level)
    {
        if (currentModel != null) Destroy(currentModel);

        if (data == null || data.Levels.Count <= level || data.Levels[level].Model_view == null)
        {
            Debug.LogWarning($"{name}: No Model_view for level {level}!");
            return;
        }

        Transform parent = modelSlot ? modelSlot : transform;
        currentModel = Instantiate(data.Levels[level].Model_view, parent.position, parent.rotation, parent);
    }

    protected void ClearModelSlot()
    {
        if (modelSlot == null) return;
        for (int i = modelSlot.childCount - 1; i >= 0; i--)
        {
            Destroy(modelSlot.GetChild(i).gameObject);
        }
    }

    protected T GetModule<T>() where T : BuildingModule
    {
        return data?.Modules?.Find(m => m is T) as T;
    }

    public virtual int GetUpgradeCost()
    {
        if (data == null || currentLevel >= data.Levels.Count - 1) return 0;
        return data.Levels[currentLevel + 1].Cost;
    }

    public virtual int GetSellPrice()
    {
        if (data == null) return 0;

        float multiplier = sceneSettings != null ? sceneSettings.SellPriceMultiplier : 0.8f;
        int totalCost = 0;
        for (int i = 0; i <= currentLevel && i < data.Levels.Count; i++)
        {
            totalCost += data.Levels[i].Cost;
        }
        return Mathf.FloorToInt(totalCost * multiplier);
    }

    public virtual bool CanUpgrade()
    {
        if (data == null || currentLevel >= data.Levels.Count - 1) return false;
        return CurrencyManager.Instance.CanAfford(GetUpgradeCost());
    }

    public virtual void Sell()
    {
        if (CurrencyManager.Instance == null) return;
        CurrencyManager.Instance.AddCurrency(GetSellPrice());
        DestroyBuilding();
    }

    public virtual Vector3 GetUIPosition()
    {
        return transform.position + Vector3.up * 1.5f;
    }

    public virtual List<string> GetUpgradeParameters()
    {
        if (data == null) return new List<string>();

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
        if (data?.Modules == null) return new List<string>();

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
        if (data?.Modules == null) return new List<string>();

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
        if (data == null || data.Levels.Count == 0) return "Макс. уровень";
        int maxLevel = data.Levels.Count - 1;
        return currentLevel >= maxLevel ? "Макс. уровень" : $"Уровень: {currentLevel + 1}/{maxLevel + 1}";
    }

    public virtual string GetBuildingName()
    {
        return data != null ? data.Name : "Безымянное здание";
    }

    private void OnMouseDown()
    {
        if (!IsPlaced || BuildingUpgradeUIManager.Instance == null || (BuildingManager.Instance != null && BuildingManager.Instance.IsBuildingMode)) return;
        BuildingUpgradeUIManager.Instance.ShowUI(this);
    }
}