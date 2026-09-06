using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Effects/Reduce Cooldown")]
public class ReduceCooldownEffect : UpgradeEffectBase
{
    public float percent = 0.1f;
    public override void Apply(PlayerStats stats) => stats.ReduceCooldown(percent);
}