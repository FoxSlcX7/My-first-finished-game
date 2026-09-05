using UnityEngine;

public enum EnemyType { Melee, Ranged, Flying }

[CreateAssetMenu(fileName = "EnemyData", menuName = "Arcane Arsenal/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Enemy";
    public EnemyType type = EnemyType.Melee;

    [Header("Stats")]
    public int maxHealth = 10;
    public float moveSpeed = 3f;
    public float acceleration = 15f;
    public int contactDamage = 10;
    public float damageCooldown = 1f;

    [Header("Detection")]
    public float detectionRange = 10f;
    public float attackRange = 1.5f;

    [Header("Ranged (только для type = Ranged)")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;
    public float shootInterval = 2.5f;

    [Header("Visual")]
    public Sprite sprite;
    public Color color = Color.white;
    public Vector3 spriteScale = Vector3.one;

    [Header("Knockback")]
    [Tooltip("0 = отлетает полностью, 1 = иммунитет")]
    [Range(0f, 1f)] public float knockbackResistance = 0f;
    [Tooltip("Минимальный интервал между отбрасываниями (защита от спама)")]
    public float knockbackCooldown = 0.15f;
    public float knockbackStagger = 0.25f;
    public float contactKnockbackForce = 8f;
}