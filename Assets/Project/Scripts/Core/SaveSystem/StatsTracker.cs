using UnityEngine;

public static class StatsTracker
{
    private static bool _subscribed;
    private static float _runStartTime;

    // Старт каждого забега
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnSceneLoaded()
    {
        _runStartTime = Time.realtimeSinceStartup;
        SaveSystem.Data.runsPlayed++;

        // Подписываемся ОДИН раз — иначе при перезагрузке сцены
        // слушатели задублируются и статистика будет считаться дважды!
        if (!_subscribed)
        {
            _subscribed = true;
            GameEvents.OnEnemyDied.AddListener(OnEnemyKilled);
            GameEvents.OnPlayerDied.AddListener(OnPlayerDied);
            GameEvents.OnSlotAChanged.AddListener(OnSpellEquipped);
            GameEvents.OnSlotBChanged.AddListener(OnSpellEquipped);
            Application.quitting += SaveSystem.Save; // сохранение при выходе
        }
    }

    private static void OnEnemyKilled()
    {
        SaveSystem.Data.totalKills++;
    }

    private static void OnSpellEquipped(SpellSO spell)
    {
        if (spell != null)
        {
            SaveSystem.UnlockSpell(spell.name);
        }
    }

    private static void OnPlayerDied()
    {
        SaveSystem.Data.totalDeaths++;
        SaveSystem.Data.totalPlayTime += Time.realtimeSinceStartup - _runStartTime;
        SaveSystem.Save();
    }
}