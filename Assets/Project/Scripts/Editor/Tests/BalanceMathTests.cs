using NUnit.Framework;
using UnityEngine;

public class BalanceMathTests
{
    [Test]
    public void BalanceConfig_EnemyHealth_ScalesCorrectly()
    {
        // Arrange: Создаем конфиг прямо в памяти (без создания файла в проекте)
        BalanceConfig config = ScriptableObject.CreateInstance<BalanceConfig>();

        // Настраиваем линейную кривую от 1 до 10 этажа (множитель от 1.0 до 3.0)
        config.EnemyHealthCurve = AnimationCurve.Linear(1, 1f, 10, 3f);

        // Act
        float floor1 = config.GetEnemyHealthMultiplier(1);
        float floor10 = config.GetEnemyHealthMultiplier(10);
        float floor15 = config.GetEnemyHealthMultiplier(15); // Проверка clamp (ограничения за пределами графика)

        // Assert
        Assert.AreEqual(1f, floor1);
        Assert.AreEqual(3f, floor10);
        Assert.AreEqual(3f, floor15, "Множитель должен ограничиваться 10-м этажом (значением 3)");
    }
}