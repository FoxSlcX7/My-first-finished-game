using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private float damageCooldown = 1f;

    private Transform _player;
    private int _currentHealth;
    private float _lastDamageTime;

    private void Start()
    {
        _player = GameObject.FindWithTag("Player")?.transform;
        _currentHealth = maxHealth;
    }

    private void Update()
    {
        if (_player == null) return;

        // Простое преследование игрока
        Vector2 direction = (_player.position - transform.position).normalized;
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;

        if (_currentHealth <= 0)
        {
            Die();
        }
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
    }

    private void Die()
    {
        // Пока просто уничтожаем, потом добавим эффекты и дроп
        Destroy(gameObject);
    }
}