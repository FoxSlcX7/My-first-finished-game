using UnityEngine;
using UnityEngine.SceneManagement;

public static class StatsTracker
{
    private static bool _initialized;
    private static bool _isRunActive;
    private static float _runStartTime;
    private static int _runKills;

    public static int LastRunKills => _runKills;
    public static int LastRunEarnedStones { get; private set; }

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

        // Обработка закрытия приложения (Alt+F4, выход из игры, стоп в редакторе)
        Application.quitting += OnApplicationQuitting;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Игнорируем мирные сцены — в них забег не идет
        if (scene.name == "Hub" || scene.name == "Boot" || scene.name == "MainMenu")
        {
            if (_isRunActive)
            {
                FinalizeRun(isDeath: false);
            }
            _isRunActive = false;
            return;
        }

        // Если предыдущий забег не был завершен
        if (_isRunActive)
        {
            FinalizeRun(isDeath: false);
        }

        _runStartTime = Time.realtimeSinceStartup;
        _runKills = 0;
        LastRunEarnedStones = 0;
        _isRunActive = true;

        if (SaveSystem.Data != null)
        {
            SaveSystem.Data.runsPlayed++;
        }
    }

    private static void OnEnemyKilled()
    {
        _runKills++;
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
        FinalizeRun(isDeath: true);
    }

    private static void OnApplicationQuitting()
    {
        if (_isRunActive)
        {
            FinalizeRun(isDeath: false);
        }
        else
        {
            SaveSystem.Save();
        }
    }

    /// <summary>
    /// Единая точка завершения забега и начисления мета-валюты.
    /// </summary>
    private static void FinalizeRun(bool isDeath)
    {
        if (!_isRunActive || SaveSystem.Data == null) return;
        _isRunActive = false;

        float runDuration = Time.realtimeSinceStartup - _runStartTime;

        if (isDeath)
        {
            SaveSystem.Data.totalDeaths++;
        }
        SaveSystem.Data.totalPlayTime += runDuration;

        int floor = DungeonDirector.Instance != null ? DungeonDirector.Instance.Floor : 1;
        int timeMinutes = Mathf.FloorToInt(runDuration / 60f);

        // Защита от спам-перезапусков: не начисляем руны, если игрок сразу вышел без боя
        bool isValidRun = _runKills > 0 || floor > 1 || runDuration >= 15f;

        if (isValidRun)
        {
            LastRunEarnedStones = (floor * 15) + (_runKills / 2) + timeMinutes;
            SaveSystem.Data.runeStones += LastRunEarnedStones;
            Debug.Log($"[StatsTracker] Забег завершен (Смерть={isDeath}). Начислено Рунных Камней: {LastRunEarnedStones} (Всего: {SaveSystem.Data.runeStones})");
        }
        else
        {
            LastRunEarnedStones = 0;
            Debug.Log("[StatsTracker] Забег был прерван слишком быстро без активности, руны не начислены.");
        }

        SaveSystem.Save();
    }
}