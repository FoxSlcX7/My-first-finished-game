using UnityEngine;

public class AOEZone : MonoBehaviour
{
    private SpriteRenderer _sr;
    private int _damage;
    private float _radius;
    private float _lifetime;
    private float _timer;
    private float _targetScale;

    public void Init(int damage, float radius, float lifetime, Color color)
    {
        _damage = damage;
        _radius = radius;
        _lifetime = lifetime;
        _targetScale = radius * 2f;

        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null)
        {
            _sr.color = color;
        }

        transform.localScale = Vector3.one * (_targetScale * 0.2f);
        DealDamage(); // урон сразу при появлении
    }

    private void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _radius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player")) continue;

            Health health = hit.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(_damage);

                Knockback kb = hit.GetComponent<Knockback>();
                if (kb != null)
                {
                    Vector2 dir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                    kb.Apply(dir);
                }
            }
        }
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        float t = Mathf.Clamp01(_timer / _lifetime);

        // Быстрое расширение в первые 30% времени жизни
        float expand = Mathf.Clamp01(t / 0.3f);
        transform.localScale = Vector3.one * Mathf.Lerp(_targetScale * 0.2f, _targetScale, expand);

        if (_sr != null)
        {
            Color c = _sr.color;
            c.a = 1f - t; // растворение
            _sr.color = c;
        }

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}