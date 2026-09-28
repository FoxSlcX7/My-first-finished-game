using System.Collections.Generic;
using UnityEngine;

public class DungeonDirector : MonoBehaviour
{
    public static DungeonDirector Instance { get; private set; }
    public int Floor { get; private set; } = 1;

    [Header("Генерация и окружение")]
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private MapVisualizer visualizer;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private DungeonConfigSO config;

    [Header("Биомы")]
    [SerializeField] private BiomeConfigSO[] biomes;

    private readonly List<GameObject> _roomObjects = new();
    private static Vector2 ToWorld(Vector2Int tile) => new Vector2(tile.x + 0.5f, tile.y + 0.5f);

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable() { mapGenerator.OnMapGenerated += BuildRooms; }
    private void OnDisable() { mapGenerator.OnMapGenerated -= BuildRooms; }

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

    private void BuildRooms()
    {
        ClearRoomObjects();

        BiomeConfigSO currentBiome = GetCurrentBiome();
        if (currentBiome != null)
        {
            if (visualizer != null)
            {
                visualizer.SetBiomeTiles(currentBiome.floorTile, currentBiome.wallTile);
                if (mapGenerator != null)
                {
                    visualizer.PaintFloor(mapGenerator.FloorPositions);
                    visualizer.PaintWalls(mapGenerator.WallPositions); // внутри само построит тени!
                }
            }

            var player = GameManager.Instance?.PlayerTransform?.GetComponent<PlayerController>();
            if (player != null)
                player.SetSurfaceTraction(currentBiome.surfaceTraction);

            Debug.Log($"[DungeonDirector] Применен биом: {currentBiome.biomeName} (Сцепление: {currentBiome.surfaceTraction})");
        }

        List<Room> rooms = mapGenerator.Rooms;
        if (rooms == null || rooms.Count == 0) return;

        // Стартовая комната
        int startIndex = -1;
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].FloorPositions.Contains(Vector2Int.zero)) { startIndex = i; break; }
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

        int chestIndex = -1;
        List<int> candidates = new List<int>();
        for (int i = 0; i < rooms.Count; i++)
            if (i != startIndex && i != stairsIndex) candidates.Add(i);
        if (candidates.Count > 0) chestIndex = candidates[Random.Range(0, candidates.Count)];

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

            GameObject roomObj = new GameObject($"Room_{i}_{role}");
            roomObj.transform.SetParent(transform);
            RoomController rc = roomObj.AddComponent<RoomController>();
            rc.Init(rooms[i], mapGenerator.FloorPositions, role, config, biomeEnemies);
            _roomObjects.Add(roomObj);
            controllers.Add(rc);
        }

        PlaceAltars(rooms, controllers, startIndex, stairsIndex);
        PlacePlayerAt(rooms[startIndex].Center);
        SpawnCorridorDecorations(mapGenerator.FloorPositions, rooms);

        Debug.Log($"[DungeonDirector] Этаж {Floor}: комнат={rooms.Count}, биом={currentBiome?.biomeName}");
    }

    public void NextFloor()
    {
        Floor++;

        foreach (var enemy in FindObjectsByType<EnemyController>(FindObjectsInactive.Exclude))
            Destroy(enemy.gameObject);

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
            if (go != null) Destroy(go);
        _roomObjects.Clear();
    }

    private void PlaceAltars(List<Room> rooms, List<RoomController> controllers, int startIndex, int stairsIndex)
    {
        if (config.altarPrefab == null || config.altarSpellPool == null || config.altarSpellPool.Length == 0) return;

        int count = Random.Range(config.altarCountMin, config.altarCountMax + 1);

        List<int> candidates = new List<int>();
        for (int i = 0; i < rooms.Count; i++)
            if (i != startIndex && i != stairsIndex) candidates.Add(i);

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

            if (rc.IsCleared) altarComp.Unlock();
            else rc.OnRoomCleared += altarComp.Unlock;
        }
    }

    /// <summary>
    /// Спавнит опасности и разрушаемые объекты в коридорах этажа
    /// </summary>
    private void SpawnCorridorDecorations(HashSet<Vector2Int> allFloorTiles, List<Room> rooms)
    {
        BiomeConfigSO biome = GetCurrentBiome();
        if (biome == null) return;

        // 1. Выделяем чистые клетки коридоров (общий пол МИНУС полы комнат)
        HashSet<Vector2Int> corridorTiles = new HashSet<Vector2Int>(allFloorTiles);
        foreach (var room in rooms)
        {
            foreach (var tile in room.FloorPositions)
            {
                corridorTiles.Remove(tile);
            }
        }

        // 2. Создаем контейнер и сразу добавляем в список на очистку
        GameObject corridorPropsObj = new GameObject("[Corridor_Props]");
        corridorPropsObj.transform.SetParent(transform);
        _roomObjects.Add(corridorPropsObj);

        foreach (var tile in corridorTiles)
        {
            Vector2 worldPos = new Vector2(tile.x + 0.5f, tile.y + 0.5f);

            // Спавн лавы / опасности в коридоре
            if (biome.hazardPrefab != null && Random.value < biome.corridorHazardChance)
            {
                Instantiate(biome.hazardPrefab, worldPos, Quaternion.identity, corridorPropsObj.transform);
                continue; // Не спавним ящик на ту же клетку
            }

            // Спавн ящика / баррикады в коридоре
            if (biome.destructiblePrefab != null && Random.value < biome.corridorDestructibleChance)
            {
                Instantiate(biome.destructiblePrefab, worldPos, Quaternion.identity, corridorPropsObj.transform);
            }
        }
    }
}