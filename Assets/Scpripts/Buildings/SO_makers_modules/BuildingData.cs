using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Buildings/BuildingData", order = 1)]
public class BuildingData : ScriptableObject
{
    public List<BuildingLevelData> Levels = new List<BuildingLevelData>();
    public List<BuildingModule> Modules = new List<BuildingModule>();
}

[System.Serializable]
public class BuildingLevelData
{
    public int Cost; // Стоимость уровня/покупки
    public int MaxHealth = 100; // Здоровье
}
