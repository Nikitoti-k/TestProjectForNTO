using System.Collections.Generic;
using UnityEngine;

// Паттерн пула для снарядов 
public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    [SerializeField] private TurretProjectile projectilePrefab;
    [SerializeField] private int poolSize = 50;

    private List<TurretProjectile> projectilePool = new List<TurretProjectile>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            TurretProjectile projectile = Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);
            projectile.gameObject.SetActive(false);
            projectilePool.Add(projectile);
        }
    }

    public TurretProjectile GetProjectile()
    {
        foreach (var projectile in projectilePool)
        {
            if (!projectile.gameObject.activeInHierarchy)
            {
                return projectile;
            }
        }
        // Расширяем пул, если все заняты
        TurretProjectile newProjectile = Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);
        newProjectile.gameObject.SetActive(false);
        projectilePool.Add(newProjectile);
        return newProjectile;
    }
}