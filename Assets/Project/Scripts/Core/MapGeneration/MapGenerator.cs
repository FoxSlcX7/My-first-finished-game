using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Size")]
    [SerializeField] private int mapWidth = 60;
    [SerializeField] private int mapHeight = 60;
    [SerializeField] private int targetFloorTiles = 600; // целевой размер локации
    [SerializeField] private int walkLength = 120;
    [SerializeField] private int maxWalks = 12;

    [Header("Rooms")]
    [SerializeField] private int roomMinSize = 5;
    [SerializeField] private int roomMaxSize = 9;

    [Header("References")]
    [SerializeField] private MapVisualizer visualizer;

    private HashSet<Vector2Int> _floorPositions;
    private HashSet<Vector2Int> _wallPositions;
    private List<Room> _rooms;
    private BoundsInt _bounds;

    public HashSet<Vector2Int> FloorPositions => _floorPositions;
    public List<Room> Rooms => _rooms;

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        _floorPositions = new HashSet<Vector2Int>();
        _wallPositions = new HashSet<Vector2Int>();
        _rooms = new List<Room>();
        visualizer.Clear();

        _bounds = new BoundsInt(-mapWidth / 2, -mapHeight / 2, 0, mapWidth, mapHeight, 1);

        // Шаг 1: Random Walk до целевого размера
        Vector2Int currentPosition = Vector2Int.zero;
        for (int i = 0; i < maxWalks && _floorPositions.Count < targetFloorTiles; i++)
        {
            var walkPath = RandomWalkGenerator.Generate(currentPosition, walkLength, _bounds);
            _floorPositions.UnionWith(walkPath);
            currentPosition = walkPath.ElementAt(Random.Range(0, walkPath.Count));
        }

        // Шаг 2: убираем диагональные щели
        FixDiagonalPinches();

        // Шаг 3: комнаты
        CarveRooms();

        // Шаг 4: коридоры между комнатами
        ConnectRooms();

        // Шаг 5: проверка связности (Flood Fill)
        RemoveUnreachableFloor();

        // Шаг 6: стены
        GenerateWalls();

        // Шаг 7: отрисовка
        visualizer.PaintFloor(_floorPositions);
        visualizer.PaintWalls(_wallPositions);

        Debug.Log($"[MapGenerator] Пол: {_floorPositions.Count} тайлов, комнат: {_rooms.Count}");
    }

    // ═══════════════════════════════════════
    // Анти-диагональ: заполняем перемычки
    // ═══════════════════════════════════════
    private void FixDiagonalPinches()
    {
        Vector2Int[] diagonals =
        {
            new Vector2Int(1, 1), new Vector2Int(1, -1),
            new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };

        bool changed = true;
        int guard = 0;

        while (changed && guard < 4)
        {
            changed = false;
            guard++;

            foreach (var pos in _floorPositions.ToList())
            {
                foreach (var d in diagonals)
                {
                    if (!_floorPositions.Contains(pos + d)) continue;

                    Vector2Int a = pos + new Vector2Int(d.x, 0);
                    Vector2Int b = pos + new Vector2Int(0, d.y);

                    // Диагональная пара без перемычки → добавляем перемычку
                    if (!_floorPositions.Contains(a) && !_floorPositions.Contains(b))
                    {
                        _floorPositions.Add(a);
                        changed = true;
                    }
                }
            }
        }
    }

    // ═══════════════════════════════════════
    // Комнаты (масштабируются от размера карты)
    // ═══════════════════════════════════════
    private void CarveRooms()
    {
        List<Vector2Int> floorList = _floorPositions.ToList();
        int roomCount = Mathf.Clamp(floorList.Count / 80, 4, 10);

        for (int i = 0; i < roomCount; i++)
        {
            Vector2Int center = floorList[Random.Range(0, floorList.Count)];
            int size = Random.Range(roomMinSize, roomMaxSize + 1);
            int halfSize = size / 2;

            HashSet<Vector2Int> roomFloor = new HashSet<Vector2Int>();
            for (int x = -halfSize; x <= halfSize; x++)
            {
                for (int y = -halfSize; y <= halfSize; y++)
                {
                    Vector2Int pos = center + new Vector2Int(x, y);
                    pos.x = Mathf.Clamp(pos.x, _bounds.xMin, _bounds.xMax - 1);
                    pos.y = Mathf.Clamp(pos.y, _bounds.yMin, _bounds.yMax - 1);
                    roomFloor.Add(pos);
                    _floorPositions.Add(pos);
                }
            }

            _rooms.Add(new Room(center, roomFloor));
        }
    }

    private void ConnectRooms()
    {
        if (_rooms.Count < 2) return;

        for (int i = 0; i < _rooms.Count - 1; i++)
        {
            var corridor = CorridorConnector.Connect(_rooms[i].Center, _rooms[i + 1].Center);

            var wideCorridor = new HashSet<Vector2Int>(corridor);
            foreach (var pos in corridor)
            {
                wideCorridor.Add(pos + Vector2Int.up);
                wideCorridor.Add(pos + Vector2Int.down);
                wideCorridor.Add(pos + Vector2Int.left);
                wideCorridor.Add(pos + Vector2Int.right);
            }

            _floorPositions.UnionWith(wideCorridor);
        }
    }

    // ═══════════════════════════════════════
    // Flood Fill: удаляем недостижимые тайлы
    // ═══════════════════════════════════════
    private void RemoveUnreachableFloor()
    {
        // Игрок спавнится в (0,0) — walk всегда начинается оттуда
        if (!_floorPositions.Contains(Vector2Int.zero)) return;

        HashSet<Vector2Int> reached = new HashSet<Vector2Int> { Vector2Int.zero };
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(Vector2Int.zero);

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            foreach (var d in dirs)
            {
                Vector2Int next = current + d;
                if (_floorPositions.Contains(next) && reached.Add(next))
                    queue.Enqueue(next);
            }
        }

        int removed = _floorPositions.RemoveWhere(p => !reached.Contains(p));
        if (removed > 0)
            Debug.Log($"[MapGenerator] Удалено недостижимых тайлов: {removed}");
    }

    private void GenerateWalls()
    {
        foreach (var pos in _floorPositions)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    Vector2Int neighbor = pos + new Vector2Int(x, y);
                    if (!_floorPositions.Contains(neighbor))
                    {
                        _wallPositions.Add(neighbor);
                    }
                }
            }
        }
    }
}