using UnityEngine;

public abstract class BuildingBase : MonoBehaviour
{
    public Vector3 Position => transform.position;
    public HexCoord GridPosition { get; private set; }
    public int CurrentHealth { get; protected set; }

    [SerializeField] protected int maxHealth = 100;

    public virtual void Initialize(HexCoord coord)
    {
        GridPosition = coord;
        CurrentHealth = maxHealth;
    }

    public virtual void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
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