using UnityEngine;

public class PlayerXP : MonoBehaviour
{
    public static PlayerXP Instance { get; private set; }

    [SerializeField] private int baseRequired = 5;
    [SerializeField] private int growthPerLevel = 3;

    public int Level { get; private set; } = 1;
    public int CurrentXP { get; private set; }
    public int RequiredXP => baseRequired + (Level - 1) * growthPerLevel;

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