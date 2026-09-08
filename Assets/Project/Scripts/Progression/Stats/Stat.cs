using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Стат с базовым значением и списком модификаторов.
/// Автоматически пересчитывает итоговое значение.
/// </summary>
public class Stat
{
    public StatType Type { get; private set; }
    public float BaseValue { get; private set; }
    public float ComputedValue { get; private set; }

    private readonly List<StatModifier> _modifiers = new();
    private bool _isDirty = true; // Флаг: нужно ли пересчитать

    public Stat(StatType type, float baseValue = 1f)
    {
        Type = type;
        BaseValue = baseValue;
        ComputedValue = baseValue;
    }

    /// <summary>
    /// Добавляет модификатор и помечает стат как "грязный"
    /// </summary>
    public void AddModifier(StatModifier modifier)
    {
        _modifiers.Add(modifier);
        _isDirty = true;
    }

    /// <summary>
    /// Удаляет модификатор по source (имени апгрейда/баффа)
    /// </summary>
    public void RemoveModifier(string source)
    {
        int removed = _modifiers.RemoveAll(m => m.Source == source);
        if (removed > 0) _isDirty = true;
    }

    /// <summary>
    /// Удаляет все истёкшие временные модификаторы.
    /// Вызывается каждый кадр из PlayerStats.
    /// </summary>
    public void RemoveExpiredModifiers()
    {
        int removed = _modifiers.RemoveAll(m => m.IsExpired());
        if (removed > 0) _isDirty = true;
    }

    /// <summary>
    /// Очищает все модификаторы (при смерти/рестарте)
    /// </summary>
    public void ClearModifiers()
    {
        _modifiers.Clear();
        _isDirty = true;
    }

    /// <summary>
    /// Возвращает текущее значение (пересчитывает если нужно)
    /// </summary>
    public float GetValue()
    {
        if (_isDirty)
        {
            Recalculate();
            _isDirty = false;
        }
        return ComputedValue;
    }

    /// <summary>
    /// Возвращает список всех активных модификаторов (для UI)
    /// </summary>
    public IReadOnlyList<StatModifier> GetModifiers() => _modifiers;

    /// <summary>
    /// Пересчитывает итоговое значение.
    /// Формула: (Base + Add) * Multiply * FinalMultiply
    /// </summary>
    private void Recalculate()
    {
        float total = BaseValue;
        float addSum = 0f;
        float multiplySum = 1f;
        float finalMultiplySum = 1f;

        foreach (var mod in _modifiers)
        {
            switch (mod.Type)
            {
                case StatModifier.ModifierType.Add:
                    addSum += mod.Value;
                    break;
                case StatModifier.ModifierType.Multiply:
                    multiplySum += mod.Value; // 0.1 = +10%
                    break;
                case StatModifier.ModifierType.FinalMultiply:
                    finalMultiplySum *= mod.Value; // 2.0 = x2
                    break;
            }
        }

        total = (BaseValue + addSum) * multiplySum * finalMultiplySum;
        ComputedValue = total;
    }

    /// <summary>
    /// Устанавливает базовое значение (используется при инициализации)
    /// </summary>
    public void SetBaseValue(float value)
    {
        BaseValue = value;
        _isDirty = true;
    }

    public override string ToString()
    {
        return $"{Type}: {GetValue():F2} (base: {BaseValue:F2}, mods: {_modifiers.Count})";
    }
}