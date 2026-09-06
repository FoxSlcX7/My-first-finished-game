using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIXPBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private TextMeshProUGUI levelText;

    private PlayerXP _xp;

    private void OnEnable()
    {
        if (_xp == null)
            _xp = PlayerXP.Instance != null ? PlayerXP.Instance : FindAnyObjectByType<PlayerXP>();

        GameEvents.OnXPChanged?.AddListener(HandleXP);

        if (_xp != null) HandleXP(_xp.CurrentXP, _xp.RequiredXP);
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