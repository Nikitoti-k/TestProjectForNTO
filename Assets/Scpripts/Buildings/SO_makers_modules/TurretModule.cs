using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TurretModule", menuName = "Buildings/Modules/TurretModule", order = 2)]
public class TurretModule : BuildingModule
{
    public List<TurretLevelData> LevelData = new List<TurretLevelData>();
}

[System.Serializable]
public class TurretLevelData
{
    public float Damage;
    public float FireRate; // ��������� � �������
    public float Range;
    public GameObject ProjectilePrefab; // ������ ������������

    // �����: ������ ������ ��� ����� ������ (assign � ����������)
    public GameObject ModelPrefab;
}