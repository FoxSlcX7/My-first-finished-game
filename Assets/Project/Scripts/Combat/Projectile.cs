using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;

    [Header("Столкновение со стенами")]
    [Tooltip("Слой стен. Создай слой Wall и назначь его тайлмапу стен.")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckRadius = 0.1f;

    private int _damage = 1;
    private float _knockbackForce = 8f;
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
        _timer += Time.deltaTime;
        if (_timer >= lifetime)
        {
            ReturnToPool();
            return;
        }

        MoveWithWallCheck();
    }

    /// <summary>
    /// Движение с проверкой стены через CircleCast.
    /// Не зависит от настроек Rigidbody2D и защищает от проскакивания стен.
    /// </summary>
    private void MoveWithWallCheck()
    {
        Vector2 origin = transform.position;
        float step = speed * Time.deltaTime;

        if (wallLayer.value != 0 && _direction.sqrMagnitude > 0.01f)
        {
            RaycastHit2D hit = Physics2D.CircleCast(origin, wallCheckRadius, _direction, step, wallLayer);
            if (hit.collider != null)
            {
                ReturnToPool();
                return;
            }
        }

        transform.position = origin + _direction * step;
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