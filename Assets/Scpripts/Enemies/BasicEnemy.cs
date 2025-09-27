// Базовый враг: движется к HQ или ближайшему зданию по NavMesh, атакует в радиусе.
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class BasicEnemy : EnemyBase
{
    private NavMeshAgent agent;
    private Vector3 hqPosition;
    private BuildingBase currentTarget;
    [SerializeField] private LayerMask buildingLayer;

    public override void Initialize()
    {
        base.Initialize();
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogWarning($"{name}: No NavMeshAgent!");
            return;
        }
        agent.speed = data?.Speed ?? 3f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        var hexGrid = FindObjectOfType<HexGrid>();
        if (hexGrid?.GetHeadquarters() != null)
        {
            hqPosition = hexGrid.GetHeadquarters().Position;
        }
        else
        {
            Debug.LogWarning($"{name}: HQ not found!");
        }
    }

    private void OnEnable()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            StartCoroutine(WaitAndMove());
        }
        else
        {
            Debug.LogWarning($"{name}: Can't start move in OnEnable: no agent or not on NavMesh");
        }
    }

    private IEnumerator WaitAndMove()
    {
        yield return null;
        if (agent != null && agent.isOnNavMesh)
        {
            Move();
            Debug.Log($"{name}: Move started");
        }
        else
        {
            Debug.LogWarning($"{name}: Agent not on NavMesh after delay!");
        }
    }

    private void Update()
    {
        if (data == null || agent == null || !agent.isOnNavMesh || !gameObject.activeInHierarchy) return;

        if (currentTarget != null)
        {
            try
            {
                if (!currentTarget.gameObject.activeInHierarchy)
                {
                    ResetTarget();
                }
                else if (Vector3.Distance(transform.position, currentTarget.Position) <= GetEffectiveAttackRange(currentTarget))
                {
                    agent.isStopped = true;
                    if (CanAttack())
                    {
                        AttackTarget(currentTarget);
                        ResetAttackTimer();
                    }
                }
                else
                {
                    agent.isStopped = false;
                    Move();
                }
            }
            catch
            {
                ResetTarget();
            }
        }
        else
        {
            CheckForBuildings();
            Move();
        }
    }

    protected override void Move()
    {
        if (data == null || agent == null || !agent.isOnNavMesh) return;

        Vector3 dest = currentTarget != null ? currentTarget.Position : hqPosition;
        if (agent.destination != dest)
        {
            float range = currentTarget != null ? GetEffectiveAttackRange(currentTarget) : GetAttackRange();
            Vector3 offset = Random.insideUnitCircle * range;
            agent.SetDestination(dest + new Vector3(offset.x, 0f, offset.y));
        }
    }

    protected override void AttackTarget(BuildingBase target)
    {
        if (data == null || target == null) return;
        int effectiveDamage = data.Damage;
       
        target.TakeDamage(effectiveDamage);
        Debug.Log($"Enemy attacks {target.name} for {effectiveDamage} damage");
    }

    private void CheckForBuildings()
    {
        if (data == null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, GetDetectionRange(), buildingLayer);
        BuildingBase closest = null;
        float minDist = float.MaxValue;
        Vector3 dirToHQ = (hqPosition - transform.position).normalized;

        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent<BuildingBase>(out var building) || !building.gameObject.activeInHierarchy) continue;

            float dist = Vector3.Distance(transform.position, building.Position);
            float distToHQ = Vector3.Distance(transform.position, hqPosition);
            if (dist < minDist && dist <= distToHQ)
            {
                Vector3 dirToBuilding = (building.Position - transform.position).normalized;
                if (Vector3.Angle(dirToHQ, dirToBuilding) <= 90f)
                {
                    minDist = dist;
                    closest = building;
                }
            }
        }

        SetTarget(closest);
    }

    protected float GetEffectiveAttackRange(BuildingBase target)
    {
        float baseRange = GetAttackRange();
        if (target is Headquarters) return baseRange * 4f;
        return baseRange;
    }

    private void SetTarget(BuildingBase newTarget)
    {
        if (currentTarget != null)
        {
            currentTarget.OnBuildingDestroyed.RemoveListener(ResetTarget);
        }
        currentTarget = newTarget;
        if (currentTarget != null)
        {
            currentTarget.OnBuildingDestroyed.AddListener(ResetTarget);
            Debug.Log($"{name}: New target: {currentTarget.name}");
        }
    }

    // Исправлено: проверка agent перед isStopped.
    private void ResetTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.OnBuildingDestroyed.RemoveListener(ResetTarget);
            currentTarget = null;
        }
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            CheckForBuildings();
            Move();
        }
    }

    protected override void Deactivate()
    {
        if (currentTarget != null)
        {
            currentTarget.OnBuildingDestroyed.RemoveListener(ResetTarget);
            currentTarget = null;
        }
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true; // Останавливаем перед деактивацией.
        }
        base.Deactivate();
    }
}