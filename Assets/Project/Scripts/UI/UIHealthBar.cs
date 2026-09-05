using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI healthText;

    private void OnEnable()
    {
        if (GameEvents.OnHealthChanged != null)
        {
            GameEvents.OnHealthChanged.AddListener(UpdateHealthBar);
        }
    }

    private void OnDisable()
    {
        if (GameEvents.OnHealthChanged != null)
        {
            GameEvents.OnHealthChanged.RemoveListener(UpdateHealthBar);
        }
    }

    private void UpdateHealthBar(int current, int max)
    {
        current = Mathf.Max(0, current); // защита от отрицательных чисел

        float fillAmount = max > 0 ? (float)current / max : 0f;
        fillImage.fillAmount = Mathf.Clamp01(fillAmount);

        if (healthText != null)
        {
            // Интерполяция строк: подставляет значения в текст
            healthText.text = $"{current}/{max}";

            // Бонус: краснеет, когда HP мало (меньше 30%)
            healthText.color = fillAmount <= 0.3f ? Color.red : Color.white;
        }
    }
}