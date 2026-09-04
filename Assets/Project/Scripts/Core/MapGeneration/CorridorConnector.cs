using System.Collections.Generic;
using UnityEngine;

public static class CorridorConnector
{
    public static HashSet<Vector2Int> Connect(Vector2Int from, Vector2Int to)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        Vector2Int position = from;

        corridor.Add(position);

        // Сначала по X, потом по Y (или наоборот — рандомно)
        bool moveXFirst = Random.value > 0.5f;

        if (moveXFirst)
        {
            position = MoveHorizontal(position, to.x, corridor);
            position = MoveVertical(position, to.y, corridor);
        }
        else
        {
            position = MoveVertical(position, to.y, corridor);
            position = MoveHorizontal(position, to.x, corridor);
        }

        return corridor;
    }

    private static Vector2Int MoveHorizontal(Vector2Int start, int targetX, HashSet<Vector2Int> corridor)
    {
        Vector2Int pos = start;
        while (pos.x != targetX)
        {
            pos.x += (targetX > pos.x) ? 1 : -1;
            corridor.Add(pos);
        }
        return pos;
    }

    private static Vector2Int MoveVertical(Vector2Int start, int targetY, HashSet<Vector2Int> corridor)
    {
        Vector2Int pos = start;
        while (pos.y != targetY)
        {
            pos.y += (targetY > pos.y) ? 1 : -1;
            corridor.Add(pos);
        }
        return pos;
    }
}