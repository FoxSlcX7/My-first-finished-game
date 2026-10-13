using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISpellComboIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject indicator;
    [SerializeField] private Image comboIcon;
    [SerializeField] private TextMeshProUGUI comboNameText;
    [SerializeField] private TextMeshProUGUI hotkeyText;

    [Header("Appearance")]
    [SerializeField] private Color readyColor = Color.white;
    [SerializeField] private Color cooldownColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private string hotkeyLabel = "[SPACE]";

    private SpellComboSO _currentCombo;

    private void OnEnable()
    {
        GameEvents.OnComboStateChanged?.AddListener(OnComboStateChanged);

        // Подписываемся на статический ивент напрямую — ссылка в инспекторе не нужна
        SpellCaster.OnComboReadyChanged += HandleComboReadyChanged;

        if (SpellCaster.Instance != null && _currentCombo != null)
        {
            SetReadyVisual(SpellCaster.Instance.IsComboReady());
        }
    }

    private void OnDisable()
    {
        GameEvents.OnComboStateChanged?.RemoveListener(OnComboStateChanged);
        SpellCaster.OnComboReadyChanged -= HandleComboReadyChanged;
    }

    private void OnComboStateChanged(SpellComboSO combo)
    {
        _currentCombo = combo;
        bool hasCombo = combo != null;

        if (indicator != null)
            indicator.SetActive(hasCombo);

        if (comboIcon != null && combo != null)
            comboIcon.sprite = combo.icon;

        if (comboNameText != null)
            comboNameText.text = hasCombo ? combo.comboName : string.Empty;

        if (hotkeyText != null)
            hotkeyText.text = hasCombo ? hotkeyLabel : string.Empty;

        if (hasCombo)
        {
            // Проверяем готовность через синглтон
            bool ready = SpellCaster.Instance != null && SpellCaster.Instance.IsComboReady();
            SetReadyVisual(ready);
        }
    }

    private void HandleComboReadyChanged(bool isReady)
    {
        if (_currentCombo == null) return;
        SetReadyVisual(isReady);
    }

    private void SetReadyVisual(bool ready)
    {
        Color targetColor = ready ? readyColor : cooldownColor;

        if (indicator != null)
        {
            Image[] images = indicator.GetComponentsInChildren<Image>();
            foreach (Image img in images)
            {
                img.color = targetColor;
            }
        }

        if (comboNameText != null)
            comboNameText.color = targetColor;

        if (hotkeyText != null)
            hotkeyText.color = ready ? Color.yellow : cooldownColor;
    }
}