using UnityEngine;

/// <summary>
/// Strategy-эффект апгрейда. Данные и логика разделены,
/// как в SpellEffectBase у заклинаний.
/// </summary>
public abstract class UpgradeEffectBase : ScriptableObject
{
    public abstract void Apply(PlayerStats stats);
}