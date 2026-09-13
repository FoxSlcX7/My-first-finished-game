using UnityEngine;

public class PlayerXP : MonoBehaviour
{
    public static PlayerXP Instance { get; private set; }

    [SerializeField] private BalanceConfig _balanceConfig;

    public int Level { get; private set; } = 1;
    public int CurrentXP { get; private set; }
    public int RequiredXP
    {
        get
        {
            if (_balanceConfig != null) return (int)_balanceConfig.GetXpForNextLevel(Level);
            return 5 + (Level - 1) * 3; // Базовый фоллбэк
        }
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Стартовое событие, чтобы UI сразу отрисовал 0/5
        GameEvents.OnXPChanged?.Raise(CurrentXP, RequiredXP);
    }

    public void AddXP(int amount)
    {
        CurrentXP += amount;

        while (CurrentXP >= RequiredXP)
        {
            CurrentXP -= RequiredXP;
            Level++;
            GameEvents.OnLevelUp?.Raise();
        }

        GameEvents.OnXPChanged?.Raise(CurrentXP, RequiredXP);
    }
}