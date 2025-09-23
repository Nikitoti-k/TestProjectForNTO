using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FactoryModule", menuName = "Buildings/Modules/FactoryModule", order = 4)]
public class FactoryModule : BuildingModule
{
    public List<FactoryLevelData> LevelData = new List<FactoryLevelData>();
}

[System.Serializable]
public class FactoryLevelData
{
    public int MoneyPerWave; // Деньги после волны
}