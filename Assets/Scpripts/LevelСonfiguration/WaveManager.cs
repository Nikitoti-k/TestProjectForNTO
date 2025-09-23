using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [SerializeField] private WaveData waveData; // Фиксированные волны
    [SerializeField] private InfiniteWaveData infiniteData; // Бесконечный режим
    [SerializeField] private float spawnRadius = 2f; // Радиус спавна для всех волн
    [SerializeField] private GameObject spawnIndicatorPrefab; // Prefab зелёной стрелки
    public UnityEvent OnWaveStarted;
    public UnityEvent OnWaveEnded;

    private int currentWaveIndex = 0;
    private List<EnemyBase> activeEnemies = new List<EnemyBase>();
    private bool isWaveActive = false;
    private bool isInfiniteMode = false;
    private GameObject currentFrontObj; // Храним prefab фронта
    private GameObject currentSpawnIndicator; // Храним стрелку

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
        if (spawnIndicatorPrefab != null)
        {
            currentSpawnIndicator = Instantiate(spawnIndicatorPrefab, Vector3.zero, Quaternion.identity);
            currentSpawnIndicator.SetActive(false); // Скрываем до первого показа
            ShowSpawnIndicator(); // Показываем для первой волны
        }
    }

    public void StartNextWave()
    {
        if (isWaveActive)
        {
            Debug.LogWarning("Cannot start wave: already active!");
            return;
        }

        isWaveActive = true;
        // Показываем стрелку до начала волны
        ShowSpawnIndicator();
        if (currentWaveIndex < waveData.Waves.Count)
        {
            Wave wave = waveData.Waves[currentWaveIndex];
            StartCoroutine(SpawnWave(wave));
        }
        else
        {
            isInfiniteMode = true;
            int infiniteWaveNumber = currentWaveIndex - waveData.Waves.Count + 1;
            StartCoroutine(SpawnInfiniteWave(infiniteWaveNumber));
        }
        OnWaveStarted.Invoke();
        Debug.Log($"Wave {currentWaveIndex + 1} started!");
    }

    private void ShowSpawnIndicator()
    {
        if (currentSpawnIndicator == null) return;

        Vector3 indicatorPos = Vector3.zero;
        bool showIndicator = false;

        if (currentWaveIndex < waveData.Waves.Count)
        {
            Wave wave = waveData.Waves[currentWaveIndex];
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
        else
        {
            int infiniteWaveNumber = currentWaveIndex - waveData.Waves.Count + 1;
            if (infiniteData.SpawnType == SpawnType.SequentialFronts && infiniteData.SpawnFrontPrefab != null)
            {
                GameObject tempFrontObj = Instantiate(infiniteData.SpawnFrontPrefab, Vector3.zero, Quaternion.identity);
                tempFrontObj.SetActive(false);
                SpawnPointMap map = tempFrontObj.GetComponent<SpawnPointMap>();
                List<Transform> points = map != null ? map.GetSpawnPoints() : new List<Transform>();
                if (points.Count > 0)
                {
                    int frontIndex = (infiniteWaveNumber - 1) % points.Count;
                    indicatorPos = points[frontIndex].position;
                    showIndicator = true;
                }
                Destroy(tempFrontObj);
            }
        }

        if (showIndicator)
        {
            indicatorPos.y += 2f; // Смещение над землёй
            currentSpawnIndicator.transform.position = indicatorPos;
            currentSpawnIndicator.transform.rotation = Quaternion.identity;
            currentSpawnIndicator.SetActive(true);
        }
        else
        {
            currentSpawnIndicator.SetActive(false);
        }
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        // Скрываем стрелку в начале спавна
        if (currentSpawnIndicator != null)
        {
            currentSpawnIndicator.SetActive(false);
        }

        List<Vector3> spawnPoints = GetSpawnPoints(wave);
        for (int typeIndex = 0; typeIndex < wave.Enemies.Count; typeIndex++)
        {
            EnemyConfig config = wave.Enemies[typeIndex];
            for (int spawnIndex = 0; spawnIndex < config.Count; spawnIndex++)
            {
                Vector3 spawnPos;
                if (wave.UseCircleSpawn)
                {
                    Vector2 circlePoint = Random.insideUnitCircle.normalized * wave.CircleSpawnRadius;
                    spawnPos = new Vector3(circlePoint.x, 0, circlePoint.y);
                }
                else
                {
                    spawnPos = spawnPoints.Count > 0 ? spawnPoints[Random.Range(0, spawnPoints.Count)] : EnemyManager.Instance.GetRandomSpawnPoint().position;
                }

                if (spawnRadius > 0)
                {
                    Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
                    spawnPos += new Vector3(randomOffset.x, 0, randomOffset.y);
                    Plane groundPlane = new Plane(Vector3.up, 0);
                    Ray ray = new Ray(spawnPos + Vector3.up * 100, Vector3.down);
                    if (groundPlane.Raycast(ray, out float distance))
                    {
                        spawnPos = ray.GetPoint(distance);
                    }
                }

                EnemyBase enemy = EnemyManager.Instance.SpawnEnemy(config.EnemyPrefab, spawnPos);
                if (enemy != null)
                {
                    activeEnemies.Add(enemy);
                    enemy.OnDeactivated.AddListener(EnemyDeactivated);
                }
                else
                {
                    Debug.LogWarning($"Failed to spawn enemy {config.EnemyPrefab.name}!");
                }

                float interval = config.Interval;
                if (config.UseCurve && config.IntervalCurve != null)
                {
                    float progress = (float)spawnIndex / config.Count;
                    interval = config.IntervalCurve.Evaluate(progress);
                }

                yield return new WaitForSeconds(interval);
            }
        }
    }

    private IEnumerator SpawnInfiniteWave(int infiniteWaveNumber)
    {
        // Скрываем стрелку в начале спавна
        if (currentSpawnIndicator != null)
        {
            currentSpawnIndicator.SetActive(false);
        }

        List<Transform> spawnPoints = null;
        if (infiniteData.SpawnType == SpawnType.SequentialFronts && infiniteData.SpawnFrontPrefab != null)
        {
            currentFrontObj = Instantiate(infiniteData.SpawnFrontPrefab, Vector3.zero, Quaternion.identity);
            currentFrontObj.SetActive(false); // Hidden
            SpawnPointMap map = currentFrontObj.GetComponent<SpawnPointMap>();
            spawnPoints = map != null ? map.GetSpawnPoints() : new List<Transform>();
        }

        for (int typeIndex = 0; typeIndex < infiniteData.Enemies.Count; typeIndex++)
        {
            InfiniteWaveData.EnemyTemplate template = infiniteData.Enemies[typeIndex];
            if (infiniteWaveNumber < template.StartWave) continue;

            int count = Mathf.RoundToInt(template.BaseCount + template.CountGrowthRate * infiniteWaveNumber);
            float interval = template.BaseInterval * (1 - template.IntervalReductionRate * infiniteWaveNumber);
            interval = Mathf.Max(interval, 0.5f); // Минимум интервала

            for (int spawnIndex = 0; spawnIndex < count; spawnIndex++)
            {
                Vector3 spawnPos = GetInfiniteSpawnPoint(infiniteData, infiniteWaveNumber, spawnPoints);
                if (spawnRadius > 0)
                {
                    Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
                    spawnPos += new Vector3(randomOffset.x, 0, randomOffset.y);
                    Plane groundPlane = new Plane(Vector3.up, 0);
                    Ray ray = new Ray(spawnPos + Vector3.up * 100, Vector3.down);
                    if (groundPlane.Raycast(ray, out float distance))
                    {
                        spawnPos = ray.GetPoint(distance);
                    }
                }

                EnemyBase enemy = EnemyManager.Instance.SpawnEnemy(template.EnemyPrefab, spawnPos);
                if (enemy != null)
                {
                    activeEnemies.Add(enemy);
                    enemy.OnDeactivated.AddListener(EnemyDeactivated);
                }
                else
                {
                    Debug.LogWarning($"Failed to spawn enemy {template.EnemyPrefab.name}!");
                }

                yield return new WaitForSeconds(interval);
            }
        }

        int reward = Mathf.RoundToInt(infiniteData.BaseReward + infiniteData.RewardGrowthRate * infiniteWaveNumber);
        CurrencyManager.Instance.AddCurrency(reward);
    }

    private List<Vector3> GetSpawnPoints(Wave wave)
    {
        List<Vector3> spawnPoints = new List<Vector3>();

        if (wave.UseCircleSpawn)
        {
            // Пустой список, точки генерируются в SpawnWave
            return spawnPoints;
        }

        if (wave.UseRandomFronts)
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
                    if (point != null)
                        spawnPoints.Add(point.position);
                }
            }
            return spawnPoints;
        }

        if (wave.SpawnFrontPrefab == null)
        {
            Debug.LogWarning("SpawnFrontPrefab not assigned! Falling back to global spawn points.");
            if (EnemyManager.Instance.SpawnPoints == null || EnemyManager.Instance.SpawnPoints.Length == 0)
            {
                spawnPoints.Add(transform.position);
            }
            else
            {
                foreach (Transform point in EnemyManager.Instance.SpawnPoints)
                {
                    if (point != null)
                        spawnPoints.Add(point.position);
                }
            }
            return spawnPoints;
        }

        currentFrontObj = Instantiate(wave.SpawnFrontPrefab, Vector3.zero, Quaternion.identity);
        currentFrontObj.SetActive(false); // Hidden
        SpawnPointMap map = currentFrontObj.GetComponent<SpawnPointMap>();
        List<Transform> points = map != null ? map.GetSpawnPoints() : new List<Transform>();

        if (points.Count == 0)
        {
            Debug.LogWarning($"No spawn points in {wave.SpawnFrontPrefab.name}! Using global.");
            if (EnemyManager.Instance.SpawnPoints == null || EnemyManager.Instance.SpawnPoints.Length == 0)
            {
                spawnPoints.Add(transform.position);
            }
            else
            {
                foreach (Transform point in EnemyManager.Instance.SpawnPoints)
                {
                    if (point != null)
                        spawnPoints.Add(point.position);
                }
            }
            return spawnPoints;
        }

        // Фильтруем по индексам
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
                if (point != null)
                    spawnPoints.Add(point.position);
            }
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning($"No valid spawn points for wave! Using WaveManager transform.");
            spawnPoints.Add(transform.position);
        }

        return spawnPoints;
    }

    private Vector3 GetInfiniteSpawnPoint(InfiniteWaveData data, int infiniteWaveNumber, List<Transform> spawnPoints)
    {
        Vector3 spawnPos;
        if (data.SpawnType == SpawnType.Circle)
        {
            Vector2 circlePoint = Random.insideUnitCircle.normalized * data.CircleRadius;
            spawnPos = new Vector3(circlePoint.x, 0, circlePoint.y);
        }
        else // SequentialFronts
        {
            if (data.SpawnFrontPrefab == null || spawnPoints == null || spawnPoints.Count == 0)
            {
                Debug.LogWarning("SpawnFrontPrefab not assigned or no spawn points in InfiniteWaveData! Using WaveManager transform.");
                spawnPos = transform.position;
            }
            else
            {
                int frontIndex = (infiniteWaveNumber - 1) % spawnPoints.Count; // -1 для коррекции (волна 1 = индекс 0)
                Transform front = spawnPoints[frontIndex];
                spawnPos = front != null ? front.position : transform.position;
            }
        }

        return spawnPos;
    }

    private void EnemyDeactivated(EnemyBase enemy)
    {
        activeEnemies.Remove(enemy);
        if (activeEnemies.Count == 0 && isWaveActive)
        {
            isWaveActive = false;
            OnWaveEnded.Invoke();
            currentWaveIndex++;
            if (currentFrontObj != null)
            {
                Destroy(currentFrontObj);
                currentFrontObj = null;
            }
            // Показываем стрелку для следующей волны
            ShowSpawnIndicator();
        }
    }
}