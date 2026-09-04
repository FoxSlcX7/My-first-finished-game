using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private MeleeEnemy meleePrefab;
    [SerializeField] private RangedEnemy rangedPrefab;
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private float rangedSpawnChance = 0.3f;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int maxEnemies = 15;
    [SerializeField] private float minSpawnDistanceFromPlayer = 8f;
    [SerializeField] private float spawnCheckRadius = 0.4f;
    [SerializeField] private LayerMask wallLayerMask;

    private float _timer;
    private int _activeEnemies;
    private List<Vector2Int> _validSpawnPoints;

    private void Start()
    {
        Invoke(nameof(CacheSpawnPoints), 0.15f);
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

        Health enemyHealth = null;

        if (Random.value < rangedSpawnChance)
        {
            RangedEnemy enemy = Instantiate(rangedPrefab, spawnPos.Value, Quaternion.identity);
            enemyHealth = enemy.GetComponent<Health>();
        }
        else
        {
            MeleeEnemy enemy = Instantiate(meleePrefab, spawnPos.Value, Quaternion.identity);
            enemyHealth = enemy.GetComponent<Health>();
        }

        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += HandleEnemyDeath;
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