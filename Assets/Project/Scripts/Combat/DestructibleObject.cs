using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DestructibleObject : MonoBehaviour
{
    [Header("Прочность")]
    [SerializeField] private int maxHealth = 20;
    [Tooltip("Если включено, объект разрушается от любого 1 попадания")]
    [SerializeField] private bool oneHitBreak = false;

    [Header("Визуальный отклик")]
    [SerializeField] private DamageFlash damageFlash;
    [SerializeField] private GameObject destroyVFXPrefab;

    [Header("Награды (Дроп)")]
    [SerializeField] private GameObject xpOrbPrefab;
    [Range(0f, 1f)]
    [SerializeField] private float xpDropChance = 0.75f;
    [SerializeField] private int minXpOrbs = 1;
    [SerializeField] private int maxXpOrbs = 3;

    [Space(5)]
    [SerializeField] private GameObject healthPickupPrefab;
    [Range(0f, 1f)]
    [SerializeField] private float healthDropChance = 0.15f;

    private int _currentHealth;
    private bool _isDestroyed;

    private void Awake()
    {
        _currentHealth = maxHealth;
        if (damageFlash == null) damageFlash = GetComponent<DamageFlash>();
    }

    public void TakeDamage(int damage)
    {
        if (_isDestroyed) return;

        if (oneHitBreak)
        {
            _currentHealth = 0;
        }
        else
        {
            _currentHealth -= damage;
        }

        if (damageFlash != null)
        {
            damageFlash.CallDamageFlash();
        }

        if (_currentHealth <= 0)
        {
            BreakObject();
        }
    }

    private void BreakObject()
    {
        _isDestroyed = true;

        // Спавним частицы разлома / щепки
        if (destroyVFXPrefab != null)
        {
            Instantiate(destroyVFXPrefab, transform.position, Quaternion.identity);
        }

        SpawnLoot();

        Destroy(gameObject);
    }

    private void SpawnLoot()
    {
        // 1. Дроп опыта (XP)
        if (xpOrbPrefab != null && Random.value <= xpDropChance)
        {
            int count = Random.Range(minXpOrbs, maxXpOrbs + 1);
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Random.insideUnitCircle * 0.4f;
                Instantiate(xpOrbPrefab, (Vector2)transform.position + offset, Quaternion.identity);
            }
        }

        // 2. Дроп аптечки / сердечка
        if (healthPickupPrefab != null && Random.value <= healthDropChance)
        {
            Instantiate(healthPickupPrefab, transform.position, Quaternion.identity);
        }
    }

    // Обработка прямых столкновений со снарядами, если снаряд не проверяет Destructible сам
    /*private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isDestroyed) return;

        // Попадание снаряда игрока
        if (other.TryGetComponent<Projectile>(out var proj))
        {
            TakeDamage(proj.Damage);
            Destroy(proj.gameObject);
        }
    }*/
}