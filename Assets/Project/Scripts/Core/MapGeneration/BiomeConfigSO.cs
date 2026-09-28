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

    [Header("Опасные зоны биома")]
    [Tooltip("Префаб зоны опасности (например, лава для Глубин или шипы)")]
    public GameObject hazardPrefab;
    [Range(0, 5)] public int minHazardsPerRoom = 0;
    [Range(0, 10)] public int maxHazardsPerRoom = 0;

    [Header("Враги биома")]
    public EnemyController[] biomeEnemies;

    [Header("Атмосфера")]
    public Color ambientLightColor = Color.white;

    [Header("Разрушаемые объекты")]
    [Tooltip("Префаб ящика, бочки или кристалла для этого биома")]
    public GameObject destructiblePrefab;
    [Range(0, 5)] public int minDestructiblesPerRoom = 1;
    [Range(0, 10)] public int maxDestructiblesPerRoom = 4;

    [Header("Коридоры")]
    [Tooltip("Шанс появления лавы/ловушки на клетке коридора (0.02 = ~2% клеток)")]
    [Range(0f, 0.1f)] public float corridorHazardChance = 0.03f;

    [Tooltip("Шанс появления разрушаемого ящика в коридоре (0.015 = ~1.5% клеток)")]
    [Range(0f, 0.1f)] public float corridorDestructibleChance = 0.02f;
}