using UnityEngine;

public enum EnemyType 
{
    Melee,    // Скелеты (ближний бой, замах)
    Ranged,   // Маги (кайт, стрельба)
    Flying,   // Летучие мыши (парение, укус)
    Leaping,  // Слаймы (прыжки с приземлением)
    Kamikaze  // Камикадзе (подрыв вблизи игрока)
}

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

    [Header("Detection & Combat Ranges")]
    public float detectionRange = 10f;
    [Tooltip("Дистанция, с которой моб начинает атаку (для мага - стрельба, для слайма - прыжок, для скелета - замах)")]
    public float attackRange = 1.5f;
    [Tooltip("Точный радиус пятна урона при ударе/приземлении")]
    public float hitRadius = 0.8f;

    [Header("Ranged (только для type = Ranged)")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;
    public float shootInterval = 2.5f;

    [Header("Visual")]
    public Sprite sprite;
    public Color color = Color.white;
    public Vector3 spriteScale = Vector3.one;

    [Header("Knockback")]
    [Range(0f, 1f)] public float knockbackResistance = 0f;
    public float knockbackCooldown = 0.15f;
    public float knockbackStagger = 0.25f;
    public float contactKnockbackForce = 8f;
}