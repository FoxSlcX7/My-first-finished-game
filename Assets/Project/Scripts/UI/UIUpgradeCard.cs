using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIUpgradeCard : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;     // если у тебя TextMeshPro — замени тип
    [SerializeField] private TextMeshProUGUI description;

    private Button _button;
    private UpgradeDataSO _data;
    private Action<UpgradeDataSO> _onPick;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() => _onPick?.Invoke(_data));
    }

    public void Setup(UpgradeDataSO data, Action<UpgradeDataSO> onPick)
    {
        _data = data;
        _onPick = onPick;
        gameObject.SetActive(true);

        if (icon != null) { icon.sprite = data.icon; icon.enabled = data.icon != null; }
        if (title != null) title.text = data.upgradeName;
        if (description != null) description.text = data.description;
    }
}