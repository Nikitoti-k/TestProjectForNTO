using System.Collections.Generic;

// ������ ��� ���������� ����
[System.Serializable]
public class SaveData
{
    public int waveIndex; 
    public int currency; 
    public List<BuildingPlacement> buildings; 
}
[System.Serializable]
public class BuildingPlacement
{
    public int q, r; 
    public string buildingName;
    public int level; 
}