using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyDataSO data;

    private Health _health;
    private Rigidbody2D _rb;
    private Transform _player;
    private IEnemyState _currentState;
    private float _lastDamageTime;
    private float _nextShootTime;

    // Публичные свойства для состояний
    public EnemyDataSO Data => data;
    public Health Health => _health;
    public Rigidbody2D Rb => _rb;
    public Transform Player => _player;
    public float LastDamageTime => _lastDamageTime;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _rb = GetComponent<Rigidbody2D>();
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.linearDamping = 6f;
    }

    private void Start()
    {
        if (data == null)
        {
            Debug.LogError($"EnemyController: не назначен EnemyDataSO на {gameObject.name}!");
            return;
        }

        _player = GameManager.Instance?.PlayerTransform;
        _health.OnDeath += HandleDeath;
        _health.Initialize(data.maxHealth);
        SetState(new IdleState());
        ApplyVisuals();
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnDeath -= HandleDeath;
    }

    private void Update()
    {
        _currentState?.Execute();
    }

    public void SetState(IEnemyState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter(this);
    }

    // ═══════════════════════════════════════
    // Хелперы для состояний
    // ═══════════════════════════════════════

    public float DistanceToPlayer()
    {
        if (_player == null) return float.MaxValue;
        return Vector2.Distance(transform.position, _player.position);
    }

    public void MoveTowardsPlayer()
    {
        if (_player == null) return;
        Vector2 direction = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        Vector2 targetVelocity = direction * data.moveSpeed;
        Vector2 velocityChange = targetVelocity - _rb.linearVelocity;
        _rb.AddForce(velocityChange * data.acceleration, ForceMode2D.Force);
    }

    public void StopMovement()
    {
        _rb.linearVelocity = Vector2.zero;
    }

    public void TryDealContactDamage()
    {
        if (Time.time - _lastDamageTime < data.damageCooldown) return;
        if (_player == null) return;

        Health playerHealth = _player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(data.contactDamage);
            _lastDamageTime = Time.time;

            Knockback kb = _player.GetComponent<Knockback>();
            if (kb != null)
            {
                Vector2 dir = (_player.position - transform.position).normalized;
                kb.Apply(dir);
            }
        }
    }

    public bool TryShoot()
    {
        if (Time.time < _nextShootTime) return false;
        _nextShootTime = Time.time + data.shootInterval;
        return true;
    }

    /// <summary>
    /// Определяет какое состояние атаки использовать в зависимости от типа врага
    /// </summary>
    public IEnemyState GetAttackState()
    {
        return data.type switch
        {
            EnemyType.Ranged => new RangedAttackState(),
            _ => new AttackState() // Melee и Flying пока используют контактный урон
        };
    }

    private void HandleDeath()
    {
        SetState(new DeadState());
    }

    private void ApplyVisuals()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning($"EnemyController: не найден SpriteRenderer на {gameObject.name}!");
            return;
        }

        if (data.sprite != null)
        {
            sr.sprite = data.sprite;
        }

        sr.color = data.color;

        if (data.spriteScale != Vector3.one && data.spriteScale != Vector3.zero)
        {
            transform.localScale = data.spriteScale;
        }
    }
}