using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public event Action OnDeath;
    public event Action<int> OnHealthChanged;
    public event Action<int> OnDamaged; // int = полученный урон

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }
    
    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        OnDamaged?.Invoke(damage);
        OnHealthChanged?.Invoke(CurrentHealth);

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();

        if (CompareTag("Player"))
        {
            GameManager.Instance?.GameOver();
        }

        Destroy(gameObject);
    }
}