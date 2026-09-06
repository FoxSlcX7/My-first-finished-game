using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Effects/Add Projectile")]
public class AddProjectileEffect : UpgradeEffectBase
{
    public override void Apply(PlayerStats stats) => stats.AddProjectile();
}