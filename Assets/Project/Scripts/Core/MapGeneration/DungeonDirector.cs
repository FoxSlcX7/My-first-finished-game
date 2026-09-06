using System.Collections.Generic;
using UnityEngine;

public class DungeonDirector : MonoBehaviour
{
    public static DungeonDirector Instance { get; private set; }

    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private DungeonConfigSO config;

    private readonly List<GameObject> _roomObjects = new();
    public int Floor { get; private set; } = 1;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable() { mapGenerator.OnMapGenerated += BuildRooms; }
    private void OnDisable() { mapGenerator.OnMapGenerated -= BuildRooms; }

    // ═══════════════════════════════════════
    // Распределение ролей после каждой генерации
    // ═══════════════════════════════════════
    private void BuildRooms()
    {
        ClearRoomObjects();

        List<Room> rooms = mapGenerator.Rooms;
        if (rooms == null || rooms.Count == 0) return;

        // Стартовая комната — та, что СОДЕРЖИТ точку спавна (0,0).
        // Первая комната генератора всегда в (0,0), так что это комната 0.
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

        // Лестница — самая дальняя от стартовой комнаты
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

        for (int i = 0; i < rooms.Count; i++)
        {
            RoomRole role = RoomRole.Combat;
            if (i == startIndex) role = RoomRole.Safe;
            else if (i == stairsIndex) role = RoomRole.Stairs;
            else if (i == chestIndex) role = RoomRole.Chest;

            GameObject roomObj = new GameObject($"Room_{i}_{role}");
            roomObj.transform.SetParent(transform);
            RoomController rc = roomObj.AddComponent<RoomController>();
            rc.Init(rooms[i], mapGenerator.FloorPositions, role, config);
            _roomObjects.Add(roomObj);
        }

        // Игрок — всегда в центр safe-комнаты, на каждом этаже
        PlacePlayerAt(rooms[startIndex].Center);

        Debug.Log($"[DungeonDirector] Этаж {Floor}: комнат={rooms.Count}, start={startIndex}, stairs={stairsIndex}, chest={chestIndex}");
    }

    // ═══════════════════════════════════════
    // Переход на следующий этаж
    // ═══════════════════════════════════════
    public void NextFloor()
    {
        Floor++;

        foreach (var enemy in FindObjectsByType<EnemyController>(FindObjectsInactive.Exclude))
            Destroy(enemy.gameObject);

        foreach (var pickup in FindObjectsByType<SpellPickup>(FindObjectsInactive.Exclude))
            Destroy(pickup.gameObject);

        ClearRoomObjects(); // вместе с детьми: блокеры, сундук, лестница

        mapGenerator.GenerateMap();        // в конце OnMapGenerated → BuildRooms → PlacePlayerAt
        enemySpawner.RefreshSpawnPoints(); // пере-кэш точек пола
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
}