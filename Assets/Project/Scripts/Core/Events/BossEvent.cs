using System;
using UnityEngine;

public static class BossEvents
{
    // Событие спавна босса: передает имя и компонент здоровья
    public static event Action<string, Health> OnBossSpawned;

    // Смена фазы (1, 2, 3) для UI и аудио
    public static event Action<int> OnBossPhaseChanged;

    // Победа над боссом
    public static event Action OnBossDefeated;

    public static void TriggerBossSpawned(string bossName, Health health) => OnBossSpawned?.Invoke(bossName, health);
    public static void TriggerPhaseChanged(int newPhase) => OnBossPhaseChanged?.Invoke(newPhase);
    public static void TriggerBossDefeated() => OnBossDefeated?.Invoke();
}