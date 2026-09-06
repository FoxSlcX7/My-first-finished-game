using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Effects/Increase Damage")]
public class IncreaseDamageEffect : UpgradeEffectBase
{
    public float percent = 0.15f;
    public override void Apply(PlayerStats stats) => stats.AddDamage(percent);
}