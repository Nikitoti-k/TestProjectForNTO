// Хранит список доступных построек для интерфейса строительства.
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AccessibleBuildings", menuName = "Buildings/AccessibleBuildings", order = 2)]
public class AccessibleBuildings : ScriptableObject
{
    public List<BuildingInfo> Buildings = new List<BuildingInfo>();
}

[System.Serializable]
public class BuildingInfo
{
    public GameObject BuildingPrefab;
    public BuildingData BuildingData;
}