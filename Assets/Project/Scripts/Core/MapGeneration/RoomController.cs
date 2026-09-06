using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomRole { Safe, Combat, Chest, Stairs }

[RequireComponent(typeof(BoxCollider2D))]
public class RoomController : MonoBehaviour
{
    [Tooltip("На сколько тайлов вглубь игрок должен зайти, чтобы комната активировалась.")]
    [SerializeField] private int activationInset = 2;

    private Room _room;
    private HashSet<Vector2Int> _globalFloor;
    private RoomRole _role;
    private DungeonConfigSO _config;

    // Тайлы КОРИДОРА, где встают блокеры (внешняя сторона проёма)
    private readonly HashSet<Vector2Int> _doorTiles = new();
    private readonly List<GameObject> _blockers = new();
    private readonly List<EnemyController> _waveEnemies = new();
    private Coroutine _lockRoutine;
    private int _aliveCount;
    private bool _activated;
    private bool _cleared;

    // ═══════════════════════════════════════
    // Поводок: волновые враги не могут покинуть арену,
    // пока комната не зачищена
    // ═══════════════════════════════════════
    private void Update()
    {
        if (!_activated || _cleared) return;

        BoundsInt b = _room.Bounds;

        foreach (var enemy in _waveEnemies)
        {
            if (enemy == null) continue;

            Vector2 pos = enemy.transform.position;
            int x = Mathf.FloorToInt(pos.x);
            int y = Mathf.FloorToInt(pos.y);

            if (x < b.xMin || x > b.xMax - 1 || y < b.yMin || y > b.yMax - 1)
            {
                int cx = Mathf.Clamp(x, b.xMin, b.xMax - 1);
                int cy = Mathf.Clamp(y, b.yMin, b.yMax - 1);
                enemy.transform.position = new Vector2(cx + 0.5f, cy + 0.5f);
            }
        }
    }

    public void Init(Room room, HashSet<Vector2Int> globalFloor, RoomRole role, DungeonConfigSO config)
    {
        _room = room;
        _globalFloor = globalFloor;
        _role = role;
        _config = config;

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.offset = new Vector2(_room.Bounds.center.x, _room.Bounds.center.y);
        col.size = new Vector2(_room.Bounds.size.x, _room.Bounds.size.y);

        ComputeDoors();

        if (_role == RoomRole.Stairs && _config.stairsPrefab != null)
            Instantiate(_config.stairsPrefab, ToWorld(_room.Center), Quaternion.identity, transform);
    }

    // ═══════════════════════════════════════
    // Активация: игрок зашёл внутрь (не на кромке)
    // ═══════════════════════════════════════
    private void OnTriggerEnter2D(Collider2D other) => TryActivate(other);
    private void OnTriggerStay2D(Collider2D other) => TryActivate(other);

    private void TryActivate(Collider2D other)
    {
        if (_activated || _cleared) return;
        if (_role == RoomRole.Safe || _role == RoomRole.Stairs) return;
        if (!other.CompareTag("Player")) return;
        if (!IsPlayerDeepInside()) return;

        _activated = true;
        SpawnWave();
        _lockRoutine = StartCoroutine(LockDoors());
    }

    private bool IsPlayerDeepInside()
    {
        Transform player = GameManager.Instance?.PlayerTransform;
        if (player == null) return false;
        int x = Mathf.FloorToInt(player.position.x);
        int y = Mathf.FloorToInt(player.position.y);
        BoundsInt b = _room.Bounds;
        return x >= b.xMin + activationInset && x <= b.xMax - 1 - activationInset
            && y >= b.yMin + activationInset && y <= b.yMax - 1 - activationInset;
    }

    // ═══════════════════════════════════════
    // Двери: 8 направлений — запечатываем и диагональные «ступеньки»
    // на стыках комнаты с коридором
    // ═══════════════════════════════════════
    private void ComputeDoors()
    {
        Vector2Int[] dirs =
        {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
        new Vector2Int(1, 1),  new Vector2Int(1, -1),
        new Vector2Int(-1, 1), new Vector2Int(-1, -1)
    };

        foreach (var tile in _room.FloorPositions)
        {
            if (!_globalFloor.Contains(tile)) continue;

            foreach (var d in dirs)
            {
                Vector2Int n = tile + d;

                // тайл снаружи комнаты и это пол → точка выхода
                if (_room.Bounds.Contains(new Vector3Int(n.x, n.y, 0))) continue;
                if (!_globalFloor.Contains(n)) continue;

                _doorTiles.Add(n); // HashSet сам уберёт дубликаты, break не нужен
            }
        }
    }

