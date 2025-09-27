using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

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
            Debug.LogError("BuildingUpgradeUIManager: Missing UI components!");
            return;
        }

        worldCanvas.gameObject.SetActive(false);
        upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        sellButton.onClick.AddListener(OnSellButtonClicked);
    }

    private void Update()
    {
        if (currentBuilding == null) return;

        // �������� UI ��� ����� ��� ������
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit) || hit.collider.GetComponent<IBuildingInteractable>() == null)
            {
                HideUI();
            }
        }
    }

    // ��������� ����������� � SO ����� ������ �������� ���������� ������������� ��������� ��� ������� ������
    public void ShowUI(IBuildingInteractable building)
    {
        if (WaveManager.Instance != null && WaveManager.Instance.IsWaveActive)
        {
            GameUIManager.Instance?.ShowError("������ ����������������� � �������� �� ����� �����!");
            return;
        }

        if (building == null)
        {
            Debug.LogError("BuildingUpgradeUIManager: Null building!");
            return;
        }

        if (currentBuilding != building)
        {
            HideUI();
        }

        currentBuilding = building;
        worldCanvas.gameObject.SetActive(true);
        worldCanvas.transform.position = building.GetUIPosition();

        bool isMaxLevel = building.GetLevelDisplay() == "����. �������";
        upgradeButton.interactable = building.CanUpgrade() && !isMaxLevel;
        var upgradeText = upgradeButton.GetComponentInChildren<TextMeshProUGUI>();
        upgradeText.text = isMaxLevel ? "����." : $"��������: {building.GetUpgradeCost()}";

        var sellText = sellButton.GetComponentInChildren<TextMeshProUGUI>();
        sellText.text = $"�������: {building.GetSellPrice()}";

        parametersText.text = building.GetUpgradeParameters().Count > 0 ? string.Join("\n", building.GetUpgradeParameters()) : "��� ���������";
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
        if (WaveManager.Instance != null && WaveManager.Instance.IsWaveActive)
        {
            GameUIManager.Instance?.ShowError("������ �������� ������ �� ����� �����!");
            return;
        }
        if (currentBuilding == null || !currentBuilding.CanUpgrade()) return;
        currentBuilding.Upgrade();
        ShowUI(currentBuilding);
    }

    private void OnSellButtonClicked()
    {
        if (WaveManager.Instance != null && WaveManager.Instance.IsWaveActive)
        {
            GameUIManager.Instance?.ShowError("������ ��������� ������ �� ����� �����!");
            return;
        }
        if (currentBuilding == null) return;
        currentBuilding.Sell();
        HideUI();
    }
}