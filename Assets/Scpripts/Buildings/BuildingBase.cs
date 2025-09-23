using UnityEngine;

public abstract class BuildingBase : MonoBehaviour
{
    [SerializeField] protected BuildingData data; // Может быть null для Headquarters
    public Vector3 Position => transform.position;
    public HexCoord GridPosition { get; protected set; }
    public virtual int CurrentHealth { get; protected set; }
    public bool IsPlaced { get; protected set; }
    protected int currentLevel = 0;

    public virtual void Initialize(HexCoord coord)
    {
        GridPosition = coord;
        IsPlaced = true;
        if (data != null && data.Levels.Count > 0)
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
        Destroy(gameObject);
    }

    public virtual void Upgrade()
    {
        if (data != null && currentLevel < data.Levels.Count - 1)
        {
            UpgradeToLevel(currentLevel + 1);
        }
    }

    protected virtual void UpgradeToLevel(int level)
    {
        currentLevel = level;
        if (data != null && data.Levels.Count > level)
        {
            CurrentHealth = data.Levels[level].MaxHealth;
        }
    }

    protected T GetModule<T>() where T : BuildingModule
    {
        return data?.Modules.Find(m => m is T) as T;
    }
}