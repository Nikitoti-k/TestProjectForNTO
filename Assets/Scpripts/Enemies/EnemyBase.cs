// Базовый класс для врагов: хэндлит HP, damage, движение, атаку. Абстрактный для Move/Attack.
using UnityEngine;
using UnityEngine.Events;

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] protected EnemyData data;
    protected int currentHealth;
    protected float nextAttackTime;
    public UnityEvent<EnemyBase> OnDeactivated = new UnityEvent<EnemyBase>();

    public virtual void Initialize()
    {
        if (data == null)
        {
            Debug.LogWarning($"{name}: EnemyData is null!");
            return;
        }
        currentHealth = data.MaxHealth;
        HealthBarManager.Instance?.ShowHealthBar(this, currentHealth, data.MaxHealth, false); // Показываем плашку.
    }

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;
        HealthBarManager.Instance?.UpdateHealthBar(this, currentHealth, data.MaxHealth); // Обновляем плашку.
        if (currentHealth <= 0)
        {
            Deactivate();
        }
    }

    protected virtual void Deactivate()
    {
        HealthBarManager.Instance?.HideHealthBar(this); // Скрываем плашку.
        gameObject.SetActive(false);
        OnDeactivated.Invoke(this);
        if (CurrencyManager.Instance != null && data != null)
        {
            CurrencyManager.Instance.AddCurrency(data.Reward);
        }
    }

    protected bool CanAttack() => Time.time >= nextAttackTime;

    protected void ResetAttackTimer()
    {
        if (data != null)
        {
            nextAttackTime = Time.time + (1f / data.AttackRate);
        }
    }

    protected float GetAttackRange() => data?.AttackRange ?? 1f;

    protected float GetDetectionRange() => data?.DetectionRange ?? 5f;

    protected abstract void Move();
    protected abstract void AttackTarget(BuildingBase target);
}