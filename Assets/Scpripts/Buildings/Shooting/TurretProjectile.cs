using UnityEngine;
// Класс снаряда для стрельы турели
public class TurretProjectile : MonoBehaviour
{
    private EnemyBase target;
    private Vector3 lastTargetPosition; 
    private float damage;
    private float speed = 10f;
    private float maxLifetime = 5f;

    private float spawnTime;

    public void Initialize(EnemyBase target, float damage)
    {
        this.target = target;
        this.damage = damage;
        if (target != null)
        {
            lastTargetPosition = target.transform.position;
        }
        spawnTime = Time.time;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            
            transform.position = Vector3.MoveTowards(transform.position, lastTargetPosition, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, lastTargetPosition) < 0.1f)
            {
                Deactivate();
            }
        }
        else
        {
         
            lastTargetPosition = target.transform.position;
            transform.position = Vector3.MoveTowards(transform.position, lastTargetPosition, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, lastTargetPosition) < 0.1f)
            {
                target.TakeDamage((int)damage);
                Deactivate();
            }
        }

        
        if (Time.time > spawnTime + maxLifetime)
        {
            Deactivate();
        }
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}