using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 9f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float knockbackForce = 10f;

    [Header("Столкновение со стенами")]
    [Tooltip("Слой стен. Создай слой Wall и назначь его тайлмапу стен.")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckRadius = 0.1f;

    private Vector2 _direction;
    private float _timer;

    public void Init(Vector2 direction)
    {
        _direction = direction.normalized;
        _timer = 0f;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        MoveWithWallCheck();
    }

    private void MoveWithWallCheck()
    {
        Vector2 origin = transform.position;
        float step = speed * Time.deltaTime;

        if (wallLayer.value != 0 && _direction.sqrMagnitude > 0.01f)
        {
            RaycastHit2D hit = Physics2D.CircleCast(origin, wallCheckRadius, _direction, step, wallLayer);
            if (hit.collider != null)
            {
                Destroy(gameObject);
                return;
            }
        }

        transform.position = origin + _direction * step;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Health>()?.TakeDamage(damage);
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
                pc.ApplyKnockback(_direction, knockbackForce);
            Destroy(gameObject);
        }
    }
}