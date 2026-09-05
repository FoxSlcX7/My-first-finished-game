using UnityEngine;

public static class GameEvents
{
    // Здоровье
    public static HealthGameEvent OnHealthChanged;
    public static IntGameEvent OnPlayerDamaged;
    public static IntGameEvent OnDamaged;
    public static GameEvent OnPlayerDied;
    public static GameEvent OnEnemyDied;

    // Заклинания
    public static SpellGameEvent OnSpellCast;
    public static SpellGameEvent OnSlotAChanged;
    public static SpellGameEvent OnSlotBChanged;

    // Комбо
    public static ComboGameEvent OnComboStateChanged;
    public static ComboGameEvent OnComboCast;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        OnHealthChanged = Resources.Load<HealthGameEvent>("Events/OnHealthChangedEvent");
        OnPlayerDamaged = Resources.Load<IntGameEvent>("Events/OnPlayerDamagedEvent");
        OnDamaged = Resources.Load<IntGameEvent>("Events/OnDamagedEvent");
        OnPlayerDied = Resources.Load<GameEvent>("Events/OnPlayerDied");
        OnEnemyDied = Resources.Load<GameEvent>("Events/OnEnemyDiedEvent");
        OnSpellCast = Resources.Load<SpellGameEvent>("Events/OnSpellCast");
        OnSlotAChanged = Resources.Load<SpellGameEvent>("Events/OnSlotAChanged");
        OnSlotBChanged = Resources.Load<SpellGameEvent>("Events/OnSlotBChanged");
        OnComboStateChanged = Resources.Load<ComboGameEvent>("Events/OnComboStateChanged");
        OnComboCast = Resources.Load<ComboGameEvent>("Events/OnComboCast");

        Verify("OnHealthChangedEvent", OnHealthChanged);
        Verify("OnPlayerDamagedEvent", OnPlayerDamaged);
        Verify("OnDamagedEvent", OnDamaged);
        Verify("OnPlayerDied", OnPlayerDied);
        Verify("OnEnemyDiedEvent", OnEnemyDied);
        Verify("OnSpellCast", OnSpellCast);
        Verify("OnSlotAChanged", OnSlotAChanged);
        Verify("OnSlotBChanged", OnSlotBChanged);
        Verify("OnComboStateChanged", OnComboStateChanged);
        Verify("OnComboCast", OnComboCast);

        Debug.Log("✅ GameEvents: все события загружены");
    }

    private static void Verify(string name, Object asset)
    {
        if (asset == null)
        {
            Debug.LogError($"❌ Не загрузилось событие: {name}");
        }
    }
}