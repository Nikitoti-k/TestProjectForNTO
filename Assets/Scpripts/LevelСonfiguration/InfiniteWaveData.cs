// Хранит данные для бесконечных волн: враги, спавн, награда + изменение этих параметров 
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InfiniteWaveData", menuName = "Waves/InfiniteWaveData", order = 2)]
public class InfiniteWaveData : ScriptableObject
{
    [System.Serializable]
    public class EnemyTemplate
    {
        public GameObject EnemyPrefab; 
        public int BaseCount = 10;
        public float CountGrowthRate = 0.1f; 
        public int StartWave = 1; 
        public float BaseInterval = 2f; 
        public float IntervalReductionRate = 0.05f; 
    }

    public List<EnemyTemplate> Enemies = new List<EnemyTemplate>();
    public int BaseReward = 100;
    public float RewardGrowthRate = 0.2f; 
    public SpawnType SpawnType = SpawnType.Circle;
    public float CircleRadius = 10f; 
    public GameObject SpawnFrontPrefab;

    private void OnValidate()
    {
        foreach (var enemy in Enemies)
        {
            if (enemy.EnemyPrefab == null)
                Debug.LogWarning("InfiniteWaveData: пропущен EnemyPrefab!");
        }
        if (SpawnType == SpawnType.SequentialFronts && SpawnFrontPrefab == null)
            Debug.LogWarning("InfiniteWaveData: пропущен SpawnFrontPrefab для SequentialFronts!");
    }
}

public enum SpawnType
{
    Circle, // Спавн на круге.
    SequentialFronts // Спавнпоинты по очереди.
}