using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewBiomeConfig", menuName = "Dungeon/Biome Config")]
public class BiomeConfigSO : ScriptableObject
{
    [Header("Идентификация биома")]
    public string biomeName = "Кристальные Пещеры";
    public int startFloor = 1;
    public int endFloor = 4;

    [Header("Тайлы окружения")]
    public TileBase floorTile;
    public TileBase wallTile;

    [Header("Физика поверхности")]
    [Tooltip("Множитель управляемости: 1.0 = обычный пол, 0.25 = скользкий лед (дрифт/инерция)")]
    [Range(0.1f, 1f)] public float surfaceTraction = 1.0f;

    [Header("Враги биома")]
    public EnemyController[] biomeEnemies;

    [Header("Атмосфера")]
    public Color ambientLightColor = Color.white;
}