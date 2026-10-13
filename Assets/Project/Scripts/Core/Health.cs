using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    [Tooltip("Длительность неуязвимости игрока после получения урона (i-frames в секундах).")]
    [SerializeField] private float invulnerabilityDuration = 0.5f;

    public event System.Action<int> OnDamaged;
    public event System.Action OnDeath;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsDead => _isDead;
    public bool IsInvulnerable => _isInvulnerable;

    private bool _isPlayer;
    private bool _isDead;
    private bool _isInvulnerable;
    private SpriteRenderer _spriteRenderer;
    private Coroutine _invulnerabilityRoutine;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        _isPlayer = CompareTag("Player");
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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
        if (_isDead || damage <= 0) return;

        // Если игрок во фреймах неуязвимости — урон не проходит
        if (_isPlayer && _isInvulnerable) return;

        int finalDamage = damage;
        // Учет брони игрока
        if (_isPlayer && PlayerStats.Instance != null)
        {
            finalDamage = Mathf.Max(1, Mathf.RoundToInt(damage * (1f - PlayerStats.Instance.Armor)));
        }

        CurrentHealth -= finalDamage;

        OnDamaged?.Invoke(finalDamage);
        GameEvents.OnDamaged?.Raise(finalDamage);

        if (_isPlayer)
        {
            GameEvents.OnPlayerDamaged?.Raise(finalDamage);
            GameEvents.OnHealthChanged?.Raise(CurrentHealth, MaxHealth);
        }

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
        else if (_isPlayer && invulnerabilityDuration > 0f)
        {
            // Запуск i-frames после получения урона
            TriggerInvulnerability();
        }
    }

    /// <summary>
    /// Запуск неуязвимости на заданное время (или по умолчанию).
    /// Можно вызывать из внешних скриптов (например, при рывке или парировании).
    /// </summary>
    public void TriggerInvulnerability(float duration = -1f)
    {
        if (_isDead) return;

        float actualDuration = duration > 0f ? duration : invulnerabilityDuration;
        if (actualDuration <= 0f) return;

        if (_invulnerabilityRoutine != null)
            StopCoroutine(_invulnerabilityRoutine);

        _invulnerabilityRoutine = StartCoroutine(InvulnerabilityRoutine(actualDuration));
    }

    private IEnumerator InvulnerabilityRoutine(float duration)
    {
        _isInvulnerable = true;
        float elapsed = 0f;
        float flashInterval = 0.08f;
        bool visible = true;

        while (elapsed < duration)
        {
            if (_isDead) yield break;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;

            visible = !visible;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = visible;
            }
        }

        ResetInvulnerabilityVisuals();
        _isInvulnerable = false;
        _invulnerabilityRoutine = null;
    }

    private void ResetInvulnerabilityVisuals()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = true;
        }
    }

    public void Heal(int amount)
    {
        if (_isDead || amount <= 0) return;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        if (_isPlayer)
            GameEvents.OnHealthChanged?.Raise(CurrentHealth, MaxHealth);
    }

    public void Initialize(int maxHp)
    {
        maxHealth = maxHp;
        CurrentHealth = maxHp;
        _isDead = false;
        _isInvulnerable = false;

        if (_invulnerabilityRoutine != null)
        {
            StopCoroutine(_invulnerabilityRoutine);
            _invulnerabilityRoutine = null;
        }
        ResetInvulnerabilityVisuals();
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        if (_invulnerabilityRoutine != null)
        {
            StopCoroutine(_invulnerabilityRoutine);
            _invulnerabilityRoutine = null;
        }
        ResetInvulnerabilityVisuals();

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