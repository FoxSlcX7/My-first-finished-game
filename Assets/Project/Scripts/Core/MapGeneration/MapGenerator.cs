using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public event System.Action OnMapGenerated;

    [Header("Размеры подземелья")]
    [SerializeField] private int mapWidth = 80;
    [SerializeField] private int mapHeight = 80;

    [Header("Настройки комнат")]
    [Tooltip("Количество комнат на этаже")]
    [SerializeField] private int roomCountMin = 6;
    [SerializeField] private int roomCountMax = 8;

    [Tooltip("Минимальный и максимальный размер боевой комнаты (тайлы)")]
    [SerializeField] private int roomMinSize = 12;
    [SerializeField] private int roomMaxSize = 18;

    [Tooltip("Минимальное расстояние между центрами комнат")]
    [SerializeField] private int minDistanceBetweenRooms = 18;

    [Header("Ссылки")]
    [SerializeField] private MapVisualizer visualizer;

    private HashSet<Vector2Int> _floorPositions;
    private HashSet<Vector2Int> _wallPositions;
    private List<Room> _rooms;
    private BoundsInt _bounds;

    private readonly Dictionary<Vector2Int, int> _roomAt = new();
    private readonly HashSet<Vector2Int> _roomRing = new();

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

        CarveRooms();
        BuildRoomLookup();
        ConnectRoomsSmarter(); // Умное соединение ближайших соседей
        FixDiagonalPinches();
        RemoveUnreachableFloor();
        GenerateWalls();

        visualizer.PaintFloor(_floorPositions);
        visualizer.PaintWalls(_wallPositions);

        Debug.Log($"[MapGenerator] Готово! Пол: {_floorPositions.Count} тайлов, комнат: {_rooms.Count}");
        OnMapGenerated?.Invoke();
    }

    // ═══════════════════════════════════════
    // 1. Создание просторных комнат
    // ═══════════════════════════════════════
    private void CarveRooms()
    {
        int targetRooms = Random.Range(roomCountMin, roomCountMax + 1);
        List<Vector2Int> centers = new List<Vector2Int> { Vector2Int.zero };

        int attempts = 0;
        int maxAttempts = 500;

        while (centers.Count < targetRooms && attempts < maxAttempts)
        {
            attempts++;
            int padding = roomMaxSize;
            Vector2Int candidate = new Vector2Int(
                Random.Range(_bounds.xMin + padding, _bounds.xMax - padding),
                Random.Range(_bounds.yMin + padding, _bounds.yMax - padding));

            bool canPlace = true;
            foreach (var c in centers)
            {
                if (Vector2Int.Distance(candidate, c) < minDistanceBetweenRooms)
                {
                    canPlace = false;
                    break;
                }
            }

            if (canPlace) centers.Add(candidate);
        }

        for (int i = 0; i < centers.Count; i++)
        {
            Vector2Int center = centers[i];

            // Первая комната (стартовая) чуть компактнее, остальные — просторные арены
            int sizeX = (i == 0) ? 12 : Random.Range(roomMinSize, roomMaxSize + 1);
            int sizeY = (i == 0) ? 12 : Random.Range(roomMinSize, roomMaxSize + 1);

            int halfX = sizeX / 2;
            int halfY = sizeY / 2;

            HashSet<Vector2Int> roomFloor = new HashSet<Vector2Int>();

            for (int x = -halfX; x <= halfX; x++)
            {
                for (int y = -halfY; y <= halfY; y++)
                {
                    Vector2Int pos = center + new Vector2Int(x, y);
                    pos.x = Mathf.Clamp(pos.x, _bounds.xMin + 1, _bounds.xMax - 2);
                    pos.y = Mathf.Clamp(pos.y, _bounds.yMin + 1, _bounds.yMax - 2);

                    roomFloor.Add(pos);
                    _floorPositions.Add(pos);
                }
            }

            _rooms.Add(new Room(center, roomFloor));
        }
    }

    private void BuildRoomLookup()
    {
        _roomAt.Clear();
        _roomRing.Clear();

        for (int i = 0; i < _rooms.Count; i++)
        {
            foreach (var t in _rooms[i].FloorPositions)
                _roomAt[t] = i;
        }

        foreach (var t in _roomAt.Keys.ToList())
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector2Int n = t + new Vector2Int(dx, dy);
                    if (!_roomAt.ContainsKey(n))
                        _roomRing.Add(n);
                }
            }
        }
    }

    // ═══════════════════════════════════════
    // 2. Умное соединение (MST / Ближайшие соседи)
    // Больше никаких бесконечных кишок через всю карту!
    // ═══════════════════════════════════════
    private void ConnectRoomsSmarter()
    {
        if (_rooms.Count < 2) return;

        List<int> connected = new List<int> { 0 };
        List<int> unconnected = Enumerable.Range(1, _rooms.Count - 1).ToList();

        // Соединяем дерево минимальных путей (Prim's algorithm)
        while (unconnected.Count > 0)
        {
            float shortestDist = float.MaxValue;
            int bestFrom = -1;
            int bestTo = -1;

            foreach (int u in connected)
            {
                foreach (int v in unconnected)
                {
                    float dist = Vector2Int.Distance(_rooms[u].Center, _rooms[v].Center);
                    if (dist < shortestDist)
                    {
                        shortestDist = dist;
                        bestFrom = u;
                        bestTo = v;
                    }
                }
            }

            if (bestFrom != -1 && bestTo != -1)
            {
                BuildCorridorPath(bestFrom, bestTo);
                connected.Add(bestTo);
                unconnected.Remove(bestTo);
            }
            else break;
        }

        // Дополнительный луп (1 случайная перемычка), чтобы подземелье не было строго линейным
        if (_rooms.Count >= 4)
        {
            int extraA = Random.Range(1, _rooms.Count / 2);
            int extraB = Random.Range(_rooms.Count / 2, _rooms.Count);
            BuildCorridorPath(extraA, extraB);
        }
    }

    private void BuildCorridorPath(int roomA, int roomB)
    {
        List<Vector2Int> path = FindPath(_rooms[roomA].Center, _rooms[roomB].Center, roomA, roomB);

        foreach (var t in path)
        {
            if (!_roomAt.ContainsKey(t))
                _floorPositions.Add(t);

            // Делаем коридор шириной 3 тайла, чтобы игрок и мобы свободно расходились
            Vector2Int[] nbs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var n in nbs)
            {
                Vector2Int w = t + n;
                if (!_bounds.Contains(new Vector3Int(w.x, w.y, 0))) continue;
                if (_roomAt.ContainsKey(w)) continue;
                if (TouchesOtherRoom(w, roomA, roomB)) continue;
                _floorPositions.Add(w);
            }
        }
    }

    private static float Heuristic(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    private float TileCost(Vector2Int t, int roomA, int roomB)
    {
        if (_roomAt.TryGetValue(t, out int r))
            return (r == roomA || r == roomB) ? 1f : 500f;

        if (_roomRing.Contains(t))
            return 80f;

        return 1f;
    }

    private bool TouchesOtherRoom(Vector2Int t, int a, int b)
    {
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (_roomAt.TryGetValue(t + new Vector2Int(dx, dy), out int r) && r != a && r != b)
                    return true;
            }
        return false;
    }

    private List<Vector2Int> FindPath(Vector2Int from, Vector2Int to, int roomA, int roomB)
    {
        List<Vector2Int> open = new List<Vector2Int> { from };
        HashSet<Vector2Int> closed = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float> { [from] = 0f };
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (open.Count > 0)
        {
            int bestIdx = 0;
            float bestF = float.MaxValue;
            for (int i = 0; i < open.Count; i++)
            {
                float f = gScore[open[i]] + Heuristic(open[i], to);
                if (f < bestF) { bestF = f; bestIdx = i; }
            }

            Vector2Int current = open[bestIdx];
            open.RemoveAt(bestIdx);

            if (current == to)
            {
                List<Vector2Int> path = new List<Vector2Int>();
                while (cameFrom.ContainsKey(current))
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                path.Add(from);
                path.Reverse();
                return path;
            }

            closed.Add(current);

            foreach (var d in dirs)
            {
                Vector2Int n = current + d;
                if (!_bounds.Contains(new Vector3Int(n.x, n.y, 0))) continue;
                if (closed.Contains(n)) continue;

                float tentative = gScore[current] + TileCost(n, roomA, roomB);
                if (!gScore.TryGetValue(n, out float g) || tentative < g)
                {
                    gScore[n] = tentative;
                    cameFrom[n] = current;
                    if (!open.Contains(n)) open.Add(n);
                }
            }
        }

        return new List<Vector2Int> { from, to };
    }

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
                    if (!_floorPositions.Contains(a) && !_floorPositions.Contains(b))
                    {
                        _floorPositions.Add(a);
                        changed = true;
                    }
                }
            }
        }
    }

    private void RemoveUnreachableFloor()
    {
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
        _floorPositions.RemoveWhere(p => !reached.Contains(p));
    }

    private void GenerateWalls()
    {
        HashSet<Vector2Int> outside = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        void EnqueueIfVoid(Vector2Int p)
        {
            if (!_floorPositions.Contains(p) && outside.Add(p))
                queue.Enqueue(p);
        }

        for (int x = _bounds.xMin; x < _bounds.xMax; x++)
        {
            EnqueueIfVoid(new Vector2Int(x, _bounds.yMin));
            EnqueueIfVoid(new Vector2Int(x, _bounds.yMax - 1));
        }
        for (int y = _bounds.yMin; y < _bounds.yMax; y++)
        {
            EnqueueIfVoid(new Vector2Int(_bounds.xMin, y));
            EnqueueIfVoid(new Vector2Int(_bounds.xMax - 1, y));
        }

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            foreach (var d in dirs)
            {
                Vector2Int next = current + d;
                if (!_bounds.Contains(new Vector3Int(next.x, next.y, 0))) continue;
                if (_floorPositions.Contains(next)) continue;
                if (outside.Add(next)) queue.Enqueue(next);
            }
        }

        for (int x = _bounds.xMin; x < _bounds.xMax; x++)
        {
            for (int y = _bounds.yMin; y < _bounds.yMax; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (_floorPositions.Contains(pos)) continue;

                bool isOutside = outside.Contains(pos);
                bool touchesFloor = false;

                for (int dx = -1; dx <= 1 && !touchesFloor; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (_floorPositions.Contains(pos + new Vector2Int(dx, dy)))
                            touchesFloor = true;
                    }
                }

                if (!isOutside || touchesFloor)
                    _wallPositions.Add(pos);
            }
        }
    }
}