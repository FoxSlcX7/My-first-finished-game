using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Главный координатор генерации подземелья, смены биомов, этажей и расстановки интерактивных объектов.
/// </summary>
public class DungeonDirector : MonoBehaviour
{
    #region Singleton & State

    public static DungeonDirector Instance { get; private set; }
    public int Floor { get; private set; } = 1;

    #endregion

    #region Inspector Settings

    [Header("Генерация и окружение")]
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private MapVisualizer visualizer;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private DungeonConfigSO config;

    [Header("Биомы")]
    [SerializeField] private BiomeConfigSO[] biomes;

    #endregion

    #region Private Fields

    private readonly List<GameObject> _roomObjects = new();
    private readonly List<RoomRole> _roomRoles = new();
    private readonly HashSet<int> _altarRooms = new();
    private static Vector2 ToWorld(Vector2Int tile) => new Vector2(tile.x + 0.5f, tile.y + 0.5f);

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (mapGenerator != null)
        {
            mapGenerator.OnMapGenerated += BuildRooms;
        }
    }

    private void OnDisable()
    {
        if (mapGenerator != null)
        {
            mapGenerator.OnMapGenerated -= BuildRooms;
        }
    }

    #endregion

    #region Biome Management

    public BiomeConfigSO GetCurrentBiome()
    {
        if (biomes == null || biomes.Length == 0) return null;

        foreach (var biome in biomes)
        {
            if (biome != null && Floor >= biome.startFloor && Floor <= biome.endFloor)
                return biome;
        }

        return biomes[0];
    }

    #endregion

    #region Room Role Queries (для миникарты и UI)

    /// <summary>Возвращает роль комнаты по её индексу в MapGenerator.Rooms.</summary>
    public RoomRole GetRoomRole(int roomIndex)
    {
        if (_roomRoles == null || roomIndex < 0 || roomIndex >= _roomRoles.Count) return RoomRole.Combat;
        return _roomRoles[roomIndex];
    }

    /// <summary>Есть ли в указанной комнате алтарь (SpellAltar).</summary>
    public bool RoomHasAltar(int roomIndex) => _altarRooms != null && _altarRooms.Contains(roomIndex);

    #endregion

    #region Room & Dungeon Generation

    private void BuildRooms()
    {
        ClearRoomObjects();
        _roomRoles.Clear();
        _altarRooms.Clear();

        BiomeConfigSO currentBiome = GetCurrentBiome();
        if (currentBiome != null)
        {
            if (visualizer != null)
            {
                visualizer.SetBiomeTiles(currentBiome.floorTile, currentBiome.wallTile);
                if (mapGenerator != null)
                {
                    visualizer.PaintFloor(mapGenerator.FloorPositions);
                    visualizer.PaintWalls(mapGenerator.WallPositions);
                }
            }

            var player = GameManager.Instance?.PlayerTransform?.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetSurfaceTraction(currentBiome.surfaceTraction);
            }

            Debug.Log($"[DungeonDirector] Применен биом: {currentBiome.biomeName} (Сцепление: {currentBiome.surfaceTraction})");
        }

        List<Room> rooms = mapGenerator.Rooms;
        if (rooms == null || rooms.Count == 0) return;

        // Стартовая комната
        int startIndex = -1;
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].FloorPositions.Contains(Vector2Int.zero))
            {
                startIndex = i;
                break;
            }
        }
        if (startIndex == -1)
        {
            float best = float.MaxValue;
            for (int i = 0; i < rooms.Count; i++)
            {
                float d = (rooms[i].Center - Vector2Int.zero).sqrMagnitude;
                if (d < best) { best = d; startIndex = i; }
            }
        }

        // Дальняя комната (лестница или босс)
        int stairsIndex = startIndex;
        float worst = float.MinValue;
        for (int i = 0; i < rooms.Count; i++)
        {
            if (i == startIndex) continue;
            float d = (rooms[i].Center - rooms[startIndex].Center).sqrMagnitude;
            if (d > worst) { worst = d; stairsIndex = i; }
        }

        // Комната с сундуком
        int chestIndex = -1;
        List<int> candidates = new List<int>();
        for (int i = 0; i < rooms.Count; i++)
        {
            if (i != startIndex && i != stairsIndex) candidates.Add(i);
        }
        if (candidates.Count > 0)
        {
            chestIndex = candidates[Random.Range(0, candidates.Count)];
        }

        List<RoomController> controllers = new List<RoomController>();
        EnemyController[] biomeEnemies = (currentBiome != null && currentBiome.biomeEnemies != null && currentBiome.biomeEnemies.Length > 0)
            ? currentBiome.biomeEnemies
            : null;

        for (int i = 0; i < rooms.Count; i++)
        {
            RoomRole role = RoomRole.Combat;
            if (i == startIndex) role = RoomRole.Safe;
            else if (i == stairsIndex)
            {
                role = (Floor == 5 || Floor == 10) ? RoomRole.Boss : RoomRole.Stairs;
            }
            else if (i == chestIndex) role = RoomRole.Chest;

            _roomRoles.Add(role);

            GameObject roomObj = new GameObject($"Room_{i}_{role}");
            roomObj.transform.SetParent(transform);
            RoomController rc = roomObj.AddComponent<RoomController>();
            rc.Init(rooms[i], mapGenerator.FloorPositions, role, config, biomeEnemies);
            _roomObjects.Add(roomObj);
            controllers.Add(rc);
        }

        PlaceAltars(rooms, controllers, startIndex, stairsIndex);
        PlacePlayerAt(rooms[startIndex].Center);

        HashSet<Vector2Int> allDoors = new HashSet<Vector2Int>();
        foreach (var rc in controllers)
        {
            foreach (var door in rc.DoorTiles)
            {
                allDoors.Add(door);
            }
        }

        SpawnCorridorDecorations(mapGenerator.FloorPositions, rooms, allDoors);

        Debug.Log($"[DungeonDirector] Этаж {Floor}: комнат={rooms.Count}, биом={currentBiome?.biomeName}");
    }

    #endregion

    #region Floor Progression

    public void NextFloor()
    {
        Floor++;

        // Очищаем всех врагов через статический реестр (без вызова FindObjectsByType)
        for (int i = EnemyController.ActiveEnemies.Count - 1; i >= 0; i--)
        {
            var enemy = EnemyController.ActiveEnemies[i];
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }

        ClearRoomObjects();

        mapGenerator.GenerateMap();
        enemySpawner.RefreshSpawnPoints();
    }

    private void PlacePlayerAt(Vector2Int tile)
    {
        Transform player = GameManager.Instance?.PlayerTransform;
        if (player == null) return;
        player.position = new Vector3(tile.x + 0.5f, tile.y + 0.5f, 0f);
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private void ClearRoomObjects()
    {
        foreach (var go in _roomObjects)
        {
            if (go != null) Destroy(go);
        }
        _roomObjects.Clear();
    }

    #endregion

    #region Altars & Corridor Spawning

    private void PlaceAltars(List<Room> rooms, List<RoomController> controllers, int startIndex, int stairsIndex)
    {
        if (config.altarPrefab == null || config.altarSpellPool == null || config.altarSpellPool.Length == 0) return;

        int count = Random.Range(config.altarCountMin, config.altarCountMax + 1);

        List<int> candidates = new List<int>();
        for (int i = 0; i < rooms.Count; i++)
        {
            if (i != startIndex && i != stairsIndex) candidates.Add(i);
        }

        for (int k = 0; k < count && candidates.Count > 0; k++)
        {
            int idx = candidates[Random.Range(0, candidates.Count)];
            candidates.Remove(idx);

            SpellSO spell = config.altarSpellPool[Random.Range(0, config.altarSpellPool.Length)];
            RoomController rc = controllers[idx];

            Vector2 worldPos = ToWorld(rooms[idx].Center + Vector2Int.right);

            GameObject altar = Instantiate(config.altarPrefab, worldPos, Quaternion.identity, rc.transform);
            SpellAltar altarComp = altar.GetComponent<SpellAltar>();
            if (altarComp == null) continue;

            altarComp.Init(spell);
            _altarRooms.Add(idx);

            if (rc.IsCleared) altarComp.Unlock();
            else rc.OnRoomCleared += altarComp.Unlock;
        }
    }

    private void SpawnCorridorDecorations(HashSet<Vector2Int> allFloorTiles, List<Room> rooms, HashSet<Vector2Int> doorTiles)
    {
        BiomeConfigSO biome = GetCurrentBiome();
        if (biome == null) return;

        HashSet<Vector2Int> corridorTiles = new HashSet<Vector2Int>(allFloorTiles);
        foreach (var room in rooms)
        {
            foreach (var tile in room.FloorPositions)
            {
                corridorTiles.Remove(tile);
            }
        }

        GameObject corridorPropsObj = new GameObject("[Corridor_Props]");
        corridorPropsObj.transform.SetParent(transform);
        _roomObjects.Add(corridorPropsObj);

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        HashSet<Vector2Int> occupiedTiles = new HashSet<Vector2Int>();
        List<Vector2Int> wallAdjacentTiles = new List<Vector2Int>();

        foreach (var tile in corridorTiles)
        {
            bool nearDoor = false;
            foreach (var door in doorTiles)
            {
                if (Vector2Int.Distance(tile, door) < 2.5f)
                {
                    nearDoor = true;
                    break;
                }
            }
            if (nearDoor) continue;

            int wallCount = 0;
            foreach (var d in dirs)
            {
                if (!allFloorTiles.Contains(tile + d))
                    wallCount++;
            }

            if (wallCount >= 1)
            {
                wallAdjacentTiles.Add(tile);
            }
        }

        if (biome.destructiblePrefab != null)
        {
            foreach (var tile in wallAdjacentTiles)
            {
                if (Random.value < biome.corridorDestructibleChance)
                {
                    Vector2 worldPos = new Vector2(tile.x + 0.5f, tile.y + 0.5f);
                    Instantiate(biome.destructiblePrefab, worldPos, Quaternion.identity, corridorPropsObj.transform);
                    occupiedTiles.Add(tile);
                }
            }
        }

        if (biome.hazardPrefab != null)
        {
            foreach (var tile in corridorTiles)
            {
                if (occupiedTiles.Contains(tile)) continue;

                bool nearDoor = false;
                foreach (var door in doorTiles)
                {
                    if (Vector2Int.Distance(tile, door) < 2.5f)
                    {
                        nearDoor = true;
                        break;
                    }
                }
                if (nearDoor) continue;

                if (Random.value < biome.corridorHazardChance)
                {
                    Vector2 worldPos = new Vector2(tile.x + 0.5f, tile.y + 0.5f);
                    Instantiate(biome.hazardPrefab, worldPos, Quaternion.identity, corridorPropsObj.transform);
                }
            }
        }
    }

    #endregion
}