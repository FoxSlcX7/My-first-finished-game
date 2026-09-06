using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Effects/Increase Speed")]
public class IncreaseSpeedEffect : UpgradeEffectBase
{
    public float percent = 0.1f;
    public override void Apply(PlayerStats stats) => stats.AddMoveSpeed(percent);
}