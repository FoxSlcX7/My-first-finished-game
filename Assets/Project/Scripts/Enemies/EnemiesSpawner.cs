using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float spawnRadius = 15f;
    [SerializeField] private float minSpawnDistance = 5f;
    [SerializeField] private int maxEnemies = 20;

    private Transform _player;
    private float _timer;
    private int _activeEnemies;

    private void Start()
    {
        _player = GameObject.FindWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (_player == null) return;

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

        Vector2 spawnPos = GetRandomSpawnPosition();
        Enemy enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        _activeEnemies++;

        // Отслеживаем смерть врага, чтобы уменьшить счетчик
        Health enemyHealth = enemy.GetComponent<Health>();
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += HandleEnemyDeath;
        }
    }

    private void HandleEnemyDeath()
    {
        _activeEnemies--;
    }

    private Vector2 GetRandomSpawnPosition()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float distance = Random.Range(minSpawnDistance, spawnRadius);
        return (Vector2)_player.position + randomDirection * distance;
    }
}