using System.Collections.Generic;
using UnityEngine;

public class BridgePool : MonoBehaviour
{
    public static BridgePool Instance { get; private set; }

    [SerializeField] private GameObject bridgePrefab;
    [SerializeField] private int poolSize = 50; // Начальный размер пула.

    private List<GameObject> bridgePool = new List<GameObject>();

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
                return bridgePool[i];
            }
        }

        // Расширяем пул, если все мосты заняты или уничтожены.
        var newBridge = Instantiate(bridgePrefab, Vector3.zero, Quaternion.identity);
        newBridge.SetActive(true);
        bridgePool.Add(newBridge);
        return newBridge;
    }

    public void ReturnBridge(GameObject bridge)
    {
        if (bridge != null)
        {
            bridge.SetActive(false);
            bridge.transform.SetParent(null); // Отцепляем от стены для reuse.
        }
    }

    // Очищает и переинициализирует пул при возвращении в главное меню.
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