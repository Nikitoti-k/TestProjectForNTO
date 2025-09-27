using UnityEngine;
using UnityEngine.Events;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private int startingCurrency = 1000;
    public int CurrentCurrency { get; private set; }
    public UnityEvent<int> OnCurrencyChanged;

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
        CurrentCurrency = startingCurrency;
        OnCurrencyChanged.Invoke(CurrentCurrency);
    }

    public bool CanAfford(int cost) => CurrentCurrency >= cost;

    public void SpendCurrency(int amount)
    {
        if (amount <= 0 || !CanAfford(amount)) return;
        CurrentCurrency = Mathf.Max(0, CurrentCurrency - amount);
        OnCurrencyChanged.Invoke(CurrentCurrency);
    }

    public void AddCurrency(int amount)
    {
        if (amount <= 0) return;
        CurrentCurrency += amount;
        OnCurrencyChanged.Invoke(CurrentCurrency);
    }

    public void SetCurrency(int amount) // Новый: для загрузки.
    {
        if (amount < 0) return;
        CurrentCurrency = amount;
        OnCurrencyChanged.Invoke(CurrentCurrency);
    }
}