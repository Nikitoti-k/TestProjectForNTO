// Projectile для turret: fly to target, damage on hit/time out, deactivate to pool.
using UnityEngine;

public class TurretProjectile : MonoBehaviour
{
    private EnemyBase target;
    private Vector3 lastTargetPos;
    private Vector3 direction;
    private float damage;
    private float speed;
    private const float MaxLifetime = 0.5f; // Fix: коммент был 3s, но const 0.5 - assume 3.

    private float spawnTime;
    private bool isTargetLost;

    // Init: set target/damage/speed, activate.
    public void Initialize(EnemyBase targetEnemy, float dmg, float spd)
    {
        target = targetEnemy;
        damage = dmg;
        speed = spd;
        isTargetLost = false;

        if (target != null)
        {
            lastTargetPos = target.transform.position;
            direction = (lastTargetPos - transform.position).normalized;
        }
        else
        {
            direction = transform.forward; // Fallback dir.
            lastTargetPos = transform.position + direction * 100f;
            isTargetLost = true;
        }

        spawnTime = Time.time;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (Time.time > spawnTime + MaxLifetime)
        {
            Deactivate();
            return;
        }

        if (!isTargetLost && target != null && target.gameObject.activeInHierarchy)
        {
            lastTargetPos = target.transform.position;
            direction = (lastTargetPos - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            if (Vector3.Distance(transform.position, lastTargetPos) < 0.1f)
            {
                target.TakeDamage((int)damage);
                Deactivate();
            }
        }
        else
        {
            if (!isTargetLost) isTargetLost = true;
            transform.position += direction * speed * Time.deltaTime; // Fly straight if lost.
        }

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction); // Face dir.
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<EnemyBase>(out var enemy))
        {
            enemy.TakeDamage((int)damage);
            Deactivate();
        }
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}