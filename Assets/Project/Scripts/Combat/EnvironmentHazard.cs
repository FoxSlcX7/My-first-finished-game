using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnvironmentHazard : MonoBehaviour
{
    [Header("Параметры урона")]
    [Tooltip("Урон за один тик")]
    [SerializeField] private int damagePerTick = 5;

    [Tooltip("Интервал между тиками урона в секундах")]
    [SerializeField] private float tickInterval = 0.5f;

    [Tooltip("Наносить ли мгновенный урон сразу при входе в зону")]
    [SerializeField] private bool damageOnEnter = true;

    [Header("Цели поражения")]
    [SerializeField] private bool affectsPlayer = true;
    [SerializeField] private bool affectsEnemies = true;

    [Header("Влияние на скорость (Замедление)")]
    [Tooltip("1.0 = обычная скорость, 0.6 = замедление на 40% (вязкая лава/грязь)")]
    [Range(0.2f, 1f)]
    [SerializeField] private float slowMultiplier = 0.75f;

    private readonly Dictionary<Health, float> _insideEntities = new();
    private readonly List<Health> _keysBuffer = new();

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidTarget(other, out Health health)) return;

        if (!_insideEntities.ContainsKey(health))
        {
            _insideEntities[health] = Time.time + tickInterval;

            if (damageOnEnter)
            {
                health.TakeDamage(damagePerTick);
            }

            ApplySlow(other, true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<Health>(out var health) || other.GetComponentInParent<Health>() != null)
        {
            Health targetHealth = health != null ? health : other.GetComponentInParent<Health>();
            if (targetHealth != null && _insideEntities.ContainsKey(targetHealth))
            {
                _insideEntities.Remove(targetHealth);
                ApplySlow(other, false);
            }
        }
    }

    private void Update()
    {
        if (_insideEntities.Count == 0) return;

        float currentTime = Time.time;

        // Копируем ключи в буфер, чтобы безопасно менять словарь во время итерации
        _keysBuffer.Clear();
        foreach (var key in _insideEntities.Keys)
        {
            _keysBuffer.Add(key);
        }

        for (int i = 0; i < _keysBuffer.Count; i++)
        {
            Health health = _keysBuffer[i];

            if (!_insideEntities.TryGetValue(health, out float nextTick)) continue;

            // Если объект погиб или удален
            if (health == null || health.CurrentHealth <= 0)
            {
                _insideEntities.Remove(health);
                continue;
            }

            if (currentTime >= nextTick)
            {
                health.TakeDamage(damagePerTick);

                // Проверяем, существует ли объект после получения урона (мог умереть в этом кадре)
                if (_insideEntities.ContainsKey(health))
                {
                    if (health == null || health.CurrentHealth <= 0)
                    {
                        _insideEntities.Remove(health);
                    }
                    else
                    {
                        _insideEntities[health] = currentTime + tickInterval;
                    }
                }
            }
        }
    }

    private bool IsValidTarget(Collider2D other, out Health health)
    {
        health = null;

        bool isPlayer = other.CompareTag("Player");
        bool isEnemy = other.CompareTag("Enemy");

        if ((isPlayer && affectsPlayer) || (isEnemy && affectsEnemies))
        {
            health = other.GetComponent<Health>();
            if (health == null) health = other.GetComponentInParent<Health>();
            return health != null && health.CurrentHealth > 0;
        }

        return false;
    }

    private void ApplySlow(Collider2D other, bool apply)
    {
        if (Mathf.Approximately(slowMultiplier, 1f)) return;

        if (other.CompareTag("Player"))
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.SetSurfaceTraction(apply ? slowMultiplier : 1f);
            }
        }
    }

    private void OnDisable()
    {
        _insideEntities.Clear();
        _keysBuffer.Clear();
    }
}