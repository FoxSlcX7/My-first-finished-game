using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Random Walk")]
    [SerializeField] private int walkLength = 80;
    [SerializeField] private int iterations = 4;

    [Header("Rooms")]
    [SerializeField] private int roomMinSize = 4;
    [SerializeField] private int roomMaxSize = 8;

    [Header("References")]
    [SerializeField] private MapVisualizer visualizer;

    private HashSet<Vector2Int> _floorPositions;
    private HashSet<Vector2Int> _wallPositions;
    private List<Room> _rooms;

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

        // Шаг 1: Random Walk — базовая форма карты
        Vector2Int currentPosition = Vector2Int.zero;
        for (int i = 0; i < iterations; i++)
        {
            var walkPath = RandomWalkGenerator.Generate(currentPosition, walkLength);
            _floorPositions.UnionWith(walkPath);
            // Следующая итерация начинается из случайной точки предыдущего пути
            currentPosition = walkPath.ElementAt(Random.Range(0, walkPath.Count));
        }

        // Шаг 2: Выделяем комнаты (квадратные области внутри floor)
        CarveRooms();

        // Шаг 3: Соединяем комнаты коридорами
        ConnectRooms();

        // Шаг 4: Генерируем стены вокруг floor
        GenerateWalls();

        // Шаг 5: Рисуем
        visualizer.PaintFloor(_floorPositions);
        visualizer.PaintWalls(_wallPositions);
    }

    private void CarveRooms()
    {
        // Простой подход: берём случайные точки из floor и делаем вокруг них квадратные комнаты
        List<Vector2Int> floorList = _floorPositions.ToList();
        int roomCount = Mathf.Min(5, floorList.Count / 20);

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
                    roomFloor.Add(pos);
                    _floorPositions.Add(pos); // добавляем в общий floor
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

            // Делаем коридор шире: добавляем соседние тайлы
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

    private void GenerateWalls()
    {
        foreach (var pos in _floorPositions)
        {
            // Проверяем 8 соседей
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