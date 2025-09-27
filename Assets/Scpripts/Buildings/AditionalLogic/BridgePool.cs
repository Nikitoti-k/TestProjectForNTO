using System.Collections.Generic;
using UnityEngine;

public class BridgePool : MonoBehaviour
{
    public static BridgePool Instance { get; private set; }

    [SerializeField] private GameObject bridgePrefab;
    [SerializeField] private GameSceneConfiguration sceneSettings; // Настройки сцены
    [SerializeField] private int poolSize = 50;

    private List<GameObject> bridgePool = new List<GameObject>();
    private Vector3 originalScale; // Исходный масштаб префаба для сброса

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Сохраняем исходный масштаб
        if (bridgePrefab != null)
        {
            originalScale = bridgePrefab.transform.localScale;
        }
        else
        {
            Debug.LogError("BridgePool: bridgePrefab не назначен!");
        }

        if (sceneSettings == null)
        {
            Debug.LogWarning("BridgePool: GameSceneConfiguration не назначен!");
        }
    }

    private void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var bridge = Instantiate(bridgePrefab, Vector3.zero, Quaternion.identity);
            bridge.SetActive(false);
            bridgePool.Add(bridge);
        }
    }

    public GameObject GetBridge()
    {
        // Проверяем существующие мосты.
        for (int i = 0; i < bridgePool.Count; i++)
        {
            if (bridgePool[i] != null && !bridgePool[i].activeInHierarchy)
            {
                bridgePool[i].SetActive(true);
                
                if (HexGrid.Instance != null && sceneSettings != null)
                {
                    bridgePool[i].transform.localScale = originalScale * HexGrid.Instance.CellSize * sceneSettings.BuildingScaleFactor;
                }
                return bridgePool[i];
            }
        }

        // Расширяем пул
        var newBridge = Instantiate(bridgePrefab, Vector3.zero, Quaternion.identity);
        newBridge.SetActive(true);
        
        if (HexGrid.Instance != null && sceneSettings != null)
        {
            newBridge.transform.localScale = originalScale * HexGrid.Instance.CellSize * sceneSettings.BuildingScaleFactor;
        }
        bridgePool.Add(newBridge);
        return newBridge;
    }

    public void ReturnBridge(GameObject bridge)
    {
        if (bridge != null)
        {
            
            bridge.transform.localScale = originalScale;
            bridge.SetActive(false);
            bridge.transform.SetParent(null); 
        }
    }

    // Очищает и переинициализирует пул при возвращении в главное меню
    public void ResetPool()
    {
        foreach (var bridge in bridgePool)
        {
            if (bridge != null)
            {
                Destroy(bridge);
            }
        }
        bridgePool.Clear();
        InitializePool();
    }
}