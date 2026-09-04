using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MeleeEnemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private int contactDamage = 12;
    [SerializeField] private float damageCooldown = 1.2f;

    private Transform _player;
    private float _lastDamageTime;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _player = GameManager.Instance?.PlayerTransform;
    }

    private void FixedUpdate()
    {
        if (_player == null) return;

        Vector2 direction = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        _rb.linearVelocity = direction * moveSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TryDealDamage(collision.gameObject);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TryDealDamage(collision.gameObject);
        }
    }

    private void TryDealDamage(GameObject target)
    {
        if (Time.time - _lastDamageTime < damageCooldown) return;

        target.GetComponent<Health>()?.TakeDamage(contactDamage);
        _lastDamageTime = Time.time;

        Vector2 direction = (target.transform.position - transform.position).normalized;
        Knockback kb = target.GetComponent<Knockback>();
        if (kb != null) kb.Apply(direction);
    }
}