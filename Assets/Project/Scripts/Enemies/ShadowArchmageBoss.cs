using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class ShadowArchmageBoss : MonoBehaviour
{
    [Header("Идентификация")]
    [SerializeField] private string bossName = "Архимаг Теней";

    [Header("Дистанция и Телепортация")]
    [SerializeField] private float preferredDistance = 7f;
    [SerializeField] private float emergencyTeleportDistance = 3.5f;
    [SerializeField] private float teleportCooldown = 4f;

    [Header("Фаза 1: Снаряды")]
    [SerializeField] private EnemyProjectile shadowBoltPrefab;
    [SerializeField] private float attackInterval = 1.8f;
    [SerializeField] private int boltsPerVolley = 3;
    [SerializeField] private float spreadAngle = 20f;

    [Header("Фаза 2: Клоны")]
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private float summonClonesInterval = 12f;

    [Header("Фаза 3: Теневое комбо")]
    [SerializeField] private EnemyProjectile darkComboPrefab;
    [SerializeField] private float comboInterval = 4f;

    private EnemyController _enemy;
    private Health _health;
    private SpriteRenderer _sr;

    private int _currentPhase = 1;
    private float _nextAttackTime;
    private float _nextTeleportTime;
    private float _nextCloneTime;
    private float _nextComboTime;
    private bool _isBusy;

    private void Awake()
    {
        _enemy = GetComponent<EnemyController>();
        _health = GetComponent<Health>();
        _sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (_enemy.Data != null)
        {
            _enemy.Data.knockbackResistance = 1f; // Иммунитет к отталкиванию
        }

        _health.OnDamaged += CheckPhaseTransition;
        _health.OnDeath += HandleDeath;

        _nextAttackTime = Time.time + 1f;
        _nextTeleportTime = Time.time + 2f;
        _nextCloneTime = Time.time + summonClonesInterval;
        _nextComboTime = Time.time + comboInterval;

        BossEvents.TriggerBossSpawned(bossName, _health);
        BossEvents.TriggerPhaseChanged(_currentPhase);
    }

    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnDamaged -= CheckPhaseTransition;
            _health.OnDeath -= HandleDeath;
        }
    }

    private void Update()
    {
        if (_health.CurrentHealth <= 0 || _isBusy) return;

        float distToPlayer = _enemy.DistanceToPlayer();

        // 1. Аварийная телепортация при сближении игрока
        if (distToPlayer < emergencyTeleportDistance && Time.time >= _nextTeleportTime)
        {
            StartCoroutine(TeleportRoutine());
            return;
        }

        // 2. Фаза 2: Призыв иллюзий
        if (_currentPhase >= 2 && Time.time >= _nextCloneTime)
        {
            StartCoroutine(SummonClonesRoutine());
            return;
        }

        // 3. Фаза 3: Комбо-атака
        if (_currentPhase == 3 && Time.time >= _nextComboTime)
        {
            StartCoroutine(CastDarkComboRoutine());
            return;
        }

        // 4. Базовая веерная атака
        if (Time.time >= _nextAttackTime)
        {
            ShootShadowVolley();
            _nextAttackTime = Time.time + attackInterval;
        }
    }

    private void CheckPhaseTransition(int damage)
    {
        float hpPercent = (float)_health.CurrentHealth / _health.MaxHealth;

        // Переход в Фазу 2 (<= 65% HP)
        if (_currentPhase == 1 && hpPercent <= 0.65f)
        {
            _currentPhase = 2;
            attackInterval *= 0.85f;
            BossEvents.TriggerPhaseChanged(_currentPhase);
            StartCoroutine(TeleportRoutine());
            Debug.Log($"[Boss] {bossName} перешел в ФАЗУ 2 (Иллюзии)!");
        }

        // Переход в Фазу 3 (<= 30% HP)
        if (_currentPhase == 2 && hpPercent <= 0.30f)
        {
            _currentPhase = 3;
            teleportCooldown *= 0.6f;
            attackInterval *= 0.8f;
            if (_sr != null) _sr.color = new Color(0.7f, 0.3f, 1f, 1f); // Фиолетовое теневое свечение
            BossEvents.TriggerPhaseChanged(_currentPhase);
            StartCoroutine(TeleportRoutine());
            Debug.Log($"[Boss] {bossName} перешел в ФАЗУ 3 (Теневое комбо)!");
        }
    }

    private void ShootShadowVolley()
    {
        if (shadowBoltPrefab == null || _enemy.Player == null) return;

        Vector2 toPlayer = ((Vector2)_enemy.Player.position - (Vector2)transform.position).normalized;
        int count = boltsPerVolley + (_currentPhase >= 2 ? 2 : 0);

        for (int i = 0; i < count; i++)
        {
            float offset = (i - (count - 1) * 0.5f) * spreadAngle;
            Vector2 dir = Quaternion.Euler(0, 0, offset) * toPlayer;

            EnemyProjectile bolt = Instantiate(shadowBoltPrefab, transform.position, Quaternion.identity);
            bolt.Init(dir, _enemy.Data.contactDamage);
        }
    }

    private IEnumerator CastDarkComboRoutine()
    {
        _isBusy = true;
        _enemy.StopMovement();

        yield return new WaitForSeconds(0.4f); // Каст-пауза

        if (darkComboPrefab != null && _enemy.Player != null)
        {
            Vector2 toPlayer = ((Vector2)_enemy.Player.position - (Vector2)transform.position).normalized;
            // Спавним усиленный комбо-снаряд
            EnemyProjectile comboBolt = Instantiate(darkComboPrefab, transform.position, Quaternion.identity);
            comboBolt.Init(toPlayer, Mathf.RoundToInt(_enemy.Data.contactDamage * 1.6f));
        }

        yield return new WaitForSeconds(0.3f);
        _nextComboTime = Time.time + comboInterval;
        _isBusy = false;
    }

    private IEnumerator SummonClonesRoutine()
    {
        _isBusy = true;
        _enemy.StopMovement();

        yield return new WaitForSeconds(0.3f);

        if (clonePrefab != null)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 spawnPos = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle.normalized * 2.5f;
                GameObject clone = Instantiate(clonePrefab, spawnPos, Quaternion.identity);
                // Иллюзии умирают с 1 тычки
                Health h = clone.GetComponent<Health>();
                if (h != null) h.Initialize(1);
            }
        }

        _nextCloneTime = Time.time + summonClonesInterval;
        _isBusy = false;
    }

    private IEnumerator TeleportRoutine()
    {
        _isBusy = true;
        _enemy.StopMovement();

        // 1. Анимация растворения
        float fade = 0.2f;
        float t = 0f;
        while (t < fade)
        {
            t += Time.deltaTime;
            if (_sr != null)
            {
                Color c = _sr.color;
                c.a = Mathf.Lerp(1f, 0f, t / fade);
                _sr.color = c;
            }
            yield return null;
        }

        // 2. Смена позиции (отпрыгиваем в сторону от игрока)
        if (_enemy.Player != null)
        {
            Vector2 dirFromPlayer = ((Vector2)transform.position - (Vector2)_enemy.Player.position).normalized;
            Vector2 targetPos = (Vector2)_enemy.Player.position + dirFromPlayer * preferredDistance;

            // Легкий джиттер по нормали
            targetPos += UnityEngine.Random.insideUnitCircle * 1.5f;
            transform.position = targetPos;
        }

        // 3. Появление
        t = 0f;
        while (t < fade)
        {
            t += Time.deltaTime;
            if (_sr != null)
            {
                Color c = _sr.color;
                c.a = Mathf.Lerp(0f, 1f, t / fade);
                _sr.color = c;
            }
            yield return null;
        }

        _nextTeleportTime = Time.time + teleportCooldown;
        _isBusy = false;
    }

    private void HandleDeath()
    {
        BossEvents.TriggerBossDefeated();
    }
}