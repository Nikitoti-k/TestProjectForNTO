// Компонент для плашки HP: обновляет позицию и значение HP.
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, 0f); 
    private Object target; 

    public void Initialize(Object target, int currentHealth, int maxHealth)
    {
        if (healthSlider == null)
        {
            Debug.LogError("HealthBar: Missing Slider component!");
            return;
        }
        this.target = target;
        UpdateHealth(currentHealth, maxHealth);
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        healthSlider.value = (float)currentHealth / maxHealth;
    }

    private void Update()
    {
        if (target == null) return;

        Transform targetTransform = (target as MonoBehaviour)?.transform;
        if (targetTransform != null)
        {
            transform.position = targetTransform.position + offset;
        }
    }
}