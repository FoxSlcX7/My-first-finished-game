using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMetaUpgradeCard : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button buyButton;

    private MetaUpgradeSO _upgrade;
    private Action _onBuyCallback;

    public void Setup(MetaUpgradeSO upgrade, Action onBuyCallback)
    {
        _upgrade = upgrade;
        _onBuyCallback = onBuyCallback;

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(HandleBuyClick);
        }

        UpdateVisuals();
    }

    private void HandleBuyClick()
    {
        if (MetaProgressionManager.Instance != null && MetaProgressionManager.Instance.TryBuyUpgrade(_upgrade))
        {
            _onBuyCallback?.Invoke();
            UpdateVisuals();
        }
    }

    public void UpdateVisuals()
    {
        if (_upgrade == null || MetaProgressionManager.Instance == null) return;

        int currentLvl = MetaProgressionManager.Instance.GetUpgradeLevel(_upgrade);
        bool isMax = MetaProgressionManager.Instance.IsMaxLevel(_upgrade);
        bool canAfford = MetaProgressionManager.Instance.CanAfford(_upgrade);

        if (iconImage != null)
        {
            iconImage.sprite = _upgrade.icon;
            iconImage.enabled = _upgrade.icon != null;
        }

        if (titleText != null) titleText.text = _upgrade.upgradeName;
        if (descriptionText != null) descriptionText.text = _upgrade.description;
        if (levelText != null) levelText.text = $"Ур. {currentLvl} / {_upgrade.maxLevel}";

        if (buyButton != null)
        {
            buyButton.interactable = !isMax && canAfford;
        }

        if (costText != null)
        {
            costText.text = isMax ? "МАКС" : $"{_upgrade.GetCost(currentLvl)} Рун";
        }
    }
}