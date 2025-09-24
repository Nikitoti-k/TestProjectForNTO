// Управляет основным интерфейсом: отображает кнопки построек, валюту и кнопку старта волны.
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainUIManager : MonoBehaviour
{
    public static MainUIManager Instance { get; private set; }

    [SerializeField] private AccessibleBuildings accessibleBuildings;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform buttonParent;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private float errorDisplayTime = 2f;
    [SerializeField] private Button startWaveButton;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (accessibleBuildings == null || buttonPrefab == null || buttonParent == null)
        {
            throw new System.NullReferenceException("Не заданы: accessibleBuildings, buttonPrefab или buttonParent");
        }

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged.AddListener(UpdateCurrencyUI);
            UpdateCurrencyUI(CurrencyManager.Instance.CurrentCurrency);
        }

        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var building in accessibleBuildings.Buildings)
        {
            if (building.BuildingPrefab == null || building.BuildingData == null)
            {
                throw new System.NullReferenceException($"Не заданы: BuildingPrefab или BuildingData для {building.BuildingData?.Name}");
            }

            GameObject buttonObj = Instantiate(buttonPrefab, buttonParent);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (button == null || text == null)
            {
                throw new System.NullReferenceException($"Не заданы: Button или TextMeshProUGUI для {building.BuildingData.Name}");
            }

            string displayName = building.BuildingData.Name;
            int cost = building.BuildingData.Levels.Count > 0 ? building.BuildingData.Levels[0].Cost : 0;
            text.text = $"{displayName}\n{building.BuildingData.CostDisplayName}: {cost}";

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
        if (currencyText == null)
        {
            throw new System.NullReferenceException("Не задан: currencyText");
        }
        currencyText.text = $"Деньги: {amount}";
    }

    public void ShowError(string message)
    {
        if (errorText == null)
        {
            throw new System.NullReferenceException("Не задан: errorText");
        }
        errorText.text = message;
        errorText.gameObject.SetActive(true);
        Invoke(nameof(HideError), errorDisplayTime);
    }

    private void HideError()
    {
        if (errorText == null)
        {
            throw new System.NullReferenceException("Не задан: errorText");
        }
        errorText.gameObject.SetActive(false);
    }

    private void SetWaveButtonActive(bool active)
    {
        if (startWaveButton == null)
        {
            throw new System.NullReferenceException("Не задан: startWaveButton");
        }
        startWaveButton.gameObject.SetActive(active);
    }
}