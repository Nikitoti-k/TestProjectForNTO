using UnityEngine;
using UnityEngine.Events;

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] protected EnemyData data;

    protected int currentHealth;
    protected float nextAttackTime = 0f;
    public UnityEvent<EnemyBase> OnDeactivated = new UnityEvent<EnemyBase>(); // Новое событие

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
        OnDeactivated.Invoke(this);
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
        return data != null ? data.AttackRange : 1f;
    }

    protected float GetDetectionRange()
    {
        return data != null ? data.DetectionRange : 5f;
    }

    protected abstract void Move();
    protected abstract void AttackTarget(BuildingBase target);
}