using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Effects/Increase Speed")]
public class IncreaseSpeedEffect : UpgradeEffectBase
{
    public float percent = 0.1f;
    public override void Apply(PlayerStats stats, string source) =>
    stats.AddModifier(StatType.MoveSpeed,
        new StatModifier(percent, StatModifier.ModifierType.Multiply, source));
}