using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 1;

    private Vector2 _direction;
    private float _timer;

    // Инициализация направления при спавне
    public void Init(Vector2 direction)
    {
        _direction = direction.normalized;
        _timer = 0f;
    }

    private void OnEnable()
    {
        _timer = 0f;
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

    // Пока просто логируем попадание, урон добавим позже
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>()?.TakeDamage(damage);
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        PoolManager.Instance?.ReturnProjectile(this);
    }
}