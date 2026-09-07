using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Effects/Legendary Vampirism")]
public class LegendaryVampirism : UpgradeEffectBase
{
    [Tooltip("Процент нанесённого урона возвращается как HP.")]
    public float percent = 0.15f;
    public override void Apply(PlayerStats stats) => stats.AddLifeSteal(percent);
}