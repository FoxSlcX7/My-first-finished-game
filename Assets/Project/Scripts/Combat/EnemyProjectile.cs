using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 9f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float knockbackForce = 10f;

    private Vector2 _direction;
    private float _timer;

    public void Init(Vector2 direction)
    {
        _direction = direction.normalized;
        _timer = 0f;
    }

    private void Update()
    {
        transform.position += (Vector3)(_direction * speed * Time.deltaTime);

        _timer += Time.deltaTime;
        if (_timer >= lifetime)
        {
            Destroy(gameObject);
        }
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