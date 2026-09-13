using NUnit.Framework;
using UnityEngine;

public class StatsMathTests
{
    [Test]
    public void Stat_Calculates_AddAndMultiply_Correctly()
    {
        // Arrange: Создаем стат базового урона = 10
        Stat damageStat = new Stat(StatType.Damage, 10f);

        // Act: Добавляем +5 плоского урона и множитель +50% (x1.5)
        damageStat.AddModifier(new StatModifier(5f, StatModifier.ModifierType.Add, "SharpBlade"));
        damageStat.AddModifier(new StatModifier(0.5f, StatModifier.ModifierType.Multiply, "StrengthPotion"));

        // Формула из Stat.cs: (Base + AddSum) * MultiplySum * FinalMultiplySum
        // Ожидаем: (10 + 5) * (1 + 0.5) = 15 * 1.5 = 22.5

        // Assert: Проверяем, совпадает ли ожидание с реальностью
        Assert.AreEqual(22.5f, damageStat.GetValue());
    }

    [Test]
    public void Stat_Removes_Modifier_Correctly()
    {
        // Arrange
        Stat speedStat = new Stat(StatType.MoveSpeed, 5f);
        speedStat.AddModifier(new StatModifier(2f, StatModifier.ModifierType.Add, "Boots"));

        // Убеждаемся, что бафф применился
        Assert.AreEqual(7f, speedStat.GetValue());

        // Act
        speedStat.RemoveModifier("Boots");

        // Assert
        Assert.AreEqual(5f, speedStat.GetValue());
    }

    [Test]
    public void Stat_FinalMultiply_Overrides_Correctly()
    {
        // Arrange
        Stat cooldownStat = new Stat(StatType.Cooldown, 1f);
        cooldownStat.AddModifier(new StatModifier(0.2f, StatModifier.ModifierType.Add, "SmallBuff"));

        // Act: Добавляем финальный множитель 0.5 (срезаем кулдаун вдвое)
        cooldownStat.AddModifier(new StatModifier(0.5f, StatModifier.ModifierType.FinalMultiply, "HalfCooldownCombo"));

        // Ожидаем: (1 + 0.2) * 1 * 0.5 = 0.6

        // Assert
        Assert.AreEqual(0.6f, cooldownStat.GetValue());
    }
}