using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WallModule", menuName = "Buildings/Modules/WallModule", order = 3)]
public class HadquartersModule : BuildingModule
{
    public List<HadquartersModule> LevelData = new List<HadquartersModule>();
}

[System.Serializable]
public class HeadquartersModule
{
    // Если ничего уникального, оставь пустым
    // Или добавь, например, бонус к защите
    public float ArmorBonus; // Пример: усиление брони
}