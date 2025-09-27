// Хранит данные для бесконечных волн: враги, спавн, награда.
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InfiniteWaveData", menuName = "Waves/InfiniteWaveData", order = 2)]
public class InfiniteWaveData : ScriptableObject
{
    [System.Serializable]
    public class EnemyTemplate
    {
        public GameObject EnemyPrefab; // Prefab врага.
        public int BaseCount = 10; // Начальное кол-во.
        public float CountGrowthRate = 0.1f; // Прирост кол-ва.
        public int StartWave = 1; // Волна появления.
        public float BaseInterval = 2f; // Начальный интервал.
        public float IntervalReductionRate = 0.05f; // Уменьшение интервала.
    }

    public List<EnemyTemplate> Enemies = new List<EnemyTemplate>();
    public int BaseReward = 100; // Начальная награда.
    public float RewardGrowthRate = 0.2f; // Прирост награды.
    public SpawnType SpawnType = SpawnType.Circle; // Тип спавна.
    public float CircleRadius = 10f; // Радиус круга.
    public GameObject SpawnFrontPrefab; // Prefab фронта с SpawnPointMap.

    private void OnValidate()
    {
        foreach (var enemy in Enemies)
        {
            if (enemy.EnemyPrefab == null)
                Debug.LogWarning("InfiniteWaveData: Missing EnemyPrefab!");
        }
        if (SpawnType == SpawnType.SequentialFronts && SpawnFrontPrefab == null)
            Debug.LogWarning("InfiniteWaveData: Missing SpawnFrontPrefab for SequentialFronts!");
    }
}

public enum SpawnType
{
    Circle, // Спавн на круге.
    SequentialFronts // Фронты по очереди.
}