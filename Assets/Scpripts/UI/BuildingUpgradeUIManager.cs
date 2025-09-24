// Управляет интерфейсом улучшения и продажи построек.
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BuildingUpgradeUIManager : MonoBehaviour
{
    public static BuildingUpgradeUIManager Instance { get; private set; }

    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private TextMeshProUGUI parametersText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI buildingNameText;

    private IBuildingInteractable currentBuilding;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (worldCanvas == null || upgradeButton == null || sellButton == null || parametersText == null || levelText == null || buildingNameText == null)
        {
            throw new System.NullReferenceException("Не заданы: worldCanvas, upgradeButton, sellButton, parametersText, levelText или buildingNameText");
        }

        worldCanvas.gameObject.SetActive(false);
        upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        sellButton.onClick.AddListener(OnSellButtonClicked);
    }

    private void Update()
    {
        if (currentBuilding == null)
        {
            return;
        }

        // Закрытие UI при клике вне постройки
        if (Input.GetMouseButtonDown(0) && EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.GetComponent<IBuildingInteractable>() == null)
            {
                HideUI();
            }
            else if (!Physics.Raycast(ray, out hit))
            {
                HideUI();
            }
        }
    }

    public void ShowUI(IBuildingInteractable building)
    {
        if (building == null)
        {
            throw new System.NullReferenceException("Не задан: building");
        }

        if (currentBuilding != null && currentBuilding != building)
        {
            HideUI();
        }

        currentBuilding = building;
        worldCanvas.gameObject.SetActive(true);
        worldCanvas.transform.position = building.GetUIPosition();

        bool isMaxLevel = building.GetLevelDisplay() == "Макс. уровень";
        upgradeButton.interactable = building.CanUpgrade() && !isMaxLevel;
        var upgradeText = upgradeButton.GetComponentInChildren<TextMeshProUGUI>();
        if (upgradeText == null)
        {
            throw new System.NullReferenceException("Не задан: upgradeText");
        }
        upgradeText.text = isMaxLevel ? "Макс." : $"Улучшить: {building.GetUpgradeCost()}";

        var sellText = sellButton.GetComponentInChildren<TextMeshProUGUI>();
        if (sellText == null)
        {
            throw new System.NullReferenceException("Не задан: sellText");
        }
        sellText.text = $"Продать: {building.GetSellPrice()}";

        parametersText.text = building.GetUpgradeParameters().Count > 0 ? string.Join("\n", building.GetUpgradeParameters()) : "Нет доступных улучшений";
        levelText.text = building.GetLevelDisplay();
        buildingNameText.text = building.GetBuildingName();
    }

    public void HideUI()
    {
        currentBuilding = null;
        worldCanvas.gameObject.SetActive(false);
    }

    private void OnUpgradeButtonClicked()
    {
        if (currentBuilding == null || !currentBuilding.CanUpgrade())
        {
            return;
        }

        int cost = currentBuilding.GetUpgradeCost();
        if (CurrencyManager.Instance.CanAfford(cost))
        {
            CurrencyManager.Instance.SpendCurrency(cost);
            currentBuilding.Upgrade();
            ShowUI(currentBuilding);
        }
    }

    private void OnSellButtonClicked()
    {
        if (currentBuilding == null)
        {
            return;
        }
        currentBuilding.Sell();
        HideUI();
    }
}