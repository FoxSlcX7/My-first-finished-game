using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpellPickupSpawner : MonoBehaviour
{
    [SerializeField] private SpellPickup pickupPrefab;
    [SerializeField] private SpellSO[] availableSpells;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private float minSpawnDistanceFromPlayer = 5f;
    [SerializeField] private float maxSpawnDistanceFromPlayer = 15f;
    [SerializeField] private MapGenerator mapGenerator; // ссылка на генератор карты
    [SerializeField] private LayerMask obstacleLayerMask; // слой стен, чтобы проверять столкновения

    private Transform _player;
    private float _timer;
    private List<Vector2Int> _cachedFloorPositions;

    private void Start()
    {
        _player = GameManager.Instance?.PlayerTransform;
        // Кэшируем позиции пола после генерации карты
        if (mapGenerator != null)
        {
            // Подписываемся на событие, если MapGenerator его предоставляет, либо просто ждём
            // Простой способ: отложенный вызов через Invoke
            Invoke(nameof(CacheFloorPositions), 0.2f);
        }
        else
        {
            Debug.LogError("SpellPickupSpawner: не назначен MapGenerator!");
        }
    }

    private void CacheFloorPositions()
    {
        if (mapGenerator.FloorPositions != null)
        {
            _cachedFloorPositions = mapGenerator.FloorPositions.ToList();
            Debug.Log($"SpellPickupSpawner: закешировано {_cachedFloorPositions.Count} точек пола.");
        }
        else
        {
            Debug.LogError("SpellPickupSpawner: нет позиций пола в MapGenerator!");
        }
    }

    private void Update()
    {
        if (_player == null || _cachedFloorPositions == null || _cachedFloorPositions.Count == 0) return;

        _timer += Time.deltaTime;
        if (_timer >= spawnInterval)
        {
            _timer = 0f;
            Spawn();
        }
    }

    private void Spawn()
    {
        Vector2? spawnPos = GetRandomValidPosition();
        if (!spawnPos.HasValue) return;

        SpellPickup pickup = Instantiate(pickupPrefab, spawnPos.Value, Quaternion.identity);
        int randomIndex = Random.Range(0, availableSpells.Length);
        pickup.SetSpell(availableSpells[randomIndex]);
    }

    private Vector2? GetRandomValidPosition()
    {
        if (_cachedFloorPositions == null || _cachedFloorPositions.Count == 0) return null;

        Vector2 playerPos = _player.position;

        // Пытаемся 30 раз найти подходящую точку
        for (int i = 0; i < 30; i++)
        {
            Vector2Int randomPoint = _cachedFloorPositions[Random.Range(0, _cachedFloorPositions.Count)];
            Vector2 worldPos = new Vector2(randomPoint.x + 0.5f, randomPoint.y + 0.5f); // центр тайла

            float distance = Vector2.Distance(worldPos, playerPos);
            if (distance < minSpawnDistanceFromPlayer || distance > maxSpawnDistanceFromPlayer)
                continue;

            // Проверяем, что место свободно (нет стен, других объектов)
            if (IsPositionFree(worldPos))
                return worldPos;
        }

        // Если не нашли — возвращаем null (ничего не спавним в этом цикле)
        Debug.LogWarning("SpellPickupSpawner: не удалось найти свободное место для спавна.");
        return null;
    }

    private bool IsPositionFree(Vector2 position)
    {
        // Проверяем наличие коллайдеров на слое препятствий (стены)
        Collider2D hit = Physics2D.OverlapCircle(position, 0.3f, obstacleLayerMask);
        return hit == null;
    }
}