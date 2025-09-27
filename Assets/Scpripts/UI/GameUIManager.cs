// Управляет основным UI: кнопки строительства, валюта, ошибки, волны, поражение (всё кроме UI улучшения зданий - решил разделиить отвественность)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [SerializeField] private AccessibleBuildings accessibleBuildings;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform buttonParent;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private float errorDisplayTime = 2f;
    [SerializeField] private Button startWaveButton;
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private Button exitToMenuButton;
    private Headquarters headquarters;



    private void ExitToMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
    private void Awake()
    {
       
        exitToMenuButton.onClick.AddListener(ExitToMenu);
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (accessibleBuildings == null || buttonPrefab == null || buttonParent == null || currencyText == null || errorText == null)
        {
            Debug.LogError("GameUIManager: Missing required components!");
            return;
        }


        CurrencyManager.Instance?.OnCurrencyChanged.AddListener(UpdateCurrencyUI);
        UpdateCurrencyUI(CurrencyManager.Instance?.CurrentCurrency ?? 0);

        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }
        foreach (var building in accessibleBuildings.Buildings)
        {
          
            var buttonObj = Instantiate(buttonPrefab, buttonParent);
            var button = buttonObj.GetComponent<Button>();
            var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            int cost = building.BuildingData.Levels.Count > 0 ? building.BuildingData.Levels[0].Cost : 0;
            text.text = $"{building.BuildingData.Name}\n{building.BuildingData.CostDisplayName}: {cost}";

            button.onClick.AddListener(() =>
            {
                if (CurrencyManager.Instance.CanAfford(cost))
                {
                    BuildingManager.Instance.StartBuilding(building.BuildingPrefab, cost);
                }
                else
                {
                    ShowError("Недостаточно валюты");
                }
            });
        }

        if (startWaveButton != null)
        {
            startWaveButton.onClick.AddListener(() => WaveManager.Instance?.StartNextWave());
            WaveManager.Instance?.OnWaveStarted.AddListener(() =>
            {
                startWaveButton.gameObject.SetActive(false);
                BuildingUpgradeUIManager.Instance?.HideUI(); // Новый: закрываем UI улучшений.
            });
            WaveManager.Instance?.OnWaveEnded.AddListener(() => startWaveButton.gameObject.SetActive(true));
            startWaveButton.gameObject.SetActive(true);
        }

        if (defeatPanel == null || mainMenuButton == null || restartButton == null)
        {
            Debug.LogError("GameUIManager: не найден UI поражения");
            return;
        }
        defeatPanel.SetActive(false);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        restartButton.onClick.AddListener(RestartGame);

        if (headquarters != null)
        {
            headquarters.OnDefeat.AddListener(ShowDefeatUI);
        }
        else
        {
            headquarters = FindFirstObjectByType<Headquarters>();
            if (headquarters != null) headquarters.OnDefeat.AddListener(ShowDefeatUI);
        }
    }

    private void Update()
    {
        if (headquarters == null)
        {
            headquarters = FindFirstObjectByType<Headquarters>();
            if (headquarters != null) headquarters.OnDefeat.AddListener(ShowDefeatUI);
        }
    }

    private void UpdateCurrencyUI(int amount)
    {
        currencyText.text = $"Деньги: {amount}";
    }

    public void ShowError(string message)
    {
        errorText.text = message;
        errorText.gameObject.SetActive(true);
        CancelInvoke(nameof(HideError));
        Invoke(nameof(HideError), errorDisplayTime);
    }

    private void HideError()
    {
        errorText.gameObject.SetActive(false);
    }

    private void ShowDefeatUI()
    {
        defeatPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void GoToMainMenu()
    {
        ResetGameState();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void RestartGame()
    {
        ResetGameState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ResetGameState()
    {
        Time.timeScale = 1f;
        DestroyIfExists(CurrencyManager.Instance?.gameObject);
        DestroyIfExists(EnemyManager.Instance?.gameObject);
        DestroyIfExists(BuildingManager.Instance?.gameObject);
        DestroyIfExists(BridgePool.Instance?.gameObject);
        DestroyIfExists(BuildingUpgradeUIManager.Instance?.gameObject);
        DestroyIfExists(WaveManager.Instance?.gameObject);
        Destroy(gameObject);
    }

    private void DestroyIfExists(GameObject obj)
    {
        if (obj != null) Destroy(obj);
    }
}