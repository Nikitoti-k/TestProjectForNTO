// Хранит список доступных зданий для UI строительства.
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AccessibleBuildings", menuName = "Buildings/AccessibleBuildings", order = 2)]
public class AccessibleBuildings : ScriptableObject
{
    public List<BuildingInfo> Buildings = new List<BuildingInfo>(); // Список префабов и их данных.
}

[System.Serializable]
public class BuildingInfo
{
    public GameObject BuildingPrefab;
    public BuildingData BuildingData;
}