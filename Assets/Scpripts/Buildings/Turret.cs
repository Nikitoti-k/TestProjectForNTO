using UnityEngine;

public class Turret : BuildingBase
{
    [SerializeField] private TurretData data;

    private int currentLevel = 0;
    private float nextFireTime = 0f;

    private Transform target;
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
        if (target != null && Time.time >= nextFireTime)
        {
            Attack();
            nextFireTime = Time.time + (1f / currentFireRate);
        }
    }

    private void Attack()
    {
        if (target != null)
        {
            Debug.Log($"Turret attacks for {currentDamage} damage");
        }
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

    public void SetTarget(Transform newTarget)
    {
        if (newTarget != null && Vector3.Distance(transform.position, newTarget.position) <= currentRange)
        {
            target = newTarget;
        }
    }
}