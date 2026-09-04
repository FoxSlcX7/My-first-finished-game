using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapVisualizer : MonoBehaviour
{
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTile;

    public void Clear()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }

    public void PaintFloor(IEnumerable<Vector2Int> positions)
    {
        foreach (var pos in positions)
        {
            floorTilemap.SetTile((Vector3Int)pos, floorTile);
        }
    }

    public void PaintWalls(IEnumerable<Vector2Int> positions)
    {
        foreach (var pos in positions)
        {
            wallTilemap.SetTile((Vector3Int)pos, wallTile);
        }
    }
}