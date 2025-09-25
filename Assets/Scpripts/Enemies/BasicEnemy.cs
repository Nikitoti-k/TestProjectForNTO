using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class BasicEnemy : EnemyBase
{
    private NavMeshAgent _agent;
    private Vector3 _hqPosition; // Позиция штаба (HQ)
    private BuildingBase _currentTarget; // Текущая цель (здание)
    [SerializeField] private LayerMask _buildingLayer;

    public override void Initialize()
    {
        base.Initialize();
        _agent = GetComponent<NavMeshAgent>();
        if (_agent == null)
        {
            Debug.LogWarning($"{name}: NavMeshAgent отсутствует!");
            return;
        }
        _agent.speed = data.Speed;
        _agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        var hexGrid = FindObjectOfType<HexGrid>();
        if (hexGrid != null)
        {
            var hq = hexGrid.GetHeadquarters();
            if (hq != null)
            {
                _hqPosition = hq.Position;
            }
            else
            {
                Debug.LogWarning($"{name}: Штаб (Headquarters) не найден!");
            }
        }
        else
        {
            Debug.LogWarning($"{name}: HexGrid не найден!");
        }

        // Не запускаем корутину здесь, движение начнётся в OnEnable
        if (!gameObject.activeInHierarchy)
        {
            Debug.Log($"{name}: Объект неактивен, движение начнётся при активации в OnEnable");
        }
    }

    private void OnEnable()
    {
        // Когда объект активируется, пробуем начать движение
        if (_agent != null && _agent.isOnNavMesh)
        {
            StartCoroutine(WaitAndMove());
        }
        else
        {
            Debug.LogWarning($"{name}: Не удалось начать движение в OnEnable: агент не на NavMesh или отсутствует");
        }
    }

    private IEnumerator WaitAndMove()
    {
        yield return null; // Ждём один кадр для активации агента на NavMesh
        if (_agent != null && _agent.isOnNavMesh)
        {
            Move();
            Debug.Log($"{name}: Движение начато после задержки");
        }
        else
        {
            Debug.LogWarning($"{name}: Агент не на NavMesh после задержки!");
        }
    }

    private void Update()
    {
        if (data == null || _agent == null || !_agent.isOnNavMesh) return;

        if (_currentTarget != null)
        {
            // Проверяем, активна ли цель
            if (!_currentTarget.gameObject.activeInHierarchy)
            {
                _currentTarget = null;
                _agent.isStopped = false;
                CheckForBuildings();
                Move();
            }
            else if (Vector3.Distance(transform.position, _currentTarget.Position) <= GetEffectiveAttackRange(_currentTarget))
            {
                _agent.isStopped = true;
                if (CanAttack())
                {
                    AttackTarget(_currentTarget);
                    ResetAttackTimer();
                }
            }
            else
            {
                _agent.isStopped = false;
                Move();
            }
        }
        else
        {
            // Ищем здания, если нет текущей цели
            CheckForBuildings();
            Move();
        }
    }

    protected override void Move()
    {
        if (data == null || _agent == null || !_agent.isOnNavMesh) return;

        // Выбираем цель: здание или HQ
        Vector3 destination = _currentTarget != null ? _currentTarget.Position : _hqPosition;
        if (_agent.destination != destination)
        {
            float range = _currentTarget != null ? GetEffectiveAttackRange(_currentTarget) : GetAttackRange();
            Vector3 offset = Random.insideUnitCircle * range;
            _agent.SetDestination(destination + new Vector3(offset.x, 0f, offset.y));
        }
    }

    protected override void AttackTarget(BuildingBase target)
    {
        if (data == null) return;
        target.TakeDamage(data.Damage);
        Debug.Log($"Враг атакует {target.name} на {data.Damage} урона");
    }

    private void CheckForBuildings()
    {
        if (data == null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, GetDetectionRange(), _buildingLayer);
        BuildingBase closestBuilding = null;
        float minDistance = float.MaxValue;

        // Направление к HQ
        Vector3 directionToHQ = (_hqPosition - transform.position).normalized;

        foreach (var hit in hits)
        {
            BuildingBase building = hit.GetComponent<BuildingBase>();
            if (building == null || !building.gameObject.activeInHierarchy) continue;

            float distance = Vector3.Distance(transform.position, building.Position);
            // Проверяем, ближе ли здание, чем HQ (включая равенство для самого HQ)
            float distToHQ = Vector3.Distance(transform.position, _hqPosition);
            if (distance < minDistance && distance <= distToHQ)
            {
                // Проверяем угол: здание должно быть в пределах 180 градусов от направления к HQ
                Vector3 directionToBuilding = (building.Position - transform.position).normalized;
                float angle = Vector3.Angle(directionToHQ, directionToBuilding);
                if (angle <= 90f) // Половина от 180 градусов
                {
                    minDistance = distance;
                    closestBuilding = building;
                }
            }
        }

        _currentTarget = closestBuilding;
        if (_currentTarget != null)
        {
            Debug.Log($"{name}: Новая цель: {_currentTarget.name}");
        }
    }

    protected float GetEffectiveAttackRange(BuildingBase target)
    {
        float baseRange = GetAttackRange();
        // Для штаба увеличиваем радиус атаки, чтобы учитывать его размер (7 клеток)
        if (target is Headquarters)
        {
            return baseRange * 4f; // Умножаем на коэффициент, чтобы враги останавливались дальше от центра
        }
        return baseRange;
    }
}