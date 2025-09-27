using System.Collections.Generic;
using UnityEngine;
// SO под задаваемые волны. Да, в ТЗ написано только про бесконечный режим, но решил добавить для гибкости общей системы. Можешь задавать и балансить вручную первые волны, а потом подрубать бесконечнй режим.
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
    public GameObject SpawnFrontPrefab;
    public List<int> SpawnPointIndices;
    public bool UseRandomFronts;
    public bool UseCircleSpawn; 
    public float CircleSpawnRadius;
    public int Reward;
}

[System.Serializable]
public class EnemyConfig
{
    public GameObject EnemyPrefab; 
    public int Count; 
    public float Interval;
}