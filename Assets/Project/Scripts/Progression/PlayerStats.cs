using UnityEngine;

/// <summary>
/// Множители забега. Живёт на игроке. Сбрасывается только перезапуском сцены.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    public float DamageMultiplier { get; private set; } = 1f;
    public float MoveSpeedMultiplier { get; private set; } = 1f;
    public float CooldownMultiplier { get; private set; } = 1f; // меньше = быстрее
    public int BonusProjectiles { get; private set; }

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

    /// <summary>
    /// Масштабирует урон множителем забега. Используют SpellEffect-классы.
    /// </summary>
    public static int ScaleDamage(int baseDamage)
    {
        if (Instance == null) return baseDamage;
        return Mathf.Max(1, Mathf.RoundToInt(baseDamage * Instance.DamageMultiplier));
    }

    // Вызываются эффектами
    public void AddDamage(float percent) => DamageMultiplier += percent;
    public void AddMoveSpeed(float percent) => MoveSpeedMultiplier += percent;
    public void ReduceCooldown(float percent) => CooldownMultiplier = Mathf.Max(0.2f, CooldownMultiplier - percent);
    public void AddProjectile() => BonusProjectiles++;
}