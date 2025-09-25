using UnityEngine;

public class TurretProjectile : MonoBehaviour
{
    private EnemyBase _target;
    private Vector3 _lastTargetPosition;
    private Vector3 _direction; // ����������� �������� �������
    private float _damage;
    private float _speed = 10f; // ������� �����, ������ ��������������� ����� Initialize
    private const float MaxLifetime = 3f; // 3 �������

    private float _spawnTime;
    private bool _isTargetLost; // ����, ��� ���� ��������

    public void Initialize(EnemyBase target, float damage, float speed) // ��������: �������� �������� speed
    {
        _target = target;
        _damage = damage;
        _speed = speed; // ������������� �������� �� SO
        _isTargetLost = false;

        if (_target != null)
        {
            _lastTargetPosition = _target.transform.position;
            _direction = (_lastTargetPosition - transform.position).normalized; // ��������� �����������
        }
        else
        {
            // ���� ���� ���, ���������� ������� ����������� ������� (��������, forward)
            _direction = transform.forward;
            _lastTargetPosition = transform.position + _direction * 100f; // ������ ����� ��� ���������� ��������
            _isTargetLost = true;
        }

        _spawnTime = Time.time;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        // �������� ������� �����
        if (Time.time > _spawnTime + MaxLifetime)
        {
            Deactivate();
            return;
        }

        // ���� ���� �������, ��������� ������� � �����������
        if (!_isTargetLost && _target != null && _target.gameObject.activeInHierarchy)
        {
            _lastTargetPosition = _target.transform.position;
            _direction = (_lastTargetPosition - transform.position).normalized;

            // �������� � ����
            transform.position += _direction * _speed * Time.deltaTime;

            // ���� ������ ������ ����
            if (Vector3.Distance(transform.position, _lastTargetPosition) < 0.1f)
            {
                _target.TakeDamage((int)_damage);
                Deactivate();
            }
        }
        else
        {
            // ���� �������� ��� ������� �� ���� � ����� �� ���������� �����������
            if (!_isTargetLost)
            {
                _isTargetLost = true; // ��������, ��� ���� ��������
            }
            transform.position += _direction * _speed * Time.deltaTime;
        }

        // ������������ ������ � ����������� �������� (�����������, ��� ����������� �������)
        if (_direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(_direction);
        }
    }

    // ��������� ������������
    private void OnTriggerEnter(Collider other)
    {
        // ��������� ������������ � ������
        if (other.TryGetComponent<EnemyBase>(out var enemy))
        {
            enemy.TakeDamage((int)_damage);
            Deactivate();
            return;
        }

        // ��������� ������������ � ������ �������� (��������, Ground ��� Wall)
        if (other.CompareTag("Ground"))
        {
            Deactivate();
        }
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}