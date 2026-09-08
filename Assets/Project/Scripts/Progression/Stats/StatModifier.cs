using UnityEngine;

/// <summary>
/// Модификатор стата. Может быть постоянным или временным.
/// </summary>
[System.Serializable]
public class StatModifier
{
    public enum ModifierType { Add, Multiply, FinalMultiply }

    public float Value { get; private set; }
    public ModifierType Type { get; private set; }
    public string Source { get; private set; } // Откуда пришёл (upgrade name, buff name)
    public float Duration { get; private set; } // 0 = постоянный, >0 = временный (секунды)
    public float StartTime { get; private set; } // Когда был добавлен (Time.time)

    public StatModifier(float value, ModifierType type, string source = "", float duration = 0f)
    {
        Value = value;
        Type = type;
        Source = string.IsNullOrEmpty(source) ? "Unknown" : source;
        Duration = duration;
        StartTime = Time.time;
    }

    /// <summary>
    /// Проверяет, истёк ли временный модификатор
    /// </summary>
    public bool IsExpired()
    {
        if (Duration <= 0f) return false; // Постоянный
        return Time.time >= StartTime + Duration;
    }

    public override string ToString()
    {
        string typeStr = Type switch
        {
            ModifierType.Add => "+",
            ModifierType.Multiply => "×",
            ModifierType.FinalMultiply => "×(final)",
            _ => "?"
        };
        return $"{Source}: {typeStr}{Value:F2}";
    }
}