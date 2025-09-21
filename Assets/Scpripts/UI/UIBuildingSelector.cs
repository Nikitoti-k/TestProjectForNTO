using UnityEngine;
using UnityEngine.UI;
using TMPro;
// Отвественный за UI элементы: кнопки зданий, отображение валюты и т.д.
public class UIBuildingSelector : MonoBehaviour
{
    [SerializeField] private AccessibleBuildings accessibleBuildings;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform buttonParent;
    [SerializeField] private TextMeshProUGUI currencyText; 
    [SerializeField] private TextMeshProUGUI errorText; 
    [SerializeField] private float errorDisplayTime = 2f; 

    private void Start()
    {
        if (accessibleBuildings == null || buttonPrefab == null || buttonParent == null)
        {
            Debug.LogWarning("UIBuildingSelector: Missing references");
            return;
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
                continue;

            GameObject buttonObj = Instantiate(buttonPrefab, buttonParent);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (button == null || text == null)
            {
                Debug.LogWarning("UIBuildingSelector: Button prefab missing components!!!");
                continue;
            }

            
            string displayName = building.DisplayName;
            int cost = 0;
            if (building.BuildingData is TurretData turretData && turretData.Levels.Count > 0)
            {
                cost = turretData.Levels[0].Cost;
            }
            
            text.text = $"{displayName}\nCost: {cost}";

            
            int buildingCost = cost;
            GameObject prefab = building.BuildingPrefab;

          
            button.onClick.AddListener(() =>
            {
                if (CurrencyManager.Instance.CanAfford(buildingCost))
                {
                    BuildingManager.Instance.StartBuilding(prefab, cost);
                }
                else
                {
                    ShowError("Not enough money!");
                }
            });
        }
    }

    private void UpdateCurrencyUI(int amount)
    {
        if (currencyText != null)
        {
            currencyText.text = $"Gold: {amount}";
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
}