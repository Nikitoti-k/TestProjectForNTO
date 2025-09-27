// Турель: атакует врагов в радиусе, использует пул снарядов. Наслед от BuildingBase.
using UnityEngine;
using System.Collections.Generic;

public class Turret : BuildingBase
{
    [SerializeField] private LayerMask enemyLayer;
    private float nextFireTime;
    private EnemyBase currentTarget;

    public override void Initialize(HexCoord coord)
    {
        base.Initialize(coord);
        UpdateHealthBar();
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        UpdateHealthBar();
    }

    public override void Upgrade()
    {
        base.Upgrade();
        UpdateHealthBar();
    }

    private void Update()
    {
        if (!IsPlaced) return;

        var turretModule = GetModule<TurretModule>();
        if (turretModule == null || Time.time < nextFireTime) return;

        if (currentTarget != null && currentTarget.gameObject.activeInHierarchy &&
            Vector3.Distance(transform.position, currentTarget.transform.position) <= turretModule.LevelData[currentLevel].Range)
        {
            Fire(currentTarget);
        }
        else
        {
            FindTarget(turretModule.LevelData[currentLevel].Range);
        }
    }

    private void FindTarget(float range)
    {
        currentTarget = null;
        Collider[] hits = Physics.OverlapSphere(transform.position, range, enemyLayer);
        float minDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<EnemyBase>(out var enemy) && enemy.gameObject.activeInHierarchy)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    currentTarget = enemy;
                }
            }
        }
    }

    private void Fire(EnemyBase target)
    {
        var turretModule = GetModule<TurretModule>();
        if (turretModule == null) return;

        var projectile = ProjectilePool.Instance.GetProjectile();
        projectile.transform.position = transform.position;
        projectile.GetComponent<TurretProjectile>().Initialize(target, turretModule.LevelData[currentLevel].Damage, turretModule.LevelData[currentLevel].ProjectileSpeed);

        nextFireTime = Time.time + (1f / turretModule.LevelData[currentLevel].FireRate);
    }

    private void UpdateHealthBar()
    {
        var turretModule = GetModule<TurretModule>();
        if (turretModule != null)
        {
            HealthBarManager.Instance?.ShowHealthBar(this, CurrentHealth, data.Levels[currentLevel].MaxHealth, true);
        }
    }

    protected override void DestroyBuilding()
    {
        HealthBarManager.Instance?.HideHealthBar(this);
        base.DestroyBuilding();
    }
}