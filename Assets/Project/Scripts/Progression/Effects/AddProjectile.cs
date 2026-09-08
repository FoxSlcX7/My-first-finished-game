using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Effects/Add Projectile")]
public class AddProjectileEffect : UpgradeEffectBase
{
    public override void Apply(PlayerStats stats, string source) =>
    stats.AddModifier(StatType.BonusProjectiles,
        new StatModifier(1f, StatModifier.ModifierType.Add, source));
}