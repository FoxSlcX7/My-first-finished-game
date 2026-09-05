using UnityEngine;

[CreateAssetMenu(fileName = "Spell", menuName = "Arcane Arsenal/Spell")]
public class SpellSO : ScriptableObject
{
    public string spellName;
    public ElementType element;
    public Sprite icon;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float lifetime = 3f;

    [Header("Damage")]
    public int damage = 1;
    public float knockbackForce = 8f;

    [Header("Visual")]
    public Color projectileColor = Color.white;

    [Header("Cooldown")]
    public float cooldown = 0.5f;

    [Header("Spell System v2 (Strategy)")]
    public SpellType spellType;
    public SpellEffectBase effect;
}