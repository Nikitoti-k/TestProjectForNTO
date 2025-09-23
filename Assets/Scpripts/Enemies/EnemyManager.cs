using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [SerializeField] private BasicEnemy enemyPrefab; // Базовый для пула
    [SerializeField] private int poolSize = 20;
    [SerializeField] private Transform[] spawnPoints; // Глобальные точки
    public Transform[] SpawnPoints => spawnPoints; // Публичный геттер

    private List<EnemyBase> enemyPool = new List<EnemyBase>();

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
            BasicEnemy enemy = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
            enemy.gameObject.SetActive(false);
            enemyPool.Add(enemy);
        }
    }

    public EnemyBase SpawnEnemy(GameObject enemyPrefab, Vector3 spawnPosition)
    {
        EnemyBase enemy = GetFromPool(enemyPrefab);
        if (enemy != null)
        {
            enemy.transform.position = spawnPosition;
            enemy.Initialize();
            enemy.gameObject.SetActive(true);
        }
        return enemy;
    }

    public EnemyBase SpawnEnemy(GameObject enemyPrefab, Transform spawnPoint)
    {
        return SpawnEnemy(enemyPrefab, spawnPoint != null ? spawnPoint.position : GetRandomSpawnPoint().position);
    }

    private EnemyBase GetFromPool(GameObject enemyPrefab)
    {
        EnemyBase prefabEnemy = enemyPrefab.GetComponent<EnemyBase>();
        if (prefabEnemy == null)
        {
            Debug.LogWarning($"Enemy prefab {enemyPrefab.name} has no EnemyBase component!");
            return null;
        }

        foreach (var enemy in enemyPool)
        {
            if (!enemy.gameObject.activeInHierarchy && enemy.GetType() == prefabEnemy.GetType())
            {
                return enemy;
            }
        }
        EnemyBase newEnemy = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity).GetComponent<EnemyBase>();
        newEnemy.gameObject.SetActive(false);
        enemyPool.Add(newEnemy);
        return newEnemy;
    }

    public Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points defined in EnemyManager!");
            return transform;
        }
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
}