using System.Collections;
using UnityEngine;

public class EnemyAOEZone : MonoBehaviour
{
    [SerializeField] private SpriteRenderer circleRenderer;
    [SerializeField] private float telegraphDuration = 0.7f;
    [SerializeField] private Color telegraphColor = new Color(1f, 0.2f, 0.2f, 0.35f);
    [SerializeField] private Color impactColor = new Color(1f, 0.1f, 0.1f, 0.9f);

    private int _damage;
    private float _radius;
    private float _knockbackForce;

    public void Init(int damage, float radius, float knockbackForce = 12f)
    {
        _damage = damage;
        _radius = radius;
        _knockbackForce = knockbackForce;

        transform.localScale = Vector3.one * (_radius * 2f);
        StartCoroutine(TelegraphAndHitRoutine());
    }

    private IEnumerator TelegraphAndHitRoutine()
    {
        if (circleRenderer != null)
        {
            circleRenderer.color = telegraphColor;
        }

        // Телеграф (предупреждение игроку, чтобы успел отбежать)
        float elapsed = 0f;
        while (elapsed < telegraphDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / telegraphDuration;
            // Пульсация альфы
            if (circleRenderer != null)
            {
                Color c = telegraphColor;
                c.a = Mathf.Lerp(0.2f, 0.6f, t);
                circleRenderer.color = c;
            }
            yield return null;
        }

        // Момент удара
        if (circleRenderer != null)
        {
            circleRenderer.color = impactColor;
        }

        DealDamageToPlayer();

        // Затухание после удара
        elapsed = 0f;
        float fadeDuration = 0.25f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (circleRenderer != null)
            {
                Color c = impactColor;
                c.a = Mathf.Lerp(impactColor.a, 0f, elapsed / fadeDuration);
                circleRenderer.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }

    private void DealDamageToPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _radius);
        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            Health playerHealth = hit.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(_damage);
            }

            PlayerController pc = hit.GetComponent<PlayerController>();
            if (pc != null)
            {
                Vector2 dir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                pc.ApplyKnockback(dir, _knockbackForce);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}