using UnityEngine;

public class SpellPickupSpawner : MonoBehaviour
{
    [SerializeField] private SpellPickup pickupPrefab;
    [SerializeField] private SpellSO[] availableSpells;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private float spawnRadius = 12f;
    [SerializeField] private float minSpawnDistance = 3f;

    private Transform _player;
    private float _timer;

    private void Start()
    {
        _player = GameManager.Instance?.PlayerTransform;
    }

    private void Update()
    {
        if (_player == null) return;

        _timer += Time.deltaTime;
        if (_timer >= spawnInterval)
        {
            _timer = 0f;
            Spawn();
        }
    }

    private void Spawn()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float distance = Random.Range(minSpawnDistance, spawnRadius);
        Vector2 spawnPos = (Vector2)_player.position + randomDirection * distance;

        SpellPickup pickup = Instantiate(pickupPrefab, spawnPos, Quaternion.identity);

        int randomIndex = Random.Range(0, availableSpells.Length);
        pickup.SetSpell(availableSpells[randomIndex]);
    }
}