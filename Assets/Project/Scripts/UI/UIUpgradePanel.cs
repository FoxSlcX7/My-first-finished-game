using System;
using UnityEngine;

public class UIUpgradePanel : MonoBehaviour
{
    [SerializeField] private UIUpgradeCard[] cards;

    private Action<UpgradeDataSO> _onPick;

    private void Awake() => gameObject.SetActive(false);

    public void Show(UpgradeDataSO[] choices, Action<UpgradeDataSO> onPick)
    {
        _onPick = onPick;
        gameObject.SetActive(true);

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