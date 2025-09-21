using UnityEngine;

public class BasicEnemy : EnemyBase
{
    private Vector3 targetPosition; // ������� �����
    private BuildingBase currentTarget; // ������� ���� �����
    [SerializeField] private LayerMask buildingLayer; // Layer ��� ������

    public override void Initialize()
    {
        base.Initialize();
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
        if (data == null) return;

        if (currentTarget != null)
        {
            // �������, ���� � �������
            if (Vector3.Distance(transform.position, currentTarget.Position) <= GetAttackRange())
            {
                if (CanAttack())
                {
                    AttackTarget(currentTarget);
                    ResetAttackTimer();
                }
            }
            else
            {
                currentTarget = null;
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
        if (data == null) return;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, data.Speed * Time.deltaTime);
        transform.LookAt(targetPosition);
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
        Collider[] hits = Physics.OverlapSphere(transform.position, GetAttackRange(), buildingLayer);
        foreach (var hit in hits)
        {
            BuildingBase building = hit.GetComponent<BuildingBase>();
            if (building != null)
            {
                currentTarget = building;
                return;
            }
        }
    }
}