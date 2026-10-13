using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomRole { Safe, Combat, Chest, Stairs, Boss }

[RequireComponent(typeof(BoxCollider2D))]
public class RoomController : MonoBehaviour
{
    #region Inspector Settings

    [Tooltip("На сколько тайлов вглубь игрок должен зайти, чтобы комната активировалась.")]
    [SerializeField] private int activationInset = 2;

    #endregion

    #region Public Events & State

    public event System.Action OnRoomCleared;
    public bool IsCleared => _cleared;
    public HashSet<Vector2Int> DoorTiles => _doorTiles;
    public static event System.Action<int> OnWaveEnemiesCountChanged;

    /// <summary>Роль комнаты (Safe/Combat/Chest/Stairs/Boss) для отображения на миникарте.</summary>
    public RoomRole Role => _role;

    #endregion

    #region Private Fields: Room & Dungeon Data

    private Room _room;
    private HashSet<Vector2Int> _globalFloor;
    private RoomRole _role;
    private DungeonConfigSO _config;
    private bool _activated;
    private bool _cleared;

    #endregion

    #region Private Fields: Wave & Enemies

    private EnemyController[] _wavePrefabs;
    private readonly List<EnemyController> _waveEnemies = new();
    private Coroutine _spawnRoutine;
    private int _remainingToSpawn;
    private int _aliveCount;

    #endregion

    #region Private Fields: Doors & Blockers

    private readonly HashSet<Vector2Int> _doorTiles = new();
    private readonly List<GameObject> _blockers = new();
    private Coroutine _lockRoutine;

    #endregion

    #region 1. Initialization

    public void Init(Room room, HashSet<Vector2Int> globalFloor, RoomRole role, DungeonConfigSO config, EnemyController[] customWavePrefabs = null)
    {
        _room = room;
        _globalFloor = globalFloor;
        _role = role;
        _config = config;
        _wavePrefabs = (customWavePrefabs != null && customWavePrefabs.Length > 0) ? customWavePrefabs : config.wavePrefabs;
        _wavePrefabs = FilterValidPrefabs(_wavePrefabs);

        // Настройка триггера входа в комнату
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.offset = new Vector2(_room.Bounds.center.x, _room.Bounds.center.y);
        col.size = new Vector2(_room.Bounds.size.x, _room.Bounds.size.y);

        // Расчет точек дверных проемов
        ComputeDoors();

        // Лестница на следующий этаж
        if (_role == RoomRole.Stairs && _config.stairsPrefab != null)
        {
            Instantiate(_config.stairsPrefab, ToWorld(_room.Center), Quaternion.identity, transform);
        }

        // Спавн опасностей биома (лавовые лужи / шипы)
        SpawnBiomeHazards();

        // Спавн разрушаемых объектов (ящики у стен и в углах)
        SpawnDestructibles();
    }

    private void ComputeDoors()
    {
        Vector2Int[] dirs =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new Vector2Int(1, 1), new Vector2Int(1, -1),
            new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };

