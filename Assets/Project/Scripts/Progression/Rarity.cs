using UnityEngine;

public enum Rarity { Common, Rare, Epic, Legendary }

/// <summary>
/// Презентация для UI. Баланс (веса) живёт в RarityConfigSO.
/// </summary>
public static class RarityExtensions
{
    public static Color GetColor(this Rarity rarity) => rarity switch
    {
        Rarity.Rare => new Color(0.25f, 0.6f, 1f),
        Rarity.Epic => new Color(0.7f, 0.3f, 1f),
        Rarity.Legendary => new Color(1f, 0.75f, 0.15f),
        _ => new Color(0.75f, 0.75f, 0.75f)
    };

    public static string GetDisplayName(this Rarity rarity) => rarity switch
    {
        Rarity.Rare => "Редкий",
        Rarity.Epic => "Эпический",
        Rarity.Legendary => "Легендарный",
        _ => "Обычный"
    };
}