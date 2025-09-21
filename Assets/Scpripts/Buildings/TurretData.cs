using System.Collections.Generic;
using UnityEngine;
// ���� ������ ��� ��������� "������", ���� ��������� �������. ����� ���� ������ ����� � ������ ���������, ��� ������ ��������� � ����� ���������� - ����� ���������
[CreateAssetMenu(fileName = "TurretData", menuName = "Buildings/TurretData", order = 1)]
public class TurretData : ScriptableObject
{
    public List<TurretLevelData> Levels = new List<TurretLevelData>();
}

[System.Serializable]
public class TurretLevelData
{
    public int Cost;
    public float Damage;
    public float FireRate;
    public float Range;
}