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
    public SpellSO[] rewardSpells;
}