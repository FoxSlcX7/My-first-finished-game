using UnityEngine;

[CreateAssetMenu(fileName = "NewMetaUpgrade", menuName = "Progression/Meta Upgrade")]
public class MetaUpgradeSO : ScriptableObject
{
    [Header("Идентификация")]
    public string id;
    public string upgradeName;
    [TextArea(2, 3)] public string description;
    public Sprite icon;

    [Header("Параметры прокачки")]
    public int maxLevel = 5;
    public int baseCost = 20;
    [Tooltip("Во сколько раз увеличивается цена с каждым уровнем")]
    public float costMultiplier = 1.5f;

    [Header("Влияние на характеристику")]
    public StatType statType;
    public StatModifier.ModifierType modifierType = StatModifier.ModifierType.Add;
    [Tooltip("Прибавка к стату за каждый уровень прокачки (например, 0.05 для +5% урона)")]
    public float valuePerLevel = 0.05f;

    public int GetCost(int currentLevel)
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
    }
}