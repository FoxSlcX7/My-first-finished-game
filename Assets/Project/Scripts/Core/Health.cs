using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;

    public event System.Action<int> OnDamaged;
    public event System.Action OnDeath;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }

    private bool _isPlayer;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        _isPlayer = CompareTag("Player");
    }

    private void Start()
    {
        if (_isPlayer)
        {
            GameEvents.OnHealthChanged?.Raise(CurrentHealth, MaxHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;

        OnDamaged?.Invoke(damage);
        GameEvents.OnDamaged?.Raise(damage);

        if (_isPlayer)
        {
            GameEvents.OnPlayerDamaged?.Raise(damage);
            GameEvents.OnHealthChanged?.Raise(CurrentHealth, MaxHealth);
        }

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();

        if (_isPlayer)
        {
            GameEvents.OnPlayerDied?.Raise();
            GameManager.Instance?.GameOver();

            if (TryGetComponent(out PlayerController controller)) controller.enabled = false;
            if (TryGetComponent(out Collider2D col)) col.enabled = false;

            if (TryGetComponent(out Rigidbody2D rb))
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.simulated = false;
            }

            Destroy(gameObject, 2f);
        }
        else
        {
            GameEvents.OnEnemyDied?.Raise();
        }
    }
}