using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Основной контроллер врага: стейт-машина, физическое перемещение, учет урона и статический реестр сущностей.
/// </summary>
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    #region Inspector Settings

    [Header("Данные врага")]
    [Tooltip("Конфигурация параметров врага (SO)")]
    [SerializeField] private EnemyDataSO data;

    #endregion

    #region Static Registry (Optimization)

    private static readonly List<EnemyController> s_activeEnemies = new();

    /// <summary>
    /// Глобальный список всех активных врагов на сцене (без вызова FindObjectsByType).
    /// </summary>
    public static IReadOnlyList<EnemyController> ActiveEnemies => s_activeEnemies;

    #endregion

    #region Public State & Properties

    public EnemyDataSO Data => _runtimeData;
    public Health Health => _health;
    public Rigidbody2D Rb => _rb;
    public Transform Player => _player;
    public float LastDamageTime => _lastDamageTime;
    public bool IsStaggered => Time.time < _staggerEndTime;

    #endregion

    #region Private Fields

    private Health _health;
    private Rigidbody2D _rb;
    private Collider2D _col;
    private Transform _player;
    private IEnemyState _currentState;
    private EnemyDataSO _runtimeData;

    private float _lastDamageTime;
    private float _nextShootTime;
    private float _nextKnockbackTime;
    private float _staggerEndTime;
    private Vector2 _currentMoveDir;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (data != null)
        {
            _runtimeData = Instantiate(data);
        }

        _health = GetComponent<Health>();
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();

        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.linearDamping = 6f;
    }

    private void OnEnable()
    {
        s_activeEnemies.Add(this);
    }

    private void Start()
    {
        if (_runtimeData == null)
        {
            Debug.LogError($"[EnemyController] Не назначен EnemyDataSO на объекте {gameObject.name}!");
            return;
        }

        _player = GameManager.Instance?.PlayerTransform;
        _health.OnDeath += HandleDeath;
        _health.Initialize(_runtimeData.maxHealth);

        SetState(new IdleState());
        ApplyVisuals();
    }

    private void Update()
    {
        _currentState?.Execute();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (_currentState is SlimeJumpState) return;

            StopMovement();

            if (_currentState is ChaseState)
            {
                SetState(GetAttackState());
            }
        }
    }

    private void OnDisable()
    {
        s_activeEnemies.Remove(this);
    }

    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnDeath -= HandleDeath;
        }

        if (_runtimeData != null)
        {
            Destroy(_runtimeData);
        }
    }

    #endregion

    #region State Management

    public void SetState(IEnemyState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter(this);
    }

    public IEnemyState GetAttackState()
    {
        return _runtimeData.type switch
        {
            EnemyType.Ranged => new RangedAttackState(),
            EnemyType.Leaping => new SlimeJumpState(),
            EnemyType.Kamikaze => new KamikazeExplodeState(),
            _ => new AttackState()
        };
    }

    #endregion

    #region Movement & Navigation

    public float DistanceToPlayer()
    {
        if (_player == null) return float.MaxValue;

        Collider2D playerCol = _player.GetComponent<Collider2D>();
        if (_col != null && playerCol != null)
        {
            return Vector2.Distance(_col.bounds.center, playerCol.bounds.center);
        }

        return Vector2.Distance(transform.position, _player.position);
    }

    public void MoveTowardsPlayer()
    {
        if (_player == null || IsStaggered) return;

        Vector2 origin = transform.position;
        Vector2 toPlayer = ((Vector2)_player.position - origin).normalized;
        Vector2 desiredDir = toPlayer;

        if (_runtimeData.type == EnemyType.Flying)
        {
            Vector2 perp = new Vector2(-toPlayer.y, toPlayer.x);
            float wave = Mathf.Sin((Time.time * 6f) + (transform.position.x * 1.5f)) * 0.65f;
            desiredDir = (toPlayer + perp * wave).normalized;
        }

        desiredDir = AvoidObstacles(origin, desiredDir);

        Vector2 separation = ComputeSeparation(origin);
        if (separation.sqrMagnitude > 0.001f)
        {
            desiredDir = (desiredDir * 0.8f + separation * 0.2f).normalized;
        }

        _currentMoveDir = Vector2.Lerp(_currentMoveDir == Vector2.zero ? desiredDir : _currentMoveDir, desiredDir, Time.deltaTime * 10f);

        Vector2 targetVelocity = _currentMoveDir * _runtimeData.moveSpeed;
        Vector2 velocityChange = targetVelocity - _rb.linearVelocity;
        _rb.AddForce(velocityChange * _runtimeData.acceleration, ForceMode2D.Force);
    }

    public void MoveInDirection(Vector2 direction, float speedMultiplier = 1f)
    {
        if (IsStaggered || _rb == null) return;

        Vector2 targetVelocity = direction.normalized * (_runtimeData.moveSpeed * speedMultiplier);
        Vector2 velocityChange = targetVelocity - _rb.linearVelocity;
        _rb.AddForce(velocityChange * _runtimeData.acceleration, ForceMode2D.Force);
    }

    public void StopMovement()
    {
        if (IsStaggered || _rb == null) return;
        _rb.linearVelocity = Vector2.zero;
    }

    private Vector2 AvoidObstacles(Vector2 origin, Vector2 desiredDir)
    {
        int obstacleMask = ~LayerMask.GetMask("Enemy", "Player", "Ignore Raycast");

        RaycastHit2D hitCenter = Physics2D.CircleCast(origin, 0.3f, desiredDir, 0.9f, obstacleMask);
        if (hitCenter.collider == null || hitCenter.collider.isTrigger || hitCenter.collider.CompareTag("Enemy") || hitCenter.collider.CompareTag("Player"))
        {
            return desiredDir;
        }

        Vector2 leftDir = Quaternion.Euler(0, 0, 45) * desiredDir;
        Vector2 rightDir = Quaternion.Euler(0, 0, -45) * desiredDir;

        RaycastHit2D hitLeft = Physics2D.Raycast(origin, leftDir, 1.2f, obstacleMask);
        RaycastHit2D hitRight = Physics2D.Raycast(origin, rightDir, 1.2f, obstacleMask);

        bool leftBlocked = hitLeft.collider != null && !hitLeft.collider.isTrigger && !hitLeft.collider.CompareTag("Enemy");
        bool rightBlocked = hitRight.collider != null && !hitRight.collider.isTrigger && !hitRight.collider.CompareTag("Enemy");

        float leftSpace = leftBlocked ? hitLeft.distance : 1.2f;
        float rightSpace = rightBlocked ? hitRight.distance : 1.2f;

        if (hitCenter.collider.TryGetComponent<DestructibleObject>(out var destructible))
        {
            destructible.TakeDamage(10);
        }

        return leftSpace >= rightSpace ? leftDir : rightDir;
    }

    private Vector2 ComputeSeparation(Vector2 origin)
    {
        Vector2 push = Vector2.zero;
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(origin, 0.75f);
        foreach (var col in neighbors)
        {
            if (col.gameObject == gameObject || !col.CompareTag("Enemy")) continue;
            Vector2 diff = origin - (Vector2)col.transform.position;
            float dist = diff.magnitude;
            if (dist > 0.01f)
            {
                float weight = Mathf.Clamp01(1f - (dist / 0.75f));
                push += diff.normalized * weight;
            }
        }
        return push.sqrMagnitude > 0.001f ? push.normalized : Vector2.zero;
    }

    #endregion

    #region Attacks & Combat

    public bool TryDealContactDamage(float customRange = -1f)
    {
        if (_player == null) return false;
        if (Time.time - _lastDamageTime < _runtimeData.damageCooldown) return false;

        float range = customRange > 0f ? customRange : _runtimeData.hitRadius;

        Collider2D playerCol = _player.GetComponent<Collider2D>();
        if (_col != null && playerCol != null)
        {
            ColliderDistance2D colDist = _col.Distance(playerCol);
            if (!colDist.isOverlapped && colDist.distance > range)
            {
                return false;
            }
        }
        else
        {
            if (DistanceToPlayer() > range) return false;
        }

        Health playerHealth = _player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(_runtimeData.contactDamage);
            _lastDamageTime = Time.time;

            Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
            PlayerController pc = _player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.ApplyKnockback(dir, _runtimeData.contactKnockbackForce);
            }

            return true;
        }

        return false;
    }

    public bool TryShoot()
    {
        if (Time.time < _nextShootTime) return false;
        _nextShootTime = Time.time + _runtimeData.shootInterval;
        return true;
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (_runtimeData == null) return;
        if (Time.time < _nextKnockbackTime) return;

        float actualForce = force * (1f - _runtimeData.knockbackResistance);
        if (actualForce <= 0f) return;

        _rb.AddForce(direction.normalized * actualForce, ForceMode2D.Impulse);
        _nextKnockbackTime = Time.time + _runtimeData.knockbackCooldown;
        _staggerEndTime = Time.time + _runtimeData.knockbackStagger;
    }

    private void HandleDeath()
    {
        XPDropper.Instance?.Drop(transform.position);
        SetState(new DeadState());
    }

    #endregion

    #region Visuals & Balance

    private void ApplyVisuals()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null) return;

        if (_runtimeData.sprite != null) sr.sprite = _runtimeData.sprite;
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

        _runtimeData.maxHealth = Mathf.RoundToInt(_runtimeData.maxHealth * hpMult);
        _runtimeData.contactDamage = Mathf.RoundToInt(_runtimeData.contactDamage * dmgMult);

        if (_health != null)
        {
            _health.Initialize(_runtimeData.maxHealth);
        }
    }

    #endregion
}