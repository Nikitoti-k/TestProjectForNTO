// ћодуль стены, хранит параметры брони дл€ уровней
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WallModule", menuName = "Buildings/Modules/WallModule", order = 4)]
public class WallModule : BuildingModule, IUpgradeParameterProvider
{
    [SerializeField] private string armorBonusDisplayName = "Armor Bonus";
    public List<WallLevelData> LevelData = new List<WallLevelData>();

    public string ArmorBonusDisplayName => armorBonusDisplayName;

    public List<string> GetUpgradeParameters(int currentLevel)
    {
        List<string> parameters = new List<string>();
        if (currentLevel < LevelData.Count - 1)
        {
            var currentData = LevelData[currentLevel];
            var nextData = LevelData[currentLevel + 1];
            if (!string.IsNullOrEmpty(ArmorBonusDisplayName))
                parameters.Add($"{ArmorBonusDisplayName}: {currentData.ArmorBonus} -> {nextData.ArmorBonus}");
        }
        return parameters;
    }

    public List<string> GetCurrentParameters(int currentLevel)
    {
        List<string> parameters = new List<string>();
        if (currentLevel < LevelData.Count)
        {
            var currentData = LevelData[currentLevel];
            if (!string.IsNullOrEmpty(ArmorBonusDisplayName))
                parameters.Add($"{ArmorBonusDisplayName}: {currentData.ArmorBonus}");
        }
        return parameters;
    }
}

[System.Serializable]
public class WallLevelData
{
    public float ArmorBonus;
}