using System.Xml;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float acceleration = 15f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Combat")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private float damageCooldown = 1f;

    private Health _health;
    private Rigidbody2D _rb;
    private Transform _player;
    private IEnemyState _currentState;
    private float _lastDamageTime;

    // Публичные свойства для состояний
    public Health Health => _health;
    public Rigidbody2D Rb => _rb;
    public Transform Player => _player;
    public Transform Transform => transform;
    public float MoveSpeed => moveSpeed;
    public float Acceleration => acceleration;
    public float DetectionRange => detectionRange;
    public float AttackRange => attackRange;
    public int ContactDamage => contactDamage;
    public float DamageCooldown => damageCooldown;
    public float LastDamageTime => _lastDamageTime;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _rb = GetComponent<Rigidbody2D>();
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.linearDamping = 4f;
    }

    private void Start()
    {
        _player = GameManager.Instance?.PlayerTransform;
        _health.OnDeath += HandleDeath;
        SetState(new IdleState());
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
        Vector2 targetVelocity = direction * moveSpeed;
        Vector2 velocityChange = targetVelocity - _rb.linearVelocity;
        _rb.AddForce(velocityChange * acceleration, ForceMode2D.Force);
    }

    public void StopMovement()
    {
        _rb.linearVelocity = Vector2.zero;
    }

    public void TryDealContactDamage()
    {
        if (Time.time - _lastDamageTime < damageCooldown) return;

        if (_player == null) return;

        Health playerHealth = _player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
            _lastDamageTime = Time.time;

            // Отбрасывание игрока
            Knockback kb = _player.GetComponent<Knockback>();
            if (kb != null)
            {
                Vector2 direction = (_player.position - transform.position).normalized;
                kb.Apply(direction);
            }
        }
    }

    private void HandleDeath()
    {
        SetState(new DeadState());
    }
}