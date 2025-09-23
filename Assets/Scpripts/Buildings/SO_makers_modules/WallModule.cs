using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WallModule", menuName = "Buildings/Modules/WallModule", order = 3)]
public class WallModule : BuildingModule
{
    public List<WallLevelData> LevelData = new List<WallLevelData>();
}

[System.Serializable]
public class WallLevelData
{
    // Если ничего уникального, оставь пустым
    // Или добавь, например, бонус к защите
    public float ArmorBonus; // Пример: усиление брони
}