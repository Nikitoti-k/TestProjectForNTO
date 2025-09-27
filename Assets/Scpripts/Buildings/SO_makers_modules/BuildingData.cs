// ’ранит данные о постройке: название, параметры уровней и модули
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Buildings/BuildingData", order = 1)]
public class BuildingData : ScriptableObject
{
    [SerializeField] private string name = "Building";
    [SerializeField] private string maxHPDisplayName = "Max HP";
    [SerializeField] private string costDisplayName = "Cost";
    public List<BuildingLevelData> Levels = new List<BuildingLevelData>();
    public List<BuildingModule> Modules = new List<BuildingModule>();

    public string Name => name;
    public string MaxHPDisplayName => maxHPDisplayName;
    public string CostDisplayName => costDisplayName;
}

[System.Serializable]
public class BuildingLevelData
{
    public int Cost;
    public int MaxHealth;
    public GameObject Model_view; // ћожем измен€ть модель здани€ в зависимости от уровн€
}