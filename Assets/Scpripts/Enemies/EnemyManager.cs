using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [SerializeField] private BasicEnemy enemyPrefab;
    [SerializeField] private int poolSize = 20;
    [SerializeField] private Transform[] spawnPoints;

    private List<BasicEnemy> enemyPool = new List<BasicEnemy>();

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
        InvokeRepeating(nameof(SpawnEnemy), 2f, 2f);
        InvokeRepeating(nameof(SpawnEnemy), 2f, 2f);
        InvokeRepeating(nameof(SpawnEnemy), 2f, 2f);
        InvokeRepeating(nameof(SpawnEnemy), 2f, 2f);
        InvokeRepeating(nameof(SpawnEnemy), 2f, 2f);
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

    private BasicEnemy GetFromPool()
    {
        foreach (var enemy in enemyPool)
        {
            if (!enemy.gameObject.activeInHierarchy)
            {
                return enemy;
            }
        }
        BasicEnemy newEnemy = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
        newEnemy.gameObject.SetActive(false);
        enemyPool.Add(newEnemy);
        return newEnemy;
    }

    public void SpawnEnemy()
    {
        if (spawnPoints.Length == 0) return;
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        BasicEnemy enemy = GetFromPool();
        enemy.transform.position = spawnPoint.position;
        enemy.Initialize();
        enemy.gameObject.SetActive(true);
    }
}