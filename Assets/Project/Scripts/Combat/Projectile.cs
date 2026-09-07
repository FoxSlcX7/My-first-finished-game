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
    private int _bounces;
    private int _chainTargets;
    private float _chainDamagePercent;
    private float _lifeSteal;
    private const float RicochetRadius = 6f;
    private const float ChainRadius = 4f;

    public void Init(Vector2 direction)
    {
        _direction = direction.normalized;
        _timer = 0f;
    }

    private void OnEnable()
    {
        _timer = 0f;
        var stats = PlayerStats.Instance;
        _bounces = stats != null ? stats.RicochetBounces : 0;
        _chainTargets = stats != null ? stats.ChainTargets : 0;
        _chainDamagePercent = stats != null ? stats.ChainDamagePercent : 0.5f;
        _lifeSteal = stats != null ? stats.LifeStealPercent : 0f;
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

            // Вампиризм: часть урона возвращается игроку
            if (_lifeSteal > 0f)
            {
                int heal = Mathf.RoundToInt(_damage * _lifeSteal);
                if (heal > 0 && GameManager.Instance?.PlayerTransform != null)
                    GameManager.Instance.PlayerTransform.GetComponent<Health>()?.Heal(heal);
            }

            // Цепная молния: урон по соседним врагам
            if (_chainTargets > 0)
                StrikeChain(other.gameObject);

            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.ApplyKnockback(_direction, _knockbackForce);

            // Рикошет: не возвращаемся в пул, летим в следующую цель
            if (_bounces > 0)
            {
                Transform next = FindNearestEnemy(other.gameObject, RicochetRadius);
                if (next != null)
                {
                    _direction = ((Vector2)next.position - (Vector2)transform.position).normalized;
                    transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg);
                    _bounces--;
                    return;
                }
            }

            ReturnToPool();
        }
    }

    private Transform FindNearestEnemy(GameObject exclude, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        Transform best = null;
        float bestDist = float.MaxValue;
        foreach (var h in hits)
        {
            if (!h.CompareTag("Enemy") || h.gameObject == exclude) continue;
            float d = Vector2.Distance(transform.position, h.transform.position);
            if (d < bestDist) { bestDist = d; best = h.transform; }
        }
        return best;
    }

    private void StrikeChain(GameObject firstTarget)
    {
        GameObject current = firstTarget;
        int chainDamage = Mathf.Max(1, Mathf.RoundToInt(_damage * _chainDamagePercent));
        for (int i = 0; i < _chainTargets; i++)
        {
            Transform next = FindNearestEnemy(current, ChainRadius);
            if (next == null) break;
            next.GetComponent<Health>()?.TakeDamage(chainDamage);
            current = next.gameObject;
        }
    }

    private void ReturnToPool()
    {
        PoolManager.Instance?.ReturnProjectile(this);
    }
}