// ћодуль турели, предоставл€ет параметры урона, скорострельности и дальности.
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TurretModule", menuName = "Buildings/Modules/TurretModule", order = 2)]
public class TurretModule : BuildingModule, IUpgradeParameterProvider
{
    [SerializeField] private string damageDisplayName = "Damage";
    [SerializeField] private string fireRateDisplayName = "Fire Rate";
    [SerializeField] private string rangeDisplayName = "Range";
    [SerializeField] private string projectileSpeedDisplayName = "Projectile Speed"; // Ќовый: отображение дл€ скорости снар€да
    public List<TurretLevelData> LevelData = new List<TurretLevelData>();

    public string DamageDisplayName => damageDisplayName;
    public string FireRateDisplayName => fireRateDisplayName;
    public string RangeDisplayName => rangeDisplayName;
    public string ProjectileSpeedDisplayName => projectileSpeedDisplayName; // Ќовый: геттер дл€ отображени€

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
        if (!string.IsNullOrEmpty(ProjectileSpeedDisplayName))
        {
            parameters.Add($"{ProjectileSpeedDisplayName}: {currentData.ProjectileSpeed} -> {nextData.ProjectileSpeed}"); // Ќовый: добавлено отображение скорости
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
        if (!string.IsNullOrEmpty(ProjectileSpeedDisplayName))
        {
            parameters.Add($"{ProjectileSpeedDisplayName}: {currentData.ProjectileSpeed}"); // Ќовый: добавлено отображение скорости
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
    public float ProjectileSpeed = 10f; // Ќовый: скорость снар€да по умолчанию 10
    public GameObject ProjectilePrefab;
    public GameObject ModelPrefab;
}