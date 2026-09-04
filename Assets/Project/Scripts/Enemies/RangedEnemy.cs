using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RangedEnemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float preferredDistance = 8f;
    [SerializeField] private float stopDistance = 6f;
    [SerializeField] private float shootInterval = 2.5f;
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private Transform firePoint;

    private Transform _player;
    private float _shootTimer;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.linearDamping = 4f;
    }

    private void Start()
    {
        _player = GameManager.Instance?.PlayerTransform;
    }

    private void FixedUpdate()
    {
        if (_player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        Vector2 moveDirection = Vector2.zero;

        if (distanceToPlayer < stopDistance)
            moveDirection = ((Vector2)transform.position - (Vector2)_player.position).normalized;
        else if (distanceToPlayer > preferredDistance)
            moveDirection = ((Vector2)_player.position - (Vector2)transform.position).normalized;

        Vector2 targetVelocity = moveDirection * moveSpeed;
        Vector2 velocityChange = targetVelocity - _rb.linearVelocity;
        _rb.AddForce(velocityChange * acceleration, ForceMode2D.Force);

        // Поворот
        Vector2 aimDirection = _player.position - transform.position;
        if (aimDirection.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            _rb.MoveRotation(angle);
        }
    }

    private void Update()
    {
        if (_player == null) return;
        _shootTimer += Time.deltaTime;
        if (_shootTimer >= shootInterval)
        {
            _shootTimer = 0f;
            Shoot();
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null || _player == null || firePoint == null) return;
        Vector2 direction = (_player.position - firePoint.position).normalized;
        EnemyProjectile projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        projectile.Init(direction);
    }
}