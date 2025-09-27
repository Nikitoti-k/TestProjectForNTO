using System.Collections.Generic;

// Данные для сохранения игры
[System.Serializable]
public class SaveData
{
    public int waveIndex; // Текущая волныы
    public int currency; // Количество валюты
    public List<BuildingPlacement> buildings; // Список зданий
}
[System.Serializable]
public class BuildingPlacement
{
    public int q, r; // Координаты гекса
    public string buildingName; // Название здания
    public int level; // Уровень здания
}