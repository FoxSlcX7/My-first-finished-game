using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Dungeon Config", fileName = "DungeonConfig")]
public class DungeonConfigSO : ScriptableObject
{
    [Header("Волна")]
    public EnemyController[] wavePrefabs;
    [Tooltip("Сколько тайлов комнаты приходится на 1 врага (больше = легче).")]
    public float tilesPerEnemy = 6f;
    public int minWaveCount = 2;
    public int maxWaveCount = 8;
    [Tooltip("+ врагов за каждый этаж после первого.")]
    public int enemyCountPerFloor = 1;
    public float minWaveSpawnDistance = 3f;

    [Header("Префабы контента")]
    public GameObject doorBlockerPrefab;
    public GameObject chestPrefab;
    public GameObject stairsPrefab;

    [Header("Награда из сундука")]
    

    [Header("Пейсинг волны")]
    [Tooltip("Задержка до первого врага волны (сек).")]
    public float waveStartDelay = 0.8f;
    [Tooltip("Интервал между батчами (сек).")]
    public float waveSpawnInterval = 0.9f;
    [Tooltip("Сколько врагов выходит за один батч.")]
    public int waveBatchSize = 1;

    [Header("Алтари")]
    public GameObject altarPrefab;
    public SpellSO[] altarSpellPool;
    public int altarCountMin = 1;
    public int altarCountMax = 2;
}