using System;
using UnityEngine;

public class UIUpgradePanel : MonoBehaviour
{
    [SerializeField] private UIUpgradeCard[] cards;

    private Action<UpgradeDataSO> _onPick;

    // ВАЖНО: панель должна быть ВЫКЛЮЧЕНА в сцене (галочка off) — это её начальное состояние.
    // Самоскрытия в Awake/Start НЕТ: у выключенного объекта они не выполняются,
    // а при первой активации выстрелили бы посреди Show и погасили панель.
    // Show/Hide полностью управляют активностью.

    public void Show(UpgradeDataSO[] choices, Action<UpgradeDataSO> onPick)
    {
        _onPick = onPick;
        gameObject.SetActive(true); // строго до настройки карточек

        for (int i = 0; i < cards.Length; i++)
        {
            if (i < choices.Length)
                cards[i].Setup(choices[i], Pick);
            else
                cards[i].gameObject.SetActive(false);
        }
    }

    public void Hide() => gameObject.SetActive(false);

    private void Pick(UpgradeDataSO upgrade) => _onPick?.Invoke(upgrade);
}