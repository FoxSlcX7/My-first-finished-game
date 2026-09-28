using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapVisualizer : MonoBehaviour
{
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTile;

    private TilemapShadowCaster2D _shadowCaster;

    private void Awake()
    {
        if (wallTilemap != null)
            _shadowCaster = wallTilemap.GetComponent<TilemapShadowCaster2D>();
    }

    public void Clear()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        if (_shadowCaster != null) _shadowCaster.ClearShadows();
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

        if (_shadowCaster == null && wallTilemap != null)
            _shadowCaster = wallTilemap.GetComponent<TilemapShadowCaster2D>();

        if (_shadowCaster != null)
        {
            Physics2D.SyncTransforms();
            _shadowCaster.RebuildShadows();
        }
    }

    public void SetBiomeTiles(TileBase newFloor, TileBase newWall)
    {
        if (newFloor != null) floorTile = newFloor;
        if (newWall != null) wallTile = newWall;
    }
}