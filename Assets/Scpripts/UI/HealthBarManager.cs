// Управляет пулом плашек HP для врагов и башен, отображает их через WorldCanvas.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarManager : MonoBehaviour
{
    public static HealthBarManager Instance { get; private set; }

    [SerializeField] private GameObject enemyHealthBarPrefab; // Префаб плашки для врагов.
    [SerializeField] private GameObject turretHealthBarPrefab; // Префаб плашки для башен.
    [SerializeField] private int poolSize = 50; // Начальный размер пула.

    private List<GameObject> enemyHealthBarPool = new List<GameObject>();
    private List<GameObject> turretHealthBarPool = new List<GameObject>();
    private Dictionary<Object, GameObject> activeHealthBars = new Dictionary<Object, GameObject>(); // Object как ключ для EnemyBase/Turret.

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (enemyHealthBarPrefab == null || turretHealthBarPrefab == null)
        {
            Debug.LogError("HealthBarManager: Missing health bar prefabs!");
            return;
        }
        InitializePool(enemyHealthBarPool, enemyHealthBarPrefab);
        InitializePool(turretHealthBarPool, turretHealthBarPrefab);
    }

    private void InitializePool(List<GameObject> pool, GameObject prefab)
    {
        for (int i = 0; i < poolSize; i++)
        {
            var healthBar = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            healthBar.SetActive(false);
            pool.Add(healthBar);
        }
    }

    public void ShowHealthBar(Object target, int currentHealth, int maxHealth, bool isTurret)
    {
        if (target == null || activeHealthBars.ContainsKey(target)) return;

        var pool = isTurret ? turretHealthBarPool : enemyHealthBarPool;
        var prefab = isTurret ? turretHealthBarPrefab : enemyHealthBarPrefab;
        GameObject healthBar = null;

        foreach (var bar in pool)
        {
            if (!bar.activeInHierarchy)
            {
                healthBar = bar;
                break;
            }
        }

        if (healthBar == null)
        {
            healthBar = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            pool.Add(healthBar);
        }

        healthBar.SetActive(true);
        var healthBarScript = healthBar.GetComponent<HealthBar>();
        healthBarScript.Initialize(target, currentHealth, maxHealth);
        activeHealthBars[target] = healthBar;
    }

    public void UpdateHealthBar(Object target, int currentHealth, int maxHealth)
    {
        if (target == null || !activeHealthBars.ContainsKey(target)) return;

        var healthBar = activeHealthBars[target].GetComponent<HealthBar>();
        healthBar.UpdateHealth(currentHealth, maxHealth);
    }

    public void HideHealthBar(Object target)
    {
        if (target == null || !activeHealthBars.ContainsKey(target)) return;

        var healthBar = activeHealthBars[target];
        healthBar.SetActive(false);
        activeHealthBars.Remove(target);
    }
}