using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

// ��������� ������� ������: �����, ������������, ����������.
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }
    public int CurrentWaveIndex { get; private set; } // ������� �����.

    [SerializeField] private WaveData waveData; // ������ � ������.
    [SerializeField] private InfiniteWaveData infiniteData; // ������ ��� ������������ ������.
    [SerializeField] private float spawnRadius = 2f; // ������ �������� ������.
    [SerializeField] private GameObject spawnIndicatorPrefab; // ������ ���������� ������.
    public UnityEvent OnWaveStarted; // ������� ������ �����.
    public UnityEvent OnWaveEnded; // ������� ����� �����.

    private readonly List<EnemyBase> activeEnemies = new List<EnemyBase>(); // �������� �����.
    private bool isWaveActive; // ��� �� �����.
    private bool isSpawning; // ��� �� ����� ������.
    private bool isInfiniteMode; // ����������� �����.
    private GameObject currentFrontObj; // ������� ������ ������ ������.
    private GameObject spawnIndicator; // ��������� ����� ������.

    // ������������� singleton.
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ������ ��������� ������ � ���������� ������ ����� ��������� �����.
    void Start()
    {
        if (spawnIndicatorPrefab == null)
        {
            Debug.LogError("WaveManager: ����������� spawnIndicatorPrefab!");
            return;
        }
        spawnIndicator = Instantiate(spawnIndicatorPrefab, Vector3.zero, Quaternion.identity);
        spawnIndicator.SetActive(false);
        ShowNextWaveSpawnIndicator(); // ���������� ����� ������ ��� ������ �����.
    }

    // ���������, ����������� �� ����� (��� ������ � ������).
    void Update()
    {
        if (isWaveActive && !isSpawning && activeEnemies.Count == 0)
            EndWave();
    }

    // ��������� ��������� �����.
    public void StartNextWave()
    {
        if (isWaveActive)
        {
            Debug.LogWarning("WaveManager: ����� ��� �������!");
            return;
        }

        isWaveActive = true;
        isSpawning = true;
        OnWaveStarted.Invoke();

        if (CurrentWaveIndex < waveData.Waves.Count)
            StartCoroutine(SpawnWave(waveData.Waves[CurrentWaveIndex]));
        else
        {
            isInfiniteMode = true;
            StartCoroutine(SpawnInfiniteWave(CurrentWaveIndex - waveData.Waves.Count + 1));
        }
        Debug.Log($"����� {CurrentWaveIndex + 1} ��������!");
    }

    // ������������� ������ �����.
    public void SetWaveIndex(int index)
    {
        CurrentWaveIndex = Mathf.Max(0, index);
        ShowNextWaveSpawnIndicator(); // ��������� ��������� ����� ��������� �����.
    }

    // ���������� ��������� ��� ������ ����� ������ ��������� �����.
    public void ShowNextWaveSpawnIndicator()
    {
        if (spawnIndicator == null || waveData == null)
        {
            Debug.LogWarning("WaveManager: ����������� spawnIndicator ��� waveData!");
            return;
        }

        Vector3 spawnPos = GetNextWaveSpawnPoint();
        if (spawnPos != Vector3.zero)
            ShowSpawnIndicator(spawnPos);
        else
            HideSpawnIndicator();
    }

    // ���������� ������ ����� ������ ��� ��������� �����.
    private Vector3 GetNextWaveSpawnPoint()
    {
        if (waveData == null || waveData.Waves == null)
        {
            Debug.LogWarning("WaveManager: waveData ��� Waves �� ���������!");
            return Vector3.zero;
        }

        if (CurrentWaveIndex < waveData.Waves.Count)
        {
            var wave = waveData.Waves[CurrentWaveIndex];
            if (!wave.UseCircleSpawn)
            {
                var spawnPoints = GetSpawnPoints(wave);
                return spawnPoints.Count > 0 ? spawnPoints[0] : (EnemyManager.Instance != null ? EnemyManager.Instance.GetRandomSpawnPoint().position : Vector3.zero);
            }
            // ��� ��������� ������ ���������� ����� ��� ��������� ����� �� �������.
            return Random.insideUnitCircle.normalized * wave.CircleSpawnRadius;
        }
        else if (infiniteData != null && infiniteData.SpawnType == SpawnType.SequentialFronts && infiniteData.SpawnFrontPrefab != null)
        {
            var points = GetSpawnPointsFromFront(infiniteData.SpawnFrontPrefab);
            if (points.Count > 0)
            {
                int frontIndex = (CurrentWaveIndex - waveData.Waves.Count) % points.Count;
                return points[frontIndex]?.position ?? transform.position;
            }
        }
        return EnemyManager.Instance != null ? EnemyManager.Instance.GetRandomSpawnPoint().position : transform.position;
    }

    // ���������� ��������� ������ � ��������� �������.
    private void ShowSpawnIndicator(Vector3 spawnPos)
    {
        if (spawnIndicator == null) return;
        spawnIndicator.SetActive(true);
        spawnIndicator.transform.position = spawnPos + Vector3.up * 2f; // ��������� ��������� ��� �����.
    }

    // �������� ��������� ������.
    private void HideSpawnIndicator()
    {
        if (spawnIndicator != null)
            spawnIndicator.SetActive(false);
    }

    // ������� ������ ������� �����.
    private IEnumerator SpawnWave(Wave wave)
    {
        if (wave.Enemies.Count == 0) yield break;

        var spawnPoints = GetSpawnPoints(wave);

        foreach (var config in wave.Enemies)
        {
            for (int i = 0; i < config.Count; i++)
            {
                Vector3 spawnPos = wave.UseCircleSpawn
                    ? Random.insideUnitCircle.normalized * wave.CircleSpawnRadius
                    : spawnPoints.Count > 0 ? spawnPoints[Random.Range(0, spawnPoints.Count)] : EnemyManager.Instance.GetRandomSpawnPoint().position;

                // ���������� ��������� ����� �������.
                ShowSpawnIndicator(spawnPos);

                SpawnEnemy(config.EnemyPrefab, AdjustSpawnPosition(spawnPos));
                float interval = config.UseCurve && config.IntervalCurve != null
                    ? config.IntervalCurve.Evaluate((float)i / config.Count)
                    : config.Interval;
                yield return new WaitForSeconds(interval);
            }
        }
        isSpawning = false;
        HideSpawnIndicator(); // �������� ��������� ����� ������.
    }

    // ������� ������ ����������� �����.
    private IEnumerator SpawnInfiniteWave(int waveNumber)
    {
        if (infiniteData?.Enemies == null || infiniteData.Enemies.Count == 0) yield break;

        var spawnPoints = GetInfiniteSpawnPoints();

        foreach (var template in infiniteData.Enemies)
        {
            if (waveNumber < template.StartWave) continue;

            int count = Mathf.RoundToInt(template.BaseCount + template.CountGrowthRate * waveNumber);
            float interval = Mathf.Max(template.BaseInterval * (1 - template.IntervalReductionRate * waveNumber), 0.5f);

            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos = GetInfiniteSpawnPoint(waveNumber, spawnPoints);
                ShowSpawnIndicator(spawnPos); // ���������� ��������� ����� �������.
                SpawnEnemy(template.EnemyPrefab, AdjustSpawnPosition(spawnPos));
                yield return new WaitForSeconds(interval);
            }
        }
        isSpawning = false;
        HideSpawnIndicator(); // �������� ��������� ����� ������.
    }

    // �������� ����� ������ ��� ������� �����.
    private List<Vector3> GetSpawnPoints(Wave wave)
    {
        if (wave.UseCircleSpawn) return new List<Vector3>();

        if (wave.UseRandomFronts || wave.SpawnFrontPrefab == null)
            return GetGlobalSpawnPoints();

        var points = GetSpawnPointsFromFront(wave.SpawnFrontPrefab);
        if (points.Count == 0) return GetGlobalSpawnPoints();

        var spawnPoints = new List<Vector3>();
        if (wave.SpawnPointIndices != null && wave.SpawnPointIndices.Count > 0)
        {
            foreach (int index in wave.SpawnPointIndices)
            {
                if (index >= 0 && index < points.Count && points[index] != null)
                    spawnPoints.Add(points[index].position);
            }
        }
        else
        {
            foreach (var point in points)
            {
                if (point != null) spawnPoints.Add(point.position);
            }
        }
        return spawnPoints.Count > 0 ? spawnPoints : new List<Vector3> { transform.position };
    }

    // �������� ���������� ����� ������.
    private List<Vector3> GetGlobalSpawnPoints()
    {
        var spawnPoints = new List<Vector3>();
        var points = EnemyManager.Instance?.SpawnPoints;
        if (points == null || points.Length == 0)
            spawnPoints.Add(transform.position);
        else
        {
            foreach (var point in points)
            {
                if (point != null) spawnPoints.Add(point.position);
            }
        }
        return spawnPoints;
    }

    // �������� ����� ������ ��� ����������� �����.
    private List<Transform> GetInfiniteSpawnPoints()
    {
        if (infiniteData?.SpawnType != SpawnType.SequentialFronts || infiniteData.SpawnFrontPrefab == null)
            return new List<Transform>();
        return GetSpawnPointsFromFront(infiniteData.SpawnFrontPrefab);
    }

    // ������ ����� ������ � ���������� ��� �����.
    private List<Transform> GetSpawnPointsFromFront(GameObject frontPrefab)
    {
        if (currentFrontObj != null) DestroyImmediate(currentFrontObj);
        currentFrontObj = Instantiate(frontPrefab, Vector3.zero, Quaternion.identity);
        currentFrontObj.SetActive(false);
        return currentFrontObj.TryGetComponent<SpawnPointMap>(out var map) ? map.GetSpawnPoints() : new List<Transform>();
    }

    // �������� ����� ������ ��� ����������� �����.
    private Vector3 GetInfiniteSpawnPoint(int waveNumber, List<Transform> spawnPoints)
    {
        if (infiniteData.SpawnType == SpawnType.Circle)
            return new Vector3(Random.insideUnitCircle.normalized.x * infiniteData.CircleRadius, 0, Random.insideUnitCircle.normalized.y * infiniteData.CircleRadius);

        return spawnPoints.Count > 0
            ? spawnPoints[(waveNumber - 1) % spawnPoints.Count]?.position ?? transform.position
            : transform.position;
    }

    // ������������ ������� ������ (��������� ������� � ��������� NavMesh).
    private Vector3 AdjustSpawnPosition(Vector3 spawnPos)
    {
        if (spawnRadius > 0)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            spawnPos += new Vector3(offset.x, 0, offset.y);
        }

        if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out var hit, spawnRadius, UnityEngine.AI.NavMesh.AllAreas))
            spawnPos = hit.position;

        Plane groundPlane = new Plane(Vector3.up, 0);
        if (groundPlane.Raycast(new Ray(spawnPos + Vector3.up * 100, Vector3.down), out float distance))
            spawnPos = new Ray(spawnPos + Vector3.up * 100, Vector3.down).GetPoint(distance);

        return spawnPos;
    }

    // ������� ����� � ��������� ��� � ������ ��������.
    private void SpawnEnemy(GameObject prefab, Vector3 position)
    {
        var enemy = EnemyManager.Instance.SpawnEnemy(prefab, position);
        if (enemy != null)
        {
            activeEnemies.Add(enemy);
            enemy.OnDeactivated.AddListener(EnemyDeactivated);
        }
    }

    // ������� ����� �� ������ ��������.
    private void EnemyDeactivated(EnemyBase enemy) => activeEnemies.Remove(enemy);

    // ��������� �����, ��������� �������, ��������� ����.
    private void EndWave()
    {
        isWaveActive = false;
        OnWaveEnded.Invoke();
        CurrentWaveIndex++;

        int reward = 0;
        if (isInfiniteMode)
            reward = Mathf.RoundToInt(infiniteData.BaseReward + infiniteData.RewardGrowthRate * (CurrentWaveIndex - waveData.Waves.Count));
        else if (CurrentWaveIndex > 0 && CurrentWaveIndex <= waveData.Waves.Count)
            reward = waveData.Waves[CurrentWaveIndex - 1].Reward;

        if (reward > 0)
            CurrencyManager.Instance.AddCurrency(reward);

        if (currentFrontObj != null)
        {
            Destroy(currentFrontObj);
            currentFrontObj = null;
        }

        SaveManager.Instance.SaveGame();
        ShowNextWaveSpawnIndicator(); // ���������� ����� ������ ��� ��������� �����.
    }
}