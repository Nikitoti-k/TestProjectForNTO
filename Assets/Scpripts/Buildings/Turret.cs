using UnityEngine;
// первая башня - турель
public class Turret : BuildingBase
{
    [SerializeField] private TurretData data;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Transform projectileSpawnPoint;

    private int currentLevel = 0;
    private float nextFireTime = 0f;
    private EnemyBase currentTarget;
    private float currentDamage;
    private float currentFireRate;
    private float currentRange;

    public override void Initialize(HexCoord coord)
    {
        base.Initialize(coord);
        if (data != null && data.Levels.Count > 0)
        {
            UpgradeToLevel(0);
        }
    }

    private void Update()
    {
        if (data == null) return;

        if (currentTarget != null)
        {
            if (Vector3.Distance(transform.position, currentTarget.transform.position) <= currentRange && currentTarget.gameObject.activeInHierarchy)
            {
                if (Time.time >= nextFireTime)
                {
                    Attack();
                    nextFireTime = Time.time + (1f / currentFireRate);
                }
            }
            else
            {
                currentTarget = null;
            }
        }
        else
        {
            FindTarget();
        }
    }

    private void Attack()
    {
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy || ProjectilePool.Instance == null)
        {
            return; 
        }

        TurretProjectile projectile = ProjectilePool.Instance.GetProjectile();
        projectile.transform.position = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
        projectile.Initialize(currentTarget, currentDamage);
        Debug.Log($"Turret fires at {currentTarget.name} for {currentDamage} damage");
    }

    public void Upgrade()
    {
        if (currentLevel < data.Levels.Count - 1)
        {
            UpgradeToLevel(currentLevel + 1);
        }
    }

    private void UpgradeToLevel(int level)
    {
        currentLevel = level;
        var levelData = data.Levels[level];
        currentDamage = levelData.Damage;
        currentFireRate = levelData.FireRate;
        currentRange = levelData.Range;
    }

    private void FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, currentRange, enemyLayer);
        foreach (var hit in hits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                currentTarget = enemy;
                return;
            }
        }
    }
}