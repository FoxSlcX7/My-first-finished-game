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

    [Header("Visual")]
    public Color projectileColor = Color.white;
}