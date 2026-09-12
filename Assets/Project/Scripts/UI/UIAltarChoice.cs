using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Меню алтаря: заменить A / заменить B / пропустить.
/// Предпросмотр будущего комбо под каждой кнопкой.
/// </summary>
public class UIAltarChoice : MonoBehaviour
{
    public static UIAltarChoice Instance { get; private set; }
    public static bool IsShowing { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private ComboDatabase comboDatabase;

    [Header("Контент")]
    [SerializeField] private Image spellIcon;
    [SerializeField] private TextMeshProUGUI spellName;
    [SerializeField] private TextMeshProUGUI comboPreviewA;
    [SerializeField] private TextMeshProUGUI comboPreviewB;

    [Header("Кнопки")]
    [SerializeField] private Button replaceAButton;
    [SerializeField] private Button replaceBButton;
    [SerializeField] private Button skipButton;

    private SpellSO _offered;
    private SpellCaster _caster;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        replaceAButton.onClick.AddListener(() => Pick(true));
        replaceBButton.onClick.AddListener(() => Pick(false));
        skipButton.onClick.AddListener(Close);

        SetVisible(false); // корень активен, визуально скрыты
    }

    public void Open(SpellSO offered)
    {
        if (offered == null) return;

        _offered = offered;
        _caster = FindAnyObjectByType<SpellCaster>();
        if (_caster == null) return;

        if (spellIcon != null) { spellIcon.sprite = offered.icon; spellIcon.enabled = offered.icon != null; }
        if (spellName != null) spellName.text = offered.spellName;

        if (comboPreviewA != null) comboPreviewA.text = PreviewText(offered, _caster.GetSlotB());
        if (comboPreviewB != null) comboPreviewB.text = PreviewText(_caster.GetSlotA(), offered);

        SetVisible(true);
        IsShowing = true;
        Time.timeScale = 0f;
    }

    private void Close()
    {
        SetVisible(false);
        IsShowing = false;
        if (UpgradeManager.Instance == null || !UpgradeManager.Instance.IsShowing)
            Time.timeScale = 1f;
    }

    private void Pick(bool slotA)
    {
        if (_caster != null)
        {
            if (slotA) _caster.SetSlotA(_offered);
            else _caster.SetSlotB(_offered);
        }
        Close();
    }

    /// <summary>Какое комбо получится при такой паре слотов.</summary>
    private string PreviewText(SpellSO a, SpellSO b)
    {
        if (comboDatabase == null || a == null || b == null) return "Комбо: нет";
        SpellComboSO combo = comboDatabase.FindCombo(a.element, b.element);
        return combo != null ? $"Комбо: {combo.name}" : "Комбо: нет";
    }

    private void SetVisible(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = visible;
        canvasGroup.interactable = visible;
    }
}