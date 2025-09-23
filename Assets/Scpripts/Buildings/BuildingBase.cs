using UnityEngine;

public abstract class BuildingBase : MonoBehaviour
{
    public Vector3 Position => transform.position;
    public HexCoord GridPosition { get; protected set; }
    public virtual int CurrentHealth { get; protected set; } // Виртуальное свойство
    public bool IsPlaced { get; protected set; }

    public virtual void Initialize(HexCoord coord)
    {
        GridPosition = coord;
        IsPlaced = true;
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
}