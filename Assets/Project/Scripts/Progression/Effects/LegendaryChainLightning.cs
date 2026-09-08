using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Effects/Legendary Chain Lightning")]
public class LegendaryChainLightning : UpgradeEffectBase
{
    public int targets = 2;
    [Range(0.1f, 1f)] public float damagePercent = 0.5f;
    public override void Apply(PlayerStats stats, string source)
    {
        stats.AddModifier(StatType.ChainTargets,
            new StatModifier(targets, StatModifier.ModifierType.Add, source));

        // процент цепи задаём дельтой от базы (0.5), итог = damagePercent
        stats.AddModifier(StatType.ChainDamage,
            new StatModifier(damagePercent - stats.GetStatBase(StatType.ChainDamage),
                StatModifier.ModifierType.Add, source));
    }
}