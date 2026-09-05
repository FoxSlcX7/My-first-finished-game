using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MeleeEnemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float acceleration = 15f; // сила ускорения
    [SerializeField] private int contactDamage = 12;
    [SerializeField] private float damageCooldown = 1.2f;

    private Transform _player;
    private float _lastDamageTime;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.linearDamping = 4f; // чтобы не скользил
    }

    private void Start()
    {
        _player = GameManager.Instance?.PlayerTransform;
    }

    private void FixedUpdate()
    {
        if (_player == null) return;

        Vector2 direction = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        Vector2 targetVelocity = direction * moveSpeed;
        Vector2 velocityChange = targetVelocity - _rb.linearVelocity;
        _rb.AddForce(velocityChange * acceleration, ForceMode2D.Force);
    }

    // OnCollisionEnter/Stay без изменений (как было)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            TryDealDamage(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            TryDealDamage(collision.gameObject);
    }

    private void TryDealDamage(GameObject target)
    {
        if (Time.time - _lastDamageTime < damageCooldown) return;

        target.GetComponent<Health>()?.TakeDamage(contactDamage);
        _lastDamageTime = Time.time;

        Vector2 direction = (target.transform.position - transform.position).normalized;
    }
}