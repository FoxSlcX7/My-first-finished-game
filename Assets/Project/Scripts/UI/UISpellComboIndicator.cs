using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISpellComboIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpellCaster spellCaster;
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

        if (spellCaster != null)
        {
            spellCaster.OnComboReadyChanged += OnComboReadyChanged;
        }
    }

    private void OnDisable()
    {
        GameEvents.OnComboStateChanged?.RemoveListener(OnComboStateChanged);

        if (spellCaster != null)
        {
            spellCaster.OnComboReadyChanged -= OnComboReadyChanged;
        }
    }

    // Вызывается когда меняется пара заклинаний в слотах
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

        // Обновляем цвет в зависимости от готовности
        if (hasCombo)
        {
            bool ready = spellCaster != null && spellCaster.IsComboReady();
            SetReadyVisual(ready);
        }
    }

    // Вызывается когда комбо становится готово или уходит на кулдаун
    private void OnComboReadyChanged(bool isReady)
    {
        if (_currentCombo == null) return;
        SetReadyVisual(isReady);
    }

    private void SetReadyVisual(bool ready)
    {
        Color targetColor = ready ? readyColor : cooldownColor;

        if (indicator != null)
        {
            // Меняем цвет всех изображений внутри индикатора
            Image[] images = indicator.GetComponentsInChildren<Image>();
            foreach (Image img in images)
            {
                img.color = targetColor;
            }
        }

        if (comboNameText != null)
            comboNameText.color = targetColor;

        if (hotkeyText != null)
        {
            hotkeyText.color = ready ? Color.yellow : cooldownColor;
        }
    }
}