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
        GameEvents.OnLevelUp.RemoveListener(OfferUpgrades);
        GameEvents.OnLevelUp.AddListener(OfferUpgrades);
    }

    private void OnDisable()
    {
        GameEvents.OnLevelUp?.RemoveListener(OfferUpgrades);
    }

    // ═══════════════════════════════════════
    // Debug-клавиши для тестирования редкости
    // ═══════════════════════════════════════
    private void Update()
    {
        if (!debugKeysEnabled || Keyboard.current == null) return;

        // U — принудительная выдача карточек
        if (Keyboard.current.uKey.wasPressedThisFrame)
            OfferUpgrades();

        // P — лог распределения редкостей (200 роллов, без открытия UI)
        if (Keyboard.current.pKey.wasPressedThisFrame)
            LogRarityDistribution(200);

        // L — дамп текущих множителей (проверка применения и сброса)
        if (Keyboard.current.lKey.wasPressedThisFrame && PlayerStats.Instance != null)
        {
            var s = PlayerStats.Instance;
            Debug.Log($"[StatsDebug] dmg={s.DamageMultiplier:F2} spd={s.MoveSpeedMultiplier:F2} cd={s.CooldownMultiplier:F2} proj={s.BonusProjectiles} rico={s.RicochetBounces} ls={s.LifeStealPercent:F2} chain={s.ChainTargets}");
        }
    }

    public void OfferUpgrades()
    {
        _pendingOffers++;
        if (!_showing) ShowNext();
    }

    private void ShowNext()
    {
        if (_pendingOffers <= 0 || allUpgrades == null || allUpgrades.Length == 0) return;

        _pendingOffers--;
        _showing = true;
        Time.timeScale = 0f;
        panel.Show(GetRandomChoices(3), Pick);
    }

    private void Pick(UpgradeDataSO upgrade)
    {
        if (upgrade.rarity == Rarity.Legendary)
            _takenLegendaries.Add(upgrade); // легендарки уникальны

        PlayerStats.Instance?.ApplyUpgrade(upgrade);
        panel.Hide();

        if (_pendingOffers > 0) ShowNext();
        else
        {
            _showing = false;
            Time.timeScale = 1f;
        }
    }

    // ═══════════════════════════════════════
    // Драфт: ролл редкости с учётом этажа, без повторов в одной выдаче
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

            // Несколько роллов редкости: если пул редкости пуст — роллим заново,
            // а не падаем сразу в «любой»
            for (int attempt = 0; attempt < 4 && pick == null; attempt++)
            {
                lastRoll = rarityConfig != null ? rarityConfig.Roll(floor) : Rarity.Common;
                pick = PickByRarity(lastRoll, inOffer);
            }

            // Фолбэки БЕЗ легендарок: легендарка только через прямой ролл
            if (pick == null) pick = PickAnyNonLegendary(inOffer);
            if (pick == null) pick = PickAny(inOffer);
            if (pick == null) break;

            Debug.Log($"[DraftDebug] card {i + 1}: roll={lastRoll} → pick={pick.upgradeName} ({pick.rarity})");

            result.Add(pick);
            inOffer.Add(pick);
        }
        return result.ToArray();
    }

    /// <summary>
    /// Выбор «что угодно» без легендарок — чтобы они не протекали
    /// через фолбэк чаще, чем разрешают веса конфига.
    /// </summary>
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

    /// <summary>
    // Если выпавшая редкость пуста — плавно падаем вниз (Common есть всегда).
    /// </summary>
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

    // ═══════════════════════════════════════
    // Debug: проверка распределения без открытия UI
    // ═══════════════════════════════════════
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