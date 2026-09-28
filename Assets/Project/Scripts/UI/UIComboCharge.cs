using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Двусторонняя шкала заряда комбо между иконками слотов.
/// Подписывается на глобальные события, не зависит от порядка спавна игрока.
/// </summary>
public class UIComboCharge : MonoBehaviour
{
    [SerializeField] private Image fillA; // левая половина (заряд от слота A)
    [SerializeField] private Image fillB; // правая половина
    [SerializeField] private CanvasGroup canvasGroup;

    private void OnEnable()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        // Подписываемся на статический ивент напрямую — ссылка на объект больше не нужна
        SpellCaster.OnComboChargeChanged += HandleCharge;
        GameEvents.OnComboStateChanged?.AddListener(HandleComboState);

        // Начальная отрисовка: если кастер уже есть — берем текущий заряд, иначе ставим 0
        if (SpellCaster.Instance != null)
        {
            HandleCharge(SpellCaster.Instance.ChargeA, SpellCaster.Instance.ChargeB);
        }
        else
        {
            HandleCharge(0f, 0f);
        }
    }

    private void OnDisable()
    {
        SpellCaster.OnComboChargeChanged -= HandleCharge;
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