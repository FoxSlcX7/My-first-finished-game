using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Центральный хаб статов забега: база + модификаторы по каждому StatType.
/// Формула: (Base + SumAdd) * SumMultiply * FinalMultiply.
/// Внешний код работает через compat-слой (старые свойства).
/// </summary>
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    private readonly Dictionary<StatType, Stat> _stats = new();

    private static readonly Dictionary<StatType, float> BaseValues = new()
    {
        { StatType.Damage, 1f },
        { StatType.MoveSpeed, 1f },
        { StatType.Cooldown, 1f },
        { StatType.BonusProjectiles, 0f },
        { StatType.RicochetBounces, 0f },
        { StatType.LifeSteal, 0f },
        { StatType.ChainTargets, 0f },
        { StatType.ChainDamage, 0.5f },
        { StatType.CritChance, 0f },
        { StatType.CritDamage, 1.5f },
        { StatType.Armor, 0f },
        { StatType.PickupRadius, 1f },
        { StatType.XPBoost, 1f },
    };

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        foreach (var kv in BaseValues)
            _stats[kv.Key] = new Stat(kv.Key, kv.Value);
    }

    private void Update()
    {
        foreach (var stat in _stats.Values)
            stat.RemoveExpiredModifiers();
    }

    // ═══════════════════════════════════════
    // API системы модификаторов
    // ═══════════════════════════════════════

    public float GetStat(StatType type) => GetOrCreate(type).GetValue();
    public float GetStatBase(StatType type) => GetOrCreate(type).BaseValue;
    public Stat GetStatObject(StatType type) => GetOrCreate(type);

    public void AddModifier(StatType type, StatModifier modifier) => GetOrCreate(type).AddModifier(modifier);
    public void RemoveModifier(StatType type, string source) => GetOrCreate(type).RemoveModifier(source);

    public void RemoveModifiersBySource(string source)
    {
        foreach (var stat in _stats.Values)
            stat.RemoveModifier(source);
    }

    public void ClearAllModifiers()
    {
        foreach (var stat in _stats.Values)
            stat.ClearModifiers();
    }

    public void ApplyUpgrade(UpgradeDataSO upgrade)
    {
        if (upgrade != null && upgrade.effect != null)
            upgrade.effect.Apply(this, upgrade.upgradeName); // source = имя апгрейда
    }

    public static int ScaleDamage(int baseDamage)
    {
        if (Instance == null) return baseDamage;
        return Mathf.Max(1, Mathf.RoundToInt(baseDamage * Instance.GetStat(StatType.Damage)));
    }

    /// <summary>Debug (клавиша V): разбивка статов с источниками модификаторов.</summary>
    public void DebugLogBreakdown()
    {
        bool any = false;
        foreach (var kv in _stats)
        {
            var mods = kv.Value.GetModifiers();
            if (mods.Count == 0) continue;
            any = true;

            string line = $"[StatsBreakdown] {kv.Key}: base={kv.Value.BaseValue:F2} → {kv.Value.GetValue():F2} | ";
            foreach (var m in mods) line += m + "; ";
            Debug.Log(line);
        }
        if (!any) Debug.Log("[StatsBreakdown] модификаторов нет");
    }

    private Stat GetOrCreate(StatType type)
    {
        if (!_stats.TryGetValue(type, out var stat))
        {
            stat = new Stat(type, 0f);
            _stats[type] = stat;
        }
        return stat;
    }

    // ═══════════════════════════════════════
    // COMPAT-СЛОЙ: потребители (SpellCaster, PlayerController,
    // Projectile) работают без правок. Уберём на Неделе 8.
    // ═══════════════════════════════════════

    public float DamageMultiplier => GetStat(StatType.Damage);
    public float MoveSpeedMultiplier => GetStat(StatType.MoveSpeed);
    public float CooldownMultiplier => Mathf.Max(0.2f, GetStat(StatType.Cooldown));
    public int BonusProjectiles => Mathf.RoundToInt(GetStat(StatType.BonusProjectiles));
    public int RicochetBounces => Mathf.RoundToInt(GetStat(StatType.RicochetBounces));
    public float LifeStealPercent => GetStat(StatType.LifeSteal);
    public int ChainTargets => Mathf.RoundToInt(GetStat(StatType.ChainTargets));
    public float ChainDamagePercent => GetStat(StatType.ChainDamage);
}