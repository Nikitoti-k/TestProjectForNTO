using System.IO;
using UnityEngine;
using System.Collections.Generic;

// Управляет сохранением и загрузкой игровых данных в JSON.
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    private string savePath;

    // Инициализация singleton и пути сохранения.
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        savePath = Path.Combine(Application.persistentDataPath, "save.json");
        DontDestroyOnLoad(gameObject); // Сохраняем объект между сценами.
    }

    // Очищает состояние при выходе на главную сцену.
    public void ResetForMainMenu()
    {
        // Удаляем файл сохранения.
        DeleteSave();
        // Сбрасываем состояние менеджеров.
        if (WaveManager.Instance != null)
            WaveManager.Instance.SetWaveIndex(0);
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.SetCurrency(0);
        // Очищаем пул мостов, чтобы избежать ссылок на уничтоженные объекты.
        if (BridgePool.Instance != null)
            BridgePool.Instance.ResetPool();
    }

    // Проверяет, есть ли файл сохранения.
    public bool HasSave() => File.Exists(savePath);

    // Сохраняет текущие данные игры (волна, валюта, здания).
    public void SaveGame()
    {
        if (WaveManager.Instance == null || CurrencyManager.Instance == null || HexGrid.Instance == null)
        {
            Debug.LogError("SaveManager: Отсутствуют необходимые менеджеры!");
            return;
        }

        var save = new SaveData
        {
            waveIndex = WaveManager.Instance.CurrentWaveIndex,
            currency = CurrencyManager.Instance.CurrentCurrency,
            buildings = new List<BuildingPlacement>()
        };

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
            Debug.Log($"Игра сохранена: {savePath}. Волна: {save.waveIndex}, Валюта: {save.currency}, Зданий: {save.buildings.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка сохранения: {e.Message}");
        }
    }

    // Загружает данные игры из файла.
    public void LoadGame()
    {
        if (!HasSave())
        {
            Debug.Log("SaveManager: Файл сохранения не найден.");
            return;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            var save = JsonUtility.FromJson<SaveData>(json);

            // Устанавливаем волну и валюту.
            if (WaveManager.Instance != null)
                WaveManager.Instance.SetWaveIndex(save.waveIndex);
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.SetCurrency(save.currency);

            // Очищаем текущие здания.
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

                // Удаляем собранные здания.
                foreach (var building in buildingsToRemove)
                {
                    if (building != null)
                        Destroy(building);
                }
            }

            var accessible = Resources.Load<AccessibleBuildings>("AccessibleBuildings");
            if (accessible == null)
            {
                Debug.LogError("SaveManager: AccessibleBuildings не найден в Resource!");
                return;
            }

            // Список для стен, чтобы обновить мосты после размещения.
            var walls = new List<Wall>();

            // Размещаем здания из сохранения.
            foreach (var placement in save.buildings)
            {
                var coord = new HexCoord(placement.q, placement.r);
                if (!HexGrid.Instance.IsCellFree(coord))
                {
                    Debug.LogWarning($"SaveManager: Клетка {coord} занята, пропускаем {placement.buildingName}");
                    continue;
                }

                // Проверяем соседей для штаба.
                if (placement.buildingName == "Штаб" && !AreNeighborsFree(coord))
                {
                    Debug.LogWarning($"SaveManager: Нельзя разместить Штаб в {coord}, соседние клетки заняты");
                    continue;
                }

                // Находим префаб здания.
                GameObject prefab = placement.buildingName == "Штаб"
                    ? HexGrid.Instance.headquartersPrefab
                    : accessible.Buildings.Find(b => b.BuildingData != null && b.BuildingData.Name == placement.buildingName)?.BuildingPrefab;

                if (prefab == null)
                {
                    Debug.LogError($"SaveManager: Префаб для {placement.buildingName} не найден");
                    continue;
                }

                // Создаём и размещаем здание.
                var buildingObj = Instantiate(prefab, HexGrid.Instance.GetWorldPosFromCoord(coord), Quaternion.identity);
                if (buildingObj == null || !buildingObj.TryGetComponent<BuildingBase>(out var building))
                {
                    Debug.LogError($"SaveManager: Не удалось создать здание {placement.buildingName} в {coord}");
                    if (buildingObj != null) Destroy(buildingObj);
                    continue;
                }

                HexGrid.Instance.PlaceBuilding(coord, building);
                building.UpgradeToLevel(placement.level);

                // Если здание - стена, сохраняем для последующего обновления мостов.
                if (building is Wall wall)
                {
                    walls.Add(wall);
                }

                Debug.Log($"Загружено: {placement.buildingName} в {coord}, уровень {placement.level}");
            }

            // Обновляем мосты для всех стен после размещения зданий.
            foreach (var wall in walls)
            {
                if (wall != null)
                    wall.UpdateBridges();
            }

            // Обновляем индикатор спавна для текущей волны.
            if (WaveManager.Instance != null)
                WaveManager.Instance.ShowNextWaveSpawnIndicator();

            Debug.Log($"Игра загружена: {savePath}. Волна: {save.waveIndex}, Валюта: {save.currency}, Зданий: {save.buildings.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка загрузки: {e.Message}");
        }
    }

    // Проверяет, свободны ли соседние клетки.
    private bool AreNeighborsFree(HexCoord coord)
    {
        if (HexGrid.Instance == null) return false;
        foreach (var nCoord in HexGrid.Instance.GetNeighborCoords(coord))
        {
            if (!HexGrid.Instance.IsCellFree(nCoord)) return false;
        }
        return true;
    }

    // Удаляет файл сохранения.
    public void DeleteSave()
    {
        if (HasSave())
        {
            try
            {
                File.Delete(savePath);
                Debug.Log("SaveManager: Сохранение удалено.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка удаления: {e.Message}");
            }
        }
    }
}