    // ═══════════════════════════════════════
    // Волна: спавн только по финальному полу
    // ═══════════════════════════════════════
    private void SpawnWave()
    {
        if (_config.wavePrefabs == null || _config.wavePrefabs.Length == 0) return;

        int area = _room.FloorPositions.Count;
        int byArea = Mathf.FloorToInt(area / Mathf.Max(1f, _config.tilesPerEnemy));
        int floorBonus = Mathf.Max(0, DungeonDirector.Instance.Floor - 1) * _config.enemyCountPerFloor;
        int count = Mathf.Clamp(byArea + floorBonus, _config.minWaveCount, _config.maxWaveCount);

        List<Vector2Int> tiles = new List<Vector2Int>(_room.FloorPositions);
        Vector2 playerPos = GameManager.Instance?.PlayerTransform != null
            ? (Vector2)GameManager.Instance.PlayerTransform.position
            : Vector2.zero;

        for (int i = 0; i < count; i++)
        {
            Vector2 world = Vector2.zero;
            bool found = false;
            for (int attempt = 0; attempt < 20; attempt++)
            {
                Vector2Int tile = tiles[Random.Range(0, tiles.Count)];
                if (!_globalFloor.Contains(tile)) continue;

                Vector2 candidate = ToWorld(tile);
                if (Vector2.Distance(candidate, playerPos) >= _config.minWaveSpawnDistance)
                {
                    world = candidate;
                    found = true;
                    break;
                }
            }
            if (!found) continue;

            EnemyController prefab = _config.wavePrefabs[Random.Range(0, _config.wavePrefabs.Length)];
            EnemyController enemy = Instantiate(prefab, world, Quaternion.identity, transform);
            enemy.Health.OnDeath += () => OnWaveEnemyDied(enemy);
            _waveEnemies.Add(enemy);
            _aliveCount++;
        }

        if (_aliveCount == 0) ClearRoom();
    }

    private void OnWaveEnemyDied(EnemyController enemy)
    {
        _waveEnemies.Remove(enemy);
        _aliveCount--;
        if (_aliveCount <= 0) ClearRoom();
    }

    // ═══════════════════════════════════════
    // Блокировка: двери закрываются ТОЛЬКО когда все бойцы внутри,
    // и блокер никогда не спавнится поверх существа
    // ═══════════════════════════════════════
    private IEnumerator LockDoors()
    {
        HashSet<Vector2Int> pending = new HashSet<Vector2Int>(_doorTiles);

        while (pending.Count > 0 && !_cleared)
        {
            if (AllCombatantsInside())
            {
                List<Vector2Int> placed = new List<Vector2Int>();
                foreach (var door in pending)
                {
                    Vector2 world = ToWorld(door);
                    if (!IsTileFree(world)) continue; // существо в проёме — ждём

                    if (_config.doorBlockerPrefab != null)
                        _blockers.Add(Instantiate(_config.doorBlockerPrefab, world, Quaternion.identity, transform));
                    placed.Add(door);
                }
                foreach (var p in placed) pending.Remove(p);
            }

            if (pending.Count > 0 && !_cleared) yield return null;
        }
    }

    /// <summary>
    // Печатать арену можно, только когда игрок и все враги волны внутри —
    // тогда никто не окажется вытолкнутым за дверь или запертым в блокере.
    /// </summary>
    private bool AllCombatantsInside()
    {
        Transform player = GameManager.Instance?.PlayerTransform;
        if (player == null) return false;
        if (!RoomBoundsContains(player.position)) return false;

        foreach (var enemy in _waveEnemies)
        {
            if (enemy == null) continue;
            if (!RoomBoundsContains(enemy.transform.position)) return false;
        }
        return true;
    }

    private bool RoomBoundsContains(Vector2 world)
    {
        int x = Mathf.FloorToInt(world.x);
        int y = Mathf.FloorToInt(world.y);
        return _room.Bounds.Contains(new Vector3Int(x, y, 0));
    }

    /// <summary>
    // Свободен ли тайл проёма: нет игрока и врагов в радиусе блокера.
    /// </summary>
    private bool IsTileFree(Vector2 world)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(world, 0.8f);
        foreach (var h in hits)
        {
            if (h.CompareTag("Player") || h.CompareTag("Enemy"))
                return false;
        }
        return true;
    }

    private void ClearRoom()
    {
        if (_cleared) return;
        _cleared = true;

        if (_lockRoutine != null) StopCoroutine(_lockRoutine);
        foreach (var blocker in _blockers)
            if (blocker != null) Destroy(blocker);
        _blockers.Clear();

        if (_role == RoomRole.Chest)
        {
            if (_config.chestPrefab != null)
            {
                GameObject chest = Instantiate(_config.chestPrefab, ToWorld(_room.Center), Quaternion.identity, transform);
                chest.GetComponent<Chest>()?.Init(_config.rewardSpells);
                Debug.Log($"[RoomController] Комната {_room.Center} зачищена — сундук заспавнен.");
            }
            else
            {
                Debug.LogWarning("[RoomController] Комната зачищена, но в DungeonConfig НЕ назначен chestPrefab!");
            }
        }
    }

    private static Vector2 ToWorld(Vector2Int tile) => new Vector2(tile.x + 0.5f, tile.y + 0.5f);
}