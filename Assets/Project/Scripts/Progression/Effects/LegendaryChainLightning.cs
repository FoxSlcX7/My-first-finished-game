using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Effects/Legendary Chain Lightning")]
public class LegendaryChainLightning : UpgradeEffectBase
{
    public int targets = 2;
    [Range(0.1f, 1f)] public float damagePercent = 0.5f;
    public override void Apply(PlayerStats stats) => stats.AddChain(targets, damagePercent);
}