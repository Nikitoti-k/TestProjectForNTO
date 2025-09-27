// ������ ������ � ������: �����, �����, �������.
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
    public GameObject SpawnFrontPrefab; // Prefab � SpawnPointMap.
    public List<int> SpawnPointIndices; // ������� ����� ������.
    public bool UseRandomFronts; // ������ �� ���������� �����.
    public bool UseCircleSpawn; // ����� �� �����.
    public float CircleSpawnRadius; // ������ �����.
    public int Reward; // ������� �� �����.
}

[System.Serializable]
public class EnemyConfig
{
    public GameObject EnemyPrefab; // Prefab �����.
    public int Count; // ���-�� ������.
    public float Interval; // �������� ������.
    public AnimationCurve IntervalCurve; // ������ ���������.
    public bool UseCurve; // ������������ ������.
}