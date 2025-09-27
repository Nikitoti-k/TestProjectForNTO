using System.Collections.Generic;

// Данные для сохранения игры.
[System.Serializable]
public class SaveData
{
    public int waveIndex; // Текущая волна.
    public int currency; // Количество валюты.
    public List<BuildingPlacement> buildings; // Список зданий.
}

// Данные о размещённом здании.
[System.Serializable]
public class BuildingPlacement
{
    public int q, r; // Координаты клетки (q, r).
    public string buildingName; // Название здания.
    public int level; // Уровень здания.
}