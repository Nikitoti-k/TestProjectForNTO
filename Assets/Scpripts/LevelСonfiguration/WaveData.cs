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
    public List<int> SpawnPointIndices; // ������� ����� �� SpawnPointMap
    public bool UseRandomFronts; // ���� true, ������ �� �����������
    public bool UseCircleSpawn; // ���� true, ����� �� ����� ������ (0,0,0)
    public float CircleSpawnRadius; // ������ �����
    public int Reward; // ������ �� �����
}

[System.Serializable]
public class EnemyConfig
{
    public GameObject EnemyPrefab; // Prefab �����
    public int Count; // ���-��
    public float Interval; // ������� ��������
    public AnimationCurve IntervalCurve; // ������
    public bool UseCurve; // ������������ ������
}