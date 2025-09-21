using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BasicEnemy : EnemyBase
{
    private NavMeshAgent agent;
    private Vector3 targetPosition; // Позиция штаба
    private BuildingBase currentTarget; // Текущая цель 
    [SerializeField] private LayerMask buildingLayer;

    public override void Initialize()
    {
        base.Initialize();
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogWarning($"{name}: NavMeshAgent is missing!");
            return;
        }
        agent.speed = data.Speed;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        var hexGrid = FindObjectOfType<HexGrid>();
        if (hexGrid != null)
        {
            var hq = hexGrid.GetHeadquarters();
            if (hq != null)
            {
                targetPosition = hq.Position;
            }
        }
    }

    private void Update()
    {
        if (data == null || agent == null) return;

        if (currentTarget != null)
        {
            // Проверяем, жива ли цель
            if (!currentTarget.gameObject.activeInHierarchy)
            {
                currentTarget = null;
                agent.isStopped = false;
                CheckForBuildings();
                Move();
            }
            else if (Vector3.Distance(transform.position, currentTarget.Position) <= GetAttackRange())
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
        else
        {
            CheckForBuildings();
            Move();
        }
    }

    protected override void Move()
    {
        if (data == null || agent == null) return;

        Vector3 destination = currentTarget != null ? currentTarget.Position : targetPosition;
        if (agent.destination != destination)
        {
            Vector3 offset = Random.insideUnitCircle * GetAttackRange(); 
            agent.SetDestination(destination + new Vector3(offset.x, 0f, offset.y));
        }
    }

    protected override void AttackTarget(BuildingBase target)
    {
        if (data == null) return;
        target.TakeDamage(data.Damage);
        Debug.Log($"Enemy attacks {target.name} for {data.Damage} damage");
    }

    private void CheckForBuildings()
    {
        if (data == null) return;
        Collider[] hits = Physics.OverlapSphere(transform.position, GetDetectionRange(), buildingLayer);
        BuildingBase closestBuilding = null;
        float minDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            BuildingBase building = hit.GetComponent<BuildingBase>();
            if (building != null && building.gameObject.activeInHierarchy)
            {
                float distance = Vector3.Distance(transform.position, building.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestBuilding = building;
                }
            }
        }

        currentTarget = closestBuilding;
    }
}