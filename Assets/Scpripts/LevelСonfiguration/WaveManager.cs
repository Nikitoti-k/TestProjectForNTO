using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using System.Collections;

// Управляет волнами врагов: спавн, отслеживание, завершение. Автоматический переход на бесконечный режим, если не задана WaweData - тоже сразу переключаемся на бесконечный
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }
    public int CurrentWaveIndex { get; private set; }
    public bool IsWaveActive { get { return isWaveActive; } }

    [SerializeField] private WaveData waveData;
    [SerializeField] private InfiniteWaveData infiniteData;
    [SerializeField] private GameSceneConfiguration sceneSettings;
    [SerializeField] private GameObject spawnIndicatorPrefab;
    public UnityEvent OnWaveStarted;
    public UnityEvent OnWaveEnded;

    private readonly List<EnemyBase> activeEnemies = new List<EnemyBase>();
    private bool isWaveActive;
    private bool isSpawning;
    private bool isInfiniteMode;
    private GameObject currentFrontObj;
    private GameObject spawnIndicator;

  
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    void Start()
    {
        if (spawnIndicatorPrefab == null)
        {
            Debug.LogError("WaveManager: Отсутствует spawnIndicatorPrefab!");
            return;
        }
        if (sceneSettings == null)
        {
            Debug.LogError("WaveManager: GameSceneConfiguration не назначен!");
            return;
        }
        if (infiniteData == null)
        {
            Debug.LogWarning("WaveManager: InfiniteWaveData не назначен, бесконечный режим будет недоступен!");
        }
        spawnIndicator = Instantiate(spawnIndicatorPrefab, Vector3.zero, Quaternion.identity);
        spawnIndicator.SetActive(false);
        ShowNextWaveSpawnIndicator();
    }


    void FixedUpdate()
    {
        if (isWaveActive && !isSpawning && activeEnemies.Count == 0)
            EndWave();
    }


    public void StartNextWave()
    {
        if (isWaveActive)
        {
            Debug.LogWarning("WaveManager: Волна уже активна!");
            return;
        }

        isWaveActive = true;
        isSpawning = true;
        OnWaveStarted.Invoke();

        if (waveData != null && waveData.Waves.Count > 0 && CurrentWaveIndex < waveData.Waves.Count)
        {
            StartCoroutine(SpawnWave(waveData.Waves[CurrentWaveIndex]));
            Debug.Log($"Волна {CurrentWaveIndex + 1} началась (обычная)!");
        }
        else
        {
            if (infiniteData == null)
            {
                Debug.LogError("WaveManager: InfiniteWaveData не назначен, не могу запустить бесконечную волну!");
                isWaveActive = false;
                isSpawning = false;
                return;
            }
            isInfiniteMode = true;
            StartCoroutine(SpawnInfiniteWave(CurrentWaveIndex - (waveData != null ? waveData.Waves.Count : 0) + 1));
            Debug.Log($"Волна {CurrentWaveIndex + 1} началась (бесконечная)!");
        }
    }

    // Устанавливает индекс волны
    public void SetWaveIndex(int index)
    {
        CurrentWaveIndex = Mathf.Max(0, index);
        ShowNextWaveSpawnIndicator();
    }


    public void ShowNextWaveSpawnIndicator()
    {
        if (spawnIndicator == null)
        {
            Debug.LogWarning("WaveManager: Отсутствует spawnIndicator!");
            return;
        }

        Vector3 spawnPos = GetNextWaveSpawnPoint();
        if (spawnPos != Vector3.zero)
            ShowSpawnIndicator(spawnPos);
        else
            HideSpawnIndicator();
    }


    private Vector3 GetNextWaveSpawnPoint()
    {
        if (waveData != null && waveData.Waves != null && waveData.Waves.Count > 0 && CurrentWaveIndex < waveData.Waves.Count)
        {
            var wave = waveData.Waves[CurrentWaveIndex];
            if (!wave.UseCircleSpawn)
            {
                var spawnPoints = GetSpawnPoints(wave);
                return spawnPoints.Count > 0 ? spawnPoints[0] : (EnemyManager.Instance != null ? EnemyManager.Instance.GetRandomSpawnPoint().position : Vector3.zero);
            }

            return Random.insideUnitCircle.normalized * wave.CircleSpawnRadius;
        }
        else if (infiniteData != null && infiniteData.SpawnType == SpawnType.SequentialFronts && infiniteData.SpawnFrontPrefab != null)
        {
            var points = GetSpawnPointsFromFront(infiniteData.SpawnFrontPrefab);
            if (points.Count > 0)
            {
                int frontIndex = (CurrentWaveIndex - (waveData != null ? waveData.Waves.Count : 0)) % points.Count;
                return points[frontIndex]?.position ?? transform.position;
            }
        }
        return EnemyManager.Instance != null ? EnemyManager.Instance.GetRandomSpawnPoint().position : transform.position;
    }


    private void ShowSpawnIndicator(Vector3 spawnPos)
    {
        if (spawnIndicator == null) return;
        spawnIndicator.SetActive(true);
        spawnIndicator.transform.position = spawnPos + Vector3.up * 2f;
    }


    private void HideSpawnIndicator()
    {
        if (spawnIndicator != null)
            spawnIndicator.SetActive(false);
    }

    // Спавнит врагов обычной волны
    private IEnumerator SpawnWave(Wave wave)
    {
        if (wave.Enemies.Count == 0) yield break;

        var spawnPoints = GetSpawnPoints(wave);

        foreach (var config in wave.Enemies)
        {
            if (config.EnemyPrefab == null)
            {
                Debug.LogWarning($"WaveManager: Пропущен EnemyPrefab в конфигурации волны {CurrentWaveIndex + 1}!");
                continue;
            }

            for (int i = 0; i < config.Count; i++)
            {
                Vector3 spawnPos = wave.UseCircleSpawn
                    ? Random.insideUnitCircle.normalized * wave.CircleSpawnRadius
                    : spawnPoints.Count > 0 ? spawnPoints[Random.Range(0, spawnPoints.Count)] : EnemyManager.Instance.GetRandomSpawnPoint().position;

                // Показываем индикатор перед спавном.
                ShowSpawnIndicator(spawnPos);

                SpawnEnemy(config.EnemyPrefab, AdjustSpawnPosition(spawnPos));
                yield return new WaitForSeconds(config.Interval);
            }
        }
        isSpawning = false;
        HideSpawnIndicator();
    }

    // Спавнит врагов бесконечной волны
    private IEnumerator SpawnInfiniteWave(int waveNumber)
    {
        if (infiniteData?.Enemies == null || infiniteData.Enemies.Count == 0) yield break;

        var spawnPoints = GetInfiniteSpawnPoints();

        foreach (var template in infiniteData.Enemies)
        {
            if (waveNumber < template.StartWave || template.EnemyPrefab == null) continue;

            int count = Mathf.RoundToInt(template.BaseCount + template.CountGrowthRate * waveNumber);
            float interval = Mathf.Max(template.BaseInterval * (1 - template.IntervalReductionRate * waveNumber), 0.5f);

            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos = GetInfiniteSpawnPoint(waveNumber, spawnPoints);
                ShowSpawnIndicator(spawnPos); // Показываем индикатор перед спавном.
                SpawnEnemy(template.EnemyPrefab, AdjustSpawnPosition(spawnPos));
                yield return new WaitForSeconds(interval);
            }
        }
        isSpawning = false;
        HideSpawnIndicator();
    }

    // Получает точки спавна для обычной волны
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

    // Получает точки спавна для бесконечной волны
    private List<Transform> GetInfiniteSpawnPoints()
    {
        if (infiniteData?.SpawnType != SpawnType.SequentialFronts || infiniteData.SpawnFrontPrefab == null)
            return new List<Transform>();
        return GetSpawnPointsFromFront(infiniteData.SpawnFrontPrefab);
    }

    // Создаёт фронт спавна и возвращает его точки
    private List<Transform> GetSpawnPointsFromFront(GameObject frontPrefab)
    {
        if (currentFrontObj != null) DestroyImmediate(currentFrontObj);
        currentFrontObj = Instantiate(frontPrefab, Vector3.zero, Quaternion.identity);
        currentFrontObj.SetActive(false);
        return currentFrontObj.TryGetComponent<SpawnPointMap>(out var map) ? map.GetSpawnPoints() : new List<Transform>();
    }


    private Vector3 GetInfiniteSpawnPoint(int waveNumber, List<Transform> spawnPoints)
    {
        if (infiniteData.SpawnType == SpawnType.Circle)
            return new Vector3(Random.insideUnitCircle.normalized.x * infiniteData.CircleRadius, 0, Random.insideUnitCircle.normalized.y * infiniteData.CircleRadius);

        return spawnPoints.Count > 0
            ? spawnPoints[(waveNumber - 1) % spawnPoints.Count]?.position ?? transform.position
            : transform.position;
    }


    private Vector3 AdjustSpawnPosition(Vector3 spawnPos)
    {
        if (sceneSettings == null)
        {
            Debug.LogError("WaveManager: GameSceneConfiguration не назначен, использую spawnRadius = 0!");
            return spawnPos;
        }

        if (sceneSettings.SpawnRadius > 0)
        {
            Vector2 offset = Random.insideUnitCircle * sceneSettings.SpawnRadius;
            spawnPos += new Vector3(offset.x, 0, offset.y);
        }

        if (NavMesh.SamplePosition(spawnPos, out var hit, sceneSettings.SpawnRadius, NavMesh.AllAreas))
            spawnPos = hit.position;

        Plane groundPlane = new Plane(Vector3.up, 0);
        if (groundPlane.Raycast(new Ray(spawnPos + Vector3.up * 100, Vector3.down), out float distance))
            spawnPos = new Ray(spawnPos + Vector3.up * 100, Vector3.down).GetPoint(distance);

        return spawnPos;
    }


    private void SpawnEnemy(GameObject prefab, Vector3 position)
    {
        if (prefab == null)
        {
            Debug.LogWarning("WaveManager: Пропущен префаб врага для спавна!");
            return;
        }

        var enemy = EnemyManager.Instance.SpawnEnemy(prefab, position);
        if (enemy != null)
        {
            activeEnemies.Add(enemy);
            enemy.OnDeactivated.AddListener(EnemyDeactivated);
        }
    }


    private void EnemyDeactivated(EnemyBase enemy) => activeEnemies.Remove(enemy);

    // Завершает волну, начисляет награду, сохраняет игру
    private void EndWave()
    {
        isWaveActive = false;
        OnWaveEnded.Invoke();
        CurrentWaveIndex++;

        int reward = 0;
        if (isInfiniteMode)
            reward = Mathf.RoundToInt(infiniteData.BaseReward + infiniteData.RewardGrowthRate * (CurrentWaveIndex - (waveData != null ? waveData.Waves.Count : 0)));
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
        ShowNextWaveSpawnIndicator();
    }
}