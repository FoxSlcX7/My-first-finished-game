using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Двусторонняя шкала заряда комбо между иконками слотов.
/// Видимость — от глобального события комбо, значения — от локального события кастера.
/// </summary>
public class UIComboCharge : MonoBehaviour
{
    [SerializeField] private Image fillA; // левая половина (заряд от слота A)
    [SerializeField] private Image fillB; // правая половина
    [SerializeField] private CanvasGroup canvasGroup;

    private SpellCaster _caster;

    private void OnEnable()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        _caster = FindAnyObjectByType<SpellCaster>();
        if (_caster != null)
        {
            _caster.OnComboChargeChanged += HandleCharge;
            HandleCharge(_caster.ChargeA, _caster.ChargeB);
        }

        GameEvents.OnComboStateChanged?.AddListener(HandleComboState);
    }

    private void OnDisable()
    {
        if (_caster != null)
            _caster.OnComboChargeChanged -= HandleCharge;
        GameEvents.OnComboStateChanged?.RemoveListener(HandleComboState);
    }

    private void HandleComboState(SpellComboSO combo)
    {
        if (canvasGroup != null)
            canvasGroup.alpha = combo != null ? 1f : 0f;
    }

    private void HandleCharge(float a, float b)
    {
        if (fillA != null) fillA.fillAmount = a / 100f;
        if (fillB != null) fillB.fillAmount = b / 100f;
    }
}