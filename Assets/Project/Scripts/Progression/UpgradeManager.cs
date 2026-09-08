using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [SerializeField] private UpgradeDataSO[] allUpgrades;
    [SerializeField] private UIUpgradePanel panel;
    [SerializeField] private RarityConfigSO rarityConfig;

    [Header("Debug (выключить перед релизом)")]
    [SerializeField] private bool debugKeysEnabled = true;

    private readonly HashSet<UpgradeDataSO> _takenLegendaries = new();
    private int _pendingOffers;
    private bool _showing;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable() => Subscribe();
    private void Start() => Subscribe();

    private void Subscribe()
    {
        if (GameEvents.OnLevelUp == null) return;
        GameEvents.OnLevelUp.RemoveListener(HandleLevelUp);
        GameEvents.OnLevelUp.AddListener(HandleLevelUp);
    }

    private void OnDisable()
    {
        GameEvents.OnLevelUp?.RemoveListener(HandleLevelUp);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // TAB — открыть выбор, если есть непотраченные уровни
        if (Keyboard.current.tabKey.wasPressedThisFrame)
            OpenUpgradePanel();

        if (!debugKeysEnabled) return;

        // U — debug: начислить очко апгрейда
        if (Keyboard.current.uKey.wasPressedThisFrame)
            HandleLevelUp();

        // P — лог распределения редкостей
        if (Keyboard.current.pKey.wasPressedThisFrame)
            LogRarityDistribution(200);

        // L — дамп множителей
        if (Keyboard.current.lKey.wasPressedThisFrame && PlayerStats.Instance != null)
        {
            var s = PlayerStats.Instance;
            Debug.Log($"[StatsDebug] dmg={s.DamageMultiplier:F2} spd={s.MoveSpeedMultiplier:F2} cd={s.CooldownMultiplier:F2} proj={s.BonusProjectiles} rico={s.RicochetBounces} ls={s.LifeStealPercent:F2} chain={s.ChainTargets}");
        }

        // B — временный бафф x2 урона на 5 сек
        if (Keyboard.current.bKey.wasPressedThisFrame && PlayerStats.Instance != null)
            PlayerStats.Instance.AddModifier(StatType.Damage,
                new StatModifier(1f, StatModifier.ModifierType.Multiply, "debug_buff", 5f));

        // V — разбивка статов
        if (Keyboard.current.vKey.wasPressedThisFrame && PlayerStats.Instance != null)
            PlayerStats.Instance.DebugLogBreakdown();
    }

    // ═══════════════════════════════════════
    // Level up теперь КОПИТСЯ, окно открывается по требованию
    // ═══════════════════════════════════════
    private void HandleLevelUp()
    {
        _pendingOffers++;
        RaisePending();
    }

    public bool HasPendingUpgrades => _pendingOffers > 0;

    /// <summary>Вызывается клавишей TAB и кнопкой HUD.</summary>
    public void OpenUpgradePanel()
    {
        if (_showing || _pendingOffers <= 0) return;
        if (Time.timeScale <= 0f) return; // игра на паузе чем-то другим (Game Over)
        ShowNext();
    }

    private void ShowNext()
    {
        if (_pendingOffers <= 0 || allUpgrades == null || allUpgrades.Length == 0) return;

        _pendingOffers--;
        RaisePending();
        _showing = true;
        Time.timeScale = 0f;
        panel.Show(GetRandomChoices(3), Pick);
    }

    private void Pick(UpgradeDataSO upgrade)
    {
        Debug.Log($"[DraftDebug] TAKEN: {upgrade.upgradeName} ({upgrade.rarity})");

        if (upgrade.rarity == Rarity.Legendary)
            _takenLegendaries.Add(upgrade);

        PlayerStats.Instance?.ApplyUpgrade(upgrade);
        panel.Hide();

        if (_pendingOffers > 0) ShowNext();
        else
        {
            _showing = false;
            Time.timeScale = 1f;
        }
    }

    private void RaisePending() => GameEvents.OnLevelUpPending?.Raise(_pendingOffers);

    // ═══════════════════════════════════════
    // Драфт карточек (без изменений)
    // ═══════════════════════════════════════
    private UpgradeDataSO[] GetRandomChoices(int count)
    {
        int floor = DungeonDirector.Instance != null ? DungeonDirector.Instance.Floor : 1;

        List<UpgradeDataSO> result = new List<UpgradeDataSO>();
        List<UpgradeDataSO> inOffer = new List<UpgradeDataSO>();

        for (int i = 0; i < count; i++)
        {
            UpgradeDataSO pick = null;
            Rarity lastRoll = Rarity.Common;

            for (int attempt = 0; attempt < 4 && pick == null; attempt++)
            {
                lastRoll = rarityConfig != null ? rarityConfig.Roll(floor) : Rarity.Common;
                pick = PickByRarity(lastRoll, inOffer);
            }

            if (pick == null) pick = PickAnyNonLegendary(inOffer);
            if (pick == null) pick = PickAny(inOffer);
            if (pick == null) break;

            Debug.Log($"[DraftDebug] OFFER card {i + 1}: roll={lastRoll} → {pick.upgradeName} ({pick.rarity})");

            result.Add(pick);
            inOffer.Add(pick);
        }
        return result.ToArray();
    }

    private UpgradeDataSO PickByRarity(Rarity rarity, List<UpgradeDataSO> exclude)
    {
        for (int r = (int)rarity; r >= 0; r--)
        {
            List<UpgradeDataSO> pool = new List<UpgradeDataSO>();
            foreach (var u in allUpgrades)
            {
                if (u == null || (int)u.rarity != r) continue;
                if (exclude.Contains(u)) continue;
                if (u.rarity == Rarity.Legendary && _takenLegendaries.Contains(u)) continue;
                pool.Add(u);
            }
            if (pool.Count > 0) return pool[Random.Range(0, pool.Count)];
        }
        return null;
    }

    private UpgradeDataSO PickAnyNonLegendary(List<UpgradeDataSO> exclude)
    {
        List<UpgradeDataSO> pool = new List<UpgradeDataSO>();
        foreach (var u in allUpgrades)
        {
            if (u == null || u.rarity == Rarity.Legendary || exclude.Contains(u)) continue;
            pool.Add(u);
        }
        return pool.Count > 0 ? pool[Random.Range(0, pool.Count)] : null;
    }

    private UpgradeDataSO PickAny(List<UpgradeDataSO> exclude)
    {
        List<UpgradeDataSO> pool = new List<UpgradeDataSO>();
        foreach (var u in allUpgrades)
        {
            if (u == null || exclude.Contains(u)) continue;
            if (u.rarity == Rarity.Legendary && _takenLegendaries.Contains(u)) continue;
            pool.Add(u);
        }
        return pool.Count > 0 ? pool[Random.Range(0, pool.Count)] : null;
    }

    private void LogRarityDistribution(int rolls)
    {
        if (rarityConfig == null)
        {
            Debug.LogWarning("[RarityDebug] rarityConfig не назначен!");
            return;
        }

        int floor = DungeonDirector.Instance != null ? DungeonDirector.Instance.Floor : 1;
        var counters = new Dictionary<Rarity, int>();

        for (int i = 0; i < rolls; i++)
        {
            Rarity r = rarityConfig.Roll(floor);
            counters.TryGetValue(r, out int c);
            counters[r] = c + 1;
        }

        string report = $"[RarityDebug] floor={floor}, rolls={rolls}: ";
        foreach (var kv in counters)
            report += $"{kv.Key}={kv.Value * 100f / rolls:F1}%  ";
        Debug.Log(report);
    }
}