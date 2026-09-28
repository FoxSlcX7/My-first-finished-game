using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class StoneGuardianBoss : MonoBehaviour
{
    [Header("Идентификация")]
    [SerializeField] private string bossName = "Каменный Страж";

    [Header("Фаза 1: Призыв")]
    [SerializeField] private EnemyController slimePrefab;
    [SerializeField] private float summonInterval = 8f;
    [SerializeField] private int slimesPerSummon = 2;

    [Header("Фаза 2: Топот (Stomp AOE)")]
    [SerializeField] private EnemyAOEZone stompPrefab;
    [SerializeField] private float stompInterval = 5f;
    [SerializeField] private float stompRadius = 3.5f;
    [SerializeField] private int stompDamage = 20;

    [Header("Визуал фаз")]
    [SerializeField] private Color enrageColor = new Color(1f, 0.4f, 0.4f, 1f);

    private EnemyController _enemy;
    private Health _health;
    private SpriteRenderer _sr;

    private int _currentPhase = 1;
    private float _nextSummonTime;
    private float _nextStompTime;
    private bool _isPerformingSpecial;

    private void Awake()
    {
        _enemy = GetComponent<EnemyController>();
        _health = GetComponent<Health>();
        _sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        // Невосприимчивость к отбрасыванию
        if (_enemy.Data != null)
        {
            _enemy.Data.knockbackResistance = 1f;
        }

        _health.OnDamaged += CheckPhaseTransition;
        _health.OnDeath += HandleDeath;

        _nextSummonTime = Time.time + summonInterval;
        _nextStompTime = Time.time + stompInterval;

        // Оповещаем UI о появлении босса
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
        if (_health.CurrentHealth <= 0 || _isPerformingSpecial) return;

        // Проверка таймеров способностей
        if (Time.time >= _nextSummonTime)
        {
            StartCoroutine(SummonSlimesRoutine());
            return;
        }

        if (_currentPhase >= 2 && Time.time >= _nextStompTime)
        {
            StartCoroutine(GroundStompRoutine());
        }
    }

    private void CheckPhaseTransition(int damage)
    {
        float hpPercent = (float)_health.CurrentHealth / _health.MaxHealth;

        // Переход во 2 фазу (<= 50% HP)
        if (_currentPhase == 1 && hpPercent <= 0.5f)
        {
            _currentPhase = 2;
            _enemy.Data.moveSpeed *= 1.3f;
            BossEvents.TriggerPhaseChanged(_currentPhase);
            Debug.Log($"[Boss] {bossName} перешел в ФАЗУ 2!");
        }

        // Переход в 3 фазу — Ярость (<= 25% HP)
        if (_currentPhase == 2 && hpPercent <= 0.25f)
        {
            _currentPhase = 3;
            _enemy.Data.moveSpeed *= 1.2f;
            _enemy.Data.contactDamage = Mathf.RoundToInt(_enemy.Data.contactDamage * 1.5f);
            summonInterval = Mathf.Max(4f, summonInterval * 0.7f);
            stompInterval = Mathf.Max(3f, stompInterval * 0.7f);

            if (_sr != null)
            {
                _sr.color = enrageColor;
            }

            BossEvents.TriggerPhaseChanged(_currentPhase);
            Debug.Log($"[Boss] {bossName} впал в ЯРОСТЬ (ФАЗА 3)!");
        }
    }

    private IEnumerator SummonSlimesRoutine()
    {
        _isPerformingSpecial = true;
        _enemy.StopMovement();

        yield return new WaitForSeconds(0.5f); // Короткая анимационная пауза каста

        if (slimePrefab != null)
        {
            for (int i = 0; i < slimesPerSummon; i++)
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle.normalized * 1.8f;
                Instantiate(slimePrefab, (Vector2)transform.position + offset, Quaternion.identity);
            }
        }

        yield return new WaitForSeconds(0.3f);
        _nextSummonTime = Time.time + summonInterval;
        _isPerformingSpecial = false;
    }

    private IEnumerator GroundStompRoutine()
    {
        _isPerformingSpecial = true;
        _enemy.StopMovement();

        // Создаем зону топота под ногами
        if (stompPrefab != null)
        {
            EnemyAOEZone zone = Instantiate(stompPrefab, transform.position, Quaternion.identity);
            zone.Init(stompDamage, stompRadius);

            // Тряска арены от удара гиганта
            ScreenShake.Instance?.Shake(2.0f, 0.25f);
        }

        // Ждем завершения телеграфа удара
        yield return new WaitForSeconds(0.8f);

        _nextStompTime = Time.time + stompInterval;
        _isPerformingSpecial = false;
    }

    private void HandleDeath()
    {
        BossEvents.TriggerBossDefeated();
    }
}