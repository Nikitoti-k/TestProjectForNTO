using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TurretData", menuName = "Buildings/TurretData", order = 1)]
public class TurretData : ScriptableObject
{
    public List<TurretLevelData> Levels = new List<TurretLevelData>();
}

[System.Serializable]
public class TurretLevelData
{
    public int Cost; // Возвращено: стоимость уровня
    public float Damage;
    public float FireRate; // Выстрелов в секунду
    public float Range;
    public int MaxHealth = 100; // Добавлено: здоровье по уровню
}