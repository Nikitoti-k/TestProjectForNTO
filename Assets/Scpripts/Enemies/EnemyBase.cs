using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] protected EnemyData data;

    protected int currentHealth;
    protected float nextAttackTime = 0f;

    public virtual void Initialize()
    {
        if (data == null)
        {
            Debug.LogWarning($"{name}: EnemyData is not assigned!");
            return;
        }
        currentHealth = data.MaxHealth;
    }

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Deactivate();
        }
    }

    protected virtual void Deactivate()
    {
        gameObject.SetActive(false);
        // Здесь можно добавить эффекты смерти
    }

    protected bool CanAttack()
    {
        return Time.time >= nextAttackTime;
    }

    protected void ResetAttackTimer()
    {
        if (data != null)
        {
            nextAttackTime = Time.time + (1f / data.AttackRate);
        }
    }

    protected float GetAttackRange()
    {
        return data != null ? data.AttackRange : 1f; // Fallback, если data не назначен
    }

    protected abstract void Move();
    protected abstract void AttackTarget(BuildingBase target);
}