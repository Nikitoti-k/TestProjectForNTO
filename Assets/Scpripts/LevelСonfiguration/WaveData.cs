using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Waves/WaveData", order = 1)]
public class WaveData : ScriptableObject
{
    public List<Wave> Waves = new List<Wave>();
}

[System.Serializable]
public class Wave
{
    public List<EnemyConfig> Enemies = new List<EnemyConfig>();
    public GameObject SpawnFrontPrefab; // Prefab SpawnPointMap
    public List<int> SpawnPointIndices; // Индексы точек из SpawnPointMap
    public bool UseRandomFronts; // Если true, рандом из глобального
    public bool UseCircleSpawn; // Если true, спавн на круге вокруг (0,0,0)
    public float CircleSpawnRadius; // Радиус круга
    public int Reward; // Деньги за волну
}

[System.Serializable]
public class EnemyConfig
{
    public GameObject EnemyPrefab; // Prefab врага
    public int Count; // Кол-во
    public float Interval; // Базовый интервал
    public AnimationCurve IntervalCurve; // Кривую
    public bool UseCurve; // Использовать кривую
}