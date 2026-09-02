using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    [SerializeField] private Image fillImage;

    

    private void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth не назначен в Inspector!");
            return;
        }

        Debug.Log($"Стартовое HP: {playerHealth.CurrentHealth} / {playerHealth.MaxHealth}");

        playerHealth.OnHealthChanged += UpdateHealthBar;
        UpdateHealthBar(playerHealth.CurrentHealth);
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }

    private void UpdateHealthBar(int currentHealth)
    {
        float fillAmount = (float)currentHealth / playerHealth.MaxHealth;
        fillImage.fillAmount = fillAmount;
    }
}