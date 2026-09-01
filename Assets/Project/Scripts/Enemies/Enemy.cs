using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int maxHealth = 3;

    private Transform _player;
    private int _currentHealth;

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

    private void Die()
    {
        // Пока просто уничтожаем, потом добавим эффекты и дроп
        Destroy(gameObject);
    }
}