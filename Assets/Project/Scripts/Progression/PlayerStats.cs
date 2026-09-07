using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    public float DamageMultiplier { get; private set; } = 1f;
    public float MoveSpeedMultiplier { get; private set; } = 1f;
    public float CooldownMultiplier { get; private set; } = 1f;
    public int BonusProjectiles { get; private set; }

    [Header("Легендарные механики")]
    public int RicochetBounces { get; private set; }
    public float LifeStealPercent { get; private set; }
    public int ChainTargets { get; private set; }
    public float ChainDamagePercent { get; private set; } = 0.5f;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ApplyUpgrade(UpgradeDataSO upgrade)
    {
        if (upgrade != null && upgrade.effect != null)
            upgrade.effect.Apply(this);
    }

    public static int ScaleDamage(int baseDamage)
    {
        if (Instance == null) return baseDamage;
        return Mathf.Max(1, Mathf.RoundToInt(baseDamage * Instance.DamageMultiplier));
    }

    public void AddDamage(float percent) => DamageMultiplier += percent;
    public void AddMoveSpeed(float percent) => MoveSpeedMultiplier += percent;
    public void ReduceCooldown(float percent) => CooldownMultiplier = Mathf.Max(0.2f, CooldownMultiplier - percent);
    public void AddProjectile() => BonusProjectiles++;

    public void AddRicochet(int bounces) => RicochetBounces += bounces;
    public void AddLifeSteal(float percent) => LifeStealPercent += percent;
    public void AddChain(int targets, float damagePercent)
    {
        ChainTargets += targets;
        ChainDamagePercent = damagePercent;
    }
}