using UnityEngine;

public class XPDropper : MonoBehaviour
{
    public static XPDropper Instance { get; private set; }

    [SerializeField] private GameObject xpOrbPrefab;
    [SerializeField] private int orbsPerKill = 1;

    [Header("Balance")]
    [SerializeField] private BalanceConfig balanceConfig;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Drop(Vector2 position)
    {
        if (xpOrbPrefab == null) return;

        int floor = DungeonDirector.Instance != null ? DungeonDirector.Instance.Floor : 1;
        float multiplier = balanceConfig != null ? balanceConfig.GetEnemyXpMultiplier(floor) : 1f;

        for (int i = 0; i < orbsPerKill; i++)
        {
            Vector2 jitter = Random.insideUnitCircle * 0.3f;
            GameObject orb = Instantiate(xpOrbPrefab, position + jitter, Quaternion.identity);

            if (multiplier > 1f && orb.TryGetComponent<XPOrb>(out var xpOrb))
            {
                xpOrb.MultiplyValue(multiplier);
            }
        }
    }
}