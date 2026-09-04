using UnityEngine;

[CreateAssetMenu(fileName = "Combo", menuName = "Arcane Arsenal/Spell Combo")]
public class SpellComboSO : ScriptableObject
{
    public ElementType elementA;
    public ElementType elementB;

    [Header("Cooldown")]
    public float cooldown = 1f;

    [Header("Result")]
    public string comboName;
    public Sprite icon;
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;
    public float lifetime = 3f;
    public int damage = 3;
    public Color projectileColor = Color.white;

    // Проверяет, подходит ли комбо под два элемента (в любом порядке)
    public bool Matches(ElementType a, ElementType b)
    {
        return (a == elementA && b == elementB) || (a == elementB && b == elementA);
    }
}