using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [SerializeField] private WaveData waveData;
    [SerializeField] private InfiniteWaveData infiniteData;
    [SerializeField] private float spawnRadius = 2f;
    [SerializeField] private GameObject spawnIndicatorPrefab;
    public UnityEvent OnWaveStarted;
    public UnityEvent OnWaveEnded;

    private int _currentWaveIndex;
    private readonly List<EnemyBase> _activeEnemies = new List<EnemyBase>();
    private bool _isWaveActive;
    private bool _isInfiniteMode;
    private GameObject _currentFrontObj;
    private GameObject _currentSpawnIndicator;
    private bool _isSpawning; // ����� ����: true, ���� ��� ����� ������

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (spawnIndicatorPrefab == null) return;

        _currentSpawnIndicator = Instantiate(spawnIndicatorPrefab, Vector3.zero, Quaternion.identity);
        _currentSpawnIndicator.SetActive(false);
        ShowSpawnIndicator();
    }

    public void StartNextWave()
    {
        if (_isWaveActive)
        {
            Debug.LogWarning("Cannot start wave: already active!");
            return;
        }

        _isWaveActive = true;
        _isSpawning = true; // �����: ������������� ���� ������
        ShowSpawnIndicator();

        if (_currentWaveIndex < waveData.Waves.Count)
        {
            StartCoroutine(SpawnWave(waveData.Waves[_currentWaveIndex]));
        }
        else
        {
            _isInfiniteMode = true;
            int infiniteWaveNumber = _currentWaveIndex - waveData.Waves.Count + 1;
            StartCoroutine(SpawnInfiniteWave(infiniteWaveNumber));
        }

        OnWaveStarted.Invoke();
        Debug.Log($"Wave {_currentWaveIndex + 1} started!");
    }

    private void ShowSpawnIndicator()
    {
        if (_currentSpawnIndicator == null || waveData == null) return;

        Vector3 indicatorPos = Vector3.zero;
        bool showIndicator = false;

        if (_currentWaveIndex < waveData.Waves.Count)
        {
            Wave wave = waveData.Waves[_currentWaveIndex];
            if (!wave.UseCircleSpawn)
            {
                List<Vector3> spawnPoints = GetSpawnPoints(wave);
                if (spawnPoints.Count > 0)
                {
                    indicatorPos = spawnPoints[Random.Range(0, spawnPoints.Count)];
                    showIndicator = true;
                }
            }
        }
        else if (infiniteData != null && infiniteData.SpawnType == SpawnType.SequentialFronts && infiniteData.SpawnFrontPrefab != null)
        {
            GameObject tempFrontObj = Instantiate(infiniteData.SpawnFrontPrefab, Vector3.zero, Quaternion.identity);
            tempFrontObj.SetActive(false);
            if (tempFrontObj.TryGetComponent<SpawnPointMap>(out var map))
            {
                List<Transform> points = map.GetSpawnPoints();
                if (points.Count > 0)
                {
                    int infiniteWaveNumber = _currentWaveIndex - waveData.Waves.Count + 1;
                    int frontIndex = (infiniteWaveNumber - 1) % points.Count;
                    indicatorPos = points[frontIndex].position;
                    showIndicator = true;
                }
            }
            Destroy(tempFrontObj);
        }

        _currentSpawnIndicator.SetActive(showIndicator);
        if (showIndicator)
        {
            indicatorPos.y += 2f;
            _currentSpawnIndicator.transform.SetPositionAndRotation(indicatorPos, Quaternion.identity);
        }
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        HideSpawnIndicator();

        List<Vector3> spawnPoints = GetSpawnPoints(wave);
        foreach (EnemyConfig config in wave.Enemies)
        {
            for (int spawnIndex = 0; spawnIndex < config.Count; spawnIndex++)
            {
                Vector3 spawnPos = wave.UseCircleSpawn
                    ? new Vector3(Random.insideUnitCircle.normalized.x * wave.CircleSpawnRadius, 0, Random.insideUnitCircle.normalized.y * wave.CircleSpawnRadius)
                    : spawnPoints.Count > 0 ? spawnPoints[Random.Range(0, spawnPoints.Count)] : EnemyManager.Instance.GetRandomSpawnPoint().position;

                spawnPos = AdjustSpawnPosition(spawnPos);
                SpawnEnemy(config.EnemyPrefab, spawnPos);

                float interval = config.UseCurve && config.IntervalCurve != null
                    ? config.IntervalCurve.Evaluate((float)spawnIndex / config.Count)
                    : config.Interval;

                yield return new WaitForSeconds(interval);
            }
        }

        _isSpawning = false; // �����: ����� ��������
        CheckIfWaveEnded(); // �����: ���������, ����� �� ��������� �����
    }

    private IEnumerator SpawnInfiniteWave(int infiniteWaveNumber)
    {
        HideSpawnIndicator();

        List<Transform> spawnPoints = GetInfiniteSpawnPoints();
        foreach (InfiniteWaveData.EnemyTemplate template in infiniteData.Enemies)
        {
            if (infiniteWaveNumber < template.StartWave) continue;

            int count = Mathf.RoundToInt(template.BaseCount + template.CountGrowthRate * infiniteWaveNumber);
            float interval = Mathf.Max(template.BaseInterval * (1 - template.IntervalReductionRate * infiniteWaveNumber), 0.5f);

            for (int spawnIndex = 0; spawnIndex < count; spawnIndex++)
            {
                Vector3 spawnPos = GetInfiniteSpawnPoint(infiniteData, infiniteWaveNumber, spawnPoints);
                spawnPos = AdjustSpawnPosition(spawnPos);
                SpawnEnemy(template.EnemyPrefab, spawnPos);
                yield return new WaitForSeconds(interval);
            }
        }

        _isSpawning = false; // �����: ����� ��������
        CheckIfWaveEnded(); // �����: ���������, ����� �� ��������� ����� (����� ������� ��������� ������ CheckIfWaveEnded, ���� ��� infinite)
    }

    private List<Vector3> GetSpawnPoints(Wave wave)
    {
        List<Vector3> spawnPoints = new List<Vector3>();

        if (wave.UseCircleSpawn)
            return spawnPoints;

        if (wave.UseRandomFronts || wave.SpawnFrontPrefab == null)
        {
            AddGlobalSpawnPoints(spawnPoints);
            return spawnPoints;
        }

        _currentFrontObj = Instantiate(wave.SpawnFrontPrefab, Vector3.zero, Quaternion.identity);
        _currentFrontObj.SetActive(false);
        if (!_currentFrontObj.TryGetComponent<SpawnPointMap>(out var map))
        {
            Debug.LogWarning($"No SpawnPointMap in {wave.SpawnFrontPrefab.name}! Using global spawn points.");
            AddGlobalSpawnPoints(spawnPoints);
            return spawnPoints;
        }

        List<Transform> points = map.GetSpawnPoints();
        if (points.Count == 0)
        {
            Debug.LogWarning($"No spawn points in {wave.SpawnFrontPrefab.name}! Using global.");
            AddGlobalSpawnPoints(spawnPoints);
            return spawnPoints;
        }

        if (wave.SpawnPointIndices != null && wave.SpawnPointIndices.Count > 0)
        {
            foreach (int index in wave.SpawnPointIndices)
            {
                if (index >= 0 && index < points.Count && points[index] != null)
                {
                    spawnPoints.Add(points[index].position);
                }
                else
                {
                    Debug.LogWarning($"Invalid spawn point index {index} for {wave.SpawnFrontPrefab.name}!");
                }
            }
        }
        else
        {
            foreach (Transform point in points)
            {
                if (point != null) spawnPoints.Add(point.position);
            }
        }

        return spawnPoints.Count > 0 ? spawnPoints : new List<Vector3> { transform.position };
    }

    private List<Transform> GetInfiniteSpawnPoints()
    {
        if (infiniteData.SpawnType != SpawnType.SequentialFronts || infiniteData.SpawnFrontPrefab == null)
            return new List<Transform>();

        _currentFrontObj = Instantiate(infiniteData.SpawnFrontPrefab, Vector3.zero, Quaternion.identity);
        _currentFrontObj.SetActive(false);
        return _currentFrontObj.TryGetComponent<SpawnPointMap>(out var map) ? map.GetSpawnPoints() : new List<Transform>();
    }

    private Vector3 GetInfiniteSpawnPoint(InfiniteWaveData data, int infiniteWaveNumber, List<Transform> spawnPoints)
    {
        if (data.SpawnType == SpawnType.Circle)
        {
            Vector2 circlePoint = Random.insideUnitCircle.normalized * data.CircleRadius;
            return new Vector3(circlePoint.x, 0, circlePoint.y);
        }

        if (spawnPoints == null || spawnPoints.Count == 0 || data.SpawnFrontPrefab == null)
        {
            Debug.LogWarning("No valid spawn points for infinite wave! Using WaveManager transform.");
            return transform.position;
        }

        int frontIndex = (infiniteWaveNumber - 1) % spawnPoints.Count;
        return spawnPoints[frontIndex] != null ? spawnPoints[frontIndex].position : transform.position;
    }

    private Vector3 AdjustSpawnPosition(Vector3 spawnPos)
    {
        if (spawnRadius <= 0) return spawnPos;

        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        spawnPos += new Vector3(randomOffset.x, 0f, randomOffset.y);

        // ���������, ��������� �� ������� �� NavMesh
        if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out UnityEngine.AI.NavMeshHit hit, spawnRadius, UnityEngine.AI.NavMesh.AllAreas))
        {
            spawnPos = hit.position; // ������������ ������� �� ��������� ����� NavMesh
        }
        else
        {
            Debug.LogWarning($"Spawn position {spawnPos} is not on NavMesh! Using original position.");
        }

        // ������������ ������ �� ��������� �����
        Plane groundPlane = new Plane(Vector3.up, 0);
        Ray ray = new Ray(spawnPos + Vector3.up * 100, Vector3.down);
        if (groundPlane.Raycast(ray, out float distance))
        {
            spawnPos = ray.GetPoint(distance);
        }

        return spawnPos;
    }

    private void SpawnEnemy(GameObject prefab, Vector3 position)
    {
        EnemyBase enemy = EnemyManager.Instance.SpawnEnemy(prefab, position);
        if (enemy != null)
        {
            _activeEnemies.Add(enemy);
            enemy.OnDeactivated.AddListener(EnemyDeactivated);
        }
        else
        {
            Debug.LogWarning($"Failed to spawn enemy {prefab.name}!");
        }
    }

    private void AddGlobalSpawnPoints(List<Vector3> spawnPoints)
    {
        if (EnemyManager.Instance.SpawnPoints == null || EnemyManager.Instance.SpawnPoints.Length == 0)
        {
            Debug.LogWarning("No global spawn points in EnemyManager! Using WaveManager transform.");
            spawnPoints.Add(transform.position);
        }
        else
        {
            foreach (Transform point in EnemyManager.Instance.SpawnPoints)
            {
                if (point != null) spawnPoints.Add(point.position);
            }
        }
    }

    private void HideSpawnIndicator()
    {
        if (_currentSpawnIndicator != null)
            _currentSpawnIndicator.SetActive(false);
    }

    private void EnemyDeactivated(EnemyBase enemy)
    {
        _activeEnemies.Remove(enemy);
        CheckIfWaveEnded(); // ��������: ������ ������� ���������� ����� � �������� ��������
    }

    // ����� �����: �������� ������� ��� ���������� �����
    private void CheckIfWaveEnded()
    {
        if (_activeEnemies.Count == 0 && !_isSpawning && _isWaveActive)
        {
            _isWaveActive = false;
            OnWaveEnded.Invoke();
            _currentWaveIndex++;

            // ���������� �������: ��� ������������� ���� ��� ������������ ������
            int reward = 0;
            if (_isInfiniteMode)
            {
                int infiniteWaveNumber = _currentWaveIndex - waveData.Waves.Count; // ����� ����� ����� ++
                reward = Mathf.RoundToInt(infiniteData.BaseReward + infiniteData.RewardGrowthRate * infiniteWaveNumber);
            }
            else if (_currentWaveIndex > 0 && _currentWaveIndex <= waveData.Waves.Count)
            {
                // ��� ������������� �����: ���� ���������� (������ ����� ++)
                Wave completedWave = waveData.Waves[_currentWaveIndex - 1];
                reward = completedWave.Reward; // ������������, ��� � Wave ���� ���� Reward
            }

            if (reward > 0)
            {
                CurrencyManager.Instance.AddCurrency(reward);
                Debug.Log($"Wave completed! Added {reward} currency. Total: {CurrencyManager.Instance.CurrentCurrency}");
            }

            if (_currentFrontObj != null)
            {
                Destroy(_currentFrontObj);
                _currentFrontObj = null;
            }
            ShowSpawnIndicator();
        }
    }
}