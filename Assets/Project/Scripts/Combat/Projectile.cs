using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;

    private int _damage = 1;
    private float _knockbackForce = 8f; // ← ДОБАВЛЕНО
    private Vector2 _direction;
    private float _timer;

    public void Init(Vector2 direction)
    {
        _direction = direction.normalized;
        _timer = 0f;
    }

    private void OnEnable()
    {
        _timer = 0f;
    }

    public void SetStats(float newSpeed, float newLifetime, int newDamage, float newKnockback = 8f)
    {
        speed = newSpeed;
        lifetime = newLifetime;
        _damage = newDamage;
        _knockbackForce = newKnockback;
    }

    private void Update()
    {
        transform.position += (Vector3)(_direction * speed * Time.deltaTime);
        _timer += Time.deltaTime;
        if (_timer >= lifetime)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Health>()?.TakeDamage(_damage);

            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.ApplyKnockback(_direction, _knockbackForce);

            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        PoolManager.Instance?.ReturnProjectile(this);
    }
}