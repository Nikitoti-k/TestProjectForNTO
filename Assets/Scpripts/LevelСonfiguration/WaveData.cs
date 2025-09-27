// Хранит данные о волнах: враги, спавн, награда.
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Waves/WaveData", order = 1)]
public class WaveData : ScriptableObject
{
    public List<Wave> Waves = new List<Wave>();

    private void OnValidate()
    {
        foreach (var wave in Waves)
        {
            foreach (var enemy in wave.Enemies)
            {
                if (enemy.EnemyPrefab == null)
                    Debug.LogWarning($"WaveData: Missing EnemyPrefab in wave {Waves.IndexOf(wave) + 1}");
            }
            if (!wave.UseCircleSpawn && !wave.UseRandomFronts && wave.SpawnFrontPrefab == null)
                Debug.LogWarning($"WaveData: Missing SpawnFrontPrefab in wave {Waves.IndexOf(wave) + 1}");
        }
    }
}

[System.Serializable]
public class Wave
{
    public List<EnemyConfig> Enemies = new List<EnemyConfig>();
    public GameObject SpawnFrontPrefab; // Prefab с SpawnPointMap.
    public List<int> SpawnPointIndices; // Индексы точек спавна.
    public bool UseRandomFronts; // Рандом из глобальных точек.
    public bool UseCircleSpawn; // Спавн на круге.
    public float CircleSpawnRadius; // Радиус круга.
    public int Reward; // Награда за волну.
}

[System.Serializable]
public class EnemyConfig
{
    public GameObject EnemyPrefab; // Prefab врага.
    public int Count; // Кол-во врагов.
    public float Interval; // Интервал спавна.
    public AnimationCurve IntervalCurve; // Кривую интервала.
    public bool UseCurve; // Использовать кривую.
}