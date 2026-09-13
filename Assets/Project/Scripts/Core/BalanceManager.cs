using UnityEngine;

public class BalanceManager : MonoBehaviour
{
    public static BalanceManager Instance { get; private set; }

    [SerializeField] private BalanceConfig _currentConfig;

    // Текущий этаж подземелья (позже будем обновлять при переходе на новый уровень)
    public int CurrentFloor { get; private set; } = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetFloor(int floor)
    {
        CurrentFloor = floor;
    }

    public BalanceConfig Config => _currentConfig;
}