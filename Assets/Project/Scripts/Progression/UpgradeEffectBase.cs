using UnityEngine;

/// <summary>
/// Strategy-эффект апгрейда. Данные и логика разделены,
/// как в SpellEffectBase у заклинаний.
/// source — имя апгрейда: все модификаторы помечаются им,
/// чтобы UI показывал разбивку, а эффекты снимались точечно.
/// </summary>
public abstract class UpgradeEffectBase : ScriptableObject
{
    public abstract void Apply(PlayerStats stats, string source);
}