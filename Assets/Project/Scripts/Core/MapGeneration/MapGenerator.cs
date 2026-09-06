using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public event System.Action OnMapGenerated;

    [Header("Map Size")]
    [SerializeField] private int mapWidth = 60;
    [SerializeField] private int mapHeight = 60;

    [Header("Rooms")]
    [SerializeField] private int roomCountMin = 6;
    [SerializeField] private int roomCountMax = 9;
    [SerializeField] private int roomMinSize = 5;
    [SerializeField] private int roomMaxSize = 9;

    [Header("References")]
    [SerializeField] private MapVisualizer visualizer;

    private HashSet<Vector2Int> _floorPositions;
    private HashSet<Vector2Int> _wallPositions;
    private List<Room> _rooms;
    private BoundsInt _bounds;

    // Лук-апы для A*: какой тайл какой комнате принадлежит + «кольцо» вокруг комнат
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
        ConnectRooms();
        FixDiagonalPinches();
        RemoveUnreachableFloor();
        GenerateWalls();

        visualizer.PaintFloor(_floorPositions);
        visualizer.PaintWalls(_wallPositions);

        Debug.Log($"[MapGenerator] Пол: {_floorPositions.Count} тайлов, комнат: {_rooms.Count}");
        OnMapGenerated?.Invoke();
    }

    // ═══════════════════════════════════════
    // Комнаты: первая всегда в (0,0), остальные разбросаны с дистанцией
    // ═══════════════════════════════════════
    private void CarveRooms()
    {
        int roomCount = Random.Range(roomCountMin, roomCountMax + 1);
        int minDist = roomMaxSize + 3;

        List<Vector2Int> centers = new List<Vector2Int> { Vector2Int.zero };

        int attempts = 0;
        while (centers.Count < roomCount && attempts < 300)
        {
            attempts++;
            Vector2Int candidate = new Vector2Int(
                Random.Range(_bounds.xMin + roomMaxSize, _bounds.xMax - roomMaxSize + 1),
                Random.Range(_bounds.yMin + roomMaxSize, _bounds.yMax - roomMaxSize + 1));

            bool ok = true;
            foreach (var c in centers)
            {
                if ((candidate - c).sqrMagnitude < minDist * minDist) { ok = false; break; }
            }
            if (ok) centers.Add(candidate);
        }

        foreach (var center in centers)
        {
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

    private void BuildRoomLookup()
    {
        _roomAt.Clear();
        _roomRing.Clear();

        for (int i = 0; i < _rooms.Count; i++)
            foreach (var t in _rooms[i].FloorPositions)
                _roomAt[t] = i;

        foreach (var t in _roomAt.Keys.ToList())
        {
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector2Int n = t + new Vector2Int(dx, dy);
                    if (!_roomAt.ContainsKey(n))
                        _roomRing.Add(n);
                }
        }
    }

    // ═══════════════════════════════════════
    // Коридоры: A* обходит чужие комнаты и не липнет к их стенам
    // ═══════════════════════════════════════
    private void ConnectRooms()
    {
        if (_rooms.Count < 2) return;

        for (int i = 0; i < _rooms.Count - 1; i++)
        {
            int a = i, b = i + 1;
            List<Vector2Int> path = FindPath(_rooms[a].Center, _rooms[b].Center, a, b);

            foreach (var t in path)
            {
                if (!_roomAt.ContainsKey(t))
                    _floorPositions.Add(t);

                // Расширение до 3 тайлов, но НЕ впритык к чужим комнатам
                Vector2Int[] nbs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                foreach (var n in nbs)
                {
                    Vector2Int w = t + n;
                    if (!_bounds.Contains(new Vector3Int(w.x, w.y, 0))) continue;
                    if (_roomAt.ContainsKey(w)) continue;      // не заходим в комнаты
                    if (TouchesOtherRoom(w, a, b)) continue;   // не клеимся к чужим
                    _floorPositions.Add(w);
                }
            }
        }
    }

    private static float Heuristic(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    private float TileCost(Vector2Int t, int roomA, int roomB)
    {
        if (_roomAt.TryGetValue(t, out int r))
            return (r == roomA || r == roomB) ? 1f : 400f; // чужая комната — почти табу

        if (_roomRing.Contains(t))
            return 100f; // кольцо вокруг комнат — дорого, путь идёт вдали от стен

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

    /// <summary>
    // Простой A* (карта мала, вызывается несколько раз за генерацию).
    // Позже этот же подход используем для pathfinding врагов.
    /// </summary>
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

        // Фолбэк (практически недостижим): прямая линия
        return new List<Vector2Int> { from, to };
    }

    // ═══════════════════════════════════════
    // Анти-диагональ
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
    // Flood Fill
    // ═══════════════════════════════════════
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
        int removed = _floorPositions.RemoveWhere(p => !reached.Contains(p));
        if (removed > 0)
            Debug.Log($"[MapGenerator] Удалено недостижимых тайлов: {removed}");
    }

    // ═══════════════════════════════════════
    // Сплошная порода: никаких синих дыр
    // ═══════════════════════════════════════
    // ═══════════════════════════════════════
    // Стены: контур вокруг пола + заливка замкнутых карманов.
    // Внешняя пустота остаётся фоном (без «серого моря»),
    // синих дыр больше нет.
    // ═══════════════════════════════════════
    private void GenerateWalls()
    {
        // Шаг 1: «внешняя» пустота — не-пол, связанный с границей карты
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

        // Шаг 2: стена = (замкнутый карман) ИЛИ (контур, прилегающий к полу)
        for (int x = _bounds.xMin; x < _bounds.xMax; x++)
        {
            for (int y = _bounds.yMin; y < _bounds.yMax; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (_floorPositions.Contains(pos)) continue;

                bool isOutside = outside.Contains(pos);

                bool touchesFloor = false;
                for (int dx = -1; dx <= 1 && !touchesFloor; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (_floorPositions.Contains(pos + new Vector2Int(dx, dy)))
                            touchesFloor = true;
                    }

                if (!isOutside || touchesFloor)
                    _wallPositions.Add(pos);
            }
        }
    }
}