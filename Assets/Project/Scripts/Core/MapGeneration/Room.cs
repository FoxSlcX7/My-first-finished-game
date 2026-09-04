using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public Vector2Int Center { get; }
    public HashSet<Vector2Int> FloorPositions { get; }
    public BoundsInt Bounds { get; }

    public Room(Vector2Int center, HashSet<Vector2Int> floorPositions)
    {
        Center = center;
        FloorPositions = floorPositions;

        // Вычисляем границы комнаты
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (var pos in floorPositions)
        {
            if (pos.x < minX) minX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y > maxY) maxY = pos.y;
        }

        Bounds = new BoundsInt(minX, minY, 0, maxX - minX + 1, maxY - minY + 1, 1);
    }
}