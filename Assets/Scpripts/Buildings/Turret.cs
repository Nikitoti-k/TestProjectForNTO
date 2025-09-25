// ��������� �������: �������� �� ������, ���������, ����� ������.
using UnityEngine;

public class Turret : BuildingBase
{
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform modelSlot;

    private float nextFireTime = 0f;
    private EnemyBase currentTarget;
    private float currentDamage;
    private float currentFireRate;
    private float currentRange;
    private float currentProjectileSpeed; // �����: ������� �������� �������
    private GameObject currentModel;

    public override void Initialize(HexCoord coord)
    {
        base.Initialize(coord);
        UpgradeToLevel(0);
    }

    private void Update()
    {
        if (data == null)
        {
            return;
        }

        var turretModule = GetModule<TurretModule>();
        if (turretModule == null)
        {
            return;
        }

        if (currentTarget != null)
        {
            if (Vector3.Distance(transform.position, currentTarget.transform.position) <= currentRange && currentTarget.gameObject.activeInHierarchy)
            {
                if (Time.time >= nextFireTime)
                {
                    Attack(turretModule);
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

    // ��������: ���� ������ �� ���� � �������������� ���
    private void Attack(TurretModule module)
    {
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy || ProjectilePool.Instance == null)
        {
            return;
        }

        TurretProjectile projectile = ProjectilePool.Instance.GetProjectile();
        projectile.transform.position = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
        projectile.Initialize(currentTarget, currentDamage, currentProjectileSpeed); // ��������: �������� �������� ��������
    }

    public override void Upgrade()
    {
        base.Upgrade();
    }

    protected override void UpgradeToLevel(int level)
    {
        base.UpgradeToLevel(level);
        var turretModule = GetModule<TurretModule>();
        if (turretModule == null || turretModule.LevelData.Count <= level)
        {
            return;
        }

        var levelData = turretModule.LevelData[level];
        currentDamage = levelData.Damage;
        currentFireRate = levelData.FireRate;
        currentRange = levelData.Range;
        currentProjectileSpeed = levelData.ProjectileSpeed; // �����: ������������� �������� �������
        UpdateVisual(levelData);
    }

    // ����� ������: ���������� ������ � ������ ����� �� �����
    private void UpdateVisual(TurretLevelData levelData)
    {
        if (currentModel != null)
        {
            Destroy(currentModel);
        }

        if (levelData.ModelPrefab == null)
        {
            return;
        }

        Transform parent = modelSlot != null ? modelSlot : transform;
        currentModel = Instantiate(levelData.ModelPrefab, parent.position, parent.rotation, parent);
    }

    // ����� ���������� �����: ���������� OverlapSphere � ��������� �� ����������
    private void FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, currentRange, enemyLayer);
        if (hits.Length == 0)
        {
            return;
        }

        EnemyBase closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }

        currentTarget = closestEnemy;
    }
}