        foreach (var tile in _room.FloorPositions)
        {
            if (!_globalFloor.Contains(tile)) continue;

            foreach (var d in dirs)
            {
                Vector2Int n = tile + d;

                // Если тайл снаружи комнаты и это пол — это точка выхода/двери
                if (_room.Bounds.Contains(new Vector3Int(n.x, n.y, 0))) continue;
                if (!_globalFloor.Contains(n)) continue;

                _doorTiles.Add(n);
            }
        }
    }

    #endregion

    #region 2. Room Activation & Lifecycle

    private void Update()
    {
        if (!_activated || _cleared) return;

        // Пассивный сторожевой таймер: когда спавн завершен, проверяем отсутствие живых врагов
        if (_remainingToSpawn <= 0)
        {
            CheckRoomCleared();
        }
    }

    private void OnTriggerEnter2D(Collider2D other) => TryActivate(other);
    private void OnTriggerStay2D(Collider2D other) => TryActivate(other);

    private void TryActivate(Collider2D other)
    {
        if (_activated || _cleared) return;
        if (_role == RoomRole.Safe || _role == RoomRole.Stairs) return;
        if (!other.CompareTag("Player")) return;
        if (!IsPlayerDeepInside()) return;

        _activated = true;

        if (_role == RoomRole.Boss)
        {
            SpawnBoss();
        }
        else
        {
            SpawnWave();
        }

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

    #endregion

    #region 3. Wave & Enemy Management

    private void SpawnWave()
    {
        if (_wavePrefabs == null || _wavePrefabs.Length == 0)
        {
            ClearRoom();
            return;
        }

        int area = _room.FloorPositions.Count;
        int byArea = Mathf.FloorToInt(area / Mathf.Max(1f, _config.tilesPerEnemy));
        int floorBonus = Mathf.Max(0, DungeonDirector.Instance.Floor - 1) * _config.enemyCountPerFloor;
        int count = Mathf.Clamp(byArea + floorBonus, _config.minWaveCount, _config.maxWaveCount);

        _remainingToSpawn = count;
        _spawnRoutine = StartCoroutine(SpawnWaveRoutine());
    }

    /// <summary>
    /// Возвращает массив префабов без null-элементов (защита от битых ссылок).
    /// </summary>
    private static EnemyController[] FilterValidPrefabs(EnemyController[] prefabs)
    {
        if (prefabs == null || prefabs.Length == 0) return new EnemyController[0];

        List<EnemyController> valid = new List<EnemyController>();
        foreach (EnemyController p in prefabs)
        {
            if (p != null) valid.Add(p);
        }
        return valid.ToArray();
    }

    /// <summary>
    /// Выбирает валидный префаб для спавна. Сначала случайный, при null — первый валидный.
    /// </summary>
    private EnemyController PickValidPrefab()
    {
        if (_wavePrefabs == null || _wavePrefabs.Length == 0) return null;

        EnemyController chosen = _wavePrefabs[Random.Range(0, _wavePrefabs.Length)];
        if (chosen != null) return chosen;

        foreach (EnemyController p in _wavePrefabs)
        {
            if (p != null) return p;
        }
        return null;
    }

    private void SpawnBoss()
    {
        int floor = DungeonDirector.Instance != null ? DungeonDirector.Instance.Floor : 1;
        EnemyController prefabToSpawn = (floor >= 10) ? _config.boss2Prefab : _config.boss1Prefab;

        if (prefabToSpawn == null)
        {
            ClearRoom();
            return;
        }

        Vector2 spawnPos = ToWorld(_room.Center);
        EnemyController boss = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, transform);

        if (_config.balanceConfig != null)
        {
            boss.ApplyBalance(_config.balanceConfig, floor);
        }

        boss.Health.OnDeath += () =>
        {
            _waveEnemies.Remove(boss);
            _aliveCount = 0;
            _remainingToSpawn = 0;
            ClearRoom();
        };

        _waveEnemies.Add(boss);
        _aliveCount = 1;
    }

    private IEnumerator SpawnWaveRoutine()
    {
        yield return new WaitForSeconds(_config.waveStartDelay);

        while (_remainingToSpawn > 0 && !_cleared)
        {
            int batch = Mathf.Min(Mathf.Max(1, _config.waveBatchSize), _remainingToSpawn);
            for (int i = 0; i < batch; i++)
            {
                SpawnOneEnemy();
            }

            CheckRoomCleared();

            if (_remainingToSpawn > 0)
            {
                yield return new WaitForSeconds(_config.waveSpawnInterval);
            }
        }

        // Финальная проверка по окончании всех волн спавна
        CheckRoomCleared();
    }

    private void SpawnOneEnemy()
    {
        List<Vector2Int> tiles = new List<Vector2Int>(_room.FloorPositions);
        Vector2 playerPos = GameManager.Instance?.PlayerTransform != null
            ? (Vector2)GameManager.Instance.PlayerTransform.position
            : Vector2.zero;

        Vector2 world = Vector2.zero;
        bool found = false;

        // 1. Поиск точки на безопасном расстоянии от игрока
        for (int attempt = 0; attempt < 30; attempt++)
        {
            Vector2Int tile = tiles[Random.Range(0, tiles.Count)];
            if (!_globalFloor.Contains(tile)) continue;

            Vector2 candidate = ToWorld(tile);

            if (Vector2.Distance(candidate, playerPos) < _config.minWaveSpawnDistance)
                continue;

            Collider2D[] hits = Physics2D.OverlapCircleAll(candidate, 0.4f);
            bool isBlocked = false;
            foreach (var hit in hits)
            {
                if (hit.isTrigger) continue;
                isBlocked = true;
                break;
            }

            if (isBlocked) continue;

            world = candidate;
            found = true;
            break;
        }

        // 2. Фолбэк: если безопасная точка не найдена, спавним на любой свободной клетке арены
        if (!found)
        {
            foreach (var tile in tiles)
            {
                if (!_globalFloor.Contains(tile)) continue;
                Vector2 candidate = ToWorld(tile);
                Collider2D[] hits = Physics2D.OverlapCircleAll(candidate, 0.35f);
                bool blocked = false;
                foreach (var h in hits)
                {
                    if (!h.isTrigger) { blocked = true; break; }
                }
                if (!blocked)
                {
                    world = candidate;
                    found = true;
                    break;
                }
            }
        }

        _remainingToSpawn--;
        if (!found) return;

        EnemyController prefab = PickValidPrefab();
        if (prefab == null)
        {
            // Нет валидного префаба — пропускаем спавн (счётчик уже уменьшен выше)
            return;
        }

        EnemyController enemy = Instantiate(prefab, world, Quaternion.identity, transform);

        int floor = DungeonDirector.Instance != null ? DungeonDirector.Instance.Floor : 1;
        if (_config != null && _config.balanceConfig != null)
        {
            enemy.ApplyBalance(_config.balanceConfig, floor);
        }

        enemy.SetState(new ChaseState());

        enemy.Health.OnDeath += () => OnWaveEnemyDied(enemy);
        _waveEnemies.Add(enemy);
        _aliveCount++;
    }

    private void OnWaveEnemyDied(EnemyController enemy)
    {
        _waveEnemies.Remove(enemy);
        _aliveCount--;
        CheckRoomCleared();
    }

    private void CheckRoomCleared()
    {
        if (_cleared) return;

        _waveEnemies.RemoveAll(e => e == null || e.Health == null || e.Health.IsDead);
        int totalRemaining = _remainingToSpawn + _waveEnemies.Count;

        // Оповещаем UI о количестве оставшихся врагов
        OnWaveEnemiesCountChanged?.Invoke(totalRemaining);

        if (totalRemaining <= 0)
        {
            ClearRoom();
        }
    }

    #endregion

    #region 4. Doors & Arena Locking

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
                    {
                        _blockers.Add(Instantiate(_config.doorBlockerPrefab, world, Quaternion.identity, transform));
                    }
                    placed.Add(door);
                }
                foreach (var p in placed) pending.Remove(p);
            }

            if (pending.Count > 0 && !_cleared) yield return null;
        }
    }

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
        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
        _cleared = true;

        // Открываем двери (уничтожаем блокеры)
        if (_lockRoutine != null) StopCoroutine(_lockRoutine);
        foreach (var blocker in _blockers)
        {
            if (blocker != null) Destroy(blocker);
        }
        _blockers.Clear();

        // Спавн сундука в сокровищнице
        if (_role == RoomRole.Chest)
        {
            if (_config.chestPrefab != null)
            {
                Instantiate(_config.chestPrefab, ToWorld(_room.Center), Quaternion.identity, transform);
                Debug.Log($"[RoomController] Комната {_room.Center} зачищена — сундук заспавнен.");
            }
            else
            {
                Debug.LogWarning("[RoomController] Комната зачищена, но в DungeonConfig НЕ назначен chestPrefab!");
            }
        }

        // Завершение забега / спавн лестницы при победе над боссом
        if (_role == RoomRole.Boss)
        {
            int currentFloor = DungeonDirector.Instance != null ? DungeonDirector.Instance.Floor : 1;

            if (currentFloor >= 10)
            {
                Debug.Log("[RoomController] Архимаг повержен! Забег успешно пройден!");
                GameManager.Instance?.Victory();
            }
            else if (_config.stairsPrefab != null)
            {
                Instantiate(_config.stairsPrefab, ToWorld(_room.Center), Quaternion.identity, transform);
                Debug.Log("[RoomController] Босс повержен! Заспавнена лестница на следующий этаж.");
            }
        }

        OnWaveEnemiesCountChanged?.Invoke(0);
        OnRoomCleared?.Invoke();
    }

    #endregion

    #region 5. Biome Props & Hazards

    private void SpawnBiomeHazards()
    {
        if (_role == RoomRole.Safe) return;

        BiomeConfigSO currentBiome = DungeonDirector.Instance?.GetCurrentBiome();
        if (currentBiome == null || currentBiome.hazardPrefab == null) return;
        if (currentBiome.maxHazardsPerRoom <= 0) return;

        int count = Random.Range(currentBiome.minHazardsPerRoom, currentBiome.maxHazardsPerRoom + 1);
        if (count <= 0) return;

        List<Vector2Int> candidates = new List<Vector2Int>();
        Vector2Int center = _room.Center;

        foreach (var tile in _room.FloorPositions)
        {
            if (tile == center || tile == center + Vector2Int.right) continue;

            bool nearDoor = false;
            foreach (var door in _doorTiles)
            {
                if (Vector2Int.Distance(tile, door) < 2f)
                {
                    nearDoor = true;
                    break;
                }
            }

            if (!nearDoor) candidates.Add(tile);
        }

        for (int i = 0; i < count && candidates.Count > 0; i++)
        {
            int idx = Random.Range(0, candidates.Count);
            Vector2Int chosenTile = candidates[idx];
            candidates.RemoveAt(idx);

            Vector2 spawnPos = ToWorld(chosenTile);
            Instantiate(currentBiome.hazardPrefab, spawnPos, Quaternion.identity, transform);
        }
    }

    private void SpawnDestructibles()
    {
        if (_role == RoomRole.Safe) return;

        BiomeConfigSO currentBiome = DungeonDirector.Instance?.GetCurrentBiome();
        if (currentBiome == null || currentBiome.destructiblePrefab == null) return;
        if (currentBiome.maxDestructiblesPerRoom <= 0) return;

        int count = Random.Range(currentBiome.minDestructiblesPerRoom, currentBiome.maxDestructiblesPerRoom + 1);
        if (count <= 0) return;

        List<Vector2Int> candidates = new List<Vector2Int>();
        Vector2Int center = _room.Center;
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var tile in _room.FloorPositions)
        {
            // Отступ от центра арены (алтари, сундуки)
            if (Vector2Int.Distance(tile, center) < 3f) continue;

            // Отступ от дверей минимум 3 тайла
            bool nearDoor = false;
            foreach (var door in _doorTiles)
            {
                if (Vector2Int.Distance(tile, door) < 3f)
                {
                    nearDoor = true;
                    break;
                }
            }
            if (nearDoor) continue;

            // Спавним ТОЛЬКО у стен или в углах (минимум 1 сосед — стена)
            int wallCount = 0;
            foreach (var d in dirs)
            {
                if (!_globalFloor.Contains(tile + d))
                    wallCount++;
            }

            if (wallCount >= 1)
            {
                candidates.Add(tile);
            }
        }

        for (int i = 0; i < count && candidates.Count > 0; i++)
        {
            int idx = Random.Range(0, candidates.Count);
            Vector2Int chosenTile = candidates[idx];
            candidates.RemoveAt(idx);

            Vector2 spawnPos = ToWorld(chosenTile);
            Instantiate(currentBiome.destructiblePrefab, spawnPos, Quaternion.identity, transform);
        }
    }

    #endregion

    #region 6. Coordinate Helpers

    private static Vector2 ToWorld(Vector2Int tile) => new Vector2(tile.x + 0.5f, tile.y + 0.5f);

    #endregion
}