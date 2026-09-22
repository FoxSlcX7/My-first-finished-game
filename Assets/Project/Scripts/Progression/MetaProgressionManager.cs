using System;
using UnityEngine;

public class MetaProgressionManager : MonoBehaviour
{
    public static MetaProgressionManager Instance { get; private set; }

    [Header("Каталог улучшений")]
    [SerializeField] private MetaUpgradeSO[] availableUpgrades;

    public event Action OnRuneStonesChanged;
    public event Action<MetaUpgradeSO, int> OnUpgradePurchased;

    public MetaUpgradeSO[] AvailableUpgrades => availableUpgrades;
    public int RuneStones => SaveSystem.Data != null ? SaveSystem.Data.runeStones : 0;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public int GetUpgradeLevel(MetaUpgradeSO upgrade)
    {
        if (upgrade == null || SaveSystem.Data == null) return 0;
        return SaveSystem.Data.GetUpgradeLevel(upgrade.id);
    }

    public bool IsMaxLevel(MetaUpgradeSO upgrade)
    {
        if (upgrade == null) return true;
        return GetUpgradeLevel(upgrade) >= upgrade.maxLevel;
    }

    public bool CanAfford(MetaUpgradeSO upgrade)
    {
        if (upgrade == null || IsMaxLevel(upgrade)) return false;
        int cost = upgrade.GetCost(GetUpgradeLevel(upgrade));
        return RuneStones >= cost;
    }

    public bool TryBuyUpgrade(MetaUpgradeSO upgrade)
    {
        if (!CanAfford(upgrade)) return false;

        int currentLvl = GetUpgradeLevel(upgrade);
        int cost = upgrade.GetCost(currentLvl);

        SaveSystem.Data.runeStones -= cost;
        int nextLvl = currentLvl + 1;
        SaveSystem.Data.SetUpgradeLevel(upgrade.id, nextLvl);

        SaveSystem.Save();

        OnRuneStonesChanged?.Invoke();
        OnUpgradePurchased?.Invoke(upgrade, nextLvl);

        Debug.Log($"[MetaProgression] Куплен {upgrade.upgradeName} ур. {nextLvl}. Потрачено: {cost}, остаток: {SaveSystem.Data.runeStones}");
        return true;
    }
}