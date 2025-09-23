using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private AccessibleBuildings accessibleBuildings;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform buttonParent;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private float errorDisplayTime = 2f;
    [SerializeField] private Button startWaveButton;

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
        if (accessibleBuildings == null || buttonPrefab == null || buttonParent == null)
        {
            Debug.LogWarning("Не заданы классы!");
            return;
        }

        // Валюта
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged.AddListener(UpdateCurrencyUI);
            UpdateCurrencyUI(CurrencyManager.Instance.CurrentCurrency);
        }

        // Кнопки зданий
        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var building in accessibleBuildings.Buildings)
        {
            if (building.BuildingPrefab == null || building.BuildingData == null)
                continue;

            GameObject buttonObj = Instantiate(buttonPrefab, buttonParent);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (button == null || text == null)
            {
                Debug.LogWarning("Нет компонентов!");
                continue;
            }

            string displayName = building.DisplayName;
            int cost = 0;
            if (building.BuildingData is TurretData turretData && turretData.Levels.Count > 0)
            {
                cost = turretData.Levels[0].Cost;
            }

            text.text = $"{displayName}\nСтоимость: {cost}";

            int buildingCost = cost;
            GameObject prefab = building.BuildingPrefab;

            button.onClick.AddListener(() =>
            {
                if (CurrencyManager.Instance.CanAfford(buildingCost))
                {
                    BuildingManager.Instance.StartBuilding(prefab, buildingCost);
                }
                else
                {
                    ShowError("Недостаточно валюты");
                }
            });
        }

        // Кнопка волны
        if (startWaveButton != null)
        {
            startWaveButton.onClick.AddListener(() => WaveManager.Instance.StartNextWave());
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveStarted.AddListener(() => SetWaveButtonActive(false));
                WaveManager.Instance.OnWaveEnded.AddListener(() => SetWaveButtonActive(true));
            }
            SetWaveButtonActive(true);
        }
    }

    private void UpdateCurrencyUI(int amount)
    {
        if (currencyText != null)
        {
            currencyText.text = $"Деньги: {amount}";
        }
    }

    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
            Invoke(nameof(HideError), errorDisplayTime);
        }
    }

    private void HideError()
    {
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }
    }

    private void SetWaveButtonActive(bool active)
    {
        if (startWaveButton != null)
        {
            startWaveButton.gameObject.SetActive(active);
        }
    }
}