using UnityEngine;

public class TurretProjectile : MonoBehaviour
{
    private EnemyBase _target;
    private Vector3 _lastTargetPosition;
    private Vector3 _direction; // Направление движения снаряда
    private float _damage;
    private float _speed = 10f; // Хардкод удалён, теперь устанавливается через Initialize
    private const float MaxLifetime = 3f; // 3 секунды

    private float _spawnTime;
    private bool _isTargetLost; // Флаг, что цель потеряна

    public void Initialize(EnemyBase target, float damage, float speed) // Изменено: добавлен параметр speed
    {
        _target = target;
        _damage = damage;
        _speed = speed; // Устанавливаем скорость из SO
        _isTargetLost = false;

        if (_target != null)
        {
            _lastTargetPosition = _target.transform.position;
            _direction = (_lastTargetPosition - transform.position).normalized; // Начальное направление
        }
        else
        {
            // Если цели нет, используем текущее направление снаряда (например, forward)
            _direction = transform.forward;
            _lastTargetPosition = transform.position + _direction * 100f; // Далёкая точка для начального движения
            _isTargetLost = true;
        }

        _spawnTime = Time.time;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        // Проверка таймера жизни
        if (Time.time > _spawnTime + MaxLifetime)
        {
            Deactivate();
            return;
        }

        // Если цель активна, обновляем позицию и направление
        if (!_isTargetLost && _target != null && _target.gameObject.activeInHierarchy)
        {
            _lastTargetPosition = _target.transform.position;
            _direction = (_lastTargetPosition - transform.position).normalized;

            // Движение к цели
            transform.position += _direction * _speed * Time.deltaTime;

            // Если снаряд достиг цели
            if (Vector3.Distance(transform.position, _lastTargetPosition) < 0.1f)
            {
                _target.TakeDamage((int)_damage);
                Deactivate();
            }
        }
        else
        {
            // Цель потеряна или никогда не была — летим по последнему направлению
            if (!_isTargetLost)
            {
                _isTargetLost = true; // Отмечаем, что цель потеряна
            }
            transform.position += _direction * _speed * Time.deltaTime;
        }

        // Поворачиваем снаряд в направлении движения (опционально, для визуального эффекта)
        if (_direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(_direction);
        }
    }

    // Обработка столкновений
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем столкновение с врагом
        if (other.TryGetComponent<EnemyBase>(out var enemy))
        {
            enemy.TakeDamage((int)_damage);
            Deactivate();
            return;
        }

        // Проверяем столкновение с твёрдым объектом (например, Ground или Wall)
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