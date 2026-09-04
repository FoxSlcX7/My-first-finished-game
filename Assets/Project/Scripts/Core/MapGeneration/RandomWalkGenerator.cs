using System.Collections.Generic;
using UnityEngine;

public class RandomWalkGenerator
{
    public static HashSet<Vector2Int> Generate(Vector2Int startPosition, int walkLength)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();
        Vector2Int currentPosition = startPosition;
        path.Add(currentPosition);

        for (int i = 0; i < walkLength; i++)
        {
            Vector2Int direction = GetRandomDirection();
            currentPosition += direction;
            path.Add(currentPosition);
        }

        return path;
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