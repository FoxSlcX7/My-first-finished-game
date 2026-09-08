using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Effects/Legendary Ricochet")]
public class LegendaryRicochet : UpgradeEffectBase
{
    public int bounces = 2;

    public override void Apply(PlayerStats stats, string source) =>
        stats.AddModifier(StatType.RicochetBounces,
            new StatModifier(bounces, StatModifier.ModifierType.Add, source));
}