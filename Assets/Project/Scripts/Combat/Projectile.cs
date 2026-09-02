using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;

    private int _damage = 1;
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

    public void SetDamage(int damage)
    {
        _damage = damage;
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
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        PoolManager.Instance?.ReturnProjectile(this);
    }
}