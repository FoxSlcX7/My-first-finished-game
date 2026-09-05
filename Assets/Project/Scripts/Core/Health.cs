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

            // Полностью останавливаем физику, чтобы труп не улетел
            if (TryGetComponent(out Rigidbody2D rb))
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.simulated = false; // ⭐ Ключевая строка
            }

            Destroy(gameObject, 2f);
        }
        else
        {
            GameEvents.OnEnemyDied?.Raise();

            if (TryGetComponent(out Collider2D col)) col.enabled = false;

            // Отключаем все скрипты врага (AI, Knockback и т.д.)
            MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                if (script is Animator || script is Health) continue;
                script.enabled = false;
            }

            // Полностью останавливаем физику
            if (TryGetComponent(out Rigidbody2D rb))
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.simulated = false; // ⭐ Ключевая строка
            }

            Destroy(gameObject, 1.5f);
        }
    }
}