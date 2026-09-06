using System.Collections.Generic;
using UnityEngine;

public static class RandomWalkGenerator
{
    public static HashSet<Vector2Int> Generate(Vector2Int startPosition, int walkLength, BoundsInt bounds)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();
        Vector2Int currentPosition = ClampToBounds(startPosition, bounds);
        path.Add(currentPosition);

        for (int i = 0; i < walkLength; i++)
        {
            Vector2Int direction = GetRandomDirection();
            currentPosition = ClampToBounds(currentPosition + direction, bounds);
            path.Add(currentPosition);

            // ⭐ «Толщина»: добавляем случайного соседа,
            // чтобы коридоры были ~2 тайла, а не 1
            path.Add(ClampToBounds(currentPosition + GetRandomDirection(), bounds));
        }

        return path;
    }

    private static Vector2Int ClampToBounds(Vector2Int pos, BoundsInt bounds)
    {
        pos.x = Mathf.Clamp(pos.x, bounds.xMin, bounds.xMax - 1);
        pos.y = Mathf.Clamp(pos.y, bounds.yMin, bounds.yMax - 1);
        return pos;
    }

    private static Vector2Int GetRandomDirection()
    {
        int random = Random.Range(0, 4);
        return random switch
        {
            0 => Vector2Int.up,
            1 => Vector2Int.down,
            2 => Vector2Int.left,
            3 => Vector2Int.right,
            _ => Vector2Int.zero
        };
    }
}