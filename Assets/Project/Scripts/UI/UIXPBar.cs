using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIXPBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private TextMeshProUGUI levelText;

    private void OnEnable()
    {
        GameEvents.OnXPChanged?.AddListener(HandleXP);

        // Если PlayerXP уже инициализирован, сразу выводим текущее состояние
        if (PlayerXP.Instance != null)
        {
            HandleXP(PlayerXP.Instance.CurrentXP, PlayerXP.Instance.RequiredXP);
        }
    }

    private void OnDisable()
    {
        GameEvents.OnXPChanged?.RemoveListener(HandleXP);
    }

    private void HandleXP(int current, int required)
    {
        if (slider != null)
        {
            slider.maxValue = required;
            slider.value = current;
        }

        if (xpText != null) xpText.text = $"XP {current}/{required}";

        if (levelText != null && PlayerXP.Instance != null)
            levelText.text = $"Lv {PlayerXP.Instance.Level}";
    }
}