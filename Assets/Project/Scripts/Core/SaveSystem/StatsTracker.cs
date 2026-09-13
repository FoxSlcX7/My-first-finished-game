using UnityEngine;
using UnityEngine.SceneManagement;

public static class StatsTracker
{
    private static bool _initialized;
    private static float _runStartTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        if (_initialized) return;
        _initialized = true;

        SceneManager.sceneLoaded += OnSceneLoaded;
        GameEvents.OnEnemyDied.AddListener(OnEnemyKilled);
        GameEvents.OnPlayerDied.AddListener(OnPlayerDied);
        GameEvents.OnSlotAChanged.AddListener(OnSpellEquipped);
        GameEvents.OnSlotBChanged.AddListener(OnSpellEquipped);
        Application.quitting += SaveSystem.Save;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _runStartTime = Time.realtimeSinceStartup;
        if (SaveSystem.Data != null)
        {
            SaveSystem.Data.runsPlayed++;
        }
    }

    private static void OnEnemyKilled()
    {
        if (SaveSystem.Data != null)
            SaveSystem.Data.totalKills++;
    }

    private static void OnSpellEquipped(SpellSO spell)
    {
        if (spell != null)
            SaveSystem.UnlockSpell(spell.name);
    }

    private static void OnPlayerDied()
    {
        if (SaveSystem.Data != null)
        {
            SaveSystem.Data.totalDeaths++;
            SaveSystem.Data.totalPlayTime += Time.realtimeSinceStartup - _runStartTime;
            SaveSystem.Save();
        }
    }
}