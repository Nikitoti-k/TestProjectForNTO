using System.IO;
using UnityEngine;
using System.Collections.Generic;

// �������� ��� ���������� � ��������, ������ �������� �� (����������: ������, ����, ������). ���������� ������ ����� JSON
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    private string savePath;

    [SerializeField] private GameSceneConfiguration sceneSettings; 

    void Awake()
    {
       
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        savePath = Path.Combine(Application.persistentDataPath, "save.json");
        DontDestroyOnLoad(gameObject);
    }

   
    public void ResetForMainMenu()
    {
        DeleteSave(); 
    
        if (WaveManager.Instance != null)
            WaveManager.Instance.SetWaveIndex(0);
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.SetCurrency(0);
    
        if (BridgePool.Instance != null)
            BridgePool.Instance.ResetPool();
    }

  
    public bool HasSave() => File.Exists(savePath);

   
    public void SaveGame()
    {
        
        if (WaveManager.Instance == null || CurrencyManager.Instance == null || HexGrid.Instance == null)
        {
            Debug.LogError("SaveManager: ����������� ������ ���������!");
            return;
        }

        var save = new SaveData
        {
            waveIndex = WaveManager.Instance.CurrentWaveIndex,
            currency = CurrencyManager.Instance.CurrentCurrency,
            buildings = new List<BuildingPlacement>()
        };

        // �������� ������ � ������� �� �����
        foreach (var kvp in HexGrid.Instance.Cells)
        {
            if (kvp.Value.Building != null && kvp.Value.Building.gameObject != null)
            {
                save.buildings.Add(new BuildingPlacement
                {
                    q = kvp.Key.q,
                    r = kvp.Key.r,
                    buildingName = kvp.Value.Building.GetBuildingName(),
                    level = kvp.Value.Building.CurrentLevel
                });
            }
        }

        try
        {
            string json = JsonUtility.ToJson(save, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"���� ���������: {savePath}. �����: {save.waveIndex}, ������: {save.currency}, ������� ������: {save.buildings.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"������ ����������: {e.Message}");
        }
    }

    
    public void LoadGame()
    {
        if (!HasSave())
        {
            Debug.Log("SaveManager: ���� ���������� �� ������.");
            return;
        }

        if (sceneSettings == null)
        {
            Debug.LogError("SaveManager: GameSceneConfiguration �� ��������!");
            return;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            var save = JsonUtility.FromJson<SaveData>(json);

           
            if (WaveManager.Instance != null)
                WaveManager.Instance.SetWaveIndex(save.waveIndex);
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.SetCurrency(save.currency);

            
            if (HexGrid.Instance != null)
            {
                var cells = HexGrid.Instance.Cells;
                var buildingsToRemove = new List<GameObject>();
                foreach (var cell in cells.Values)
                {
                    if (cell.Building != null && cell.Building.gameObject != null)
                    {
                        buildingsToRemove.Add(cell.Building.gameObject);
                        HexGrid.Instance.FreeCell(cell.Coord);
                    }
                }

               
                foreach (var building in buildingsToRemove)
                {
                    if (building != null)
                        Destroy(building);
                }
            }

            var accessible = Resources.Load<AccessibleBuildings>("AccessibleBuildings");
            if (accessible == null)
            {
                Debug.LogError("SaveManager: AccessibleBuildings �� ������ � Resource!");
                return;
            }

            var walls = new List<Wall>(); 

            
            foreach (var placement in save.buildings)
            {
                var coord = new HexCoord(placement.q, placement.r);
                if (!HexGrid.Instance.IsCellFree(coord))
                {
                    Debug.LogWarning($"SaveManager: ������ {coord} ������, ���������� {placement.buildingName}");
                    continue;
                }

              
                if (placement.buildingName == "����" && !AreNeighborsFree(coord))
                {
                    Debug.LogWarning($"SaveManager: ������ ���������� ���� � {coord}, �������� ������ ������");
                    continue;
                }

             
                GameObject prefab = placement.buildingName == "����"
                    ? sceneSettings.HeadquartersPrefab
                    : accessible.Buildings.Find(b => b.BuildingData != null && b.BuildingData.Name == placement.buildingName)?.BuildingPrefab;

                if (prefab == null)
                {
                    Debug.LogError($"SaveManager: ������ ��� {placement.buildingName} �� ������");
                    continue;
                }

                
                var buildingObj = Instantiate(prefab, HexGrid.Instance.GetWorldPosFromCoord(coord), Quaternion.identity);
                if (buildingObj == null || !buildingObj.TryGetComponent<BuildingBase>(out var building))
                {
                    Debug.LogError($"SaveManager: �� ������� ������� ������ {placement.buildingName} � {coord}");
                    if (buildingObj != null) Destroy(buildingObj);
                    continue;
                }

                HexGrid.Instance.PlaceBuilding(coord, building);
                building.UpgradeToLevel(placement.level);

              
                if (building is Wall wall)
                {
                    walls.Add(wall);
                }

                Debug.Log($"���������: {placement.buildingName} � {coord}, ������� {placement.level}");
            }

            // ��������� ��������� ��� ���� ����
            foreach (var wall in walls)
            {
                if (wall != null)
                    wall.UpdateBridges();
            }

           
            if (WaveManager.Instance != null)
                WaveManager.Instance.ShowNextWaveSpawnIndicator();

            Debug.Log($"���� ���������: {savePath}. �����: {save.waveIndex}, ������: {save.currency}, ������: {save.buildings.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"������ ��������: {e.Message}");
        }
    }

   
    private bool AreNeighborsFree(HexCoord coord)
    {
        if (HexGrid.Instance == null) return false;
        foreach (var nCoord in HexGrid.Instance.GetNeighborCoords(coord))
        {
            if (!HexGrid.Instance.IsCellFree(nCoord)) return false;
        }
        return true;
    }

   
    public void DeleteSave()
    {
        if (HasSave())
        {
            try
            {
                File.Delete(savePath);
                Debug.Log("SaveManager: ���������� �������.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"������ ��������: {e.Message}");
            }
        }
    }
}