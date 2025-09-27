// ��������� ����� ������, ������� �� � ������, ��������
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [SerializeField] private WaveData waveData; 
    [SerializeField] private InfiniteWaveData infiniteWaveData; 
    [SerializeField] private int poolSizePerType = 20;
    [SerializeField] private Transform[] spawnPoints;
    public Transform[] SpawnPoints => spawnPoints;

    private Dictionary<GameObject, List<EnemyBase>> enemyPools = new Dictionary<GameObject, List<EnemyBase>>(); 

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
        InitializePools();
    }

    
    private void InitializePools()
    {
        HashSet<GameObject> uniquePrefabs = new HashSet<GameObject>();

        
        if (waveData != null && waveData.Waves != null)
        {
            foreach (var wave in waveData.Waves)
            {
                foreach (var enemyConfig in wave.Enemies)
                {
                    if (enemyConfig.EnemyPrefab != null)
                    {
                        uniquePrefabs.Add(enemyConfig.EnemyPrefab);
                    }
                }
            }
        }

      
        if (infiniteWaveData != null && infiniteWaveData.Enemies != null)
        {
            foreach (var enemyTemplate in infiniteWaveData.Enemies)
            {
                if (enemyTemplate.EnemyPrefab != null)
                {
                    uniquePrefabs.Add(enemyTemplate.EnemyPrefab);
                }
            }
        }

        if (uniquePrefabs.Count == 0)
        {
            Debug.LogError("EnemyManager: �� ������� ������� ������ � WaveData ��� InfiniteWaveData!");
            return;
        }

        // �������������� ���� ��� ������� ����������� �������
        foreach (var prefab in uniquePrefabs)
        {
            if (prefab == null) continue;
            enemyPools[prefab] = new List<EnemyBase>();

            for (int i = 0; i < poolSizePerType; i++)
            {
                EnemyBase enemy = Instantiate(prefab, Vector3.zero, Quaternion.identity).GetComponent<EnemyBase>();
                if (enemy == null)
                {
                    Debug.LogWarning($"EnemyManager: ������ {prefab.name} �� �������� ��������� EnemyBase!");
                    continue;
                }
                enemy.gameObject.SetActive(false);
                enemyPools[prefab].Add(enemy);
            }
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
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemyManager: ������� null ������!");
            return null;
        }

        EnemyBase prefabEnemy = enemyPrefab.GetComponent<EnemyBase>();
        if (prefabEnemy == null)
        {
            Debug.LogWarning($"EnemyManager: ������ {enemyPrefab.name} �� �������� ��������� EnemyBase!");
            return null;
        }

       
        if (!enemyPools.ContainsKey(enemyPrefab))
        {
            enemyPools[enemyPrefab] = new List<EnemyBase>();
            Debug.LogWarning($"EnemyManager: ��� ��� ������� {enemyPrefab.name} �� ���������������, ������� �����.");
        }

        // ���� ���������� ������ � ����
        foreach (var enemy in enemyPools[enemyPrefab])
        {
            if (!enemy.gameObject.activeInHierarchy)
            {
                return enemy;
            }
        }

        // ���� ��� ��������� ������, ������� �����
        EnemyBase newEnemy = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity).GetComponent<EnemyBase>();
        if (newEnemy == null)
        {
            Debug.LogWarning($"EnemyManager: �� ������� ������� ����� ��������� ��� ������� {enemyPrefab.name}!");
            return null;
        }
        newEnemy.gameObject.SetActive(false);
        enemyPools[enemyPrefab].Add(newEnemy);
        return newEnemy;
    }

    public Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("�� ��������� ����� ������!");
            return transform;
        }
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
}