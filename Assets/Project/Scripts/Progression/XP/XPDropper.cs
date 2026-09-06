using UnityEngine;

public class XPDropper : MonoBehaviour
{
    public static XPDropper Instance { get; private set; }

    [SerializeField] private GameObject xpOrbPrefab;
    [SerializeField] private int orbsPerKill = 1;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Drop(Vector2 position)
    {
        if (xpOrbPrefab == null) return;
        for (int i = 0; i < orbsPerKill; i++)
        {
            Vector2 jitter = Random.insideUnitCircle * 0.3f;
            Instantiate(xpOrbPrefab, position + jitter, Quaternion.identity);
        }
    }
}