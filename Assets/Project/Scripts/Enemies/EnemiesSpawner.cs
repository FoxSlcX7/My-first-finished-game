using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private EnemyController meleePrefab;
    [SerializeField] private EnemyController rangedPrefab;

    [Header("Map")]
    [SerializeField] private MapGenerator mapGenerator;

    [Header("Spawn Settings")]
    [SerializeField] private float rangedSpawnChance = 0.3f;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int maxEnemies = 15;
    [SerializeField] private float minSpawnDistanceFromPlayer = 8f;

    [Header("Validation")]
    [SerializeField] private float spawnCheckRadius = 0.4f;
    [SerializeField] private LayerMask wallLayerMask;

    private float _timer;
    private int _activeEnemies;
    private List<Vector2Int> _validSpawnPoints;

    private void Start()
    {
        Invoke(nameof(CacheSpawnPoints), 0.15f);
    }

    private void OnEnable()
    {
        if (GameEvents.OnEnemyDied != null)
        {
            GameEvents.OnEnemyDied.AddListener(HandleEnemyDeath);
        }
    }

    private void OnDisable()
    {
        if (GameEvents.OnEnemyDied != null)
        {
            GameEvents.OnEnemyDied.RemoveListener(HandleEnemyDeath);
        }
    }

    private void CacheSpawnPoints()
    {
        if (mapGenerator == null)
        {
            Debug.LogError("EnemySpawner: не назначен MapGenerator!");
            return;
        }

        _validSpawnPoints = mapGenerator.FloorPositions.ToList();
        Debug.Log($"Спавн-точек загружено: {_validSpawnPoints.Count}");
    }

    private void Update()
    {
        if (_validSpawnPoints == null || _validSpawnPoints.Count == 0) return;
        if (GameManager.Instance?.PlayerTransform == null) return;

        _timer += Time.deltaTime;

        if (_timer >= spawnInterval)
        {
            _timer = 0f;
            TrySpawn();
        }
    }

    private void TrySpawn()
    {
        if (_activeEnemies >= maxEnemies) return;

        Vector2? spawnPos = GetRandomValidSpawnPosition();
        if (!spawnPos.HasValue) return;

        EnemyController prefab = Random.value < rangedSpawnChance ? rangedPrefab : meleePrefab;

        if (prefab != null)
        {
            Instantiate(prefab, spawnPos.Value, Quaternion.identity);
            _activeEnemies++;
        }
    }

    private Vector2? GetRandomValidSpawnPosition()
    {
        Transform player = GameManager.Instance?.PlayerTransform;
        Vector2 playerPos = player != null ? player.position : Vector2.zero;

        for (int i = 0; i < 30; i++)
        {
            Vector2Int randomPoint = _validSpawnPoints[Random.Range(0, _validSpawnPoints.Count)];
            Vector2 worldPos = new Vector2(randomPoint.x + 0.5f, randomPoint.y + 0.5f);

            if (player != null)
            {
                float dist = Vector2.Distance(worldPos, playerPos);
                if (dist < minSpawnDistanceFromPlayer) continue;
            }

            if (IsPositionFree(worldPos))
                return worldPos;
        }

        return null;
    }

    private bool IsPositionFree(Vector2 position)
    {
        Collider2D hit = Physics2D.OverlapCircle(position, spawnCheckRadius, wallLayerMask);
        return hit == null;
    }

    private void HandleEnemyDeath()
    {
        _activeEnemies = Mathf.Max(0, _activeEnemies - 1);
    }
}