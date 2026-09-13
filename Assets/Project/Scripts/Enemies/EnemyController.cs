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
    private float _nextKnockbackTime;
    private float _staggerEndTime;
    private EnemyDataSO _runtimeData;

    // Публичные свойства для состояний
    public EnemyDataSO Data => _runtimeData;
    public Health Health => _health;
    public Rigidbody2D Rb => _rb;
    public Transform Player => _player;
    public float LastDamageTime => _lastDamageTime;
    public bool IsStaggered => Time.time < _staggerEndTime;

    private void Awake()
    {
        if (data != null) _runtimeData = Instantiate(data); // Создаем уникальную копию для этого врага

        _health = GetComponent<Health>();
        _rb = GetComponent<Rigidbody2D>();
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.linearDamping = 6f;
    }

    private void Start()
    {
        if (_runtimeData == null)
        {
            Debug.LogError($"EnemyController: не назначен EnemyDataSO на {gameObject.name}!");
            return;
        }

        _player = GameManager.Instance?.PlayerTransform;
        _health.OnDeath += HandleDeath;

        // Берем уже отскалированное ХП
        _health.Initialize(_runtimeData.maxHealth);
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
        if (IsStaggered) return;

        Vector2 direction = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        Vector2 targetVelocity = direction * _runtimeData.moveSpeed;
        Vector2 velocityChange = targetVelocity - _rb.linearVelocity;
        _rb.AddForce(velocityChange * _runtimeData.acceleration, ForceMode2D.Force);
    }

    public void StopMovement()
    {
        if (IsStaggered) return;
        _rb.linearVelocity = Vector2.zero;
    }

    public void TryDealContactDamage()
    {
        if (Time.time - _lastDamageTime < _runtimeData.damageCooldown) return;
        if (_player == null) return;

        Health playerHealth = _player.GetComponent<Health>();
        if (playerHealth != null)
        {
            // Берем уже отскалированный урон
            playerHealth.TakeDamage(_runtimeData.contactDamage);
            _lastDamageTime = Time.time;

            Vector2 dir = (_player.position - transform.position).normalized;
            PlayerController pc = _player.GetComponent<PlayerController>();
            if (pc != null)
                pc.ApplyKnockback(dir, _runtimeData.contactKnockbackForce);
        }
    }

    public bool TryShoot()
    {
        if (Time.time < _nextShootTime) return false;
        _nextShootTime = Time.time + _runtimeData.shootInterval;
        return true;
    }

    /// <summary>
    /// Применяет отбрасывание с учётом сопротивления и кулдауна из EnemyDataSO.
    /// </summary>
    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (_runtimeData == null) return;
        if (Time.time < _nextKnockbackTime) return; // защита от частых попаданий

        float actualForce = force * (1f - _runtimeData.knockbackResistance);
        if (actualForce <= 0f) return;

        _rb.AddForce(direction.normalized * actualForce, ForceMode2D.Impulse);
        _nextKnockbackTime = Time.time + _runtimeData.knockbackCooldown;
        _staggerEndTime = Time.time + _runtimeData.knockbackStagger;
    }

    /// <summary>
    /// Определяет какое состояние атаки использовать в зависимости от типа врага
    /// </summary>
    public IEnemyState GetAttackState()
    {
        return _runtimeData.type switch
        {
            EnemyType.Ranged => new RangedAttackState(),
            _ => new AttackState() // Melee и Flying пока используют контактный урон
        };
    }

    private void HandleDeath()
    {
        XPDropper.Instance?.Drop(transform.position);
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

        if (_runtimeData.sprite != null)
        {
            sr.sprite = _runtimeData.sprite;
        }

        sr.color = _runtimeData.color;

        if (_runtimeData.spriteScale != Vector3.one && _runtimeData.spriteScale != Vector3.zero)
        {
            transform.localScale = _runtimeData.spriteScale;
        }
    }

    public void ApplyBalance(BalanceConfig config, int floor)
    {
        if (config == null || _runtimeData == null) return;

        float hpMult = config.GetEnemyHealthMultiplier(floor);
        float dmgMult = config.GetEnemyDamageMultiplier(floor);

        // Применяем скалирование ТОЛЬКО к клону. 
        // Формула опирается на базовый 'data', чтобы избежать геометрической прогрессии багов.
        _runtimeData.maxHealth = Mathf.Max(1, Mathf.RoundToInt(data.maxHealth * hpMult));
        _runtimeData.contactDamage = Mathf.Max(1, Mathf.RoundToInt(data.contactDamage * dmgMult));

        // В будущем для добавления скорости понадобится лишь одна строка здесь:
        // _runtimeData.moveSpeed = data.moveSpeed * config.GetEnemySpeedMultiplier(floor);
    }
}