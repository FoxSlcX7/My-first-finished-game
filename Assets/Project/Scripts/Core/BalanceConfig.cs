using UnityEngine;

[CreateAssetMenu(fileName = "NewBalanceConfig", menuName = "Progression/Balance Config")]
public class BalanceConfig : ScriptableObject
{
    [Header("Enemy Scaling (Ось X - номер этажа)")]
    [Tooltip("Множитель здоровья врагов")]
    public AnimationCurve EnemyHealthCurve = AnimationCurve.Linear(1, 1f, 10, 3f);

    [Tooltip("Множитель урона врагов")]
    public AnimationCurve EnemyDamageCurve = AnimationCurve.Linear(1, 1f, 10, 2.5f);

    [Header("Player Progression (Ось X - текущий уровень)")]
    [Tooltip("Сколько XP нужно для получения следующего уровня")]
    public AnimationCurve XpRequirementCurve = AnimationCurve.EaseInOut(1, 100, 20, 2000);

    [Header("XP Drop Scaling")]
    [Tooltip("Множитель выпадающего опыта с врагов по этажам")]
    public AnimationCurve EnemyXpDropCurve = AnimationCurve.Linear(1, 1f, 10, 4f);

    // Методы для получения значений по кривой
    public float GetEnemyHealthMultiplier(int floorIndex)
    {
        // Ограничиваем этаж, чтобы не уйти за пределы графика (например, от 1 до 10)
        return EnemyHealthCurve.Evaluate(Mathf.Clamp(floorIndex, 1, 10));
    }

    public float GetEnemyDamageMultiplier(int floorIndex)
    {
        return EnemyDamageCurve.Evaluate(Mathf.Clamp(floorIndex, 1, 10));
    }

    public int GetXpForNextLevel(int currentLevel)
    {
        // Уровень от 1 до 20 (или твой кап)
        return Mathf.RoundToInt(XpRequirementCurve.Evaluate(Mathf.Clamp(currentLevel, 1, 20)));
    }

    public float GetEnemyXpMultiplier(int floor)
    {
        return EnemyXpDropCurve.Evaluate(Mathf.Clamp(floor, 1, 10));
    }
}