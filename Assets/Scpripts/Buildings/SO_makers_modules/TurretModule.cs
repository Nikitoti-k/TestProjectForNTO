// Модуль турели, предоставляет параметры урона, скорострельности и дальности.
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TurretModule", menuName = "Buildings/Modules/TurretModule", order = 2)]
public class TurretModule : BuildingModule, IUpgradeParameterProvider
{
    [SerializeField] private string damageDisplayName = "Damage";
    [SerializeField] private string fireRateDisplayName = "Fire Rate";
    [SerializeField] private string rangeDisplayName = "Range";
    public List<TurretLevelData> LevelData = new List<TurretLevelData>();

    public string DamageDisplayName => damageDisplayName;
    public string FireRateDisplayName => fireRateDisplayName;
    public string RangeDisplayName => rangeDisplayName;

    public List<string> GetUpgradeParameters(int currentLevel)
    {
        if (currentLevel >= LevelData.Count - 1)
        {
            return new List<string>();
        }

        var currentData = LevelData[currentLevel];
        var nextData = LevelData[currentLevel + 1];
        List<string> parameters = new List<string>();
        if (!string.IsNullOrEmpty(DamageDisplayName))
        {
            parameters.Add($"{DamageDisplayName}: {currentData.Damage} -> {nextData.Damage}");
        }
        if (!string.IsNullOrEmpty(FireRateDisplayName))
        {
            parameters.Add($"{FireRateDisplayName}: {currentData.FireRate} -> {nextData.FireRate}");
        }
        if (!string.IsNullOrEmpty(RangeDisplayName))
        {
            parameters.Add($"{RangeDisplayName}: {currentData.Range} -> {nextData.Range}");
        }
        return parameters;
    }

    public List<string> GetCurrentParameters(int currentLevel)
    {
        if (currentLevel >= LevelData.Count)
        {
            return new List<string>();
        }

        var currentData = LevelData[currentLevel];
        List<string> parameters = new List<string>();
        if (!string.IsNullOrEmpty(DamageDisplayName))
        {
            parameters.Add($"{DamageDisplayName}: {currentData.Damage}");
        }
        if (!string.IsNullOrEmpty(FireRateDisplayName))
        {
            parameters.Add($"{FireRateDisplayName}: {currentData.FireRate}");
        }
        if (!string.IsNullOrEmpty(RangeDisplayName))
        {
            parameters.Add($"{RangeDisplayName}: {currentData.Range}");
        }
        return parameters;
    }
}

[System.Serializable]
public class TurretLevelData
{
    public float Damage;
    public float FireRate;
    public float Range;
    public GameObject ProjectilePrefab;
    public GameObject ModelPrefab;
}